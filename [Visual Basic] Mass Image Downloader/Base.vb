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

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

Public Class SeleniumBase

    Const Headless = True
    Const TimeoutSeconds = 20

    Protected _Driver As ChromeDriver
    Protected _JS As IJavaScriptExecutor

    Public _Directory As String
    Public _Keyword As String

    Protected isDirOK As Boolean

    Public Sub New(ByVal url As String, ByVal keyword As String, ByVal directory As String)
        _Keyword = keyword
        _Directory = directory

        If (_Directory.EndsWith("\") = False) Then
            _Directory = String.Concat(_Directory, "\")
        End If
        isDirOK = isDirectoryOK()

        Console.WriteLine("Initializing {0}...", If(Headless, "Headless Chrome (Selenium)", "Chrome (Selenium)"))
        Dim options As New ChromeOptions()
        Dim service As ChromeDriverService = ChromeDriverService.CreateDefaultService()
        If (Headless) Then
            options.AddArguments("--headless")
        End If

        service.HideCommandPromptWindow = True 'This Is what hides the initial messages actually
        service.SuppressInitialDiagnosticInformation = True

        _Driver = New ChromeDriver(service, options, TimeSpan.FromSeconds(TimeoutSeconds))
        _JS = DirectCast(_Driver, IJavaScriptExecutor)

        _Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMilliseconds(250)
        _Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(TimeoutSeconds)
        _Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1)

        _Driver.Navigate().GoToUrl(url)
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

End Class