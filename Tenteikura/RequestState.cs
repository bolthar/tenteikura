using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Tenteikura
{
    public class RequestState
    {
        public const int BUFFER_SIZE = 1024;

        public WebRequest Request { get; private set; }

        public StringBuilder RequestData;
        public byte[] BufferRead;
        public Stream ResponseStream;

        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public RequestState(WebRequest request)
        {
            BufferRead = new byte[BUFFER_SIZE];
            RequestData = new StringBuilder(String.Empty);
            Request = request;
            ResponseStream = null;
        }
    }
}
