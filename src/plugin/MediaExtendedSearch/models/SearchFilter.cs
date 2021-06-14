using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class SearchFilter
    {
        public string SearchParameter { get; set; }
        public string SearchValue { get; set; }
        public string BoolOperator { get; set; }
        public bool IsWildCard { get; set; }
        public bool IsRange { get; set; }
        public string RangeOperator { get; set; }

    }
}