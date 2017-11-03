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

        Dim keyword As String
        Dim saveDirectory As String = "Images\"

        If (My.Application.CommandLineArgs.Count = 2) Then
            'Command Line Args
            keyword = My.Application.CommandLineArgs(0)
            saveDirectory = My.Application.CommandLineArgs(1)
        Else
            'User will Input Args instead
            Console.WriteLine("Input the keyword you would like to search:")
            keyword = Console.ReadLine()
            Console.WriteLine("Input the location where you would like to save the images:")
            saveDirectory = Console.ReadLine()
        End If

            Dim scraper As New GoogleScraper(keyword, saveDirectory)
        scraper.GetImages()
        Console.ReadLine()

    End Sub

End Module
