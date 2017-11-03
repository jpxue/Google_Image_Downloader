# Google Image Downloader

A simple **multi-threaded** program which uses **Selenium** to obtain images via **Google Images**.
Can easily be modified to include other search engines (see below).

Scrapes **700-1000** images in **~40s** on my Core i5 (depends # of threads and internet speed).

![alt tag](https://raw.githubusercontent.com/jpxue/Google_Image_Downloader/master/app.png)

# How To Run

You can input the search keyword that you would like to use as well as the save directory as variable and run the application.
Alternatively, you can also use it via the command prompt as follows:

###### ImageDownloader.exe <search_keyword> <save_directory>

Example:
###### ImageDownloader.exe wallpaper C:\Users\JP\Desktop\OutputFolder\

# Details
Threads can be modified by changing the 'ThreadDivider' constant in Google.

Google limits the amount of images shown to <1000; I have tried searching for ways to overcome this but have found none.

# Porting to Other Search Engines
In Google.vb:
Edit the BaseURL variable to your search engine query URL + POST data. 
Update the getImageLinks() function, make it search for href or img tags (Regex, HTMLAgilityPack or Selenium FindElements).
Lastly, comment out all the code in ShowAll() except for the scrolling down part.
