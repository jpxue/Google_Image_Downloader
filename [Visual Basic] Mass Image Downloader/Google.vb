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

Imports System.Threading
Imports System.Net

Imports OpenQA.Selenium

Namespace Scraper
    Public Class GoogleSearch
        Inherits SeleniumBase

        Const MaxThreads = 69
        Private _Threads As Integer = 0

        Sub New(ByVal keyword As String, ByVal directory As String)
            MyBase.New(String.Concat("https://www.google.com/search?q=", keyword), keyword, directory)
        End Sub

        Private Sub GrabImageURLs(ByVal fromURL As String, ByVal pageNo As Integer)
            Try
                Dim html As String = New WebClient().DownloadString(fromURL)
                Dim _imgLinks As List(Of String) = FindImgURLsByParsing(html)

                If (_imgLinks.Count > 0) Then
                    Console.WriteLine("Scraped {0} image links from {1}", imgLinks.Count.ToString(), fromURL)
                    Console.Title = String.Format("Grabbing URLs: Google Page = {0}   |   Image-Links = {1}", pageNo.ToString(), imgLinks.Count.ToString())
                    imgLinks.AddRange(_imgLinks)
                End If

            Catch ex As Exception When TypeOf ex Is WebException OrElse TypeOf ex Is ArgumentNullException OrElse TypeOf ex Is NotSupportedException
                '
            Finally
                _Threads -= 1
            End Try
        End Sub

        Dim imgLinks As New List(Of String)
        ''' <summary>
        ''' Google Search -> Gets Links -> Visit Each Link -> Get Image Links -> Download Images via ThreadedDownloader
        ''' </summary>
        Public Sub GetImages()
            If (isDirOK = False) Then
                Console.WriteLine("{0} does NOT exist and could NOT be created...")
                Return
            End If

            If (_Driver Is Nothing) Then
                Console.WriteLine("Instance of Driver is NULL - {0}", Thread.CurrentThread.Name)
                Return
            End If


            imgLinks.Clear()
            Dim pageNo As UInteger = 1
            Dim newPageFound As Boolean = True

            While newPageFound
                'Get Links
                Console.ForegroundColor = ConsoleColor.Blue
                Console.WriteLine("Gettings links off Google Page {0}...", pageNo.ToString())
                Console.ForegroundColor = ConsoleColor.Gray

                Dim links As List(Of String) = FindURLsByParsing(_Driver.PageSource)
                If (links.Count < 1) Then
                    Return
                End If

                'Get Image URLs from each Link
                For Each googleResult As String In links
                    _Threads += 1
                    Dim thread = New Thread(Sub() Me.GrabImageURLs(googleResult, pageNo))
                    thread.IsBackground = True
                    thread.Name = String.Concat("URLGrabber#", _Threads.ToString())
                    thread.Start()
                Next

                Try
                    _Driver.FindElementById("pnnext").Click()
                    pageNo += 1
                    newPageFound = True
                Catch ex As Exception When TypeOf ex Is WebDriverException OrElse TypeOf ex Is NoSuchElementException
                    newPageFound = False
                End Try
            End While

            Thread.Sleep(5000)
            Console.BackgroundColor = ConsoleColor.Green
            Console.ForegroundColor = ConsoleColor.Blue
            Console.WriteLine("No more pages left!")
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.Gray

            'Download All Images
            Dim noOfThreads = MaxThreads

            Dim threads(noOfThreads) As Thread
            Console.WriteLine("Creating {0} Threads!!!", noOfThreads.ToString("#"))
            For i As Integer = 1 To noOfThreads
                Console.WriteLine("Starting Thread {0}.", i.ToString())

                Dim dl As New ThreadedDownloader(imgLinks, _Directory, _Keyword)
                Dim thread As New Thread(New ThreadStart(AddressOf dl.Download))
                thread.IsBackground = True
                thread.Name = String.Concat("ImageDownloader#", i.ToString())
                threads(i) = thread
            Next

            'Start Threads in one go (looks nicer when debugging lol)
            For i As Integer = 1 To noOfThreads
                threads(i).Start()
            Next

        End Sub

        Private Function isURLValid(ByVal url As String)
            If (url.StartsWith("http") <> True And url.StartsWith("www") <> True) Then
                Return False
            End If

            'Sites we don't want to visit
            If (url.Contains("google") Or url.Contains("youtube")) Then
                Return False
            End If

            Return True
        End Function

        Private Function isValidImgURL(ByVal url As String) As Boolean
            'URL has already been checked before hand for http or www protocols
            'Does not include all image formats obv, but most common ones
            If (url.EndsWith(".jpg") Or url.EndsWith(".png") Or url.EndsWith(".gif") _
                Or url.EndsWith(".bmp") Or url.EndsWith(".jpeg") Or url.EndsWith(".svg") _
                Or url.EndsWith(".tif") Or url.EndsWith(".tiff") Or url.EndsWith(".c4a") _
                Or url.EndsWith(".ppm") Or url.EndsWith(".pgm") Or url.EndsWith(".pbm") _
                Or url.EndsWith(".pnm") Or url.EndsWith(".bpg") Or url.EndsWith(".hdr") _
                Or url.EndsWith(".raw")) Then
                Return True
            End If

            Return False
        End Function

        Private Function FindURLsByParsing(ByVal src As String) As List(Of String)
            Dim urls As List(Of String) = Parse("href=""", """", src)
            If (urls.Count < 1) Then
                Return New List(Of String)
            End If

            Dim validURLs As New List(Of String)
            For i As Integer = 0 To urls.Count - 1
                If (isURLValid(urls(i)) = True) Then
                    'Console.WriteLine(urls(i))
                    validURLs.Add(urls(i))
                End If
            Next
            urls = Nothing
            Return validURLs
        End Function

        Private Function FindImgURLsByParsing(ByVal src As String) As List(Of String)
            Dim urls As List(Of String) = Parse("src=""", """", src)
            urls.AddRange(Parse("""image"":""", """", src))

            Dim validImgUrls As New List(Of String)
            For i As Integer = 0 To urls.Count - 1
                If (isValidImgURL(urls(i))) Then
                    validImgUrls.Add(urls(i))
                    'Console.WriteLine(urls(i))
                End If
            Next

            Return validImgUrls
        End Function

        'Twice as fast as doing this by Regex
        Private Function Parse(ByVal startID As String, ByVal endID As String, ByVal src As String) As List(Of String)
            Dim list As New List(Of String)

            Dim index As Integer = src.IndexOf(startID, 0)
            While index > -1
                Dim urlStart = index + startID.Length
                Dim urlEnd = src.IndexOf(endID, urlStart) - urlStart

                If (urlEnd > 0) Then
                    Dim str As String = src.Substring(urlStart, urlEnd)
                    If (list.Contains(str) = False) Then
                        list.Add(str)
                    End If
                End If

                index = src.IndexOf(startID, urlStart) 'next
            End While

            Return list
        End Function

        'Private _ahrefRegExp As String = "<?href\s*=\s*[""'].+?[""'][^>]*?"
        'Private _ahrefRegex As New Regex(_ahrefRegExp, RegexOptions.None)
        'Private Function FindURLsByRegex(ByVal html As String) As List(Of String)
        '    Dim urls As New List(Of String)

        '    For Each urlMatch As Match In _ahrefRegex.Matches(html)
        '        If urlMatch.Success Then
        '            Dim url As String = urlMatch.ToString()
        '            url = url.Substring(6, url.Length - 7)

        '            If (url.Contains("google") = False And (url.StartsWith("http") Or url.StartsWith("www"))) Then
        '                urls.Add(url)
        '                Console.WriteLine(url.ToString())
        '            End If
        '        End If
        '    Next

        '    Return urls
        'End Function


    End Class


    Public Class GoogleImages
        Inherits SeleniumBase

        Const ThreadDivider = 15 'The Higher this is THE LESS THREADS. 15 is perfect for 4 threaded CPU with decent internet

        Sub New(ByVal keyword As String, ByVal directory As String)
            MyBase.New(String.Concat("https://www.google.com/search?tbm=isch&source=hp&q=", keyword), keyword, directory)
        End Sub

        ''' <summary>
        ''' Google Images -> Scroll Down + Show More Button -> Get Links -> Download Images via ThreadedDownloader
        ''' </summary>
        Public Sub GetImages()
            If (isDirOK = False) Then
                Console.WriteLine("{0} does NOT exist and could NOT be created...")
                Return
            End If

            If (_Driver Is Nothing) Then
                Console.WriteLine("Instance of Driver is NULL - {0}", Thread.CurrentThread.Name)
                Return
            End If

            'Scroll all the way down so as to expose all images
            Console.WriteLine("Exposing all images for more results...")
            ShowAll()

            'Get the URLs
            Console.WriteLine("Getting image list...")
            Dim urls As List(Of String) = getImageLinks().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries).ToList()

            'Create the Threads
            If (urls.Count < (ThreadDivider * 2)) Then
                Console.WriteLine("Thread Divider is too large, make sure it is below {0}...", (urls.Count / 2).ToString())
                Return
            End If

            Dim noOfThreads = urls.Count / ThreadDivider

            Dim threads(noOfThreads) As Thread
            Console.WriteLine("Creating {0} Threads!!!", noOfThreads.ToString("#"))
            For i As Integer = 1 To noOfThreads
                Console.WriteLine("Starting Thread {0}.", i.ToString())

                Dim dl As New ThreadedDownloader(urls, _Directory, _Keyword)
                Dim thread As New Thread(New ThreadStart(AddressOf dl.Download))
                thread.IsBackground = True
                thread.Name = String.Concat("ImageDownloader#", i.ToString())
                threads(i) = thread
            Next

            'Start Threads in one go (looks nicer when debugging lol)
            For i As Integer = 1 To noOfThreads
                threads(i).Start()
            Next

        End Sub

        Private Function getImageLinks() As String
            If (_JS Is Nothing) Then
                Return String.Empty
            End If

            Dim o As Object = _JS.ExecuteScript("var imgs=document.getElementsByTagName(""a"");var i=0;var aray=new Array();var j=-1;var ret = """";while(++i<imgs.length){if(imgs[i].href.indexOf(""/imgres?imgurl=http"")>0){aray[++j]=decodeURIComponent(imgs[i].href).split(/=|%|&/)[1].split(""?imgref"")[0];ret = ret + aray[j] + ""\n"";}}return ret;")
            Return o.ToString()
        End Function

        'Sleeps is over prolonged and loop is excessive (just to be safe)
        Private Sub ShowAll()
            For i As Integer = 0 To 10
                _JS.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);")
                _JS.ExecuteScript("document.getElementById('smb').click();")
                Thread.Sleep(1000)
            Next
        End Sub

    End Class
End Namespace

