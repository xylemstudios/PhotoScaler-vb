Public Class WebsafeTranslator
#Region " :.: Part of websafe filter :.:"
    Public Shared Function WebsafeColor(ByVal c As Color) As Color
        Dim _step As Short = 51
        Dim result As String = ""
        Dim r, g, b As Byte
        Dim hexColor As String = ToHtml(c)
        hexColor = hexColor.Replace("#", "")
        r = HexToDec(hexColor.Remove(2, 4))
        g = HexToDec(hexColor.Remove(0, 2).Remove(2, 2))
        b = HexToDec(hexColor.Remove(0, 4))
        Dim webSafe As Integer
        webSafe = _step * Math.Round(r / _step)
        result &= (Hex(webSafe)).PadLeft(2, "0")
        webSafe = _step * Math.Round(g / _step)
        result &= (Hex(webSafe)).PadLeft(2, "0")
        webSafe = _step * Math.Round(b / _step)
        result &= (Hex(webSafe)).PadLeft(2, "0")
        Return ColorTranslator.FromHtml("#" & result)
    End Function
    Public Shared Function HexToDec(ByVal hex As String) As Integer
        Dim resultInteger As Integer = 0
        Dim index As Integer = hex.Length
        For Each c As String In hex
            index -= 1
            If Not c = "#" Then
                resultInteger += HexToDecimalValue(c) * 16 ^ index
            End If
        Next
        Return resultInteger
    End Function
    Public Shared Function HexToDecimalValue(ByVal hexCharacter As String) As Integer
        Dim resultInteger As Integer = 0
        Select Case hexCharacter
            Case "A"
                resultInteger = 10
            Case "B"
                resultInteger = 11
            Case "C"
                resultInteger = 12
            Case "D"
                resultInteger = 13
            Case "E"
                resultInteger = 14
            Case "F"
                resultInteger = 15
            Case Else
                resultInteger = hexCharacter
        End Select
        Return resultInteger
    End Function
    Public Shared Function ToHtml(ByVal colorObj As System.Drawing.Color) As String
        Return ColorTranslator.ToHtml(Color.FromArgb(colorObj.R, colorObj.G, colorObj.B))
        'Return "#" & FixHex(Hex(colorObj.R)) & FixHex(Hex(colorObj.G)) & FixHex(Hex(colorObj.B))
    End Function
#End Region
End Class
