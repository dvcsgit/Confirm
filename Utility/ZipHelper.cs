using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Utility
{
    public class ZipHelper
    {
        public static void CompressFolder(string FileName, ZipOutputStream zipStream)
        {
            var fileInfo = new FileInfo(FileName);

            var entry = new ZipEntry(fileInfo.Name);

            entry.DateTime = fileInfo.LastWriteTime; // Note the zip format stores 2 second granularity

            entry.Size = fileInfo.Length;

            zipStream.PutNextEntry(entry);

            byte[] buffer = new byte[4096];

            using (FileStream fs = System.IO.File.OpenRead(FileName))
            {
                StreamUtils.Copy(fs, zipStream, buffer);
            }

            zipStream.CloseEntry();
        }
    }
}
