using System;
using System.IO;

namespace Pipopolam.Net
{
    public class FileContent
    {
        public string FileName { get; set; }
        public Stream Stream { get; set; }

        public FileContent(string fileName, Stream stream)
        {
            FileName = fileName;
            Stream = stream;
        }
    }
}
