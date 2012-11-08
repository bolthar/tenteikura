using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenteikura.LinkFilters
{
    //Discards links which are not 
    //on the starting url's domain.
    public class NoExternalLinks : ILinkFilter
    {
        private Uri _uri;

        public NoExternalLinks(Uri startingUri)
        {
            _uri = startingUri;
        }

        public bool Matches(Uri link)
        {
            return link.Authority != _uri.Authority;
        }
    }
}
