Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Browses the folders inside "Work stuff" on the Nextcloud server and lets you create new ones there.
''' Sub-folders are loaded from the server when a folder is expanded.
''' </summary>
Public Class RemoteFolderDialog
    Inherits Form

    Private Const PlaceholderText As String = "Loading..."

    Private ReadOnly _client As NextcloudClient
    Private ReadOnly _describeError As Func(Of Exception, String)
    Private ReadOnly _initialFolder As String
    Private ReadOnly _loaded As New HashSet(Of TreeNode)
    Private ReadOnly _tree As New TreeView()
    Private ReadOnly _btnNewFolder As New Button()
    Private ReadOnly _btnOk As New Button()
    Private ReadOnly _btnCancel As New Button()

    ''' <summary>The chosen folder, relative to "Work stuff" ("" means Work stuff itself).</summary>
    Public ReadOnly Property SelectedFolder As String
        Get
            Return If(TryCast(_tree.SelectedNode?.Tag, String), "")
        End Get
    End Property

    Public Sub New(client As NextcloudClient, initialFolder As String, describeError As Func(Of Exception, String))
        _client = client
        _describeError = describeError
        _initialFolder = AppSettings.NormalizeRemoteFolder(initialFolder)

        AutoScaleDimensions = New SizeF(96.0!, 96.0!)
        AutoScaleMode = AutoScaleMode.Dpi
        Font = New Font("Segoe UI", 9.0!)
        Text = "Choose Nextcloud Folder"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(400, 430)

        _tree.SetBounds(12, 12, 376, 365)
        _tree.HideSelection = False
        _tree.Sorted = True
        _btnNewFolder.SetBounds(12, 390, 120, 28)
        _btnNewFolder.Text = "New Folder..."
        _btnOk.SetBounds(212, 390, 85, 28)
        _btnOk.Text = "OK"
        _btnOk.DialogResult = DialogResult.OK
        _btnCancel.SetBounds(303, 390, 85, 28)
        _btnCancel.Text = "Cancel"
        _btnCancel.DialogResult = DialogResult.Cancel
        AcceptButton = _btnOk
        CancelButton = _btnCancel
        Controls.AddRange(New Control() {_tree, _btnNewFolder, _btnOk, _btnCancel})

        AddHandler _tree.BeforeExpand, AddressOf Tree_BeforeExpand
        AddHandler _btnNewFolder.Click, AddressOf BtnNewFolder_Click
        SetBusy(True)
    End Sub

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Await _client.EnsureFolderAsync(AppSettings.NextcloudBaseFolder)
            Dim root = MakeNode(AppSettings.NextcloudBaseFolder, "")
            _tree.Nodes.Add(root)
            Await LoadChildrenAsync(root)
            _tree.SelectedNode = root

            ' Open the tree at the folder that is currently configured.
            Dim current = root
            For Each part In If(_initialFolder = "", Array.Empty(Of String)(), _initialFolder.Split("/"c))
                Dim child = FindChild(current, part)
                If child Is Nothing Then Exit For
                current.Expand()
                current = child
                Await LoadChildrenAsync(current)
            Next
            root.Expand()
            _tree.SelectedNode = current
            _tree.Focus()
        Catch ex As Exception
            If IsDisposed Then Return
            ShowError("Could not read the folders on Nextcloud", ex)
            DialogResult = DialogResult.Cancel
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub Tree_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs)
        If _loaded.Contains(e.Node) Then Return
        Try
            SetBusy(True)
            Await LoadChildrenAsync(e.Node)
        Catch ex As Exception
            ShowError("Could not read the folders on Nextcloud", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub BtnNewFolder_Click(sender As Object, e As EventArgs)
        Dim parent = If(_tree.SelectedNode, If(_tree.Nodes.Count > 0, _tree.Nodes(0), Nothing))
        If parent Is Nothing Then Return

        Dim parentPath = AppSettings.FullRemoteFolder(CStr(parent.Tag))
        Dim folderName = InputBox($"Name for the new folder inside ""{parentPath}"":", "New Nextcloud Folder", "").Trim()
        If folderName = "" Then Return
        If folderName = "." OrElse folderName = ".." OrElse folderName.IndexOfAny({"/"c, "\"c}) >= 0 Then
            MessageBox.Show(Me, "Folder names can't contain / or \ and can't be ""."" or ""..""",
                            Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            SetBusy(True)
            Await LoadChildrenAsync(parent)
            Dim node = FindChild(parent, folderName)
            If node Is Nothing Then
                Dim relativePath = If(CStr(parent.Tag) = "", folderName, CStr(parent.Tag) & "/" & folderName)
                Await _client.EnsureFolderAsync(AppSettings.FullRemoteFolder(relativePath))
                node = MakeNode(folderName, relativePath)
                node.Nodes.Clear()   ' A brand-new folder is empty.
                _loaded.Add(node)
                parent.Nodes.Add(node)
            End If
            parent.Expand()
            _tree.SelectedNode = node
        Catch ex As Exception
            ShowError("Could not create the folder", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Function LoadChildrenAsync(node As TreeNode) As Task
        If Not _loaded.Add(node) Then Return
        Try
            Dim names = Await _client.ListFoldersAsync(AppSettings.FullRemoteFolder(CStr(node.Tag)))
            node.Nodes.Clear()
            For Each folderName In names
                Dim relativePath = If(CStr(node.Tag) = "", folderName, CStr(node.Tag) & "/" & folderName)
                node.Nodes.Add(MakeNode(folderName, relativePath))
            Next
        Catch ex As Exception
            _loaded.Remove(node)   ' Try again next time it is expanded.
            Throw
        End Try
    End Function

    ''' <summary>A folder node whose sub-folders haven't been loaded yet (it shows an expand arrow until they are).</summary>
    Private Shared Function MakeNode(name As String, relativePath As String) As TreeNode
        Dim node As New TreeNode(name) With {.Tag = relativePath}
        node.Nodes.Add(New TreeNode(PlaceholderText))
        Return node
    End Function

    Private Shared Function FindChild(parent As TreeNode, folderName As String) As TreeNode
        Return parent.Nodes.Cast(Of TreeNode)().FirstOrDefault(
            Function(n) n.Tag IsNot Nothing AndAlso n.Text.Equals(folderName, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Sub SetBusy(busy As Boolean)
        If IsDisposed Then Return
        UseWaitCursor = busy
        _btnNewFolder.Enabled = Not busy
        _btnOk.Enabled = Not busy
    End Sub

    Private Sub ShowError(action As String, ex As Exception)
        If IsDisposed Then Return
        MessageBox.Show(Me, action & ":" & Environment.NewLine & Environment.NewLine & _describeError(ex),
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Class
