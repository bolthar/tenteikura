using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Security.Cryptography;

namespace Tenteikura
{
    public class Page
    {
        public HtmlDocument Document { get; private set; }
        public Uri Uri { get; private set; }
        public String Hash { get; private set; }

        public Page(String document, Uri uri)
        {
            Document = new HtmlDocument();
            //gets an MD5 Hash of the downloaded document's absolute URI,
            //which will then used for persistence
            Hash = BitConverter.ToString(
                    MD5.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(uri.AbsoluteUri)
                        )
                    ).Replace("-", "");
            Document.LoadHtml(document);
            Uri = uri;
        }

        public String HTML
        {
            get
            {
                return Document.DocumentNode.InnerHtml;
            }
        }
        
        public String Title
        {
            //Various guards are needed, the document could be in an invalid state.
            get
            {
                if (Document.DocumentNode == null) return String.Empty;
                var titleSelector = Document.DocumentNode.SelectNodes("//title");
                if (titleSelector == null) return String.Empty;
                var titleNode = titleSelector.FirstOrDefault();
                return titleNode != null ? titleNode.InnerText : String.Empty;
            }
        }

        //this returns all the urls on the page contained on <a> tags.
        //Malformed urls are silently discarded.
        public IEnumerable<Uri> Links
        {
            get
            {
                //Get all <a> tags on the page
                var linkNodes = Document.DocumentNode.SelectNodes("//a");
                if (linkNodes == null) return new List<Uri>();
                return linkNodes
                    //Extract the "href" attribute from all the tags, 
                    //defaulting to "#" if no attribute is found on tag
                    .Select(x => x.GetAttributeValue("href", "#"))
                    //Exclude all the malformed urls
                    .Where(x => System.Uri.IsWellFormedUriString(x, UriKind.RelativeOrAbsolute))
                    //Map all strings to the correspondent Uri
                    .Select(x => GetUriFromHref(x));
            }
        }

        private Uri GetUriFromHref(String href)
        {
            Uri uri = new Uri(href, UriKind.RelativeOrAbsolute);
            //If the uri is already absolute, return it
            if (uri.IsAbsoluteUri) return uri;
            //otherwise, make it into an absolute uri based on this page's Uri
            return new Uri(
                String.Format(
                    "{0}://{1}/{2}", 
                    Uri.Scheme, Uri.Authority.TrimEnd('/'), href.Trim('/'))
                ); 
            //NOTE: this seems to be actually the easier way to
            //extract the base URI from a URI in .NET
        }
    }
}
