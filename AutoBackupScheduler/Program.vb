Imports System.Threading
Imports System.Windows.Forms

Module Program

    <STAThread>
    Sub Main(args As String())
        ' Only one copy may run, otherwise two watchers/schedulers would make duplicate backups.
        Dim createdNew As Boolean
        Using mutex As New Mutex(True, "AutoBackupSchedulerV2_SingleInstance_8F41D2B6", createdNew)
            If Not createdNew Then
                MessageBox.Show("Auto Backup Scheduler is already running. Look for its icon in the system tray.",
                                "Auto Backup Scheduler", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Application.SetHighDpiMode(HighDpiMode.SystemAware)
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            Dim startHidden = args.Any(Function(a) a.Equals("/minimized", StringComparison.OrdinalIgnoreCase))
            Application.Run(New MainForm(startHidden))
        End Using
    End Sub

End Module
