Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Win32

Public Class MainForm

    Private Const RunKeyPath As String = "Software\Microsoft\Windows\CurrentVersion\Run"
    Private Const RunValueName As String = "AutoBackupSchedulerV2"

    Private _settings As AppSettings
    ''' <summary>One text box per entry in <see cref="AppSettings.SourceNames"/>, in the same order.</summary>
    Private ReadOnly _sourceBoxes As TextBox()
    ''' <summary>The enable check box and Browse button for each entry in <see cref="_sourceBoxes"/>.</summary>
    Private ReadOnly _sourceChecks As CheckBox()
    Private ReadOnly _sourceBrowseButtons As Button()
    Private ReadOnly _watchers As New List(Of FileSystemWatcher)
    ''' <summary>Indexes of the source folders changed since the last on-change backup.</summary>
    Private ReadOnly _changedSources As New HashSet(Of Integer)
    Private WithEvents _quietTimer As New System.Windows.Forms.Timer()
    Private WithEvents _scheduleTimer As New System.Windows.Forms.Timer() With {.Interval = 30000}
    Private _backupRunning As Boolean
    Private _queuedReason As String
    Private ReadOnly _queuedSources As New HashSet(Of Integer)
    Private _startHidden As Boolean
    Private _exiting As Boolean
    Private _trayHintShown As Boolean
    Private _passwordWarningPending As Boolean

    Public Sub New(startHidden As Boolean)
        InitializeComponent()
        _sourceBoxes = {txtMasterControl, txtCrowdGate, txtAlarm}
        _sourceChecks = {chkMasterControl, chkCrowdGate, chkAlarm}
        _sourceBrowseButtons = {btnBrowseMasterControl, btnBrowseCrowdGate, btnBrowseAlarm}
        _startHidden = startHidden
        ' Reuse the icon embedded in the .exe (ApplicationIcon in the .vbproj) for the window and tray.
        Dim appIcon = If(Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath), SystemIcons.Shield)
        Icon = appIcon
        notifyIcon.Icon = appIcon
        cboDayOfWeek.Items.AddRange(CultureInfo.CurrentCulture.DateTimeFormat.DayNames)

        _settings = AppSettings.Load()
        LoadSettingsIntoControls()
        ApplyRuntimeSettings()
        _scheduleTimer.Start()
        Log("Auto Backup Scheduler v2 started.")

        lblRemoteBase.Text = AppSettings.NextcloudBaseFolder & " /"

        If _settings.NextcloudEnabled AndAlso _settings.NextcloudCredentialsUnreadable Then
            Log("WARNING: " & AppSettings.PasswordUnreadableMessage)
            txtServer.PlaceholderText = "Re-enter the server URL"
            txtUser.PlaceholderText = "Re-enter the username"
            txtPassword.PlaceholderText = "Re-enter the app password"
            For Each lbl In {lblServer, lblUser, lblPassword}
                lbl.ForeColor = Color.Firebrick
            Next
            If startHidden Then
                notifyIcon.ShowBalloonTip(10000, "Nextcloud login needs to be re-entered",
                                          "Open Auto Backup Scheduler, re-enter the Nextcloud server URL, username and app password, then click Save Settings.", ToolTipIcon.Warning)
            Else
                _passwordWarningPending = True
            End If
        End If
    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If _passwordWarningPending Then
            _passwordWarningPending = False
            ShowWarning(AppSettings.PasswordUnreadableMessage)
            txtServer.Focus()
        End If
    End Sub

    ' Lets the app start straight into the tray when launched with /minimized (Start with Windows).
    Protected Overrides Sub SetVisibleCore(value As Boolean)
        If _startHidden Then
            _startHidden = False
            If Not IsHandleCreated Then CreateHandle()
            value = False
        End If
        MyBase.SetVisibleCore(value)
    End Sub

#Region "Settings"

    Private Sub LoadSettingsIntoControls()
        Dim s = _settings
        For i = 0 To _sourceBoxes.Length - 1
            _sourceBoxes(i).Text = s.SourceFolders(i)
            _sourceChecks(i).Checked = s.SourceEnabled(i)
        Next
        UpdateSourceControls()
        txtBackup.Text = s.BackupFolder
        chkAutoOnChange.Checked = s.AutoOnChange
        nudQuiet.Value = Math.Max(nudQuiet.Minimum, Math.Min(nudQuiet.Maximum, CDec(s.QuietSeconds)))
        chkSchedule.Checked = s.ScheduleEnabled
        rbWeekly.Checked = s.Schedule = ScheduleKind.Weekly
        rbMonthly.Checked = s.Schedule = ScheduleKind.Monthly
        cboDayOfWeek.SelectedIndex = CInt(s.ScheduleDayOfWeek)
        nudDayOfMonth.Value = Math.Max(1D, Math.Min(31D, CDec(s.ScheduleDayOfMonth)))
        dtpTime.Value = DateTime.Today.Add(s.ScheduleTime)
        chkNextcloud.Checked = s.NextcloudEnabled
        txtServer.Text = s.NextcloudUrl
        txtUser.Text = s.NextcloudUser
        txtPassword.Text = s.NextcloudPassword
        txtRemote.Text = s.NextcloudFolder
        chkAllowSelfSigned.Checked = s.AllowInvalidCertificate
        chkMinimizeToTray.Checked = s.MinimizeToTray
        chkStartWithWindows.Checked = s.StartWithWindows
        UpdateScheduleControls()
        UpdateNextcloudControls()
        UpdateStatusLabels()
    End Sub

    ''' <summary>Validates the form, copies it into the settings, saves and applies them.</summary>
    Private Function SaveFromControls() As Boolean
        Dim sources = _sourceBoxes.Select(Function(t) t.Text.Trim()).ToArray()
        Dim enabled = _sourceChecks.Select(Function(c) c.Checked).ToArray()
        Dim target = txtBackup.Text.Trim()

        For i = 0 To sources.Length - 1
            Dim source = sources(i)
            If Not enabled(i) Then Continue For
            If source = "" Then
                ShowWarning($"Choose the {AppSettings.SourceNames(i)} directory, or uncheck it if this farm doesn't have one.")
                Return False
            End If
            If Not Directory.Exists(source) Then
                ShowWarning($"The {AppSettings.SourceNames(i)} directory does not exist.")
                Return False
            End If
            If target <> "" AndAlso
               Path.GetFullPath(source).TrimEnd("\"c).Equals(Path.GetFullPath(target).TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
                ShowWarning($"The backup storage folder must be different from the {AppSettings.SourceNames(i)} directory.")
                Return False
            End If
        Next
        If chkNextcloud.Checked AndAlso (txtServer.Text.Trim() = "" OrElse txtUser.Text.Trim() = "" OrElse txtPassword.Text = "") Then
            ShowWarning("To upload to Nextcloud, fill in the server URL, username and app password.")
            Return False
        End If

        Dim s = _settings
        Dim newDay = CType(Math.Max(0, cboDayOfWeek.SelectedIndex), DayOfWeek)
        Dim newKind = If(rbMonthly.Checked, ScheduleKind.Monthly, ScheduleKind.Weekly)
        Dim newTime = New TimeSpan(dtpTime.Value.Hour, dtpTime.Value.Minute, 0)
        Dim scheduleChanged = s.ScheduleEnabled <> chkSchedule.Checked OrElse s.Schedule <> newKind OrElse
                              s.ScheduleDayOfWeek <> newDay OrElse s.ScheduleDayOfMonth <> CInt(nudDayOfMonth.Value) OrElse
                              s.ScheduleTime <> newTime

        s.SourceFolders = sources
        s.SourceEnabled = enabled
        s.BackupFolder = target
        s.AutoOnChange = chkAutoOnChange.Checked
        s.QuietSeconds = CInt(nudQuiet.Value)
        s.ScheduleEnabled = chkSchedule.Checked
        s.Schedule = newKind
        s.ScheduleDayOfWeek = newDay
        s.ScheduleDayOfMonth = CInt(nudDayOfMonth.Value)
        s.ScheduleTime = newTime
        s.NextcloudEnabled = chkNextcloud.Checked
        s.NextcloudUrl = txtServer.Text.Trim()
        s.NextcloudUser = txtUser.Text.Trim()
        s.NextcloudPassword = txtPassword.Text
        s.NextcloudFolder = AppSettings.NormalizeRemoteFolder(txtRemote.Text)
        txtRemote.Text = s.NextcloudFolder
        s.AllowInvalidCertificate = chkAllowSelfSigned.Checked
        s.MinimizeToTray = chkMinimizeToTray.Checked
        s.StartWithWindows = chkStartWithWindows.Checked

        If Not s.ScheduleEnabled Then
            s.NextScheduledRun = Nothing
        ElseIf scheduleChanged OrElse Not s.NextScheduledRun.HasValue Then
            s.NextScheduledRun = s.ComputeNextRun(DateTime.Now)
        End If

        If Not TrySaveSettings() Then Return False
        For Each lbl In {lblServer, lblUser, lblPassword}
            lbl.ForeColor = SystemColors.ControlText
        Next
        txtServer.PlaceholderText = "https://cloud.example.lan"
        txtUser.PlaceholderText = ""
        txtPassword.PlaceholderText = ""
        ApplyRuntimeSettings()
        Return True
    End Function

    Private Function TrySaveSettings() As Boolean
        Try
            _settings.Save()
            Return True
        Catch ex As Exception
            Log("Could not save settings: " & ex.Message)
            ShowWarning("Could not save settings: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub ApplyRuntimeSettings()
        ConfigureWatcher()
        UpdateStartWithWindows()
        UpdateStatusLabels()
    End Sub

    Private Sub UpdateStartWithWindows()
        Try
            Using key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable:=True)
                If key Is Nothing Then Return
                ' Remove the startup entry left by the old "NextcloudBackup" version.
                key.DeleteValue("NextcloudBackup", throwOnMissingValue:=False)
                If _settings.StartWithWindows Then
                    key.SetValue(RunValueName, $"""{Application.ExecutablePath}"" /minimized")
                Else
                    key.DeleteValue(RunValueName, throwOnMissingValue:=False)
                End If
            End Using
        Catch ex As Exception
            Log("Could not update the Start with Windows setting: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Option 2: watch the folder for changes"

    Private Sub ConfigureWatcher()
        For Each watcher In _watchers
            watcher.EnableRaisingEvents = False
            watcher.Dispose()
        Next
        _watchers.Clear()
        _changedSources.Clear()
        _quietTimer.Stop()

        If Not _settings.AutoOnChange Then Return

        For Each i In ConfiguredSources()
            Dim index = i
            Dim folder = _settings.SourceFolders(i)
            If Not Directory.Exists(folder) Then
                Log($"Automatic backup on change is on, but the {AppSettings.SourceNames(i)} directory was not found.")
                Continue For
            End If

            Try
                Dim watcher = New FileSystemWatcher(folder) With {
                    .IncludeSubdirectories = True,
                    .InternalBufferSize = 65536,
                    .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.DirectoryName Or NotifyFilters.LastWrite Or
                                    NotifyFilters.Size Or NotifyFilters.Attributes Or NotifyFilters.CreationTime Or NotifyFilters.Security
                }
                Dim changed As FileSystemEventHandler = Sub(sender, e) OnSourceChanged(index, e)
                AddHandler watcher.Changed, changed
                AddHandler watcher.Created, changed
                AddHandler watcher.Deleted, changed
                AddHandler watcher.Renamed, Sub(sender, e) OnSourceChanged(index, e)
                AddHandler watcher.Error, Sub(sender, e) OnWatcherError(index, e)
                watcher.EnableRaisingEvents = True
                _watchers.Add(watcher)
                Log($"Watching the {AppSettings.SourceNames(i)} directory ({folder}) for changes.")
            Catch ex As Exception
                Log($"Could not watch the {AppSettings.SourceNames(i)} directory for changes: " & ex.Message)
            End Try
        Next
    End Sub

    ' Runs on a background thread.
    Private Sub OnSourceChanged(sourceIndex As Integer, e As FileSystemEventArgs)
        Dim backupFolder = _settings.BackupFolder
        Try
            If backupFolder <> "" AndAlso BackupEngine.IsSameOrInside(e.FullPath, backupFolder) Then Return
        Catch ex As Exception
        End Try
        If IsHandleCreated Then BeginInvoke(New Action(Sub() RestartQuietTimer(sourceIndex)))
    End Sub

    ' Runs on a background thread. Usually means too many changes at once; restart and back up to be safe.
    Private Sub OnWatcherError(sourceIndex As Integer, e As ErrorEventArgs)
        If Not IsHandleCreated Then Return
        BeginInvoke(New Action(
            Sub()
                Log("Folder watcher error (" & e.GetException().Message & "). Restarting the watchers.")
                Dim pending = _changedSources.ToList()
                ConfigureWatcher()
                _changedSources.UnionWith(pending)
                RestartQuietTimer(sourceIndex)
            End Sub))
    End Sub

    ''' <summary>Waits for a quiet period so a burst of changes produces one backup, not hundreds.</summary>
    Private Sub RestartQuietTimer(sourceIndex As Integer)
        If Not _settings.AutoOnChange Then Return
        _changedSources.Add(sourceIndex)
        _quietTimer.Stop()
        _quietTimer.Interval = Math.Max(1, _settings.QuietSeconds) * 1000
        _quietTimer.Start()
        If Not _backupRunning Then lblStatus.Text = "Change detected - waiting for activity to settle..."
    End Sub

    Private Async Sub _quietTimer_Tick(sender As Object, e As EventArgs) Handles _quietTimer.Tick
        _quietTimer.Stop()
        Dim changed = _changedSources.ToList()
        _changedSources.Clear()
        Await RunBackupAsync("Folder contents changed", changed)
    End Sub

#End Region

#Region "Option 3: weekly / monthly schedule"

    Private Async Sub _scheduleTimer_Tick(sender As Object, e As EventArgs) Handles _scheduleTimer.Tick
        Dim s = _settings
        If Not s.ScheduleEnabled OrElse Not s.NextScheduledRun.HasValue Then Return
        If DateTime.Now < s.NextScheduledRun.Value Then Return

        ' Also catches up on a run that was missed because the PC was off.
        s.NextScheduledRun = s.ComputeNextRun(DateTime.Now)
        TrySaveSettings()
        UpdateStatusLabels()
        Await RunBackupAsync("Scheduled backup", ConfiguredSources())
    End Sub

    Private Sub UpdateScheduleControls()
        Dim enabled = chkSchedule.Checked
        rbWeekly.Enabled = enabled
        rbMonthly.Enabled = enabled
        cboDayOfWeek.Enabled = enabled
        nudDayOfMonth.Enabled = enabled
        dtpTime.Enabled = enabled
        cboDayOfWeek.Visible = rbWeekly.Checked
        nudDayOfMonth.Visible = rbMonthly.Checked
        lblDay.Text = If(rbMonthly.Checked, "Day of month:", "Day:")
        Dim dayControlLeft = If(rbMonthly.Checked, 125, 95)
        cboDayOfWeek.Left = dayControlLeft
        nudDayOfMonth.Left = dayControlLeft
    End Sub

    Private Sub ScheduleOptionChanged(sender As Object, e As EventArgs) Handles chkSchedule.CheckedChanged, rbWeekly.CheckedChanged, rbMonthly.CheckedChanged
        UpdateScheduleControls()
    End Sub

#End Region

#Region "Backup + Nextcloud upload"

    ''' <summary>Indexes of the source folders that are checked and have been filled in.</summary>
    Private Function ConfiguredSources() As List(Of Integer)
        Return Enumerable.Range(0, AppSettings.SourceNames.Length).Where(Function(i) _settings.IsSourceActive(i)).ToList()
    End Function

    ''' <param name="sources">Indexes into <see cref="AppSettings.SourceNames"/> of the folders to back up.</param>
    Private Async Function RunBackupAsync(reason As String, sources As IEnumerable(Of Integer)) As Task
        If _backupRunning Then
            If _queuedReason Is Nothing Then Log($"{reason}: a backup is already running; another will start when it finishes.")
            _queuedReason = reason
            _queuedSources.UnionWith(sources)
            Return
        End If

        Dim s = _settings
        Dim toBackUp = sources.Distinct().Where(Function(i) s.IsSourceActive(i)).OrderBy(Function(i) i).ToList()
        If toBackUp.Count = 0 Then Return

        _backupRunning = True
        ' Pending on-change backups for these directories are covered by this run.
        _changedSources.ExceptWith(toBackUp)
        If _changedSources.Count = 0 Then _quietTimer.Stop()
        Dim target = s.BackupFolder
        Dim failed = False
        Dim anySucceeded = False

        For Each i In toBackUp
            Dim name = AppSettings.SourceNames(i)
            Dim source = s.SourceFolders(i)
            Try
                SetBusy(True, $"Backing up {name}...")
                Log($"{reason}: backing up the {name} directory ({source})")
                Dim result = Await Task.Run(Function() BackupEngine.CreateBackup(source, BackupEngine.BackupBaseName(name), target, AppSettings.KeepCount))

                Log($"Created {Path.GetFileName(result.ZipPath)} ({result.FilesAdded:N0} files, {FormatSize(New FileInfo(result.ZipPath).Length)}).")
                If result.SkippedItems.Count > 0 Then
                    Log($"Skipped {result.SkippedItems.Count} item(s) that could not be read (locked or access denied):")
                    For Each item In result.SkippedItems.Take(10)
                        Log("    " & item)
                    Next
                    If result.SkippedItems.Count > 10 Then Log($"    ...and {result.SkippedItems.Count - 10} more.")
                End If
                For Each deletedName In result.DeletedBackups
                    Log($"Removed old backup {deletedName} (keeping the newest {AppSettings.KeepCount}).")
                Next
                anySucceeded = True
            Catch ex As Exception
                failed = True
                Log($"{name.ToUpperInvariant()} BACKUP FAILED: " & DescribeError(ex))
                notifyIcon.ShowBalloonTip(5000, $"{name} backup failed", DescribeError(ex), ToolTipIcon.Error)
            End Try
        Next

        If anySucceeded Then
            s.LastBackup = DateTime.Now
            TrySaveSettings()
        End If

        If anySucceeded AndAlso s.NextcloudEnabled Then
            Try
                If s.NextcloudCredentialsUnreadable Then Throw New InvalidOperationException(AppSettings.PasswordUnreadableMessage)
                SetBusy(True, "Uploading to Nextcloud...")
                Await SyncToNextcloudAsync(target, ConfiguredSources().Select(Function(i) BackupEngine.BackupBaseName(AppSettings.SourceNames(i))))
            Catch ex As Exception
                failed = True
                Log("NEXTCLOUD UPLOAD FAILED: " & DescribeError(ex))
                Log("The local backup is safe and will be uploaded on the next successful connection.")
                notifyIcon.ShowBalloonTip(5000, "Nextcloud upload failed", DescribeError(ex), ToolTipIcon.Warning)
            End Try
        End If

        If Not failed AndAlso Not Visible Then
            notifyIcon.ShowBalloonTip(3000, "Backup complete", $"{reason} finished successfully.", ToolTipIcon.Info)
        End If

        _backupRunning = False
        SetBusy(False, If(failed, "Last backup had a problem - see the log below.", "Idle"))
        UpdateStatusLabels()

        If _queuedReason IsNot Nothing Then
            Dim nextReason = _queuedReason
            Dim nextSources = _queuedSources.ToList()
            _queuedReason = Nothing
            _queuedSources.Clear()
            Await RunBackupAsync(nextReason, nextSources)
        End If
    End Function

    ''' <summary>
    ''' For each directory, uploads every local backup that isn't on the server yet (so a failed upload is
    ''' retried next time), then deletes the oldest ones on the server so only the newest 10 remain there too.
    ''' </summary>
    Private Async Function SyncToNextcloudAsync(localFolder As String, baseNames As IEnumerable(Of String)) As Task
        Dim s = _settings
        Dim remoteFolder = AppSettings.FullRemoteFolder(s.NextcloudFolder)
        Using client As New NextcloudClient(s.NextcloudUrl, s.NextcloudUser, s.NextcloudPassword, s.AllowInvalidCertificate)
            Await client.EnsureFolderAsync(remoteFolder)
            Dim remoteNames As New HashSet(Of String)(Await client.ListFilesAsync(remoteFolder), StringComparer.OrdinalIgnoreCase)

            For Each baseName In baseNames
                For Each localFile In BackupEngine.ListBackups(localFolder, baseName).AsEnumerable().Reverse()
                    Dim name = Path.GetFileName(localFile)
                    If remoteNames.Contains(name) Then Continue For
                    Log($"Uploading {name} ({FormatSize(New FileInfo(localFile).Length)}) to Nextcloud...")
                    Await client.UploadFileAsync(localFile, remoteFolder)
                    remoteNames.Add(name)
                    Log($"Uploaded {name}.")
                Next

                Dim remoteBackups = remoteNames.Where(Function(n) BackupEngine.IsBackupFileName(n, baseName)).
                                                OrderByDescending(Function(n) n, StringComparer.OrdinalIgnoreCase).
                                                ToList()
                For Each old In remoteBackups.Skip(AppSettings.KeepCount)
                    Await client.DeleteFileAsync(remoteFolder, old)
                    remoteNames.Remove(old)
                    Log($"Removed old backup {old} from Nextcloud.")
                Next
            Next
        End Using
    End Function

    Private Sub SetBusy(busy As Boolean, status As String)
        progressBar.Visible = busy
        btnBackupNow.Enabled = Not busy
        mnuBackupNow.Enabled = Not busy
        lblStatus.Text = status
        Dim tip = "Auto Backup Scheduler - " & status
        notifyIcon.Text = If(tip.Length > 63, tip.Substring(0, 63), tip)
    End Sub

    Private Sub UpdateStatusLabels()
        Dim s = _settings
        lblLastBackup.Text = "Last backup: " & If(s.LastBackup.HasValue, s.LastBackup.Value.ToString("g"), "never")
        If s.ScheduleEnabled AndAlso s.NextScheduledRun.HasValue Then
            lblNextRun.Text = "Next scheduled backup: " & s.NextScheduledRun.Value.ToString("ddd, MMM d, yyyy 'at' t")
        Else
            lblNextRun.Text = "Scheduled backups are off."
        End If
    End Sub

#End Region

#Region "Buttons"

    Private Async Sub btnBackupNow_Click(sender As Object, e As EventArgs) Handles btnBackupNow.Click
        ' Use whatever is currently on screen, so you don't have to press Save first.
        If Not SaveFromControls() Then Return
        If ConfiguredSources().Count = 0 OrElse _settings.BackupFolder = "" Then
            ShowWarning("Check at least one directory to back up and choose a backup storage folder first.")
            Return
        End If
        Await RunBackupAsync("Manual backup", ConfiguredSources())
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If SaveFromControls() Then
            Log("Settings saved.")
            lblStatus.Text = "Settings saved."
        End If
    End Sub

    Private Sub btnBrowseSource_Click(sender As Object, e As EventArgs) Handles btnBrowseMasterControl.Click, btnBrowseCrowdGate.Click, btnBrowseAlarm.Click
        Dim index = Array.IndexOf(_sourceBrowseButtons, sender)
        Dim box = _sourceBoxes(index)
        Dim selected = PickFolder($"Select the {AppSettings.SourceNames(index)} directory", box.Text)
        If selected IsNot Nothing Then box.Text = selected
    End Sub

    Private Sub chkSource_CheckedChanged(sender As Object, e As EventArgs) Handles chkMasterControl.CheckedChanged, chkCrowdGate.CheckedChanged, chkAlarm.CheckedChanged
        UpdateSourceControls()
    End Sub

    ''' <summary>Greys out the path and Browse button of unchecked directories.</summary>
    Private Sub UpdateSourceControls()
        If _sourceChecks Is Nothing Then Return   ' CheckedChanged fires during InitializeComponent.
        For i = 0 To _sourceChecks.Length - 1
            _sourceBoxes(i).Enabled = _sourceChecks(i).Checked
            _sourceBrowseButtons(i).Enabled = _sourceChecks(i).Checked
        Next
    End Sub

    Private Sub btnBrowseBackup_Click(sender As Object, e As EventArgs) Handles btnBrowseBackup.Click
        Dim selected = PickFolder("Select where backups are stored", txtBackup.Text)
        If selected IsNot Nothing Then txtBackup.Text = selected
    End Sub

    Private Sub btnNewFolder_Click(sender As Object, e As EventArgs) Handles btnNewFolder.Click
        Dim createdInDialog As Boolean
        Dim parent = PickFolder("Choose where to create the new backup folder", txtBackup.Text, createdInDialog)
        If parent Is Nothing Then Return

        ' A folder made (and renamed) with the picker's own "New folder" button is used as-is.
        If createdInDialog Then
            txtBackup.Text = parent
            Log("Created backup folder " & parent)
            Return
        End If

        Dim name = InputBox("Name for the new backup folder:", "Create Backup Folder", "Backups").Trim()
        If name = "" Then Return
        If name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
            ShowWarning("That name contains characters that aren't allowed in a folder name.")
            Return
        End If

        Dim newFolder = Path.Combine(parent, name)
        Try
            Directory.CreateDirectory(newFolder)
            txtBackup.Text = newFolder
            Log("Created backup folder " & newFolder)
        Catch ex As Exception
            ShowWarning("Could not create the folder: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnTestNextcloud_Click(sender As Object, e As EventArgs) Handles btnTestNextcloud.Click
        btnTestNextcloud.Enabled = False
        UseWaitCursor = True
        Try
            Using client As New NextcloudClient(txtServer.Text, txtUser.Text, txtPassword.Text, chkAllowSelfSigned.Checked)
                Await client.TestConnectionAsync()
                Await client.EnsureFolderAsync(AppSettings.FullRemoteFolder(txtRemote.Text))
            End Using
            MessageBox.Show(Me, $"Connected to Nextcloud successfully and the folder ""{AppSettings.FullRemoteFolder(txtRemote.Text)}"" is ready.",
                            "Nextcloud", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(Me, "Could not connect to Nextcloud:" & Environment.NewLine & Environment.NewLine & DescribeError(ex),
                            "Nextcloud", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            UseWaitCursor = False
            btnTestNextcloud.Enabled = True
        End Try
    End Sub

    Private Sub btnBrowseRemote_Click(sender As Object, e As EventArgs) Handles btnBrowseRemote.Click
        Try
            Using client As New NextcloudClient(txtServer.Text, txtUser.Text, txtPassword.Text, chkAllowSelfSigned.Checked),
                  dlg As New RemoteFolderDialog(client, txtRemote.Text, AddressOf DescribeError)
                If dlg.ShowDialog(Me) = DialogResult.OK Then txtRemote.Text = dlg.SelectedFolder
            End Using
        Catch ex As Exception
            ShowWarning(DescribeError(ex))
        End Try
    End Sub

    Private Sub chkNextcloud_CheckedChanged(sender As Object, e As EventArgs) Handles chkNextcloud.CheckedChanged
        UpdateNextcloudControls()
    End Sub

    Private Sub UpdateNextcloudControls()
        Dim enabled = chkNextcloud.Checked
        For Each ctl In New Control() {txtServer, txtUser, txtPassword, txtRemote, btnBrowseRemote, chkAllowSelfSigned, btnTestNextcloud}
            ctl.Enabled = enabled
        Next
    End Sub

#End Region

#Region "Tray / window"

    Private Sub ShowFromTray()
        Show()
        WindowState = FormWindowState.Normal
        Activate()
    End Sub

    Private Sub notifyIcon_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles notifyIcon.MouseDoubleClick
        ShowFromTray()
    End Sub

    Private Sub mnuOpen_Click(sender As Object, e As EventArgs) Handles mnuOpen.Click
        ShowFromTray()
    End Sub

    Private Async Sub mnuBackupNow_Click(sender As Object, e As EventArgs) Handles mnuBackupNow.Click
        Await RunBackupAsync("Manual backup", ConfiguredSources())
    End Sub

    Private Sub mnuExit_Click(sender As Object, e As EventArgs) Handles mnuExit.Click
        _exiting = True
        Close()
    End Sub

    Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If WindowState = FormWindowState.Minimized AndAlso _settings IsNot Nothing AndAlso _settings.MinimizeToTray Then Hide()
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not _exiting AndAlso e.CloseReason = CloseReason.UserClosing AndAlso _settings.MinimizeToTray Then
            e.Cancel = True
            Hide()
            If Not _trayHintShown Then
                notifyIcon.ShowBalloonTip(3000, "Auto Backup Scheduler is still running",
                                          "Automatic backups continue in the background. Right-click the tray icon to exit.", ToolTipIcon.Info)
                _trayHintShown = True
            End If
            Return
        End If

        If _backupRunning AndAlso e.CloseReason = CloseReason.UserClosing Then
            Dim answer = MessageBox.Show(Me, "A backup is in progress. Exit anyway?", "Auto Backup Scheduler",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If answer = DialogResult.No Then
                e.Cancel = True
                _exiting = False
                Return
            End If
        End If

        For Each watcher In _watchers
            watcher.Dispose()
        Next
        notifyIcon.Visible = False
    End Sub

#End Region

#Region "Helpers"

    Private Function PickFolder(description As String, initial As String) As String
        Return PickFolder(description, initial, Nothing)
    End Function

    ''' <summary>
    ''' Shows the folder picker. If a folder was made with the dialog's own "New folder" button and renamed,
    ''' Windows often still returns the folder it was created in; in that case the new folder is returned instead.
    ''' </summary>
    Private Function PickFolder(description As String, initial As String, ByRef createdInDialog As Boolean) As String
        createdInDialog = False
        Using dlg As New FolderBrowserDialog() With {.Description = description, .UseDescriptionForTitle = True, .ShowNewFolderButton = True}
            If Directory.Exists(initial) Then dlg.SelectedPath = initial
            Dim openedAt = DateTime.Now
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return Nothing

            Dim selected = dlg.SelectedPath
            Try
                If Directory.GetCreationTime(selected) >= openedAt Then
                    createdInDialog = True
                    Return selected
                End If
                Dim newest = New DirectoryInfo(selected).GetDirectories().
                                 Where(Function(d) d.CreationTime >= openedAt).
                                 OrderByDescending(Function(d) d.CreationTime).
                                 FirstOrDefault()
                If newest IsNot Nothing Then
                    createdInDialog = True
                    Return newest.FullName
                End If
            Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
            End Try
            Return selected
        End Using
    End Function

    Private Sub Log(message As String)
        Dim line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}"
        If txtLog.TextLength > 200000 Then txtLog.Text = txtLog.Text.Substring(100000)
        txtLog.AppendText(line & Environment.NewLine)

        Try
            Directory.CreateDirectory(AppSettings.DataFolder)
            Dim logFile = Path.Combine(AppSettings.DataFolder, "backup.log")
            If File.Exists(logFile) AndAlso New FileInfo(logFile).Length > 5 * 1024 * 1024 Then
                File.Copy(logFile, logFile & ".old", overwrite:=True)
                File.Delete(logFile)
            End If
            File.AppendAllText(logFile, line & Environment.NewLine)
        Catch ex As Exception
            ' Logging to disk is best-effort.
        End Try
    End Sub

    Private Sub ShowWarning(message As String)
        MessageBox.Show(Me, message, "Auto Backup Scheduler", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Function DescribeError(ex As Exception) As String
        Dim root = ex.GetBaseException()
        Return HideNextcloudLogin(If(root Is ex, ex.Message, $"{ex.Message} ({root.Message})"))
    End Function

    ''' <summary>
    ''' Keeps the Nextcloud server address and username out of the log and pop-ups
    ''' (network errors often include the host name).
    ''' </summary>
    Private Function HideNextcloudLogin(text As String) As String
        Dim secrets As New List(Of String)
        For Each server In {txtServer.Text, _settings.NextcloudUrl}
            If String.IsNullOrWhiteSpace(server) Then Continue For
            secrets.Add(server.Trim())
            Try
                secrets.Add(New Uri(NextcloudClient.NormalizeServerRoot(server)).Host)
            Catch ex As UriFormatException
            End Try
        Next
        For Each user In {txtUser.Text.Trim(), _settings.NextcloudUser}
            If String.IsNullOrEmpty(user) Then Continue For
            secrets.Add(user)
            secrets.Add(Uri.EscapeDataString(user))
        Next
        ' Longest first, so the full URL is hidden before its host name; very short values would hide ordinary words.
        For Each secret In secrets.Where(Function(x) x.Length >= 3).Distinct().OrderByDescending(Function(x) x.Length)
            text = text.Replace(secret, "***", StringComparison.OrdinalIgnoreCase)
        Next
        Return text
    End Function

    Private Shared Function FormatSize(bytes As Long) As String
        Dim units = {"bytes", "KB", "MB", "GB", "TB"}
        Dim size = CDbl(bytes)
        Dim unit = 0
        While size >= 1024 AndAlso unit < units.Length - 1
            size /= 1024
            unit += 1
        End While
        Return If(unit = 0, $"{bytes} bytes", $"{size:0.0} {units(unit)}")
    End Function

#End Region

End Class
