Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports HtmlAgilityPack

Public Class Form1
    Private Sub scrap_Click(sender As Object, e As EventArgs) Handles scrap.Click
        'borro carpeta y creo
        Dim mydir As New DirectoryInfo(dirname.Text)
        If mydir.Exists = True Then
            'System.IO.Directory.Delete(dirname.Text, True)
        Else
            mydir.Create()
        End If


        'primero tendría que leer la web y cojer las imagenes
        Dim lawebes As String = readweb(url.Text)
        Dim lawebin As String = readweb(Replace(url.Text, "/es/", "/en/"))

        txlog.Text = extraer(lawebin, "<img", ">")
        'Exit Sub

        Dim lineas = Split(txlog.Text, vbCrLf)
        For i = 0 To lineas.Length - 1
            'If InStr(lineas(i).ToString, "png") > 0 Or InStr(lineas(i).ToString, "jpg") > 0 Then
            Dim linea = lineas(i).ToString
            'linea = Replace(linea, """", "")
            If InStr(linea, patron.Text) > 0 Then

                Dim imagen As String = extraer(linea, "src", patron.Text)

                imagen = Replace(imagen, """", "")
                imagen = Replace(imagen, "=", "")
                ' ****lo comento para hacerlo internacional

                'imagen = Replace(imagen, "/en_", "/sp_")
                'imagen = Replace(imagen, "_en", "_es")
                'imagen = Replace(imagen, "_EN_LR", "_ES")
                'imagen = Replace(imagen, "_EN", "_ES")

                ' ****

                imagen = Replace(imagen, vbCrLf, Nothing)
                imagen = imagen & patron.Text
                'solo commander 16
                'imagen = Replace(imagen, "_EN.png", "_SP.png")

                Dim tit As String = extraer(linea, "alt=""", """")
                tit = Replace(tit, """", "")
                tit = Replace(tit, "=", "")
                tit = RemoveWhitespace(tit)
                tit = Replace(tit, vbCrLf, Nothing)
                tit = Trim(tit)
                txlog.AppendText("Saving " & tit & " from " & imagen & vbCrLf)
                tit = Replace(tit, "&rsquo;", "'")
                tit = Replace(tit, "///", "")
                tit = Replace(tit, "//", "")
                tit = Replace(tit, "/", "")
                tit = Replace(tit, "&AElig;", "Ae")

                Dim lands = "forest,plain,island,mountain,swamp,wastes"
                Dim myland = Split(lands, ",")

                For a = 0 To myland.Count - 1
                    If InStr(LCase(tit), myland(a)) > 0 Then
                        Dim myy = 1
                        While File.Exists(dirname.Text & "/" & tit & myy & ".full.jpg") = True
                            myy = myy + 1
                            'If myy > 9 Then Exit While
                        End While
                        tit = tit & myy

                        Exit For
                    End If
                Next a

                If InStr(imagen, "/") = 0 Or IsNothing(tit) = False Then
                    If Directory.Exists(dirname.Text) = False Then Directory.CreateDirectory(dirname.Text)
                    If File.Exists(dirname.Text & "/" & tit & ".full.jpg") = False Then
                        Dim hayerror = False
                        Dim downloaded = False
                        Try
                            Dim Client As New WebClient
                            Client.DownloadFile(imagen, dirname.Text & "/" & tit & ".full.jpg")
                            txlog.AppendText(imagen & " " & tit & vbCrLf)
                            downloaded = True
                            'Client.Dispose()
                        Catch
                            hayerror = True
                        End Try
                        'comento esto para no bajar en inglés
                        'If downloaded = False Then
                        '    Try
                        '        Dim Client As New WebClient
                        '        Client.DownloadFile(Replace(imagen, "/sp_", "/en_"), dirname.Text & "/" & tit & ".full.jpg")
                        '        txlog.AppendText(imagen & " " & tit & vbCrLf)
                        '        downloaded = True
                        '    Catch
                        '        hayerror = True
                        '    End Try
                        ' End If

                        If hayerror Then

                            'txlog.AppendText(Err.Description & imagen & " " & tit)
                            '    'Exit Sub
                        End If


                        txlog.AppendText(imagen & " " & tit & vbCrLf)
                    End If
                End If

            End If

        Next i
        txlog.AppendText("END!")
    End Sub

    Function RemoveWhitespace(fs As String) As String
        Try
            fs = Regex.Replace(fs, "[ ]{2,}", " ")
            fs = Regex.Replace(fs, "\s+", " ")
        Catch
        End Try

        Return fs
    End Function

    Function normalizar(t) As String
        normalizar = t
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'url.Text = "https://magic.wizards.com/es/articles/archive/card-image-gallery/hour-devastation"
    End Sub

    Public Function extraer(ByRef strSource As String, ByRef strStart As String, ByRef strEnd As String,
                            Optional ByRef startPos As Integer = 0)

        Dim iPos As Integer, iEnd As Integer, strResult As String, lenStart As Integer = strStart.Length
        Dim res As String
        Do Until iPos = - 1
            strResult = String.Empty
            iPos = strSource.IndexOf(strStart, startPos)
            iEnd = strSource.IndexOf(strEnd, iPos + lenStart)
            If iPos <> - 1 AndAlso iEnd <> - 1 Then
                strResult = strSource.Substring(iPos + lenStart, iEnd - (iPos + lenStart))
                res = res & strResult & vbCrLf
                startPos = iPos + lenStart
            End If
        Loop
        extraer = res
    End Function


    Function readweb(laUrl As String)
        laUrl = Replace(laUrl, "'", "")
        laUrl = Replace(laUrl, """", "")
        Dim res As String
        If laUrl = "" Then Exit Function

        Dim request As WebRequest
        Try
            request = WebRequest.Create(laUrl)
        Catch
            'txlog.AppendText("Error reading" & laUrl & "  bad URL?")
            readweb = ""
            Exit Function
        End Try


        Dim response As WebResponse
        Try
            response = request.GetResponse()
        Catch
            readweb = ""
            Exit Function
        End Try
        Dim reader As New StreamReader(response.GetResponseStream())
        Try
            res = reader.ReadToEnd()
        Catch


            Exit Function
        End Try

        reader.Close()
        response.Close()
        readweb = res
    End Function

    Function obtener(mainurl) As String
        Dim doc As HtmlDocument
        doc = New HtmlDocument()
        Dim sourceString As String = New WebClient().DownloadString(mainurl)
        doc.LoadHtml(sourceString)
        Dim i = 0
        Dim res = ""


        For Each link As HtmlNode In doc.DocumentNode.SelectNodes("//img[@src]")
            Dim linkAddress = GetAbsoluteUrl(link.Attributes("src").Value, mainurl)
            Console.WriteLine("Image: {0}", linkAddress)
            res = res & linkAddress.ToString & ","
        Next
        obtener = res
    End Function

    Function GetAbsoluteUrl(partialUrl As String, baseUrl As String)
        Dim myUri = New Uri(partialUrl, UriKind.RelativeOrAbsolute)
        If (myUri.IsAbsoluteUri = False) Then
            myUri = New Uri(New Uri(baseUrl), partialUrl)
        End If
        GetAbsoluteUrl = myUri
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim mydir As New DirectoryInfo(dirname.Text)
        For Each fi In mydir.GetFiles
            Dim n As String = LCase(fi.Name)
            Dim borrar = False

            If InStr(n, "plains") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "forest") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "island") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "swamp") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "mountain") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "wastes") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
        Next
    End Sub


    '
End Class
