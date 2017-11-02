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

Module Module1

    Sub Main()
        Console.Title = "[VB.Net] Google Image Downloader"

        Dim keyword As String = "cats"
        Dim saveDirectory As String = "Images\"

        If (My.Application.CommandLineArgs.Count = 2) Then
            keyword = My.Application.CommandLineArgs(0)
            saveDirectory = My.Application.CommandLineArgs(1)
        Else
            Console.WriteLine("No arguments detected, using pre-compiled settings!")
        End If

        Dim scraper As New GoogleScraper(keyword, saveDirectory)
        scraper.GetImages()
        Console.ReadLine()

    End Sub

End Module
