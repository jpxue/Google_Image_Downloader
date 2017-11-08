# Google Image Downloader

A simple **multi-threaded** program which uses **Selenium/Headless-Chrome** to obtain images via **Google Images** or by means of a **Google Search**.

# How it Works
**Google Images** uses the Google Images search engine to obtain images.
**Google Search** scrapes links via a Google Search until the program runs out of pages to scrape. Each link is than scanned for images.

Scrapes **700-1000** images in **~40s** on my Core i5 (depends # of threads and internet speed).
This can be increased to **3000-4000* if you use the Google Search scraper.

![alt tag](https://raw.githubusercontent.com/jpxue/Google_Image_Downloader/master/app.png)

# How To Run

Run the application and follow the instructions.
Alternatively, you can also use it via the command prompt as follows:

###### ImageDownloader.exe <search_keyword> <save_directory> <use_google_images>

Example:
###### ImageDownloader.exe wallpaper C:\Users\JP\Desktop\OutputFolder\ yes

# Details
Threads can be modified by changing the 'ThreadDivider' constant for the GoogleImages Class or the _Threads variable for the GoogleSearch Class.

# Limitations
Google Images limits the amount of images shown for each search field to < 1000; I have tried searching for ways to overcome this but have found none.
In response to the latter I tried scraping links instead (via a normal Google Search) and getting images from those links. This lets me download < 4000 images which are nevertheless, not as accurate (again this number is limited by the amount of results/search pages Google allows you to see).