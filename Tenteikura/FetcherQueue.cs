using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tenteikura
{
    //This acts as a rudimentary thread pool with a fixed limit
    //of running threads.
    public class FetcherQueue
    {
        //Stores all the fetchers to be executed
        private IList<Fetcher> _queue = new List<Fetcher>();
        //Stores all the currently executing fetchers
        private IList<Fetcher> _executing = new List<Fetcher>();
        //The callback that is called on successful page download
        private Action<Page> _callback;
        private Object _lockObject = new Object();

        //This event is called when all the processing is over - see below
        public event Action ProcessingOver;

        private const int MAX_CONCURRENT_THREADS = 8;
        private const int DISPATCHER_THREAD_STACK_SIZE = 64 * 1024 * 1024; //64 MB

        //The queue takes a callback as a parameter, which
        //is invoked any time a page is successfully downloaded.
        public FetcherQueue(Action<Page> OnPageLoaded)
        {
            _callback = OnPageLoaded;
        }

        //This method starts the processing. At least one fetcher should
        //be already in queue before calling this method, otherwise 
        //the ProcessingOver event is fired immediately.
        public void Process()
        {
            //Two threads are left spinning indefinitely - this one 
            //takes care of moving fetchers from the job queue to 
            //the executing queue and starting the fetchers.
            ThreadStart enqueuerThread = () =>
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if (_queue.Any() && _executing.Count() < MAX_CONCURRENT_THREADS)
                        {
                            Fetcher nextFetcher = _queue.First();
                            _queue.Remove(nextFetcher);
                            _executing.Add(nextFetcher);
                            nextFetcher.Fetch();
                        }
                    }
                }
            };
            //This thread checks which ones of the executing fetchers
            //have completed. It then invokes the approriate callback.
            ThreadStart dispatcherThread = () =>
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if(_executing.Any(x => x.Completed))
                        {
                            Fetcher completedFetcher = _executing.First();
                            if(completedFetcher.DownloadedPage != null)
                            {
                                OnCompleted(completedFetcher);
                            } else 
                            {
                                RemoveFetcher(completedFetcher);
                            }
                        }
                    }
                }
            };
            new Thread(enqueuerThread).Start();
            //The dispatcher thread's stack, due to the callback, can
            //grow quite a lot - giving the thread stack more space 
            //(default is 1Mb) is not exactly elegant but effective in
            //this instance. Running the queue on more than one 
            //system (as hypotized in the document) should mitigate
            //the issue as well.
            new Thread(dispatcherThread, DISPATCHER_THREAD_STACK_SIZE).Start();
        }

        public void Enqueue(Fetcher fetcher)
        {
            lock (_lockObject)
            {
                //If a fetcher with the same url is already in queue it is not 
                //enqueued - it is assumed that two pages with the same 
                //url are the same page, so no need to download them twice.
                if (_queue.All(x => x.Uri.AbsoluteUri != fetcher.Uri.AbsoluteUri))
                {
                    _queue.Add(fetcher);
                }
            }
        }

        //Callback invoked on successful completion.
        private void OnCompleted(Fetcher fetcher)
        {
            RemoveFetcher(fetcher);
            _callback(fetcher.DownloadedPage);
            //If, after invoking the parent's callback, no jobs are in queue
            //and no jobs are executing it means that the crawling is over,
            //so the ProcessingOver event is fired.
            if (!_queue.Any() && !_executing.Any() && ProcessingOver != null)
            {
                ProcessingOver();
            }
        }

        //Callback invoked on error - it just removes the 
        //fetcher silently.
        private void RemoveFetcher(Fetcher fetcher)
        {
            lock (_lockObject)
            {
                _executing.Remove(fetcher);
            }
        }
    }
}
