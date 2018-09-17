Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices.Marshal
Public Class ImageFilters

    Public Shared Function ApplyIntensifySingle(ByVal red As Integer, ByVal green As Integer, ByVal blue As Integer) As Color
        Dim outR As Short = red
        Dim outG As Short = green
        Dim outB As Short = blue
        If outR = outG And outR = outB Then
            If outR >= 126 Then
                'brighter
                outR += 20
                outB += 20
                outG += 20
                If outR > 255 Then outR = 255
                If outG > 255 Then outG = 255
                If outB > 255 Then outB = 255
                Return Color.FromArgb(outR, outG, outB)
            Else
                'darker
                outR -= 20
                outB -= 20
                outG -= 20
                If outR < 0 Then outR = 0
                If outG < 0 Then outG = 0
                If outB < 0 Then outB = 0
                Return Color.FromArgb(outR, outG, outB)
            End If
        Else
            Select Case Math.Max(Math.Max(outR, outG), outB)
                Case outR
                    If outR = outG Or outR = outB Then
                        GoTo except
                    End If
                    outR += 20
                    outG += 3 + Math.Round(outR / 255 * 5)
                    outB += 2 + Math.Round(outR / 255 * 5)
                Case outG
                    If outG = outR Or outG = outB Then
                        GoTo except
                    End If
                    outG += 20
                    outR += 2 + Math.Round(outG / 255 * 5)
                    outB += 3 + Math.Round(outG / 255 * 5)
                Case outB
                    If outB = outR Or outB = outG Then
                        GoTo except
                    End If
                    outB += 20
                    outR += 2 + Math.Round(outB / 255 * 5)
                    outG += 3 + Math.Round(outB / 255 * 5)

            End Select
            If outR > 255 Then outR = 255
            If outG > 255 Then outG = 255
            If outB > 255 Then outB = 255
            Return Color.FromArgb(outR, outG, outB)
            Exit Function
except:
            If outR = outG Then
                outR += 20
                outG += 20
                outB += 3 + Math.Round(outR / 255 * 5)
                If outR > 255 Then outR = 255
                If outG > 255 Then outG = 255
                If outB > 255 Then outB = 255
                Return Color.FromArgb(outR, outG, outB)
            ElseIf outR = outB Then
                outR += 20
                outB += 20
                outG += 3 + Math.Round(outB / 255 * 5)
                If outR > 255 Then outR = 255
                If outG > 255 Then outG = 255
                If outB > 255 Then outB = 255
                Return Color.FromArgb(outR, outG, outB)
            ElseIf outB = outG Then
                outB += 20
                outG += 20
                outR += 3 + Math.Round(outG / 255 * 5)
                If outR > 255 Then outR = 255
                If outG > 255 Then outG = 255
                If outB > 255 Then outB = 255
                Return Color.FromArgb(outR, outG, outB)
            End If
        End If
    End Function
    Public Shared Function ApplyIntensify(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            'send the values
            red = ApplyIntensifySingle(red, green, blue).R
            green = ApplyIntensifySingle(red, green, blue).G
            blue = ApplyIntensifySingle(red, green, blue).B
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function ApplySepia(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' apply sepia tone in temp variables
            Dim outR = (red * 0.393) + (green * 0.769) + (blue * 0.189)
            Dim outG = (red * 0.349) + (green * 0.686) + (blue * 0.168)
            Dim outB = (red * 0.272) + (green * 0.534) + (blue * 0.131)
            'send the values back to the original variables
            red = outR
            green = outG
            blue = outB
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function ApplyBrightness(ByVal b As Bitmap, ByVal level As Integer) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            blue = blue + level
            green = green + level
            red = red + level
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function ApplyNegative(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim stride As Integer = bmd.Stride
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        For i As Integer = 0 To pixels.Length - 1
            pixels(i) = (Not pixels(i) And &HFFFFFF) Or (pixels(i) And &HFF000000)
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function RedOnly(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            blue = 0
            green = 0
            red = red
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function GreenOnly(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            blue = 0
            green = green
            red = 0
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function BlueOnly(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            blue = blue
            green = 0
            red = 0
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function AlphaSharpen(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            alpha = If(alpha > 126, 255, 0)
            ' clip anything that is out of range 0 to 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function AlphaOnly(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha As Integer
        Dim val As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            ' get alpha
            val = alpha
            ' clip anything that is out of range 0 to 255
            If val < 0 Then val = 0 Else If val > 255 Then val = 255
            pixels(i) = (255 << 24) _
                      Or (val << 16) _
                      Or (val << 8) _
                      Or val
            ' first one must be 255 in this case!!
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function Websafe(ByVal b As Bitmap) As Bitmap
        Dim bmd As BitmapData = _
        b.LockBits(New Rectangle(0, 0, b.Width, b.Height), _
        System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim scan0 As IntPtr = bmd.Scan0
        Dim pixels(b.Width * b.Height - 1) As Integer
        Copy(scan0, pixels, 0, pixels.Length)
        Dim alpha, red, green, blue As Integer
        ' loop through all pixels
        For i As Integer = 0 To pixels.Length - 1
            alpha = (pixels(i) >> 24) And &HFF
            red = (pixels(i) >> 16) And &HFF
            green = (pixels(i) >> 8) And &HFF
            blue = pixels(i) And &HFF
            ' calculate the new values by adding the level the user has selected
            Dim _c As Color = WebsafeTranslator.WebsafeColor(Color.FromArgb(red, green, blue))
            blue = _c.B
            green = _c.G
            red = _c.R
            ' clip anything that is out of range 0 to 255
            If blue < 0 Then blue = 0 Else If blue > 255 Then blue = 255
            If green < 0 Then green = 0 Else If green > 255 Then green = 255
            If red < 0 Then red = 0 Else If red > 255 Then red = 255
            pixels(i) = (alpha << 24) _
                      Or (red << 16) _
                      Or (green << 8) _
                      Or blue
        Next
        Copy(pixels, 0, scan0, pixels.Length)
        b.UnlockBits(bmd)
        Return b
    End Function
    Public Shared Function FlipImg(ByVal b As Bitmap, ByVal flipType As String) As Bitmap
        If flipType.Contains("h") Then
            b.RotateFlip(RotateFlipType.RotateNoneFlipX)
        End If
        If flipType.Contains("v") Then
            b.RotateFlip(RotateFlipType.RotateNoneFlipY)
        End If
        Return b
    End Function
End Class