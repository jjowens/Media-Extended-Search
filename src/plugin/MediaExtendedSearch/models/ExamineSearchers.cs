using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace My.MediaExtendedSearch.Models
{
    public class ExamineSearchers
    {
        public IEnumerable<ExamineSearcher> Searchers { get; set; }

        public int TotalSearchers { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public bool NoAvailableSearchers { get; set; }

    }
}