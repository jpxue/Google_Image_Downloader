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
Imports System.Threading

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

Friend Class ImageDownloader

    Shared _Threads As UInteger = 0
    Shared _URLs As List(Of String)
    Shared _Start As DateTime = DateTime.Now
    Private Max As UInteger

    Sub New(ByVal urls As List(Of String))
        _URLs = urls
        Max = _URLs.Count + 1
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

        filePath = filePath.Replace(".ash", ".jpg")

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
            wc.DownloadFile(url, String.Concat(GoogleScraper._Directory, fileName))
            SyncLock outputLock
                GoogleScraper._Scraped += 1
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
        Return String.Format("Images = {0}  |  Threads Running = {1}  |  Keyword = {2}", GoogleScraper._Scraped.ToString(), _Threads.ToString(), GoogleScraper._Keyword)
    End Function

    Public Sub Download()
        _Start = DateTime.Now
        _Threads += 1

        Dim url As String = GetOneURL()

        While (url <> String.Empty)
            DownloadImage(url)
            url = GetOneURL() 'Cycle

            Console.Title = getProgressString()
            Thread.Sleep(10)
        End While

        _Threads -= 1
        Console.Title = If(_Threads > 0, getProgressString(), String.Format("Scraped {0} Images!  |  Keyword = {1}", GoogleScraper._Scraped.ToString(), GoogleScraper._Keyword))
        'Console.WriteLine("{0} has terminated.", Thread.CurrentThread.Name)

        SyncLock outputLock
            If (_Threads <= 0) Then
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.WriteLine("Finished!!!")
                Console.WriteLine("Scraped a total of {0} images in {1}", GoogleScraper._Scraped.ToString(), GoogleScraper._Directory)
                Console.ForegroundColor = ConsoleColor.Gray

                Dim ts As TimeSpan = DateTime.Now - _Start
                Console.WriteLine("Download Time = {0}s", ts.TotalSeconds.ToString("#.##"))
                Console.ReadLine()
            End If
        End SyncLock
    End Sub
End Class

Public Class GoogleScraper

    Const ThreadDivider = 15 'The Higher this is THE LESS THREADS. 15 is perfect for 4 threaded CPU with decent internet
    Const TimeoutSeconds = 20

    Public Shared _Scraped As UInteger
    Public Shared _Directory As String
    Public Shared _Keyword As String

    Private _Driver As ChromeDriver
    Private _JS As IJavaScriptExecutor
    Private BaseURL As String = "https://www.google.com/search?tbm=isch&source=hp&biw=1920&bih=974&ei=O6L6WYaXAcirsAeVqZ-YDQ&q=" ' "https://www.bing.com/images/search?q="

    Sub New(ByVal keyword As String, ByVal dir As String)
        _Scraped = 1
        _Keyword = keyword
        _Directory = dir
        Initialize()

        If (_Directory.EndsWith("\") = False) Then
            _Directory = String.Concat(_Directory, "\")
        End If
        isDirectoryOK()
    End Sub

    Protected Overrides Sub Finalize()
        _Driver.Close()
        _Driver.Dispose()
        MyBase.Finalize()
    End Sub

    Private Function isDirectoryOK() As Boolean
        If (Directory.Exists(_Directory) = True) Then
            Return True
        End If

        Dim dirCreated As Boolean = False
        Try
            Directory.CreateDirectory(_Directory)
            Exit Try
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException _
            OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is PathTooLongException _
            OrElse TypeOf ex Is DirectoryNotFoundException OrElse TypeOf ex Is NotSupportedException
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("{0}: {1}", ex.GetType().ToString(), ex.Message)
            Console.ForegroundColor = ConsoleColor.Gray
            Exit Try
        Finally
            dirCreated = Directory.Exists(_Directory)
        End Try

        Return dirCreated
    End Function

    Public Sub Initialize()
        Console.WriteLine("Initializing Selenium...")
        Dim options As New ChromeOptions()
        Dim service As ChromeDriverService = ChromeDriverService.CreateDefaultService()
        options.AddArguments("--silent")

        service.HideCommandPromptWindow = True 'This Is what hides the initial messages actually
        service.SuppressInitialDiagnosticInformation = True

        _Driver = New ChromeDriver(service, options, TimeSpan.FromSeconds(TimeoutSeconds))
        _JS = DirectCast(_Driver, IJavaScriptExecutor)

        _Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMilliseconds(250)
        _Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(TimeoutSeconds)
        _Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(TimeoutSeconds)

        _Driver.Navigate().GoToUrl(String.Concat(BaseURL, _Keyword))
    End Sub


    Private Function getImageLinks() As String
        If (_JS Is Nothing) Then
            Return String.Empty
        End If

        Dim o As Object = _JS.ExecuteScript("var cont=document.getElementsByTagName(""body"")[0];var imgs=document.getElementsByTagName(""a"");var i=0;var divv= document.createElement(""div"");var aray=new Array();var j=-1;var ret = """";while(++i<imgs.length){if(imgs[i].href.indexOf(""/imgres?imgurl=http"")>0){aray[++j]=decodeURIComponent(imgs[i].href).split(/=|%|&/)[1].split(""?imgref"")[0];ret = ret + aray[j] + ""\n"";}}cont.insertBefore(divv,cont.childNodes[0]);return ret;")
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

    Public Sub GetImages()
        If (isDirectoryOK() = False) Then
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

            Dim dl As New ImageDownloader(urls)
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

End Class
