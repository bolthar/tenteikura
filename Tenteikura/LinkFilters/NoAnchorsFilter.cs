using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenteikura.LinkFilters
{
    //This filter discards anchor links
    public class NoAnchorsFilter : ILinkFilter
    {
        public bool Matches(Uri link)
        {
            return link.Segments.Count() != 0 && link.AbsoluteUri.Any(x => x == '#');
        }
    }
}
