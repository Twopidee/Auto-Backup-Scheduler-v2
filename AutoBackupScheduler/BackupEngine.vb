Imports System.IO
Imports System.IO.Compression
Imports System.Text.RegularExpressions

Public Class BackupResult
    Public Property ZipPath As String
    Public Property FilesAdded As Integer
    Public Property SkippedItems As New List(Of String)
    Public Property DeletedBackups As New List(Of String)
End Class

''' <summary>
''' Creates timestamped zip backups named "Backup_{ComputerName}_{SourceName}_{yyyy-MM-dd_HH-mm-ss}.zip"
''' (e.g. "Backup_PC1_MasterControl_...") and keeps only the newest ones per source. The computer name keeps
''' backups from different locations apart, so one computer's cleanup never deletes another computer's
''' backups in a shared Nextcloud folder.
''' </summary>
Public Module BackupEngine

    Private Const TimestampFormat As String = "yyyy-MM-dd_HH-mm-ss"

    ''' <param name="sourceName">One of <see cref="AppSettings.SourceNames"/>, e.g. "Master Control".</param>
    Public Function BackupBaseName(sourceName As String) As String
        Return SafeName(Environment.MachineName, "PC") & "_" & SafeName(sourceName.Replace(" ", ""), "Folder")
    End Function

    Private Function SafeName(name As String, fallback As String) As String
        For Each c In Path.GetInvalidFileNameChars()
            name = name.Replace(c, "_"c)
        Next
        Return If(name = "", fallback, name)
    End Function

    Public Function IsBackupFileName(fileName As String, baseName As String) As Boolean
        Dim pattern = "^Backup_" & Regex.Escape(baseName) & "_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\.zip$"
        Return Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase)
    End Function

    Public Function IsSameOrInside(target As String, folder As String) As Boolean
        Return NormalizeDir(target).StartsWith(NormalizeDir(folder), StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function NormalizeDir(folder As String) As String
        Return Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder)) & Path.DirectorySeparatorChar
    End Function

    ''' <summary>Local backups for this source folder, newest first.</summary>
    Public Function ListBackups(backupFolder As String, baseName As String) As List(Of String)
        If Not Directory.Exists(backupFolder) Then Return New List(Of String)
        Return Directory.GetFiles(backupFolder, "*.zip").
            Where(Function(f) IsBackupFileName(Path.GetFileName(f), baseName)).
            OrderByDescending(Function(f) Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).
            ToList()
    End Function

    Public Function CreateBackup(sourceFolder As String, baseName As String, backupFolder As String, keep As Integer) As BackupResult
        If String.IsNullOrWhiteSpace(sourceFolder) OrElse Not Directory.Exists(sourceFolder) Then
            Throw New DirectoryNotFoundException($"The folder to back up was not found: {sourceFolder}")
        End If
        If String.IsNullOrWhiteSpace(backupFolder) Then
            Throw New InvalidOperationException("No backup storage folder has been selected.")
        End If
        If NormalizeDir(sourceFolder).Equals(NormalizeDir(backupFolder), StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("The backup storage folder cannot be the same as the folder being backed up.")
        End If

        Directory.CreateDirectory(backupFolder)
        Dim stamp = DateTime.Now
        Dim zipPath = Path.Combine(backupFolder, $"Backup_{baseName}_{stamp.ToString(TimestampFormat)}.zip")
        While File.Exists(zipPath)
            stamp = stamp.AddSeconds(1)
            zipPath = Path.Combine(backupFolder, $"Backup_{baseName}_{stamp.ToString(TimestampFormat)}.zip")
        End While

        ' If the backup folder lives inside the source folder, don't back up the backups.
        Dim excluded = If(IsSameOrInside(backupFolder, sourceFolder), backupFolder, Nothing)
        Dim result As New BackupResult With {.ZipPath = zipPath}

        ' Write to a temporary name first so a half-written zip is never mistaken for a real backup.
        Dim tempPath = zipPath & ".partial"
        Try
            Using stream As New FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)
                Using zip As New ZipArchive(stream, ZipArchiveMode.Create)
                    AddFolder(zip, sourceFolder, excluded, result)
                End Using
            End Using
            File.Move(tempPath, zipPath)
        Catch
            TryDelete(tempPath)
            Throw
        End Try

        result.DeletedBackups = ApplyRetention(backupFolder, baseName, keep)
        Return result
    End Function

    Private Sub AddFolder(zip As ZipArchive, root As String, excluded As String, result As BackupResult)
        Dim pending As New Stack(Of String)
        pending.Push(root)

        While pending.Count > 0
            Dim current = pending.Pop()
            Dim files As String() = Nothing
            Dim subDirs As String() = Nothing
            Try
                files = Directory.GetFiles(current)
                subDirs = Directory.GetDirectories(current)
            Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
                result.SkippedItems.Add($"{current} ({ex.Message})")
                Continue While
            End Try

            ' Keep empty folders so a restore recreates the same structure.
            If files.Length = 0 AndAlso subDirs.Length = 0 AndAlso current <> root Then
                zip.CreateEntry(Path.GetRelativePath(root, current).Replace("\"c, "/"c) & "/")
            End If

            For Each filePath In files
                Try
                    ' FileShare.ReadWrite lets us copy files other programs have open.
                    Using input As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
                        Dim zipEntry = zip.CreateEntry(Path.GetRelativePath(root, filePath).Replace("\"c, "/"c), CompressionLevel.Optimal)
                        Dim modified = File.GetLastWriteTime(filePath)
                        If modified.Year >= 1980 AndAlso modified.Year <= 2107 Then zipEntry.LastWriteTime = modified
                        Using output = zipEntry.Open()
                            input.CopyTo(output)
                        End Using
                    End Using
                    result.FilesAdded += 1
                Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
                    result.SkippedItems.Add($"{filePath} ({ex.Message})")
                End Try
            Next

            For Each subDir In subDirs
                If excluded IsNot Nothing AndAlso IsSameOrInside(subDir, excluded) Then Continue For
                Try
                    ' Skip junctions/symlinks to avoid loops and backing up things outside the folder.
                    If (File.GetAttributes(subDir) And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then Continue For
                Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
                    result.SkippedItems.Add($"{subDir} ({ex.Message})")
                    Continue For
                End Try
                pending.Push(subDir)
            Next
        End While
    End Sub

    ''' <summary>Deletes the oldest backups so only the newest <paramref name="keep"/> remain.</summary>
    Public Function ApplyRetention(backupFolder As String, baseName As String, keep As Integer) As List(Of String)
        Dim deleted As New List(Of String)
        For Each old In ListBackups(backupFolder, baseName).Skip(keep)
            Try
                File.Delete(old)
                deleted.Add(Path.GetFileName(old))
            Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
            End Try
        Next

        ' Leftovers from a backup that was interrupted (power loss, app killed, ...).
        For Each leftover In Directory.GetFiles(backupFolder, $"Backup_{baseName}_*.zip.partial")
            TryDelete(leftover)
        Next
        Return deleted
    End Function

    Private Sub TryDelete(filePath As String)
        Try
            If File.Exists(filePath) Then File.Delete(filePath)
        Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is IOException
        End Try
    End Sub

End Module
