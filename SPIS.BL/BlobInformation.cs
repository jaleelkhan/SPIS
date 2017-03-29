using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SPIS.BL
{
    public class BlobInformation
    {
        public Uri BlobUri { get; set; }

        public string BlobName
        {
            get
            {
                return BlobUri.Segments[BlobUri.Segments.Length - 1];
            }
        }
        public string BlobNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(BlobName);
            }
        }
        public int AdId { get; set; }
    }
}
