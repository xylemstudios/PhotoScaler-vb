#Region " Ideas "
' Planned stuff:
' Add entire directories of pictures
' Crop
' New scale modes (2+1 new since 1.3)
' Custom scale modes
' Pause/Restart long scales
' Detect 100% scale, if it is 100% then only apply other stuff
' Watermark (advanced)
' Thumbnails
' Ask before overwrite
' Keep ORIGINAL date/time attributes
' Megapixels indicator (no set)
' Suffix/Prefix
' Size (w*h) presets
' Advanced image effects (sharpen, etc)
' Show size (KB, MB, etc)
#End Region
Public Class Form1
    Public frmList As New List(Of frmScale)
    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If frmList.Count > 0 Then
            e.Cancel = True
            For Each frm As frmScale In frmList
                If frm.BackgroundWorker1.IsBusy Then
                    If frm.Visible Then
                        GoTo isBusy
                    End If
                End If
            Next
            End
isBusy:
            If MsgBox("Do you want to cancel the current operation(s)?", MsgBoxStyle.YesNoCancel + MsgBoxStyle.Exclamation, "Warning") = MsgBoxResult.Yes Then
                For Each frm As frmScale In frmList
                    If frm.BackgroundWorker1.IsBusy Then
                        frm.BackgroundWorker1.CancelAsync()
                        Me.Enabled = False
                    End If
                Next
                End
            End If
        End If
    End Sub
    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        Label5.Text = If(RadioButton1.Checked, "%", "px")
        Label7.Text = If(RadioButton1.Checked, "%", "px")
    End Sub
    Public Sub refreshPanel()
        Label13.Text = ListBox1.Items.Count & If(ListBox1.Items.Count = 1, " image", " images")
    End Sub
    Private Sub ListBox1_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.DragLeave
        refreshPanel()
    End Sub
    Private Sub Listbox1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles ListBox1.DragDrop
        Dim filestr() As String
        Dim str As String
        Dim _error As New List(Of String)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            filestr = e.Data.GetData(DataFormats.FileDrop)
            For Each str In filestr
                If str.ToString.ToLower.EndsWith(".gif") Or _
                    str.ToString.ToLower.EndsWith(".png") Or _
                    str.ToString.ToLower.EndsWith(".jpeg") Or
                    str.ToString.ToLower.EndsWith(".jpg") Or
                    str.ToString.ToLower.EndsWith(".bmp") Or
                    str.ToString.ToLower.EndsWith(".emf") Or
                    str.ToString.ToLower.EndsWith(".exif") Or
                    str.ToString.ToLower.EndsWith(".tiff") Or
                    str.ToString.ToLower.EndsWith(".wmf") Then
                    If My.Computer.FileSystem.FileExists(str) Then
                        If Not ListBox1.Items.Contains(str) Then
                            ListBox1.Items.Add(str)
                        End If
                    Else
                        _error.Add(str)
                    End If
                Else
                    _error.Add(str)
                End If
                Label13.Text = ListBox1.Items.Count & If(ListBox1.Items.Count = 1, " image", " images")
                Application.DoEvents()
            Next
            If _error.Count > 0 Then MsgBox(_error.Count & " file(s) could not be added.", MsgBoxStyle.Critical, "Error")
            refreshPanel()
        End If
    End Sub
    Private Sub Listbox1_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles ListBox1.DragEnter
        Dim filestr() As String
        Dim str As String
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            filestr = e.Data.GetData(DataFormats.FileDrop)
            For Each str In filestr
                If FileIO.FileSystem.FileExists(str) Or FileIO.FileSystem.DirectoryExists(str) Then
                    e.Effect = e.AllowedEffect
                Else
                    e.Effect = DragDropEffects.None
                    Exit For
                End If
                Application.DoEvents()
            Next
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click
        Process.Start("http://xylemstudios.com/")
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
LoopBegin:
        For Each i In ListBox1.SelectedIndices
            ListBox1.Items.RemoveAt(i)
            GoTo LoopBegin
        Next
        refreshPanel()
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ListBox1.Items.Clear()
        refreshPanel()
    End Sub
    Private Sub Button4_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim lst As New List(Of String)
        Dim tp As Integer = ListBox1.TopIndex
        For Each i In ListBox1.SelectedIndices
            lst.Add(i)
        Next
        ListBox1.ClearSelected()
        For i = 0 To ListBox1.Items.Count - 1
            ListBox1.SetSelected(i, Not lst.Contains(i))
        Next
        ListBox1.TopIndex = tp
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Button2.Enabled = Not (ListBox1.SelectedItems.Count = 0)
        Button3.Enabled = Not (ListBox1.Items.Count = 0)
        Button2.Text = IIf(ListBox1.SelectedItems.Count > 1, "Remove selected items", "Remove selected item")
        Button1.Enabled = Not (ListBox1.Items.Count = 0) And Not ComboBox1.SelectedItem = ""
        ComboBox3.Enabled = CheckBox4.Checked
        ComboBox4.Enabled = CheckBox4.Checked
        Button4.Enabled = Button3.Enabled
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Not ComboBox1.SelectedItem = "" Then
            Dim error_ As Boolean
            For Each frms As frmScale In frmList
                If frms.Visible Then
                    For Each item As String In ListBox1.Items
                        For Each i As String In frms.list
                            If item = i Then
                                error_ = True
                                Exit For
                            End If
                        Next
                        If error_ Then Exit For
                    Next
                    If error_ Then Exit For
                End If
            Next
            If error_ Then
                MsgBox("There is already an operation in progress with some or all of these images." & vbNewLine & "Please try again after the process if finished.", MsgBoxStyle.Critical, "Error")
                Exit Sub
            End If
            ''no error
            Dim ofd As New FolderBrowserDialog
            ofd.ShowNewFolderButton = True
            ofd.Description = "Please select a folder where you would like to save your converted images. A new subfolder will be created to store them."
            If ofd.ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim lst As New List(Of String)
                For Each item As String In ListBox1.Items
                    lst.Add(item)
                Next
                Dim tmpFlip As String = If(CheckBox2.Checked, "h", "") & If(CheckBox3.Checked, "v", "")
                Dim frm As New frmScale(lst, NumericUpDown1.Value, NumericUpDown2.Value, _
                                        RadioButton1.Checked, ComboBox1.SelectedItem.ToString, ofd.SelectedPath, _
                                        ComboBox2.SelectedItem, frmList.Count, CheckBox1.Checked, _
                                        tmpFlip, If(CheckBox4.Checked, ComboBox3.Text.Replace("°", "") & "|" & ComboBox4.Text, ""), _
                                        CheckBox5.Checked)
                frm.Show()
                frmList.Add(frm)
            End If
        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ComboBox2.SelectedIndex = 0
        ComboBox1.SelectedIndex = 0
        ComboBox3.SelectedIndex = 0
        ComboBox4.SelectedIndex = 0
        refreshPanel()
    End Sub
End Class
