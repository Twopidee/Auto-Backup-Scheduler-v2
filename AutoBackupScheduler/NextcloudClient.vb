Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Xml.Linq

''' <summary>
''' Minimal Nextcloud WebDAV client: create folders, list, upload and delete files.
''' Files live under https://server/remote.php/dav/files/USERNAME/...
''' </summary>
Public Class NextcloudClient
    Implements IDisposable

    Private Shared ReadOnly DavNs As XNamespace = "DAV:"
    Private ReadOnly _http As HttpClient
    Private ReadOnly _filesRoot As Uri

    Public Sub New(serverUrl As String, userName As String, password As String, allowInvalidCertificate As Boolean)
        If String.IsNullOrWhiteSpace(serverUrl) Then Throw New ArgumentException("The Nextcloud server URL is empty.")
        If String.IsNullOrWhiteSpace(userName) Then Throw New ArgumentException("The Nextcloud username is empty.")

        Dim root = NormalizeServerRoot(serverUrl)
        _filesRoot = New Uri($"{root}/remote.php/dav/files/{Uri.EscapeDataString(userName.Trim())}/")

        Dim handler As New HttpClientHandler()
        If allowInvalidCertificate Then
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        End If
        _http = New HttpClient(handler) With {.Timeout = TimeSpan.FromHours(12)}
        Dim token = Convert.ToBase64String(Encoding.UTF8.GetBytes(userName.Trim() & ":" & password))
        _http.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", token)
    End Sub

    ''' <summary>Turns the address the user typed into "https://server[/path]" without a trailing slash.</summary>
    Public Shared Function NormalizeServerRoot(serverUrl As String) As String
        Dim root = If(serverUrl, "").Trim().TrimEnd("/"c)
        If Not root.StartsWith("http://", StringComparison.OrdinalIgnoreCase) AndAlso
           Not root.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
            root = "https://" & root
        End If
        ' Accept a full WebDAV URL copied from Nextcloud as well as the plain server address.
        Dim davIndex = root.IndexOf("/remote.php", StringComparison.OrdinalIgnoreCase)
        If davIndex >= 0 Then root = root.Substring(0, davIndex)
        Return root
    End Function

    Public Async Function TestConnectionAsync() As Task
        Await PropFindAsync(_filesRoot, 0)
    End Function

    ''' <summary>Creates the remote folder (and any parents) if it doesn't exist.</summary>
    Public Async Function EnsureFolderAsync(remoteFolder As String) As Task
        Dim built = ""
        For Each part In SplitPath(remoteFolder)
            built = If(built = "", part, built & "/" & part)
            Using request As New HttpRequestMessage(New HttpMethod("MKCOL"), FolderUri(built))
                Using response = Await _http.SendAsync(request)
                    ' 405 Method Not Allowed means the folder already exists.
                    If response.StatusCode <> HttpStatusCode.MethodNotAllowed Then
                        EnsureSuccess(response, $"create the folder '{built}'")
                    End If
                End Using
            End Using
        Next
    End Function

    ''' <summary>Names of the files (not sub-folders) directly inside the remote folder.</summary>
    Public Async Function ListFilesAsync(remoteFolder As String) As Task(Of List(Of String))
        Return (Await ListEntriesAsync(remoteFolder)).Where(Function(e) Not e.IsFolder).Select(Function(e) e.Name).ToList()
    End Function

    ''' <summary>Names of the sub-folders directly inside the remote folder.</summary>
    Public Async Function ListFoldersAsync(remoteFolder As String) As Task(Of List(Of String))
        Return (Await ListEntriesAsync(remoteFolder)).Where(Function(e) e.IsFolder).Select(Function(e) e.Name).ToList()
    End Function

    Private Async Function ListEntriesAsync(remoteFolder As String) As Task(Of List(Of (Name As String, IsFolder As Boolean)))
        Dim target = FolderUri(remoteFolder)
        Dim doc = Await PropFindAsync(target, 1)
        Dim selfPath = Uri.UnescapeDataString(target.AbsolutePath).TrimEnd("/"c)
        Dim entries As New List(Of (Name As String, IsFolder As Boolean))
        For Each item In doc.Descendants(DavNs.GetName("response"))
            Dim href = item.Element(DavNs.GetName("href"))?.Value
            If String.IsNullOrEmpty(href) Then Continue For
            If Uri.IsWellFormedUriString(href, UriKind.Absolute) Then href = New Uri(href).AbsolutePath
            Dim hrefPath = Uri.UnescapeDataString(href).TrimEnd("/"c)
            ' The listing includes the folder itself; skip it.
            If hrefPath.Equals(selfPath, StringComparison.OrdinalIgnoreCase) Then Continue For
            entries.Add((hrefPath.Split("/"c).Last(), item.Descendants(DavNs.GetName("collection")).Any()))
        Next
        Return entries
    End Function

    Public Async Function UploadFileAsync(localFile As String, remoteFolder As String) As Task
        Dim name = Path.GetFileName(localFile)
        Using stream As New FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync:=True)
            Using content As New StreamContent(stream, 81920)
                content.Headers.ContentType = New MediaTypeHeaderValue("application/zip")
                content.Headers.ContentLength = stream.Length
                Using response = Await _http.PutAsync(FileUri(remoteFolder, name), content)
                    EnsureSuccess(response, $"upload {name}")
                End Using
            End Using
        End Using
    End Function

    Public Async Function DeleteFileAsync(remoteFolder As String, fileName As String) As Task
        Using response = Await _http.DeleteAsync(FileUri(remoteFolder, fileName))
            If response.StatusCode <> HttpStatusCode.NotFound Then EnsureSuccess(response, $"delete {fileName}")
        End Using
    End Function

    Private Async Function PropFindAsync(target As Uri, depth As Integer) As Task(Of XDocument)
        Const body As String = "<?xml version=""1.0""?><d:propfind xmlns:d=""DAV:""><d:prop><d:resourcetype/></d:prop></d:propfind>"
        Using request As New HttpRequestMessage(New HttpMethod("PROPFIND"), target)
            request.Headers.Add("Depth", depth.ToString())
            request.Content = New StringContent(body, Encoding.UTF8, "application/xml")
            Using response = Await _http.SendAsync(request)
                EnsureSuccess(response, "read the folder")
                Return XDocument.Parse(Await response.Content.ReadAsStringAsync())
            End Using
        End Using
    End Function

    Private Function FolderUri(remoteFolder As String) As Uri
        Dim parts = SplitPath(remoteFolder)
        If parts.Length = 0 Then Return _filesRoot
        Return New Uri(_filesRoot, String.Join("/", parts.Select(Function(p) Uri.EscapeDataString(p))) & "/")
    End Function

    Private Function FileUri(remoteFolder As String, fileName As String) As Uri
        Return New Uri(FolderUri(remoteFolder), Uri.EscapeDataString(fileName))
    End Function

    Private Shared Function SplitPath(remoteFolder As String) As String()
        Return If(remoteFolder, "").Split({"/"c, "\"c}, StringSplitOptions.RemoveEmptyEntries)
    End Function

    Private Shared Sub EnsureSuccess(response As HttpResponseMessage, action As String)
        If response.IsSuccessStatusCode Then Return
        Dim hint = ""
        Select Case response.StatusCode
            Case HttpStatusCode.Unauthorized
                hint = " Check the username and app password."
            Case HttpStatusCode.NotFound
                hint = " Check the server URL and username."
            Case HttpStatusCode.RequestEntityTooLarge
                hint = " The file is larger than your server or proxy allows (check upload size limits)."
            Case HttpStatusCode.InsufficientStorage
                hint = " The Nextcloud account is out of storage space."
        End Select
        Throw New HttpRequestException($"Nextcloud could not {action}: {CInt(response.StatusCode)} {response.ReasonPhrase}.{hint}")
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _http.Dispose()
    End Sub

End Class
