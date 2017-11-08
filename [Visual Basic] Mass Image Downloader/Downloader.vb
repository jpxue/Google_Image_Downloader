' [VB.Net] Image Downloader
' Copyright (C) 2017 Juan Xuereb
' 
' This program Is free software: you can redistribute it And/Or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, Or
' (at your option) any later version.
' 
' This program Is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' You should have received a copy of the GNU General Public License
' along with this program.  If Not, see <http://www.gnu.org/licenses/>.

Imports System.IO
Imports System.Net

Class ThreadedDownloader

    Shared _Threads As UInteger = 0
    Shared _URLs As New List(Of String)
    Shared _Start As DateTime = DateTime.Now
    Private Max As UInteger

    Private _Directory As String
    Private _Keyword As String
    Public Shared _Scraped As UInteger

    ''' <summary>
    ''' For GoogleImage Class (Implementation: Assign list by reference then run threads all @ once)
    ''' </summary>
    Sub New(ByVal urls As List(Of String), ByVal saveDir As String, ByVal keyword As String)
        _URLs = urls 'ref
        Max = _URLs.Count + 1

        _Scraped = 1
        _Directory = saveDir
        _Keyword = keyword
    End Sub

    ''' <summary>
    ''' For GoogleSearch Class (Implementation: Add URLs on the fly)
    ''' </summary>
    Sub New(ByVal url As String, ByVal saveDir As String, ByVal keyword As String)
        _URLs.Add(url)
        Max = _URLs.Count + 1

        _Directory = saveDir
        _Keyword = keyword
    End Sub

    Private getUrlLock As New Object
    Private Function GetOneURL() As String
        Dim url As String = String.Empty

        SyncLock getUrlLock
            If (_URLs.Count > 0) Then
                If (_URLs(0) Is Nothing) Then
                    _URLs.RemoveAt(0)
                    Return String.Empty
                Else
                    url = String.Copy(_URLs(0))
                    _URLs.RemoveAt(0)
                End If
            End If
        End SyncLock

        Return url
    End Function

    Private Function LegalizePath(ByVal filePath As String) As String
        Dim invalidPathChars() As Char = Path.GetInvalidFileNameChars()
        For Each c As Char In invalidPathChars
            filePath = filePath.Replace(c, "")
        Next

        filePath = filePath.Replace(".ash", ".jpg") 'don't like .ash files in desktop

        Dim dotIndex = filePath.LastIndexOf(".")
        If (dotIndex > 0) Then
            If (dotIndex + 4 < filePath.Length) Then
                filePath = filePath.Substring(0, dotIndex + 4)
            End If
        End If

        Return String.Concat((Max - _URLs.Count).ToString(), "_", filePath.ToLower())
    End Function

    Private Function isExtensionValid(ByVal file As String) As Boolean
        If (file.EndsWith(".jpg") Or file.EndsWith(".bmp") Or file.EndsWith(".png") _
                Or file.EndsWith(".gif") Or file.EndsWith(".tif") Or file.EndsWith(".ash")) Then
            Return True
        End If
        Return False
    End Function

    Private outputLock As New Object
    Private Sub DownloadImage(ByVal url As String)
        If (Uri.IsWellFormedUriString(url, UriKind.Absolute) = False) Then
            Return
        End If

        Dim fileName As String = LegalizePath(Path.GetFileName(url))

        If (isExtensionValid(fileName) = False) Then
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine(url)
            Console.ForegroundColor = ConsoleColor.Gray
            fileName = String.Concat(fileName, ".jpg")
        End If

        Dim wc As New WebClient()
        Try
            wc.DownloadFile(url, String.Concat(_Directory, fileName))
            SyncLock outputLock
                _Scraped += 1
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine(fileName)
                Console.ForegroundColor = ConsoleColor.Gray
            End SyncLock
            Exit Try
        Catch ex As Exception When TypeOf ex Is WebException OrElse TypeOf ex Is ArgumentNullException OrElse TypeOf ex Is NotSupportedException

            Exit Try
        Finally
            wc.Dispose()
        End Try
    End Sub

    Private Function getProgressString() As String
        Return String.Format("Images = {0}  |  Threads Running = {1}  |  Keyword = {2}", _Scraped.ToString(), _Threads.ToString(), _Keyword)
    End Function

    Public Sub Download()
        _Start = DateTime.Now
        _Threads += 1

        Dim url As String = GetOneURL()

        While (url <> String.Empty)
            DownloadImage(url)
            url = GetOneURL() 'Cycle

            Console.Title = getProgressString()
            'Thread.Sleep(10)
        End While

        _Threads -= 1
        Console.Title = If(_Threads > 0, getProgressString(), String.Format("Scraped {0} Images!  |  Keyword = {1}", _Scraped.ToString(), _Keyword))
        'Console.WriteLine("{0} has terminated.", Thread.CurrentThread.Name)

        SyncLock outputLock
            If (_Threads <= 0) Then
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.WriteLine("Finished!!!")
                Console.WriteLine("Scraped a total of {0} images in {1}", _Scraped.ToString(), _Directory)
                Console.ForegroundColor = ConsoleColor.Gray

                Dim ts As TimeSpan = DateTime.Now - _Start
                Console.WriteLine("Download Time = {0}s", ts.TotalSeconds.ToString("#.##"))
                Console.ReadLine()
            End If
        End SyncLock
    End Sub
End Class