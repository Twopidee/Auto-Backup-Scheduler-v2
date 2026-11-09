Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

Public Enum ScheduleKind
    Weekly = 0
    Monthly = 1
End Enum

''' <summary>
''' All user settings, stored as JSON in %AppData%\AutoBackupSchedulerV2\settings.json.
''' The Nextcloud server URL, username and password are encrypted with Windows DPAPI so only your Windows account can read them.
''' </summary>
Public Class AppSettings

    ''' <summary>Number of backups kept locally and on Nextcloud. Oldest are deleted first.</summary>
    Public Const KeepCount As Integer = 10

    ''' <summary>The folders that can be backed up, in display order. Also used in the backup file names.</summary>
    Public Shared ReadOnly SourceNames As String() = {"Master Control", "CrowdGate", "Alarm"}

    Public Shared ReadOnly Property DataFolder As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoBackupSchedulerV2")

    Private Shared ReadOnly Property SettingsFile As String = Path.Combine(DataFolder, "settings.json")

    ' Folders: one entry per name in SourceNames ("" = not used).
    Public Property SourceFolders As String() = {"", "", ""}
    ' One entry per name in SourceNames; unchecked folders are not backed up (some farms don't have every system).
    Public Property SourceEnabled As Boolean() = {True, True, True}
    Public Property BackupFolder As String = ""

    ' Option 2: back up when the folder changes
    Public Property AutoOnChange As Boolean = False
    Public Property QuietSeconds As Integer = 60

    ' Option 3: weekly / monthly schedule
    Public Property ScheduleEnabled As Boolean = False
    Public Property Schedule As ScheduleKind = ScheduleKind.Weekly
    Public Property ScheduleDayOfWeek As DayOfWeek = DayOfWeek.Sunday
    Public Property ScheduleDayOfMonth As Integer = 1
    Public Property ScheduleTime As TimeSpan = New TimeSpan(2, 0, 0)
    Public Property NextScheduledRun As DateTime? = Nothing

    Public Property LastBackup As DateTime? = Nothing

    ' Nextcloud. The server URL, username and app password are all encrypted with DPAPI.
    Public Property NextcloudEnabled As Boolean = False
    Public Property NextcloudUrlProtected As String = ""
    Public Property NextcloudUserProtected As String = ""
    Public Property NextcloudPasswordProtected As String = ""
    ''' <summary>Folder inside <see cref="NextcloudBaseFolder"/> on the server ("" = the base folder itself).</summary>
    Public Property NextcloudFolder As String = ""
    Public Property AllowInvalidCertificate As Boolean = False

    ' Older versions saved the server URL and username as plain text. They are read once,
    ' encrypted and then removed from the settings file.
    <JsonPropertyName("NextcloudUrl"), JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)>
    Public Property LegacyNextcloudUrl As String
    <JsonPropertyName("NextcloudUser"), JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)>
    Public Property LegacyNextcloudUser As String

    ' Options
    Public Property MinimizeToTray As Boolean = True
    Public Property StartWithWindows As Boolean = False

    ''' <summary>Backups always go somewhere inside this folder on the Nextcloud server.</summary>
    Public Const NextcloudBaseFolder As String = "Work stuff/CCW/Backups"

    Public Const PasswordUnreadableMessage As String =
        "The saved Nextcloud server URL, username or app password can't be read on this computer or Windows account. " &
        "This happens when settings are copied from another computer or user, or after the Windows password was reset by an administrator. " &
        "Re-enter the Nextcloud server URL, username and app password and click Save Settings."

    ''' <summary>
    ''' True when the server URL, username or password was saved but can't be decrypted here
    ''' (it was encrypted for a different Windows account or computer).
    ''' </summary>
    <JsonIgnore>
    Public ReadOnly Property NextcloudCredentialsUnreadable As Boolean
        Get
            Return IsUnreadable(NextcloudUrlProtected) OrElse IsUnreadable(NextcloudUserProtected) OrElse IsUnreadable(NextcloudPasswordProtected)
        End Get
    End Property

    <JsonIgnore>
    Public Property NextcloudUrl As String
        Get
            Return Unprotect(NextcloudUrlProtected)
        End Get
        Set(value As String)
            NextcloudUrlProtected = Protect(value)
        End Set
    End Property

    <JsonIgnore>
    Public Property NextcloudUser As String
        Get
            Return Unprotect(NextcloudUserProtected)
        End Get
        Set(value As String)
            NextcloudUserProtected = Protect(value)
        End Set
    End Property

    <JsonIgnore>
    Public Property NextcloudPassword As String
        Get
            Return Unprotect(NextcloudPasswordProtected)
        End Get
        Set(value As String)
            NextcloudPasswordProtected = Protect(value)
        End Set
    End Property

    ''' <summary>
    ''' Cleans up a remote folder typed by the user so it is relative to <see cref="NextcloudBaseFolder"/>:
    ''' "Work stuff/CCW/Backups/PC/", "CCW/Backups/PC" and "\PC" all become "PC". "." and ".." are dropped so the
    ''' path can never point outside the base folder.
    ''' </summary>
    Public Shared Function NormalizeRemoteFolder(folder As String) As String
        Dim parts = If(folder, "").Split({"/"c, "\"c}, StringSplitOptions.RemoveEmptyEntries Or StringSplitOptions.TrimEntries).
                                   Where(Function(p) p <> "." AndAlso p <> "..").ToList()
        ' Strip a leading "Work stuff/CCW/Backups", or a leading "CCW/Backups" saved back when the base was just "Work stuff".
        Dim baseParts = NextcloudBaseFolder.Split("/"c)
        For Each prefix In {baseParts, baseParts.Skip(1).ToArray()}
            If prefix.Length > 0 AndAlso parts.Count >= prefix.Length AndAlso
               parts.Take(prefix.Length).SequenceEqual(prefix, StringComparer.OrdinalIgnoreCase) Then
                parts.RemoveRange(0, prefix.Length)
                Exit For
            End If
        Next
        Return String.Join("/", parts)
    End Function

    ''' <summary>The full path on the server, e.g. "PC" becomes "Work stuff/CCW/Backups/PC".</summary>
    Public Shared Function FullRemoteFolder(relativeFolder As String) As String
        Dim relative = NormalizeRemoteFolder(relativeFolder)
        Return If(relative = "", NextcloudBaseFolder, NextcloudBaseFolder & "/" & relative)
    End Function

    Private Shared Function Protect(value As String) As String
        If String.IsNullOrEmpty(value) Then Return ""
        Dim data = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Nothing, DataProtectionScope.CurrentUser)
        Return Convert.ToBase64String(data)
    End Function

    Private Shared Function Unprotect(protectedText As String) As String
        If String.IsNullOrEmpty(protectedText) Then Return ""
        Return If(TryUnprotect(protectedText), "")
    End Function

    Private Shared Function IsUnreadable(protectedText As String) As Boolean
        Return Not String.IsNullOrEmpty(protectedText) AndAlso TryUnprotect(protectedText) Is Nothing
    End Function

    ''' <summary>Encrypts plain-text values left by older versions. Returns True if anything changed.</summary>
    Private Function UpgradeOldSettings() As Boolean
        Dim changed = False
        If LegacyNextcloudUrl IsNot Nothing Then
            If NextcloudUrlProtected = "" Then NextcloudUrl = LegacyNextcloudUrl
            LegacyNextcloudUrl = Nothing
            changed = True
        End If
        If LegacyNextcloudUser IsNot Nothing Then
            If NextcloudUserProtected = "" Then NextcloudUser = LegacyNextcloudUser
            LegacyNextcloudUser = Nothing
            changed = True
        End If
        Dim folder = NormalizeRemoteFolder(NextcloudFolder)
        If folder <> NextcloudFolder Then
            NextcloudFolder = folder
            changed = True
        End If
        If SourceFolders Is Nothing OrElse SourceFolders.Length <> SourceNames.Length Then
            Dim folders = If(SourceFolders, Array.Empty(Of String)())
            ReDim Preserve folders(SourceNames.Length - 1)
            SourceFolders = folders.Select(Function(f) If(f, "")).ToArray()
            changed = True
        End If
        If SourceEnabled Is Nothing OrElse SourceEnabled.Length <> SourceNames.Length Then
            Dim enabled = If(SourceEnabled, Array.Empty(Of Boolean)())
            Dim oldLength = enabled.Length
            ReDim Preserve enabled(SourceNames.Length - 1)
            For i = oldLength To enabled.Length - 1
                enabled(i) = True
            Next
            SourceEnabled = enabled
            changed = True
        End If
        Return changed
    End Function

    ''' <summary>True when the folder at this index is checked and has a path filled in.</summary>
    Public Function IsSourceActive(index As Integer) As Boolean
        Return SourceEnabled(index) AndAlso SourceFolders(index) <> ""
    End Function

    Private Shared Function TryUnprotect(protectedText As String) As String
        Try
            Dim data = ProtectedData.Unprotect(Convert.FromBase64String(protectedText), Nothing, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(data)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function Load() As AppSettings
        Try
            ' On first run, carry over the Nextcloud, schedule and backup storage settings from version 1
            ' (or from "NextcloudBackup", its older name). Version 1's single source folder is not used.
            Dim appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            For Each legacyFile In {Path.Combine(appData, "AutoBackupScheduler", "settings.json"),
                                    Path.Combine(appData, "NextcloudBackup", "settings.json")}
                If File.Exists(SettingsFile) Then Exit For
                If File.Exists(legacyFile) Then
                    Directory.CreateDirectory(DataFolder)
                    File.Copy(legacyFile, SettingsFile)
                End If
            Next
        Catch ex As Exception
        End Try

        Try
            If File.Exists(SettingsFile) Then
                Dim loaded = JsonSerializer.Deserialize(Of AppSettings)(File.ReadAllText(SettingsFile))
                If loaded IsNot Nothing Then
                    If loaded.UpgradeOldSettings() Then
                        Try
                            loaded.Save()   ' Removes the plain-text values from disk right away.
                        Catch ex As Exception
                        End Try
                    End If
                    Return loaded
                End If
            End If
        Catch ex As Exception
            ' Unreadable settings file: fall back to defaults.
        End Try
        Return New AppSettings()
    End Function

    Public Sub Save()
        Directory.CreateDirectory(DataFolder)
        Dim json = JsonSerializer.Serialize(Me, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(SettingsFile, json)
    End Sub

    ''' <summary>Returns the first scheduled backup time strictly after <paramref name="after"/>.</summary>
    Public Function ComputeNextRun(after As DateTime) As DateTime
        If Schedule = ScheduleKind.Weekly Then
            Dim candidate = after.Date.Add(ScheduleTime)
            Dim daysAhead = (CInt(ScheduleDayOfWeek) - CInt(candidate.DayOfWeek) + 7) Mod 7
            candidate = candidate.AddDays(daysAhead)
            If candidate <= after Then candidate = candidate.AddDays(7)
            Return candidate
        End If

        ' Monthly: days past the end of a short month (e.g. the 31st in April) run on its last day.
        Dim monthStart = New DateTime(after.Year, after.Month, 1)
        For i = 0 To 12
            Dim m = monthStart.AddMonths(i)
            Dim day = Math.Min(ScheduleDayOfMonth, DateTime.DaysInMonth(m.Year, m.Month))
            Dim candidate = New DateTime(m.Year, m.Month, day).Add(ScheduleTime)
            If candidate > after Then Return candidate
        Next
        Return after.AddMonths(1)
    End Function

End Class
