using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class SearchResultsDetails
    {
        public int TotalMediaItems { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public string Query { get; set; }
        public IEnumerable<string> ListOfAuthors {get;set;}
        public bool NoResultsFound { get; set; }
    }
}