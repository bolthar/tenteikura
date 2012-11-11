using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;

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

        //The fetch operation is assumed to be slow, so it runs asynchronously.
        public void Fetch()
        {            
            RequestState state = new RequestState(HttpWebRequest.Create(Uri));
            state.Request.BeginGetResponse(OnGetResponse, state);
        }

        //http://msdn.microsoft.com/en-us/library/86wf6409%28v=vs.71%29.aspx
        private void OnStreamRead(IAsyncResult result)
        {
            RequestState state = result.AsyncState as RequestState;
            // Retrieve the ResponseStream that was set in RespCallback. 
            Stream responseStream = state.ResponseStream;

            // Read rs.BufferRead to verify that it contains data. 
            int read = responseStream.EndRead(result);
            if (read > 0)
            {
                // Prepare a Char array buffer for converting to Unicode.
                Char[] charBuffer = new Char[RequestState.BUFFER_SIZE * 2];

                // Convert byte stream to Char array and then to String.
                // len contains the number of characters converted to Unicode.
                int len =
                   state.StreamDecode.GetChars(state.BufferRead, 0, read, charBuffer, 0);

                String str = new String(charBuffer, 0, len);

                // Append the recently read data to the RequestData stringbuilder
                // object contained in RequestState.
                state.RequestData.Append(str);

                // Continue reading data until 
                // responseStream.EndRead returns –1.
                IAsyncResult ar = responseStream.BeginRead(
                   state.BufferRead, 0, RequestState.BUFFER_SIZE,
                   new AsyncCallback(OnStreamRead), state);
            }
            else
            {
                if (state.RequestData.Length > 0)
                {
                    DownloadedPage = new Page(state.RequestData.ToString(), Uri);
                }
                Completed = true;
                // Close down the response stream.
                responseStream.Close();
            }
        }

        private void OnGetResponse(IAsyncResult result)
        {
            try
            {
                RequestState state = result.AsyncState as RequestState;
                HttpWebRequest originalRequest = state.Request as HttpWebRequest;
                HttpWebResponse response = originalRequest.EndGetResponse(result) as HttpWebResponse;
                var stream = response.GetResponseStream();
                state.ResponseStream = stream;
                stream.BeginRead(state.BufferRead, 0, RequestState.BUFFER_SIZE, OnStreamRead, state);
            }
            catch (WebException ex)
            {
                Console.WriteLine("Network or protocol error : {0}", ex.Message);
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("Uri format not supported : {0}", ex.Message);
            }
        }
    }
}
