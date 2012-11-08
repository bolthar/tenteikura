using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenteikura.Example
{
    class Program
    {
        //This is a basic example of how to use the crawler
        //In case of a cache miss, it prints out the page's title and 
        //absolute URI, and saves the page data to the filesystem.
        public static void Main(String[] args)
        {
            if ((args.Length == 2 || args.Length == 3) &&
                Uri.IsWellFormedUriString(args[0], UriKind.Absolute))
            {
                Uri startingUri = new Uri(args[0]);
                String targetDirectoryPath = args[1];
                bool followExternal = 
                    args.Length == 3 && args[2] == "--follow-external"; 
                Console.WriteLine("Loading from cache...");
                Cache cache = new Cache(startingUri, targetDirectoryPath);
                Console.WriteLine(
                    "Cache loaded - {0} pages stored in cache", cache.Count());
                Crawler crawler = new Crawler(cache, followExternal);
                Persister persister = new Persister(targetDirectoryPath, startingUri);
                //This event is fired when the crawler's process is over
                crawler.WorkComplete += () =>
                {
                    Environment.Exit(0);
                };
                //This event is fired every time a valid page is downloaded 
                crawler.NewPageFetched += (page) =>
                {
                    Console.WriteLine(page.Title + " - " + page.Uri.AbsoluteUri);
                    persister.Save(page);
                };
                crawler.Crawl(startingUri);
                Console.WriteLine("Crawler started, press CTRL+C to interrupt");
                while (true) { }
            }
            else
            {
                Console.WriteLine("Crawler");
                Console.WriteLine("Usage:");
                Console.WriteLine(
                    "Tenteikura.Example.exe <starting_uri> <target_directory> [--options]");
                Console.WriteLine(
                    "<starting_uri> : a valid absolute URL which will be the starting point for the crawler");
                Console.WriteLine(
                    "<target_directory> : the directory where the page files will be saved");
                Console.WriteLine("");
                Console.WriteLine("OPTIONS:");
                Console.WriteLine(
                    "The only option available is --follow-external, which will make the crawler fetch non-local urls as well");
                Console.WriteLine("EXAMPLE: ");
                Console.WriteLine(
                    @"Tenteikura.Example.exe http://telenor.com C:\mytargetdirectory --follow-external");
            }
        }
    }
}
