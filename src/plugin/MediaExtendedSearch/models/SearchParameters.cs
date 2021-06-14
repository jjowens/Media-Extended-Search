using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class SearchParameters
    {
        public List<SearchFilter> SearchFilters { get; set; }
        public string MediaSearchType { get; set; }
        public DateTime? CreatedDateStart { get; set; }
        public DateTime? CreatedDateEnd { get; set; }
        public DateTime? ModifiedDateStart { get; set; }
        public DateTime? ModifiedDateEnd { get; set; }
        public string SearcherTypeName { get; set; }
        public string SearcherIndexer { get; set; }
    }
}