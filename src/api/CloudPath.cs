using System;
using System.Collections.Generic;
using Microsoft.Azure.Storage.Blob;

namespace CodeFlip.CodeJar.Api
{
    public class CloudPath
    {
        public Uri CloudFile { get; private set; }

        public CloudPath(string cloudFile)
        {
            CloudFile = new Uri(cloudFile);
        }
        public List<Code> GenerateCodesFromCloudFile(long[] offset)
        {
            var cFile = new CloudBlockBlob(CloudFile);
            var codes = new List<Code>();

            for (var i = offset[0]; i < offset[1]; i += 4)
            {
                var bytes = new byte[4];
                cFile.DownloadRangeToByteArray(bytes, index: 0, blobOffset: i, length: 4);
                var seedValue = BitConverter.ToInt32(bytes, 0);
                var code = new Code()
                {
                    SeedValue = seedValue
                };
                codes.Add(code);
            }
            return codes;
        }
    }
}