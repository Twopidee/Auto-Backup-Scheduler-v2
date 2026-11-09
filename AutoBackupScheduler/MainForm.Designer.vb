<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.grpFolders = New System.Windows.Forms.GroupBox()
        Me.chkMasterControl = New System.Windows.Forms.CheckBox()
        Me.txtMasterControl = New System.Windows.Forms.TextBox()
        Me.btnBrowseMasterControl = New System.Windows.Forms.Button()
        Me.chkCrowdGate = New System.Windows.Forms.CheckBox()
        Me.txtCrowdGate = New System.Windows.Forms.TextBox()
        Me.btnBrowseCrowdGate = New System.Windows.Forms.Button()
        Me.chkAlarm = New System.Windows.Forms.CheckBox()
        Me.txtAlarm = New System.Windows.Forms.TextBox()
        Me.btnBrowseAlarm = New System.Windows.Forms.Button()
        Me.lblBackup = New System.Windows.Forms.Label()
        Me.txtBackup = New System.Windows.Forms.TextBox()
        Me.btnBrowseBackup = New System.Windows.Forms.Button()
        Me.btnNewFolder = New System.Windows.Forms.Button()
        Me.lblKeep = New System.Windows.Forms.Label()
        Me.grpManual = New System.Windows.Forms.GroupBox()
        Me.btnBackupNow = New System.Windows.Forms.Button()
        Me.lblLastBackup = New System.Windows.Forms.Label()
        Me.grpOnChange = New System.Windows.Forms.GroupBox()
        Me.chkAutoOnChange = New System.Windows.Forms.CheckBox()
        Me.lblQuiet = New System.Windows.Forms.Label()
        Me.nudQuiet = New System.Windows.Forms.NumericUpDown()
        Me.grpSchedule = New System.Windows.Forms.GroupBox()
        Me.chkSchedule = New System.Windows.Forms.CheckBox()
        Me.rbWeekly = New System.Windows.Forms.RadioButton()
        Me.rbMonthly = New System.Windows.Forms.RadioButton()
        Me.lblDay = New System.Windows.Forms.Label()
        Me.cboDayOfWeek = New System.Windows.Forms.ComboBox()
        Me.nudDayOfMonth = New System.Windows.Forms.NumericUpDown()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.dtpTime = New System.Windows.Forms.DateTimePicker()
        Me.lblNextRun = New System.Windows.Forms.Label()
        Me.grpNextcloud = New System.Windows.Forms.GroupBox()
        Me.chkNextcloud = New System.Windows.Forms.CheckBox()
        Me.lblServer = New System.Windows.Forms.Label()
        Me.txtServer = New System.Windows.Forms.TextBox()
        Me.lblUser = New System.Windows.Forms.Label()
        Me.txtUser = New System.Windows.Forms.TextBox()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.lblRemote = New System.Windows.Forms.Label()
        Me.txtRemote = New System.Windows.Forms.TextBox()
        Me.lblRemoteBase = New System.Windows.Forms.Label()
        Me.btnBrowseRemote = New System.Windows.Forms.Button()
        Me.chkAllowSelfSigned = New System.Windows.Forms.CheckBox()
        Me.btnTestNextcloud = New System.Windows.Forms.Button()
        Me.grpOptions = New System.Windows.Forms.GroupBox()
        Me.chkMinimizeToTray = New System.Windows.Forms.CheckBox()
        Me.chkStartWithWindows = New System.Windows.Forms.CheckBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.progressBar = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblLog = New System.Windows.Forms.Label()
        Me.txtLog = New System.Windows.Forms.TextBox()
        Me.notifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.trayMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuOpen = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackupNow = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.grpFolders.SuspendLayout()
        Me.grpManual.SuspendLayout()
        Me.grpOnChange.SuspendLayout()
        CType(Me.nudQuiet, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpSchedule.SuspendLayout()
        CType(Me.nudDayOfMonth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpNextcloud.SuspendLayout()
        Me.grpOptions.SuspendLayout()
        Me.trayMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpFolders
        '
        Me.grpFolders.Controls.Add(Me.chkMasterControl)
        Me.grpFolders.Controls.Add(Me.txtMasterControl)
        Me.grpFolders.Controls.Add(Me.btnBrowseMasterControl)
        Me.grpFolders.Controls.Add(Me.chkCrowdGate)
        Me.grpFolders.Controls.Add(Me.txtCrowdGate)
        Me.grpFolders.Controls.Add(Me.btnBrowseCrowdGate)
        Me.grpFolders.Controls.Add(Me.chkAlarm)
        Me.grpFolders.Controls.Add(Me.txtAlarm)
        Me.grpFolders.Controls.Add(Me.btnBrowseAlarm)
        Me.grpFolders.Controls.Add(Me.lblBackup)
        Me.grpFolders.Controls.Add(Me.txtBackup)
        Me.grpFolders.Controls.Add(Me.btnBrowseBackup)
        Me.grpFolders.Controls.Add(Me.btnNewFolder)
        Me.grpFolders.Controls.Add(Me.lblKeep)
        Me.grpFolders.Location = New System.Drawing.Point(12, 12)
        Me.grpFolders.Name = "grpFolders"
        Me.grpFolders.Size = New System.Drawing.Size(430, 262)
        Me.grpFolders.TabIndex = 0
        Me.grpFolders.TabStop = False
        Me.grpFolders.Text = "Folders"
        '
        'chkMasterControl
        '
        Me.chkMasterControl.AutoSize = True
        Me.chkMasterControl.Checked = True
        Me.chkMasterControl.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkMasterControl.Location = New System.Drawing.Point(12, 19)
        Me.chkMasterControl.Name = "chkMasterControl"
        Me.chkMasterControl.TabIndex = 0
        Me.chkMasterControl.Text = "Master Control Directory:"
        Me.chkMasterControl.UseVisualStyleBackColor = True
        '
        'txtMasterControl
        '
        Me.txtMasterControl.Location = New System.Drawing.Point(12, 40)
        Me.txtMasterControl.Name = "txtMasterControl"
        Me.txtMasterControl.Size = New System.Drawing.Size(315, 23)
        Me.txtMasterControl.TabIndex = 1
        '
        'btnBrowseMasterControl
        '
        Me.btnBrowseMasterControl.Location = New System.Drawing.Point(333, 39)
        Me.btnBrowseMasterControl.Name = "btnBrowseMasterControl"
        Me.btnBrowseMasterControl.Size = New System.Drawing.Size(85, 25)
        Me.btnBrowseMasterControl.TabIndex = 2
        Me.btnBrowseMasterControl.Text = "Browse..."
        Me.btnBrowseMasterControl.UseVisualStyleBackColor = True
        '
        'chkCrowdGate
        '
        Me.chkCrowdGate.AutoSize = True
        Me.chkCrowdGate.Checked = True
        Me.chkCrowdGate.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCrowdGate.Location = New System.Drawing.Point(12, 69)
        Me.chkCrowdGate.Name = "chkCrowdGate"
        Me.chkCrowdGate.TabIndex = 3
        Me.chkCrowdGate.Text = "CrowdGate Directory:"
        Me.chkCrowdGate.UseVisualStyleBackColor = True
        '
        'txtCrowdGate
        '
        Me.txtCrowdGate.Location = New System.Drawing.Point(12, 90)
        Me.txtCrowdGate.Name = "txtCrowdGate"
        Me.txtCrowdGate.Size = New System.Drawing.Size(315, 23)
        Me.txtCrowdGate.TabIndex = 4
        '
        'btnBrowseCrowdGate
        '
        Me.btnBrowseCrowdGate.Location = New System.Drawing.Point(333, 89)
        Me.btnBrowseCrowdGate.Name = "btnBrowseCrowdGate"
        Me.btnBrowseCrowdGate.Size = New System.Drawing.Size(85, 25)
        Me.btnBrowseCrowdGate.TabIndex = 5
        Me.btnBrowseCrowdGate.Text = "Browse..."
        Me.btnBrowseCrowdGate.UseVisualStyleBackColor = True
        '
        'chkAlarm
        '
        Me.chkAlarm.AutoSize = True
        Me.chkAlarm.Checked = True
        Me.chkAlarm.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAlarm.Location = New System.Drawing.Point(12, 119)
        Me.chkAlarm.Name = "chkAlarm"
        Me.chkAlarm.TabIndex = 6
        Me.chkAlarm.Text = "Alarm Directory:"
        Me.chkAlarm.UseVisualStyleBackColor = True
        '
        'txtAlarm
        '
        Me.txtAlarm.Location = New System.Drawing.Point(12, 140)
        Me.txtAlarm.Name = "txtAlarm"
        Me.txtAlarm.Size = New System.Drawing.Size(315, 23)
        Me.txtAlarm.TabIndex = 7
        '
        'btnBrowseAlarm
        '
        Me.btnBrowseAlarm.Location = New System.Drawing.Point(333, 139)
        Me.btnBrowseAlarm.Name = "btnBrowseAlarm"
        Me.btnBrowseAlarm.Size = New System.Drawing.Size(85, 25)
        Me.btnBrowseAlarm.TabIndex = 8
        Me.btnBrowseAlarm.Text = "Browse..."
        Me.btnBrowseAlarm.UseVisualStyleBackColor = True
        '
        'lblBackup
        '
        Me.lblBackup.AutoSize = True
        Me.lblBackup.Location = New System.Drawing.Point(12, 175)
        Me.lblBackup.Name = "lblBackup"
        Me.lblBackup.Text = "Backup storage folder:"
        '
        'txtBackup
        '
        Me.txtBackup.Location = New System.Drawing.Point(12, 193)
        Me.txtBackup.Name = "txtBackup"
        Me.txtBackup.Size = New System.Drawing.Size(315, 23)
        Me.txtBackup.TabIndex = 9
        '
        'btnBrowseBackup
        '
        Me.btnBrowseBackup.Location = New System.Drawing.Point(333, 192)
        Me.btnBrowseBackup.Name = "btnBrowseBackup"
        Me.btnBrowseBackup.Size = New System.Drawing.Size(85, 25)
        Me.btnBrowseBackup.TabIndex = 10
        Me.btnBrowseBackup.Text = "Browse..."
        Me.btnBrowseBackup.UseVisualStyleBackColor = True
        '
        'btnNewFolder
        '
        Me.btnNewFolder.Location = New System.Drawing.Point(273, 225)
        Me.btnNewFolder.Name = "btnNewFolder"
        Me.btnNewFolder.Size = New System.Drawing.Size(145, 27)
        Me.btnNewFolder.TabIndex = 11
        Me.btnNewFolder.Text = "Create New Folder..."
        Me.btnNewFolder.UseVisualStyleBackColor = True
        '
        'lblKeep
        '
        Me.lblKeep.AutoSize = True
        Me.lblKeep.ForeColor = System.Drawing.SystemColors.GrayText
        Me.lblKeep.Location = New System.Drawing.Point(12, 231)
        Me.lblKeep.Name = "lblKeep"
        Me.lblKeep.Text = "The 10 most recent backups of each directory are kept."
        '
        'grpManual
        '
        Me.grpManual.Controls.Add(Me.btnBackupNow)
        Me.grpManual.Controls.Add(Me.lblLastBackup)
        Me.grpManual.Location = New System.Drawing.Point(12, 284)
        Me.grpManual.Name = "grpManual"
        Me.grpManual.Size = New System.Drawing.Size(430, 80)
        Me.grpManual.TabIndex = 1
        Me.grpManual.TabStop = False
        Me.grpManual.Text = "1. Manual Backup"
        '
        'btnBackupNow
        '
        Me.btnBackupNow.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold)
        Me.btnBackupNow.Location = New System.Drawing.Point(12, 28)
        Me.btnBackupNow.Name = "btnBackupNow"
        Me.btnBackupNow.Size = New System.Drawing.Size(150, 36)
        Me.btnBackupNow.TabIndex = 0
        Me.btnBackupNow.Text = "Back Up Now"
        Me.btnBackupNow.UseVisualStyleBackColor = True
        '
        'lblLastBackup
        '
        Me.lblLastBackup.AutoSize = True
        Me.lblLastBackup.Location = New System.Drawing.Point(175, 38)
        Me.lblLastBackup.Name = "lblLastBackup"
        Me.lblLastBackup.Text = "Last backup: never"
        '
        'grpOnChange
        '
        Me.grpOnChange.Controls.Add(Me.chkAutoOnChange)
        Me.grpOnChange.Controls.Add(Me.lblQuiet)
        Me.grpOnChange.Controls.Add(Me.nudQuiet)
        Me.grpOnChange.Location = New System.Drawing.Point(12, 374)
        Me.grpOnChange.Name = "grpOnChange"
        Me.grpOnChange.Size = New System.Drawing.Size(430, 90)
        Me.grpOnChange.TabIndex = 2
        Me.grpOnChange.TabStop = False
        Me.grpOnChange.Text = "2. Automatic Backup When Files Change"
        '
        'chkAutoOnChange
        '
        Me.chkAutoOnChange.AutoSize = True
        Me.chkAutoOnChange.Location = New System.Drawing.Point(12, 25)
        Me.chkAutoOnChange.Name = "chkAutoOnChange"
        Me.chkAutoOnChange.TabIndex = 0
        Me.chkAutoOnChange.Text = "Back up automatically whenever the folder is modified"
        Me.chkAutoOnChange.UseVisualStyleBackColor = True
        '
        'lblQuiet
        '
        Me.lblQuiet.AutoSize = True
        Me.lblQuiet.Location = New System.Drawing.Point(30, 57)
        Me.lblQuiet.Name = "lblQuiet"
        Me.lblQuiet.Text = "Wait until there are no changes for (seconds):"
        '
        'nudQuiet
        '
        Me.nudQuiet.Location = New System.Drawing.Point(300, 54)
        Me.nudQuiet.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
        Me.nudQuiet.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudQuiet.Name = "nudQuiet"
        Me.nudQuiet.Size = New System.Drawing.Size(70, 23)
        Me.nudQuiet.TabIndex = 1
        Me.nudQuiet.Value = New Decimal(New Integer() {60, 0, 0, 0})
        '
        'grpSchedule
        '
        Me.grpSchedule.Controls.Add(Me.chkSchedule)
        Me.grpSchedule.Controls.Add(Me.rbWeekly)
        Me.grpSchedule.Controls.Add(Me.rbMonthly)
        Me.grpSchedule.Controls.Add(Me.lblDay)
        Me.grpSchedule.Controls.Add(Me.cboDayOfWeek)
        Me.grpSchedule.Controls.Add(Me.nudDayOfMonth)
        Me.grpSchedule.Controls.Add(Me.lblTime)
        Me.grpSchedule.Controls.Add(Me.dtpTime)
        Me.grpSchedule.Controls.Add(Me.lblNextRun)
        Me.grpSchedule.Location = New System.Drawing.Point(12, 474)
        Me.grpSchedule.Name = "grpSchedule"
        Me.grpSchedule.Size = New System.Drawing.Size(430, 150)
        Me.grpSchedule.TabIndex = 3
        Me.grpSchedule.TabStop = False
        Me.grpSchedule.Text = "3. Scheduled Automatic Backup"
        '
        'chkSchedule
        '
        Me.chkSchedule.AutoSize = True
        Me.chkSchedule.Location = New System.Drawing.Point(12, 25)
        Me.chkSchedule.Name = "chkSchedule"
        Me.chkSchedule.TabIndex = 0
        Me.chkSchedule.Text = "Enable scheduled backups"
        Me.chkSchedule.UseVisualStyleBackColor = True
        '
        'rbWeekly
        '
        Me.rbWeekly.AutoSize = True
        Me.rbWeekly.Checked = True
        Me.rbWeekly.Location = New System.Drawing.Point(30, 52)
        Me.rbWeekly.Name = "rbWeekly"
        Me.rbWeekly.TabIndex = 1
        Me.rbWeekly.TabStop = True
        Me.rbWeekly.Text = "Weekly"
        Me.rbWeekly.UseVisualStyleBackColor = True
        '
        'rbMonthly
        '
        Me.rbMonthly.AutoSize = True
        Me.rbMonthly.Location = New System.Drawing.Point(120, 52)
        Me.rbMonthly.Name = "rbMonthly"
        Me.rbMonthly.TabIndex = 2
        Me.rbMonthly.Text = "Monthly"
        Me.rbMonthly.UseVisualStyleBackColor = True
        '
        'lblDay
        '
        Me.lblDay.AutoSize = True
        Me.lblDay.Location = New System.Drawing.Point(30, 86)
        Me.lblDay.Name = "lblDay"
        Me.lblDay.Text = "Day:"
        '
        'cboDayOfWeek
        '
        Me.cboDayOfWeek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDayOfWeek.Location = New System.Drawing.Point(95, 82)
        Me.cboDayOfWeek.Name = "cboDayOfWeek"
        Me.cboDayOfWeek.Size = New System.Drawing.Size(125, 23)
        Me.cboDayOfWeek.TabIndex = 3
        '
        'nudDayOfMonth
        '
        Me.nudDayOfMonth.Location = New System.Drawing.Point(95, 82)
        Me.nudDayOfMonth.Maximum = New Decimal(New Integer() {31, 0, 0, 0})
        Me.nudDayOfMonth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudDayOfMonth.Name = "nudDayOfMonth"
        Me.nudDayOfMonth.Size = New System.Drawing.Size(60, 23)
        Me.nudDayOfMonth.TabIndex = 4
        Me.nudDayOfMonth.Value = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudDayOfMonth.Visible = False
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Location = New System.Drawing.Point(240, 86)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Text = "Time:"
        '
        'dtpTime
        '
        Me.dtpTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpTime.Location = New System.Drawing.Point(285, 82)
        Me.dtpTime.Name = "dtpTime"
        Me.dtpTime.ShowUpDown = True
        Me.dtpTime.Size = New System.Drawing.Size(120, 23)
        Me.dtpTime.TabIndex = 5
        '
        'lblNextRun
        '
        Me.lblNextRun.AutoSize = True
        Me.lblNextRun.ForeColor = System.Drawing.SystemColors.GrayText
        Me.lblNextRun.Location = New System.Drawing.Point(30, 120)
        Me.lblNextRun.Name = "lblNextRun"
        Me.lblNextRun.Text = "Scheduled backups are off."
        '
        'grpNextcloud
        '
        Me.grpNextcloud.Controls.Add(Me.chkNextcloud)
        Me.grpNextcloud.Controls.Add(Me.lblServer)
        Me.grpNextcloud.Controls.Add(Me.txtServer)
        Me.grpNextcloud.Controls.Add(Me.lblUser)
        Me.grpNextcloud.Controls.Add(Me.txtUser)
        Me.grpNextcloud.Controls.Add(Me.lblPassword)
        Me.grpNextcloud.Controls.Add(Me.txtPassword)
        Me.grpNextcloud.Controls.Add(Me.lblRemote)
        Me.grpNextcloud.Controls.Add(Me.lblRemoteBase)
        Me.grpNextcloud.Controls.Add(Me.txtRemote)
        Me.grpNextcloud.Controls.Add(Me.btnBrowseRemote)
        Me.grpNextcloud.Controls.Add(Me.chkAllowSelfSigned)
        Me.grpNextcloud.Controls.Add(Me.btnTestNextcloud)
        Me.grpNextcloud.Location = New System.Drawing.Point(455, 12)
        Me.grpNextcloud.Name = "grpNextcloud"
        Me.grpNextcloud.Size = New System.Drawing.Size(430, 260)
        Me.grpNextcloud.TabIndex = 4
        Me.grpNextcloud.TabStop = False
        Me.grpNextcloud.Text = "Nextcloud Upload"
        '
        'chkNextcloud
        '
        Me.chkNextcloud.AutoSize = True
        Me.chkNextcloud.Location = New System.Drawing.Point(12, 25)
        Me.chkNextcloud.Name = "chkNextcloud"
        Me.chkNextcloud.TabIndex = 0
        Me.chkNextcloud.Text = "Upload backups to my Nextcloud server"
        Me.chkNextcloud.UseVisualStyleBackColor = True
        '
        'lblServer
        '
        Me.lblServer.AutoSize = True
        Me.lblServer.Location = New System.Drawing.Point(12, 58)
        Me.lblServer.Name = "lblServer"
        Me.lblServer.Text = "Server URL:"
        '
        'txtServer
        '
        Me.txtServer.Location = New System.Drawing.Point(120, 55)
        Me.txtServer.Name = "txtServer"
        Me.txtServer.PlaceholderText = "https://cloud.example.lan"
        Me.txtServer.Size = New System.Drawing.Size(295, 23)
        Me.txtServer.TabIndex = 1
        Me.txtServer.UseSystemPasswordChar = True
        '
        'lblUser
        '
        Me.lblUser.AutoSize = True
        Me.lblUser.Location = New System.Drawing.Point(12, 91)
        Me.lblUser.Name = "lblUser"
        Me.lblUser.Text = "Username:"
        '
        'txtUser
        '
        Me.txtUser.Location = New System.Drawing.Point(120, 88)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(295, 23)
        Me.txtUser.TabIndex = 2
        Me.txtUser.UseSystemPasswordChar = True
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New System.Drawing.Point(12, 124)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Text = "App password:"
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(120, 121)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(295, 23)
        Me.txtPassword.TabIndex = 3
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'lblRemote
        '
        Me.lblRemote.AutoSize = True
        Me.lblRemote.Location = New System.Drawing.Point(12, 157)
        Me.lblRemote.Name = "lblRemote"
        Me.lblRemote.Text = "Remote folder:"
        '
        'txtRemote
        '
        Me.txtRemote.Location = New System.Drawing.Point(265, 154)
        Me.txtRemote.Name = "txtRemote"
        Me.txtRemote.PlaceholderText = "(subfolder)"
        Me.txtRemote.Size = New System.Drawing.Size(65, 23)
        Me.txtRemote.TabIndex = 4
        '
        'lblRemoteBase
        '
        Me.lblRemoteBase.AutoSize = True
        Me.lblRemoteBase.Location = New System.Drawing.Point(120, 157)
        Me.lblRemoteBase.Name = "lblRemoteBase"
        Me.lblRemoteBase.Text = "Work stuff/CCW/Backups /"
        '
        'btnBrowseRemote
        '
        Me.btnBrowseRemote.Location = New System.Drawing.Point(335, 153)
        Me.btnBrowseRemote.Name = "btnBrowseRemote"
        Me.btnBrowseRemote.Size = New System.Drawing.Size(80, 25)
        Me.btnBrowseRemote.TabIndex = 5
        Me.btnBrowseRemote.Text = "Browse..."
        Me.btnBrowseRemote.UseVisualStyleBackColor = True
        '
        'chkAllowSelfSigned
        '
        Me.chkAllowSelfSigned.AutoSize = True
        Me.chkAllowSelfSigned.Location = New System.Drawing.Point(12, 188)
        Me.chkAllowSelfSigned.Name = "chkAllowSelfSigned"
        Me.chkAllowSelfSigned.TabIndex = 6
        Me.chkAllowSelfSigned.Text = "Allow self-signed HTTPS certificate (home server)"
        Me.chkAllowSelfSigned.UseVisualStyleBackColor = True
        '
        'btnTestNextcloud
        '
        Me.btnTestNextcloud.Location = New System.Drawing.Point(12, 218)
        Me.btnTestNextcloud.Name = "btnTestNextcloud"
        Me.btnTestNextcloud.Size = New System.Drawing.Size(140, 30)
        Me.btnTestNextcloud.TabIndex = 7
        Me.btnTestNextcloud.Text = "Test Connection"
        Me.btnTestNextcloud.UseVisualStyleBackColor = True
        '
        'grpOptions
        '
        Me.grpOptions.Controls.Add(Me.chkMinimizeToTray)
        Me.grpOptions.Controls.Add(Me.chkStartWithWindows)
        Me.grpOptions.Location = New System.Drawing.Point(455, 282)
        Me.grpOptions.Name = "grpOptions"
        Me.grpOptions.Size = New System.Drawing.Size(430, 85)
        Me.grpOptions.TabIndex = 5
        Me.grpOptions.TabStop = False
        Me.grpOptions.Text = "Options"
        '
        'chkMinimizeToTray
        '
        Me.chkMinimizeToTray.AutoSize = True
        Me.chkMinimizeToTray.Location = New System.Drawing.Point(12, 25)
        Me.chkMinimizeToTray.Name = "chkMinimizeToTray"
        Me.chkMinimizeToTray.TabIndex = 0
        Me.chkMinimizeToTray.Text = "Keep running in the system tray when closed"
        Me.chkMinimizeToTray.UseVisualStyleBackColor = True
        '
        'chkStartWithWindows
        '
        Me.chkStartWithWindows.AutoSize = True
        Me.chkStartWithWindows.Location = New System.Drawing.Point(12, 52)
        Me.chkStartWithWindows.Name = "chkStartWithWindows"
        Me.chkStartWithWindows.TabIndex = 1
        Me.chkStartWithWindows.Text = "Start with Windows (in the tray)"
        Me.chkStartWithWindows.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold)
        Me.btnSave.Location = New System.Drawing.Point(455, 377)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(200, 40)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Save Settings"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'progressBar
        '
        Me.progressBar.Location = New System.Drawing.Point(455, 440)
        Me.progressBar.MarqueeAnimationSpeed = 30
        Me.progressBar.Name = "progressBar"
        Me.progressBar.Size = New System.Drawing.Size(430, 18)
        Me.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.progressBar.Visible = False
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(455, 468)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Text = "Idle"
        '
        'lblLog
        '
        Me.lblLog.AutoSize = True
        Me.lblLog.Location = New System.Drawing.Point(12, 634)
        Me.lblLog.Name = "lblLog"
        Me.lblLog.Text = "Activity log:"
        '
        'txtLog
        '
        Me.txtLog.BackColor = System.Drawing.SystemColors.Window
        Me.txtLog.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtLog.Location = New System.Drawing.Point(12, 654)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ReadOnly = True
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(873, 200)
        Me.txtLog.TabIndex = 7
        '
        'notifyIcon
        '
        Me.notifyIcon.ContextMenuStrip = Me.trayMenu
        Me.notifyIcon.Text = "Auto Backup Scheduler"
        Me.notifyIcon.Visible = True
        '
        'trayMenu
        '
        Me.trayMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuOpen, Me.mnuBackupNow, Me.mnuSeparator, Me.mnuExit})
        Me.trayMenu.Name = "trayMenu"
        '
        'mnuOpen
        '
        Me.mnuOpen.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.mnuOpen.Name = "mnuOpen"
        Me.mnuOpen.Text = "Open"
        '
        'mnuBackupNow
        '
        Me.mnuBackupNow.Name = "mnuBackupNow"
        Me.mnuBackupNow.Text = "Back Up Now"
        '
        'mnuSeparator
        '
        Me.mnuSeparator.Name = "mnuSeparator"
        '
        'mnuExit
        '
        Me.mnuExit.Name = "mnuExit"
        Me.mnuExit.Text = "Exit"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(897, 867)
        Me.Controls.Add(Me.grpFolders)
        Me.Controls.Add(Me.grpManual)
        Me.Controls.Add(Me.grpOnChange)
        Me.Controls.Add(Me.grpSchedule)
        Me.Controls.Add(Me.grpNextcloud)
        Me.Controls.Add(Me.grpOptions)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.progressBar)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblLog)
        Me.Controls.Add(Me.txtLog)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Auto Backup Scheduler v2"
        Me.grpFolders.ResumeLayout(False)
        Me.grpFolders.PerformLayout()
        Me.grpManual.ResumeLayout(False)
        Me.grpManual.PerformLayout()
        Me.grpOnChange.ResumeLayout(False)
        Me.grpOnChange.PerformLayout()
        CType(Me.nudQuiet, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpSchedule.ResumeLayout(False)
        Me.grpSchedule.PerformLayout()
        CType(Me.nudDayOfMonth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpNextcloud.ResumeLayout(False)
        Me.grpNextcloud.PerformLayout()
        Me.grpOptions.ResumeLayout(False)
        Me.grpOptions.PerformLayout()
        Me.trayMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents grpFolders As System.Windows.Forms.GroupBox
    Friend WithEvents chkMasterControl As System.Windows.Forms.CheckBox
    Friend WithEvents txtMasterControl As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseMasterControl As System.Windows.Forms.Button
    Friend WithEvents chkCrowdGate As System.Windows.Forms.CheckBox
    Friend WithEvents txtCrowdGate As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseCrowdGate As System.Windows.Forms.Button
    Friend WithEvents chkAlarm As System.Windows.Forms.CheckBox
    Friend WithEvents txtAlarm As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseAlarm As System.Windows.Forms.Button
    Friend WithEvents lblBackup As System.Windows.Forms.Label
    Friend WithEvents txtBackup As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseBackup As System.Windows.Forms.Button
    Friend WithEvents btnNewFolder As System.Windows.Forms.Button
    Friend WithEvents lblKeep As System.Windows.Forms.Label
    Friend WithEvents grpManual As System.Windows.Forms.GroupBox
    Friend WithEvents btnBackupNow As System.Windows.Forms.Button
    Friend WithEvents lblLastBackup As System.Windows.Forms.Label
    Friend WithEvents grpOnChange As System.Windows.Forms.GroupBox
    Friend WithEvents chkAutoOnChange As System.Windows.Forms.CheckBox
    Friend WithEvents lblQuiet As System.Windows.Forms.Label
    Friend WithEvents nudQuiet As System.Windows.Forms.NumericUpDown
    Friend WithEvents grpSchedule As System.Windows.Forms.GroupBox
    Friend WithEvents chkSchedule As System.Windows.Forms.CheckBox
    Friend WithEvents rbWeekly As System.Windows.Forms.RadioButton
    Friend WithEvents rbMonthly As System.Windows.Forms.RadioButton
    Friend WithEvents lblDay As System.Windows.Forms.Label
    Friend WithEvents cboDayOfWeek As System.Windows.Forms.ComboBox
    Friend WithEvents nudDayOfMonth As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents dtpTime As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblNextRun As System.Windows.Forms.Label
    Friend WithEvents grpNextcloud As System.Windows.Forms.GroupBox
    Friend WithEvents chkNextcloud As System.Windows.Forms.CheckBox
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents txtServer As System.Windows.Forms.TextBox
    Friend WithEvents lblUser As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents lblRemote As System.Windows.Forms.Label
    Friend WithEvents txtRemote As System.Windows.Forms.TextBox
    Friend WithEvents lblRemoteBase As System.Windows.Forms.Label
    Friend WithEvents btnBrowseRemote As System.Windows.Forms.Button
    Friend WithEvents chkAllowSelfSigned As System.Windows.Forms.CheckBox
    Friend WithEvents btnTestNextcloud As System.Windows.Forms.Button
    Friend WithEvents grpOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkMinimizeToTray As System.Windows.Forms.CheckBox
    Friend WithEvents chkStartWithWindows As System.Windows.Forms.CheckBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblLog As System.Windows.Forms.Label
    Friend WithEvents txtLog As System.Windows.Forms.TextBox
    Friend WithEvents notifyIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents trayMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuOpen As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackupNow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem

End Class
