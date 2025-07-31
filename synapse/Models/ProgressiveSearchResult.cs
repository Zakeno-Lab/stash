using System.Collections.Generic;
using synapse.Services;

namespace synapse.Models
{
    public class ProgressiveSearchResult
    {
        public SearchPhase Phase { get; set; }
        public List<SearchResult> Results { get; set; }
        public bool IsComplete { get; set; }
        public bool NeedsEnhancement { get; set; }

        public ProgressiveSearchResult()
        {
            Results = new List<SearchResult>();
        }
    }
}