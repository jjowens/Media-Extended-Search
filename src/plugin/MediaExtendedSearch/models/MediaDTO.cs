using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class MediaDTO
    {
        public int NodeID { get; set; }
        public string Filename { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FileType { get; set; }
        public string FullURL { get; set; }
        public string FileExtension { get; set; }
        public string RelativePath { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int FileSize { get; set; }

        public IEnumerable<MediaProp> MediaProps { get; set; }
    }
}