Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports HtmlAgilityPack

Public Class Form1
    Private Sub scrap_Click(sender As Object, e As EventArgs) Handles scrap.Click
        'borro carpeta y creo
        Dim mydir As New DirectoryInfo(dirname.Text)
        If mydir.Exists =false Then
            mydir.Create()
        End If

        Dim lawebes As String = ReadWeb(url.Text)
        Dim lawebin As String = ReadWeb(Replace(url.Text, "/es/", "/en/"))

        txlog.Text = Extract(lawebin, "<img", ">")

        Dim lineas = Split(txlog.Text, vbCrLf)

        For i = 0 To lineas.Length - 1
            Dim MyLine = lineas(i).ToString
            If InStr(MyLine, patron.Text) > 0 Then
                Dim Myimage As String = Extract(MyLine, "src", patron.Text)
                Myimage = Replace(Myimage, """", "")
                Myimage = Replace(Myimage, "=", "")

                ' ****IN OTHER LANGUAGES, UNCOMMENT THIS FOR A PERSONAL REPLACE (HERE, SCRAPPING SPANISH, LEAVE BLANK FOR ENGLISH)

                Myimage = Replace(Myimage, "/en_", "/sp_")
                Myimage = Replace(Myimage, "_en", "_es")
                Myimage = Replace(Myimage, "_EN_LR", "_ES")
                Myimage = Replace(Myimage, "_EN", "_ES")
                'solo commander 16
                'Myimage = Replace(imagen, "_EN.png", "_SP.png")
                ' ****

                Myimage = Replace(Myimage, vbCrLf, Nothing)
                Myimage = Myimage & patron.Text


                Dim tit As String = Extract(MyLine, "alt=""", """")
                tit = Replace(tit, """", "")
                tit = Replace(tit, "=", "")
                tit = RemoveWhitespace(tit)
                tit = Replace(tit, vbCrLf, Nothing)

                tit = Trim(tit)

                txlog.AppendText("Saving " & tit & " from " & Myimage & vbCrLf)
                tit = Replace(tit, "&rsquo;", "'")
                tit = Replace(tit, "///", "")
                tit = Replace(tit, "//", "")
                tit = Replace(tit, "/", "")
                tit = Replace(tit, "&AElig;", "Ae")

                'Dim lands = "forest,plain,island,mountain,swamp,wastes"
                'Dim myland = Split(lands, ",")

                'For a = 0 To myland.Count - 1
                '    If InStr(LCase(tit), myland(a)) > 0 Then
                'Dim myy = 1
                'While File.Exists(dirname.Text & "/" & tit & myy & ".full.jpg") = True
                '    myy = myy + 1
                'End While
                '        tit = tit & myy

                '        Exit For
                '    End If
                'Next a

                If InStr(Myimage, "/") = 0 Or IsNothing(tit) = False Then
                    If Directory.Exists(dirname.Text) = False Then Directory.CreateDirectory(dirname.Text)
                    'If File.Exists(dirname.Text & "/" & tit & ".full.jpg") = False Then
                    Dim ErrorExists = False
                    Dim downloaded = False
                    Try
                        Dim Client As New WebClient
                        Dim myy = 1
                        Dim type As String = patron.Text
                        Dim Myname

                        If ispromo.Checked Then
                            Myname = dirname.Text & "/" & tit & myy & ".full" & patron.Text
                        Else
                            Myname = dirname.Text & "/" & tit & ".full" & patron.Text
                        End If

                        While File.Exists(Myname) = True
                            myy = myy + 1
                            Myname = dirname.Text & "/" & tit & myy & ".full" & patron.Text
                        End While
                        Client.DownloadFile(Myimage, Myname)
                        txlog.AppendText(Myimage & " " & tit & vbCrLf)
                        downloaded = True
                    Catch
                        ErrorExists = True
                    End Try
                    txlog.AppendText(Myimage & " " & tit & vbCrLf)
                    'End If
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

    Function Normalize(t) As String
        Normalize = t
    End Function

 Public Function Extract(ByRef strSource As String, ByRef strStart As String, ByRef strEnd As String,
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
        Extract = res
    End Function
    
    Function ReadWeb(MyUrl As String)
        MyUrl = Replace(MyUrl, "'", "")
        MyUrl = Replace(MyUrl, """", "")
        Dim res As String
        If MyUrl = "" Then Exit Function

        Dim request As WebRequest
        Try
            request = WebRequest.Create(MyUrl)
        Catch
            ReadWeb = ""
            Exit Function
        End Try

        Dim response As WebResponse
        Try
            response = request.GetResponse()
        Catch
            ReadWeb = ""
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
        ReadWeb = res
    End Function

    Function GetImage(mainurl) As String
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

        GetImage = res
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
            If InStr(n, "plains") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "forest") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "island") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "swamp") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "mountain") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
            If InStr(n, "wastes") > 0 Then File.Delete(dirname.Text & "/" & fi.Name)
        Next
    End Sub
End Class
