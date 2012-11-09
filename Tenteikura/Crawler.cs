using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenteikura.LinkFilters;
using System.Threading;

namespace Tenteikura
{
    public class Crawler
    {
        public event Action<Page> NewPageFetched;
        public event Action WorkComplete;

        private Cache _cache;
        private IList<ILinkFilter> _linkFilters = new List<ILinkFilter>();
        private bool _followExternalLinks;
        private FetcherQueue _queue;

        //The crawler takes a cache as parameter. The cache can be 
        //either empty (starting url never crawled) or already full 
        //with pages previously crawled - this is determined by the
        //content of the target directory.
        public Crawler(Cache cache, bool followExternalLinks)
        {
            _queue = new FetcherQueue(OnPageLoaded);
            _queue.ProcessingOver += () =>
            {
                if (WorkComplete != null)
                {
                    WorkComplete();
                }
            };
            _cache = cache;
            _followExternalLinks = followExternalLinks;
        }

        public Crawler(Cache cache)
            : this(cache, false)
        {
        }

        public void Crawl(Uri url)
        {
            _linkFilters = new List<ILinkFilter>()
            {
                new NoAnchorsFilter(),
                new NoMailToFilter(),
                new CacheHitFilter(_cache),
            };
            //Could be done with subclassing 
            //(class NoExternalLinkCrawler : Crawler) 
            //but adds way more complexity for what it is worth.
            if (!_followExternalLinks) _linkFilters.Add(new NoExternalLinks(url));
            var fetcher = new Fetcher(url);
            //adds the starting url to the queue (even if already downloaded)
            _queue.Enqueue(fetcher);
            foreach (Page cachedPage in _cache)
            {
                //Each page already in cache is supplied to the OnPageLoaded 
                //method, so their links can be processed. This basically 
                //resumes (with a bit of overhead) the process where it
                //was stopped.
                OnPageLoaded(cachedPage);
            }
            _queue.Process();
        }

        //Every time a new page is fetched, if not in cache,
        //an event is raised. The event is listened 
        //by the main program, which decides what to do based on context.
        //Every URL on the page which is not excluded by any 
        //of the filtering criteria is then added to the queue, and round it goes.
        public void OnPageLoaded(Page page)
        {
            if (NewPageFetched != null && _cache.Get(page.Uri) == null)
            {
                _cache.Add(page);
                NewPageFetched(page);
            }
            foreach (Uri link in page.Links
                .Where(x => _linkFilters.All(y => !y.Matches(x))))
            {
                _queue.Enqueue(new Fetcher(link));
            }
        }
    }
}
