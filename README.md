# Tenteikura

A minimal C# multithreaded web crawler

## Usage

From an user's point of view, what is needed to start the crawler is
a call to the <tt>#crawl</tt> method on a <tt>Crawler</tt> instance. <tt>Crawler</tt>'s constructor 
takes a <tt>Cache</tt> instance as a parameter, which in turn requires a starting URL 
and a target directory to be instantiated.
```csharp
    String targetDirectory = @"C:\tenteikura_cache";
    Uri startingURL        = new Uri("http://www.andreadallera.com");
    Cache cache            = new Cache(startingURL, targetDirectory);
    Crawler crawler        = new Crawler(cache);
    crawler.Crawl(startingURL); //starts the crawler at http://www.andreadallera.com

<tt>Crawler</tt>'s constructor takes an optional parameter (bool, default <tt>false</tt>) which, if <tt>true</tt>, instructs the 
crawler to fetch pages outside the starting URI's domain or not:
```csharp
    new Crawler(cache, true);  //will follow urls outside the starting URI's domain
    new Crawler(cache, false); //will fetch only pages inside the starting URI's domain
    new Crawler(cache);        //same as above

This will only keep the downloaded pages in the <tt>Cache</tt> object, which is an
<tt>IEnumerable<Page></tt>:
```csharp
    foreach(Page page in cache) 
    {
        Console.WriteLine(page.Title);  //page title
        Console.WriteLine(page.HTML);   //page full HTML
        Console.WriteLine(page.Uri);    //page URI object
        Console.WriteLine(page.Hash);   //an hash of the URI's AbsoluteUri
        foreach(Uri link in page.Links) 
        {
            //the page has a IEnumerable<Uri> which contains all the links found on the page itself
            Console.WriteLine(link.AbsoluteUri);
        }
    }

<tt>Crawler</tt> exposes two events - <tt>NewPageFetched</tt> and <tt>WorkComplete</tt>:
```csharp
    //fired when a valid page not in cache is downloaded    
    crawler.NewPageFetched += (page) {
        //do something with the fetched page
    };
    //fired when the crawler has no more pages left to fetch
    crawler.WorkComplete += () {
        //shut down the application, or forward to the GUI, or whatever
    };

If you want to persist the fetched pages, a very rudimental file system 
backed storage option is available, via the <tt>Persister</tt> class:
```csharp
    Persister persister = new Persister(targetDirectory, startingURL);
    crawler.NewPageFetched += (page) {
        persister.save(page);
    };

<tt>Persister</tt> will save the page, in a subdirectory of <tt>targetDirectory</tt>
named after <tt>startingURL.Authority</tt>, as two files: one file, with filename <tt>page.Hash + ".link"</tt>, 
contains the page's absolute URI and the other, with filename <tt>page.Hash</tt>, 
contains the page itself in full.

There is an example console application on Tenteikura.Example.

## TO DO

There's an hard dependency between <tt>Cache</tt> and <tt>Persister</tt> at the moment: <tt>Cache</tt> expects 
pages from the <tt>targetDirectory + startingUri.Authority</tt> path to be in the same format as the 
ones saved from <tt>Persister</tt>, while the loading strategy should be injected (and ideally 
provided by <tt>Persister</tt> itself).

<tt>Persister</tt> should use a more effective storage strategy - maybe backed by a 
RDMS or a documental storage.

The pages are fetched in random order, so there is no traversal priority strategy of any kind.
