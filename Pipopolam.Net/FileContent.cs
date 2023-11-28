using System;
using System.IO;

namespace Pipopolam.Net
{
    public class FileContent
    {
        public string FileName { get; }
        public Stream Stream { get; }

        public FileContent(string fileName, Stream stream)
        {
            FileName = fileName;
            Stream = stream;
        }
    }
}
