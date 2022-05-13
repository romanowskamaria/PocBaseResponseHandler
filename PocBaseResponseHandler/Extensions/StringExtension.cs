namespace PocBaseResponseHandler.Extensions;

using System.IO.Compression;
using System.Text;

public static class StringExtension
{
    public static byte[] ToZipFile(this string self)
    {
        using var archiveMemoryStream = new MemoryStream();

        using (var zipArchive = new ZipArchive(archiveMemoryStream, ZipArchiveMode.Create, true))
        {
            var binaryPayload = Encoding.UTF8.GetBytes(self);
            var zipEntry = zipArchive.CreateEntry("file");

            using var zipEntryStream = zipEntry.Open();
            zipEntryStream.Write(binaryPayload, 0, binaryPayload.Length);
        }

        var result = archiveMemoryStream.ToArray();

        return result;
    }
}
