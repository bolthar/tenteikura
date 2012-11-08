using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tenteikura
{
    public class Fetcher
    {
        public Uri Uri { get; private set; }
        //Flag used by the dispatcher thread (on FetcherQueue)
        //to identify completed fetchers (and dispatch their
        //result appropriately).
        public bool Completed { get; private set; }
        public Page DownloadedPage { get; private set; }

        public Fetcher(Uri targetUri)
        {
            Uri = targetUri;
        }

        //The fetch operation is assumed to be slow, so it runs on a separate thread.
        //The callback is invoked from the thread when a page is loaded.
        public void Fetch()
        {
            new Task(DownloadPage).Start();
        }

        private void DownloadPage()
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(Uri);
                WebResponse response = request.GetResponse();
                using (StreamReader streamReader =
                    new StreamReader(response.GetResponseStream()))
                {
                    DownloadedPage = new Page(streamReader.ReadToEnd(), Uri);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Network or protocol error : {0}", ex.Message);
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("URI format not supported : {0}", ex.Message);
            }
            finally
            {
                //The completed flag is set true no matter what the 
                //outcome, so the dispatcher thread can dispose of
                //the fetcher appropriately.
                Completed = true;
            }
        }
    }
}
