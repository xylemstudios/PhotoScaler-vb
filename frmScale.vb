Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Math

Public Class frmScale
#Region "vars"
    Dim stp As New Stopwatch
    Public list As New List(Of String)
    Dim doClose As Boolean = False
    Dim usePercent As Boolean = True
    Dim imgWidth As Decimal = 0
    Dim imgHeight As Decimal = 0
    Dim resizeMode As String
    Dim _path As String 'ends with \
    Dim colorFilter As String
    Dim g As Graphics
    Dim id As Integer = 0
    Dim logEntries As New List(Of String)
    Dim ignoreList As New List(Of String)
    Dim log As Boolean
    Dim ErrorOccurred As Boolean = False
    Dim fliptype As String ' hw, h, w
    Dim rotType As String ' 90CW, 180CCW :P
    Dim sleepValue As Int16
#End Region

    Private Sub addEntry(ByVal s As String, Optional ByVal b As Boolean = False)
        logEntries.Add("[" & DateTime.Now.ToLongTimeString & "] " & If(b, "##### " & s, s))
    End Sub
    Private Sub ignore(ByVal file As String)
        ignoreList.Add(file)
    End Sub
    Private Sub saveLogs()
        Dim str As String = "PhotoScaler Log [" & Date.Today.ToShortDateString & "]" & vbNewLine
        For Each entry As String In logEntries
            str &= entry & vbNewLine
        Next
        My.Computer.FileSystem.WriteAllText(_path & "logs.txt", str, False)
    End Sub
    Public Shared Function ColorImageToGrayScaleImage(ByVal inputImage As Image) As Image
        Dim bmp As New Bitmap(inputImage)
        Dim bmpData As BitmapData = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, inputImage.PixelFormat)
        Dim pixelBytes As Integer = Image.GetPixelFormatSize(inputImage.PixelFormat) / 8

        Dim ptr As IntPtr = bmpData.Scan0
        Dim size As Integer = bmpData.Stride * bmp.Height
        Dim pixels As Byte() = New Byte(size - 1) {}
        Dim index As Integer = 0
        Dim Y As Integer = 0
        Dim mulR As Double = 0.2126
        Dim mulG As Double = 0.7152
        Dim mulB As Double = 0.0722
        Marshal.Copy(ptr, pixels, 0, size)
        For row As Integer = 0 To bmp.Height - 1
            For col As Integer = 0 To bmp.Width - 1
                index = (row * bmpData.Stride) + (col * pixelBytes)
                Y = Convert.ToInt32(Math.Round(mulR * pixels(index + 2) + mulG * pixels(index + 1) + mulB * pixels(index + 0)))
                If (Y > 255) Then Y = 255
                pixels(index + 2) = Convert.ToByte(Y)
                pixels(index + 1) = Convert.ToByte(Y)
                pixels(index + 0) = Convert.ToByte(Y)
            Next
        Next
        Marshal.Copy(pixels, 0, ptr, size)
        bmp.UnlockBits(bmpData)
        Return bmp
    End Function
    Public Shared Function ShrinkText(ByVal input As String, ByVal value As Integer) As String
        If input.Length >= value Then
            Dim temp As String = input
            Do Until temp.Length = value - 3
                temp = temp.Remove(temp.Length - 1, 1)
            Loop
            Return temp & "..."
        Else
            Return input
        End If
    End Function
    Public Sub New(ByVal filenames As List(Of String), ByVal _width As Decimal, ByVal _height As Decimal, _
                   ByVal percent As Boolean, ByVal mode As String, ByVal pathToSave As String, ByVal filter As String, _
                   ByVal idToRemove As Integer, ByVal saveLogs As Boolean, ByVal flip As String, ByVal rot As String, _
                   ByVal createSubfolder As Boolean)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        list = filenames
        usePercent = percent
        imgWidth = _width
        imgHeight = _height
        resizeMode = mode
        _path = pathToSave & IIf(pathToSave.EndsWith("\"), "", "\") & If(createSubfolder, "PhotoScaler - Scaled\", "")
        If Not FileIO.FileSystem.DirectoryExists(_path) Then
            FileIO.FileSystem.CreateDirectory(_path)
        End If
        colorFilter = filter
        id = idToRemove
        log = saveLogs
        fliptype = flip
        rotType = rot
        Control.CheckForIllegalCrossThreadCalls = False
        BackgroundWorker1.RunWorkerAsync()
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Button1.Text = "Cancel" Then
            addEntry("Cancel")
            Label2.Text = "Cancelling..."
            Label4.Text = "Cancelling..."
            BackgroundWorker1.CancelAsync()
        Else
            Form1.frmList.RemoveAt(id)
            Me.Close()
        End If
    End Sub
    Private Sub frmScale_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Button1.Text = "Cancel" Then
            e.Cancel = True
            If MsgBox("Do you want to cancel the current operation?", MsgBoxStyle.YesNoCancel + MsgBoxStyle.Exclamation, "Warning") = MsgBoxResult.Yes Then
                Button1.PerformClick()
                doClose = True
            End If
        Else
            If BackgroundWorker1.IsBusy Then BackgroundWorker1.CancelAsync()
        End If
    End Sub
    Private Sub doScaleMode(ByVal resizeMode As String)
        Select Case resizeMode
            Case "Bicubic"
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            Case "Bilinear"
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            Case "Anti-alias"
                g.CompositingQuality = Drawing2D.CompositingQuality.Default
                g.InterpolationMode = Drawing2D.InterpolationMode.Default
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
            Case "None (raw scaling)"
                g.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                g.SmoothingMode = Drawing2D.SmoothingMode.None
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighSpeed
            Case "High-speed"
                g.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                g.InterpolationMode = Drawing2D.InterpolationMode.Low
                g.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighSpeed
            Case "Balanced (good for web)"
                g.CompositingQuality = Drawing2D.CompositingQuality.AssumeLinear
                g.InterpolationMode = Drawing2D.InterpolationMode.Low
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
        End Select
    End Sub
    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            Label2.Text = "Initializing..."
            Label4.Text = "Initializing..."
            addEntry("Initialize >>", True)
            '''''''''''''''''''
            Dim doneBool As Boolean = False
            Dim count As Integer = list.Count
            Dim i As Integer = 0
            addEntry("Start internal process")
            Timer1.Start()
            stp.Start()
            Button2.Enabled = False
            addEntry("Calculating files and values")
            ProgressBar1.Maximum = Math.Ceiling(count + count / 4)
            If Not colorFilter = "None" Then
                ProgressBar1.Maximum += Math.Ceiling(count / 3)
            End If
            addEntry("Setting up booleans")
            Dim flip As Boolean = fliptype.Length > 0
            Dim rot As Boolean = rotType.Length > 0
            If flip Then ProgressBar1.Maximum += Math.Ceiling(count / 2)
            If rot Then ProgressBar1.Maximum += Math.Ceiling(count / 2)
            ProgressBar2.Maximum = count
            addEntry("Start graphics object")
            g = Graphics.FromImage(My.Resources.image)
            '''''''''''''''''''''

            If BackgroundWorker1.CancellationPending Then
                GoTo cancel
            End If


            'launch image check
            Label2.Text = "Launching image check..."
            Label4.Text = "Starting..."
            addEntry("Image check >>", True)
            Try '' needed to avoid error on cancel
                For Each file As String In list
                    Console.Out.WriteLine("hello")
                    i += 1
                    Label2.Text = "Checking images..."
                    Label4.Text = "Checking " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                    Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                    If BackgroundWorker1.CancellationPending Then
                        GoTo cancel
                        Exit For
                    End If
                    If Not ImageChecker.IsValidImage(file) Or Not FileIO.FileSystem.FileExists(file) Or ImageChecker.IsAnimatedGif(file) Then
                        ignore(file)
                        addEntry("--- ERROR: Image not valid: " & file)
                    Else
                        addEntry("Check " & file)
                    End If
                    ProgressBar1.Value = i / 4
                    ProgressBar2.Value = i
                    System.Threading.Thread.Sleep(sleepValue)
                Next
            Catch ex As Exception
            End Try



            i = 0
            ProgressBar2.Value = 0
            'start processing
            Label2.Text = "Starting rescaling..."
            Label4.Text = "Starting..."
            addEntry("Rescale >>", True)
            For Each file As String In list
                i += 1
                If BackgroundWorker1.CancellationPending Then GoTo cancel
                If Not ignoreList.Contains(file) Then
                    Try

                        Label2.Text = "Rescaling images..."
                        Label4.Text = "Rescaling " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                        Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                        Dim tempImg As New Bitmap(file)
                        Dim bmp As Bitmap

                        If usePercent Then
                            bmp = New Bitmap(CInt(tempImg.Width * imgWidth / 100), CInt(tempImg.Height * imgHeight / 100))
                        Else
                            bmp = New Bitmap(CInt(imgWidth), CInt(imgHeight))
                        End If
                        g = Graphics.FromImage(bmp)
                        doScaleMode(resizeMode)
                        g.DrawImage(tempImg, 0, 0, bmp.Width, bmp.Height)

                        bmp.Save(_path & IO.Path.GetFileName(file))
                        addEntry("Rescale " & file)
                        ProgressBar1.Value = i + count / 4
                        ProgressBar2.Value = i
                        bmp.Dispose()
                        tempImg.Dispose()
                        g.Dispose()
                    Catch ex As Exception
                    End Try
                Else
                    ProgressBar1.Value = i + count / 4
                    ProgressBar2.Value = i
                    Label2.Text = "Rescaling images..."
                    Label4.Text = "Rescaling " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                    Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                    addEntry("Ignore " & file)
                End If
                System.Threading.Thread.Sleep(sleepValue)
            Next

            i = 0
            ProgressBar2.Value = 0

            'flip

            If flip Then
                Label2.Text = "Starting flipping..."
                Label4.Text = "Starting..."
                addEntry("Flip >>", True)
                For Each file As String In list
                    i += 1
                    If BackgroundWorker1.CancellationPending Then GoTo cancel
                    If Not ignoreList.Contains(file) Then
                        Try
                            Label2.Text = "Flipping images..."
                            Label4.Text = "Flipping " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                            Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                            Dim bmp As Bitmap = Bitmap.FromFile(_path & IO.Path.GetFileName(file))
                            If fliptype.Contains("h") Then
                                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX)
                            End If
                            If fliptype.Contains("v") Then
                                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY)
                            End If
                            bmp.Save(_path & IO.Path.GetFileName(file))
                            bmp = Nothing
                            addEntry("Flip " & file)
                            ProgressBar1.Value = count + count / 4 + i / 2
                            ProgressBar2.Value = i
                        Catch ex As Exception
                        End Try
                    Else
                        ProgressBar1.Value = count + count / 4 + i / 2
                        ProgressBar2.Value = i
                        Label2.Text = "Flipping images..."
                        Label4.Text = "Flipping " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                        Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                        addEntry("Ignore " & file)
                    End If
                    System.Threading.Thread.Sleep(sleepValue)
                Next
            End If


            i = 0
            ProgressBar2.Value = 0

            'rotate

            If rot Then
                Label2.Text = "Starting rotation..."
                Label4.Text = "Starting..."
                addEntry("Rotate >>", True)
                For Each file As String In list
                    i += 1
                    If Not ignoreList.Contains(file) Then
                        Try
                            If BackgroundWorker1.CancellationPending Then GoTo cancel
                            Label2.Text = "Rotating images..."
                            Label4.Text = "Rotating " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                            Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                            Dim bmp As Bitmap = Bitmap.FromFile(_path & IO.Path.GetFileName(file))
                            Dim a As String() = rotType.Split("|")
                            Dim temp_rotVal = a(0)
                            If a(1) = "CCW" Then temp_rotVal += If(temp_rotVal = 180, 0, 180)
                            If temp_rotVal > 360 Then temp_rotVal -= 360
                            Select Case temp_rotVal
                                Case 90
                                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone)
                                Case 180
                                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone)
                                Case 270
                                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone)
                            End Select
                            bmp.Save(_path & IO.Path.GetFileName(file))
                            addEntry("Rotate " & file)
                            ProgressBar1.Value = count + count / 4 + If(flip, count / 2, 0) + i / 2
                            ProgressBar2.Value = i
                            bmp.Dispose()
                            bmp = Nothing
                        Catch ex As Exception
                        End Try
                    Else
                        If BackgroundWorker1.CancellationPending Then GoTo cancel
                        ProgressBar1.Value = count + count / 4 + If(flip, count / 2, 0) + i / 2
                        ProgressBar2.Value = i
                        Label2.Text = "Rotating images..."
                        Label4.Text = "Rotating " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"
                        Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                        addEntry("Ignore " & file)
                    End If
                    System.Threading.Thread.Sleep(sleepValue)
                Next
            End If


            'start color filters
            i = 0
            ProgressBar2.Value = 0
            ProgressBar2.Maximum = count
            Label2.Text = "Launching color filter engine..."
            Label4.Text = "Starting..."
            If colorFilter <> "None" Then addEntry("Filter >>", True)
            For Each file As String In list
                Label2.Text = "Applying color filters..."
                i += 1
                If BackgroundWorker1.CancellationPending Then GoTo cancel
                Me.Text = Math.Round(ProgressBar1.Value / ProgressBar1.Maximum * 100, 2) & "%"
                Label4.Text = "Filtering " & ShrinkText(IO.Path.GetFileName(file), 40) & " (" & i & "/" & count & ")"

                If colorFilter = "None" Then
                    doneBool = True
                    GoTo done
                End If
                If Not ignoreList.Contains(file) Then
                    Select Case colorFilter
                        Case "None"
                            doneBool = True
                            GoTo done
                        Case "Grayscale"
                            Using bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                                Dim bmpData As BitmapData = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, Image.FromFile(_path & IO.Path.GetFileName(file)).PixelFormat)
                                Dim pixelBytes As Integer = Image.GetPixelFormatSize(Image.FromFile(_path & IO.Path.GetFileName(file)).PixelFormat) / 8
                                Dim mulR As Double = 0.2126
                                Dim mulG As Double = 0.7152
                                Dim mulB As Double = 0.0722
                                Dim ptr As IntPtr = bmpData.Scan0
                                Dim size As Integer = bmpData.Stride * bmp.Height
                                Dim pixels As Byte() = New Byte(size - 1) {}
                                Dim index As Integer = 0
                                Dim Y As Integer = 0
                                Marshal.Copy(ptr, pixels, 0, size)
                                For row As Integer = 0 To bmp.Height - 1
                                    If BackgroundWorker1.CancellationPending Then GoTo cancel
                                    For col As Integer = 0 To bmp.Width - 1
                                        If BackgroundWorker1.CancellationPending Then GoTo cancel
                                        index = (row * bmpData.Stride) + (col * pixelBytes)
                                        Y = Convert.ToInt32(Math.Round(mulR * pixels(index + 2) + mulG * pixels(index + 1) + mulB * pixels(index + 0)))
                                        If (Y > 255) Then Y = 255
                                        pixels(index + 2) = Convert.ToByte(Y)
                                        pixels(index + 1) = Convert.ToByte(Y)
                                        pixels(index + 0) = Convert.ToByte(Y)
                                    Next
                                Next
                                Marshal.Copy(pixels, 0, ptr, size)
                                bmp.UnlockBits(bmpData)
                                bmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            End Using
                        Case "Brighter"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.ApplyBrightness(bmp, 20)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Darker"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.ApplyBrightness(bmp, -20)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Negative"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.ApplyNegative(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Sepia"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.ApplySepia(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Intensify"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.ApplyIntensify(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Red only"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.RedOnly(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Green only"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.GreenOnly(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Blue only"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.BlueOnly(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Alpha only"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.AlphaOnly(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Websafe"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.Websafe(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                        Case "Alpha sharpen"
                            Dim bmp As New Bitmap(_path & IO.Path.GetFileName(file))
                            Dim tempbmp As Bitmap = Nothing
                            tempbmp = ImageFilters.AlphaSharpen(bmp)
                            tempbmp.Save(_path & IO.Path.GetFileName(file) & "xylem")
                            bmp.Dispose()
                            tempbmp.Dispose()
                    End Select
                    addEntry("Filter " & file)
                Else
                    addEntry("Ignore " & file)
                End If
                ProgressBar1.Value = count + count / 4 + If(flip, count / 2, 0) + If(rot, count / 2, 0) + i / 3
                ProgressBar2.Value = i
                System.Threading.Thread.Sleep(sleepValue)
            Next
            Label2.Text = "Finalizing..."
            Label4.Text = "Finalizing..."
            i = 0
            ProgressBar2.Maximum = count
            addEntry("Finalize >>", True)
            For Each s As IO.FileInfo In New IO.DirectoryInfo(_path).GetFiles()
                If BackgroundWorker1.CancellationPending Then GoTo cancel
                If s.Extension.EndsWith("xylem") Then
                    My.Computer.FileSystem.DeleteFile(s.FullName.ToString.Remove(s.FullName.Length - 5, 5))
                    My.Computer.FileSystem.RenameFile(s.FullName, IO.Path.GetFileNameWithoutExtension(s.Name) & s.Extension.Remove(s.Extension.Length - 5, 5))
                    i += 1
                    ProgressBar2.Value = i
                    addEntry("Finalize " & IO.Path.GetFileNameWithoutExtension(s.Name) & s.Extension.Remove(s.Extension.Length - 5, 5))
                    Application.DoEvents()
                End If
                System.Threading.Thread.Sleep(sleepValue)
            Next
done:
            doneBool = True
            Try

                Button2.Enabled = True
                ProgressBar1.Value = ProgressBar1.Maximum
                ProgressBar2.Value = ProgressBar2.Maximum
                Label2.Text = "Finished"
                Label4.Text = "Finished"
                Button1.Text = "Close"
                Me.Text = "100%"
                Form1.frmList.RemoveAt(id)
                stp.Stop()
                Timer1.Stop()
                Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
                Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
                addEntry("Finished successfully for " & count & " files")
                GoTo finish
            Catch ex As Exception
            End Try
cancel:
            Try
                If doneBool Then '' prevents errors, sometimes it used to display "Cancelled" D:
                    Button2.Enabled = True
                    ProgressBar1.Value = ProgressBar1.Maximum
                    ProgressBar2.Value = ProgressBar2.Maximum
                    Label2.Text = "Finished"
                    Label4.Text = "Finished"
                    Button1.Text = "Close"
                    Me.Text = "100%"
                    Form1.frmList.RemoveAt(id)
                    stp.Stop()
                    Timer1.Stop()
                    Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
                    Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
                    addEntry("Finished successfully for " & count & " files")
                    GoTo finish
                Else
                    Button2.Enabled = False
                    Label2.Text = "Cancelled"
                    Label4.Text = "Cancelled"
                    Button1.Text = "Close"
                    ProgressBar1.Value = ProgressBar1.Maximum
                    ProgressBar2.Value = ProgressBar2.Maximum
                    Form1.frmList.RemoveAt(id)
                    stp.Stop()
                    Timer1.Stop()
                    Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
                    Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
                    If doClose Then Me.Close()
                    addEntry("Cancelled")
                    GoTo finish
                End If

            Catch ex As Exception
            End Try
        Catch ex As Exception

            ErrorOccurred = True
            Button2.Enabled = False
            Label2.Text = "Stopped"
            Label4.Text = "Stopped"
            Button1.Text = "Close"
            ProgressBar1.Value = ProgressBar1.Maximum
            ProgressBar2.Value = ProgressBar2.Maximum
            stp.Stop()
            Timer1.Stop()
            Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
            Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
            addEntry("--- ERROR: " & ex.Message)
            addEntry("Save logs")
            saveLogs()
            MsgBox("An unexpected error occured." & vbNewLine & "Please try again. If you still experience problems please send the error details to the administrator." & vbNewLine & "Logs have been saved automatically." & vbNewLine & "Advanced information:" & vbNewLine & ex.Message, MsgBoxStyle.Exclamation, "Error")
            Try
                Form1.frmList.RemoveAt(id)
            Catch ex2 As Exception
            End Try
            If BackgroundWorker1.IsBusy Then BackgroundWorker1.CancelAsync()
        End Try
finish:
        stp.Stop()
        Timer1.Stop()
        Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
        Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
        If log And Not ErrorOccurred Then addEntry("Save logs") : saveLogs()
        Exit Sub
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Process.Start(_path)
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If Button3.Tag = "down" Then
            Button3.Image = My.Resources.arrowup
            Me.Height = 258
            Button3.Tag = "up"
        Else
            Button3.Image = My.Resources.arrowdown
            Me.Height = 203
            Button3.Tag = "down"
        End If
    End Sub
    Private Sub frmScale_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Height = 203
        ComboBox1.SelectedIndex = 1
    End Sub
    Private Function SecondsToDate(ByVal Seconds As Double, Optional ByVal showSeconds As Boolean = False) As String
        If Seconds < 60 Then
            Return Seconds & " seconds"
        Else
            If showSeconds Then
                Return Date.FromOADate(Seconds / 86400).ToLongTimeString
            Else
                Return Date.FromOADate(Seconds / 86400).ToShortTimeString
            End If
        End If
    End Function
    Function GetFolderSize(ByVal DirPath As String, _
   Optional ByVal IncludeSubFolders As Boolean = True) As Long
        Dim lngDirSize As Long
        Dim objFileInfo As FileInfo
        Dim objDir As DirectoryInfo = New DirectoryInfo(DirPath)
        Dim objSubFolder As DirectoryInfo
        Try
            'add length of each file
            For Each objFileInfo In objDir.GetFiles()
                lngDirSize += objFileInfo.Length
            Next
            'call recursively to get sub folders
            'if you don't want this set optional
            'parameter to false 
            If IncludeSubFolders Then
                For Each objSubFolder In objDir.GetDirectories()
                    lngDirSize += GetFolderSize(objSubFolder.FullName)
                Next
            End If
        Catch Ex As Exception
        End Try
        Return lngDirSize
    End Function
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
        Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
    End Sub
    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        stp.Stop()
        Timer1.Stop()
        Label9.Text = SecondsToDate(Math.Round(stp.ElapsedMilliseconds / 1000), True)
        Label10.Text = New ByteConverter().ConvertByte(GetFolderSize(_path, False))
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        Select Case ComboBox1.SelectedIndex
            Case 0
                'Real-time
                sleepValue = 0
                addEntry("Change priority Real-time")
            Case 1
                'High
                sleepValue = 1
                addEntry("Change priority High")
            Case 2
                'Low
                sleepValue = 10
                addEntry("Change priority Low")
            Case 3
                'Background
                sleepValue = 50
                addEntry("Change priority Background")
        End Select
    End Sub

    Private Sub Label4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label4.Click

    End Sub

    Private Sub frmScale_Enter(sender As Object, e As EventArgs) Handles Me.Enter
    End Sub
End Class