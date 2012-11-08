using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenteikura
{
    //Interface for link filters.
    //Implementing objects represents conditions under which a
    //link should or should not be fetched.
    //Invalid urls are discarded earlier - so the discarded links 
    //are valid, but still not viable for download.
    //A bit overkill for such a simple system, but easily extendable.
    public interface ILinkFilter
    {
        bool Matches(Uri link);
    }
}
