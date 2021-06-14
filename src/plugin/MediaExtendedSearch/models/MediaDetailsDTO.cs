using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class MediaDetailsDTO
    {
        public IEnumerable<MediaDTO> MediaItems { get; set; }
        //public IEnumerable<StatDTO> StatItems { get; set; }
        public SearchResultsDetails Details { get; set; }

    }
}