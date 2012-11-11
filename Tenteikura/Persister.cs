using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tenteikura
{
    //This class acts as a rudimentary file system backed storage.
    public class Persister
    {
        private DirectoryInfo _targetFolder;

        private object _lockObject = new object();
        public Persister(String documentFolder, Uri startingUrl)
        {
            //if the targetFolder is not present, it is automatically created.
            _targetFolder = Directory.CreateDirectory(
                Path.Combine(documentFolder, startingUrl.Authority));
        }

        //When a page is saved, two files are created on the filesystem,
        //in the appropriate folder:
        //a <pagehash>.link file, which contains one line only - the 
        //absolute URI of the page and a <pagehash> file, containing the
        //actual body of the page.
        //The write operation is locked in order to avoid 
        //IO problems with possible concurrent writes
        public void Save(Page page)
        {
            String path = Path.Combine(_targetFolder.FullName, page.Hash);
            lock (_lockObject)
            {
                using (var writer = new StreamWriter(path + ".link"))
                {
                    writer.WriteLine(page.Uri.AbsoluteUri);
                }
                page.Document.Save(Path.Combine(_targetFolder.FullName, page.Hash));
            }
        }
    }
}
