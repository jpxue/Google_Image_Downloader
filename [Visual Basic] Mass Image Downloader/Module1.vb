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
        Dim useGoogleImages As Boolean

        If (My.Application.CommandLineArgs.Count = 3) Then
            'Command Line Args
            keyword = My.Application.CommandLineArgs(0)
            saveDirectory = My.Application.CommandLineArgs(1)
            useGoogleImages = My.Application.CommandLineArgs(2).ToLower().StartsWith("y")
        Else
            'User will Input Args instead
            Console.WriteLine("Input the keyword you would like to search:")
            keyword = Console.ReadLine()

            Console.WriteLine("Input the location where you would like to save the images:")
            saveDirectory = Console.ReadLine()

            Console.WriteLine()
            Console.WriteLine("Would you like to use Google Images (Y/N):")
            Console.WriteLine("Yes = Google Images -> Download Images")
            Console.WriteLine("No = Google Search -> Scrape Links -> Download Images")

            Dim yesno As String = Console.ReadLine.ToLower()
            If (yesno.StartsWith("y") <> True And yesno.StartsWith("n") <> True) Then
                Console.WriteLine("'{0}' was NOT recognized as neither yes nor a no... Restarting!", yesno)
                Console.WriteLine()
                Main()
            End If
            useGoogleImages = yesno.StartsWith("y")
        End If

        Console.WriteLine()
        If (useGoogleImages) Then
            Console.WriteLine("Scraping via Google Images Directly!")
            Dim scraper As New Scraper.GoogleImages(keyword, saveDirectory)
            scraper.GetImages()
        Else
            Console.WriteLine("Scraping via a Google Link search followed by an Image search!")
            Dim scraper As New Scraper.GoogleSearch(keyword, saveDirectory)
            scraper.GetImages()
        End If

        Console.ReadLine()
    End Sub

End Module
