using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenteikura.LinkFilters
{
    //Discards pages already in cache - these pages
    //should not be downloaded again.
    //This of course requires a cache clean 
    //to get rid of stale pages.
    public class CacheHitFilter : ILinkFilter
    {
        private Cache _cache;

        public CacheHitFilter(Cache cache)
        {
            _cache = cache;
        }

        public bool Matches(Uri link)
        {
            return _cache.Get(link) != null;
        }
    }
}
