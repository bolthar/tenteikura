using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlAgilityPack;

namespace Tenteikura
{
    //This class's task is to keep track of what pages have been downloaded already.
    //The only discriminant is the page's absolute URI so the local cache 
    //needs to be cleared if stale pages are to be fetched again.
    //On startup, the cache loads from the filesystem all the pages 
    //already downloaded - so the process can be resumed seamlessly.
    public class Cache : IEnumerable<Page>
    {
        private IList<Page> _cache = new List<Page>();
        private object _cacheLock = new Object();

        public Cache(Uri startingUri, string targetDirectory)
        {
            String path = Path.Combine(targetDirectory, startingUri.Authority);
            if (Directory.Exists(path))
            {
                foreach (String linkFile in Directory.GetFiles(path, "*.link"))
                {
                    //loads the page's url
                    String document = File.ReadAllText(linkFile.Replace(".link", ""));
                    //loads the page's body
                    Uri uri = new Uri(File.ReadAllText(linkFile)); 
                    _cache.Add(new Page(document, uri));
                }
            }
        }

        //A lock is needed because concurrent modifying access is possible
        public void Add(Page page)
        {
            lock (_cacheLock)
            {
                _cache.Add(page);
            }
        }

        public Page Get(Uri uri)
        {
            lock (_cacheLock)
            {
                return _cache.FirstOrDefault(x => x.Uri.AbsoluteUri == uri.AbsoluteUri);
            }
        }

        public IEnumerator<Page> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
