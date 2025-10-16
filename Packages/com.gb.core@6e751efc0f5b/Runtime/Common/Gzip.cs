using System.IO.Compression;
using System.IO;
using System;
using System.Text;

namespace GB
{
    public static class Gzip
    {
        public static string Compression(string str)
        {
            var rowData = Encoding.UTF8.GetBytes(str);
            byte[] compressed = null;
            using (var outStream = new MemoryStream())
            {
                using (var hgs = new GZipStream(outStream, CompressionMode.Compress))
                {
                    //outStream에 압축을 시킨다.
                    hgs.Write(rowData, 0, rowData.Length);
                }
                compressed = outStream.ToArray();
            }

            return Convert.ToBase64String(compressed);
        }
        public static string DeCompression(string compressedStr)
        {
            string output = null;
            byte[] cmpData = Convert.FromBase64String(compressedStr);
            using (var decomStream = new MemoryStream(cmpData))
            {
                using (var hgs = new GZipStream(decomStream, CompressionMode.Decompress))
                {
                    //decomStream에 압축 헤제된 데이타를 저장한다.
                    using (var reader = new StreamReader(hgs))
                    {
                        output = reader.ReadToEnd();
                    }
                }
            }

            return output;
        }


    }
}