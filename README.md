# Google Image Downloader

A simple multi-threaded program which uses Selenium to obtain images via Google Images.
Can easily be modified to include other search engines.
Scrapes 700-1000 images in ~40s on my Core i5 (depends # of threads and internet speed).

# How To Run

You can input the search keyword that you would like to use as well as the save directory as variable and run the application.
Alternatively, you can also use it via the command prompt as follows:

"ImageDownloader.exe <search_keyword> <save_directory>"

Example:
ImageDownloader.exe wallpaper C:\Users\JP\Desktop\OutputFolder\

# Details
Threads can be modified by changing the 'ThreadDivider' constant in Google.vb
Google limits the amount of images shown to <1000. I have tried searching for ways to overcome this but have found none yet :(