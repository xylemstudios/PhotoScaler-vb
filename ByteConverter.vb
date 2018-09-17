Public Class ByteConverter
#Region " ~ String Array's ~ "

    Dim binaryTypesArray As String() = {"KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}

#End Region
#Region " ~ Enums ~ "

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

#End Region
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
