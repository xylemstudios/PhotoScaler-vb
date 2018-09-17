Imports System.IO
Public Class FunctionPack
#Region " #: Date functions :# "
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
    Private Function MillisecondsToDate(ByVal Milliseconds As Double, Optional ByVal showSeconds As Boolean = False) As String
        Dim Seconds As Double = Math.Round(Milliseconds / 1000)
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
#End Region
#Region " #: FileIO functions :# "
    Function GetFolderSize(ByVal DirPath As String, _
       Optional ByVal IncludeSubFolders As Boolean = True) As Long
        Dim lngDirSize As Long
        Dim objFileInfo As FileInfo
        Dim objDir As DirectoryInfo = New DirectoryInfo(DirPath)
        Dim objSubFolder As DirectoryInfo
        Try
            For Each objFileInfo In objDir.GetFiles()
                lngDirSize += objFileInfo.Length
            Next
            If IncludeSubFolders Then
                For Each objSubFolder In objDir.GetDirectories()
                    lngDirSize += GetFolderSize(objSubFolder.FullName)
                Next
            End If
        Catch Ex As Exception
        End Try
        Return lngDirSize
    End Function
#End Region
#Region " #: String functions :# "
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
#End Region
#Region " #: Other stuff :# "
    Public Class ByteConverter
        Dim binaryTypesArray As String() = {"KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}
        Enum BinaryTypes
            KB = 0
            MB = 1
            GB = 2
            TB = 3
            PB = 4
            EB = 5
            ZB = 6
            YB = 7
        End Enum
        Public Function ConvertByte(ByVal bytes As Double) As String
            Dim mString As String = ""
            For i As Integer = 0 To binaryTypesArray.Length - 1
                Dim index As Integer = (binaryTypesArray.Length - 1) - i
                If bytes <= 1023 Then
                    mString = bytes & " bytes"
                    Exit For
                ElseIf bytes >= 1024 ^ (index + 1) Then
                    mString = Math.Round(bytes / (1024 ^ (index + 1)), 2) & " " & binaryTypesArray(index)
                    Exit For
                End If
                Application.DoEvents()
            Next
            Return mString
        End Function
        Public Function ConvertByte(ByVal bytes As Double, ByVal outputBinaryType As BinaryTypes)
            Dim index As Integer = outputBinaryType
            Return Math.Round(bytes / (1024 ^ (index + 1)), 2) & " " & binaryTypesArray(index)
        End Function
    End Class
#End Region
#Region " #: Graphics functions :# "
    Public Class GraphicsHandler
        Public Shared Function MakeGradient(ByVal StartColor As Color, ByVal EndColor As Color, ByVal width As Integer, ByVal height As Integer, Optional ByVal LinearGradientMode As Drawing2D.LinearGradientMode = Drawing2D.LinearGradientMode.Horizontal) As Image
            Dim sizeOfControl As New Size(width, height)
            Dim resultImage As New Bitmap(width, height, Imaging.PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(resultImage)
            g.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            g.InterpolationMode = Drawing2D.InterpolationMode.Low
            g.FillRectangle(New Drawing2D.LinearGradientBrush(New Rectangle(New Point(0, 0), sizeOfControl), StartColor, EndColor, LinearGradientMode), New Rectangle(New Point(0, 0), sizeOfControl))
            Return resultImage
            g.Dispose() : g.Flush() : g = Nothing
            sizeOfControl = Nothing
            resultImage.Dispose() : resultImage = Nothing
        End Function
        Public Function IconFromFilePath(ByVal filePath As String) As Bitmap
            Dim result As Icon = Nothing
            If Not IsNothing(filePath) Or Not filePath = "" Then
                Try
                    result = Icon.ExtractAssociatedIcon(filePath)
                Catch ''# swallow and return nothing. You could supply a default Icon here as well
                    result = Nothing
                End Try
                Return result.ToBitmap
            Else
                Return Nothing
            End If
        End Function
    End Class
#End Region
End Class
