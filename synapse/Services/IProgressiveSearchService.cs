using synapse.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace synapse.Services
{
    public interface IProgressiveSearchService
    {
        Task<ProgressiveSearchResult> SearchPhase1Async(
            IEnumerable<ClipboardItem> items, 
            string searchQuery,
            CancellationToken cancellationToken);

        Task<ProgressiveSearchResult> SearchPhase2Async(
            IEnumerable<ClipboardItem> items, 
            string searchQuery,
            List<SearchResult> phase1Results,
            CancellationToken cancellationToken);

        bool ShouldEnhanceResults(List<SearchResult> exactResults, string query);
    }
}