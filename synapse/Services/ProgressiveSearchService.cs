using synapse.Models;
using synapse.Services.SearchStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace synapse.Services
{
    public class ProgressiveSearchService : IProgressiveSearchService
    {
        private readonly ExactSearchStrategy _exactStrategy;
        private readonly HybridSearchStrategy _hybridStrategy;
        private readonly SearchConfiguration _configuration;

        public ProgressiveSearchService() : this(new SearchConfiguration())
        {
        }

        public ProgressiveSearchService(SearchConfiguration configuration)
        {
            _configuration = configuration ?? new SearchConfiguration();
            _exactStrategy = new ExactSearchStrategy();
            _hybridStrategy = new HybridSearchStrategy(_configuration);
        }

        public async Task<ProgressiveSearchResult> SearchPhase1Async(
            IEnumerable<ClipboardItem> items,
            string searchQuery,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var results = new List<SearchResult>();
                
                // Process quickly with exact matching
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (_exactStrategy.Matches(item, searchQuery))
                    {
                        var score = _exactStrategy.CalculateScore(item, searchQuery);
                        var matches = _exactStrategy.GetMatches(item.Content ?? "", searchQuery).ToList();
                        results.Add(new SearchResult(item, score, matches));
                    }
                }

                var sortedResults = results.OrderByDescending(r => r.Score).ToList();
                
                return new ProgressiveSearchResult
                {
                    Phase = SearchPhase.Exact,
                    Results = sortedResults,
                    IsComplete = !ShouldEnhanceResults(sortedResults, searchQuery),
                    NeedsEnhancement = ShouldEnhanceResults(sortedResults, searchQuery)
                };
            }, cancellationToken);
        }

        public async Task<ProgressiveSearchResult> SearchPhase2Async(
            IEnumerable<ClipboardItem> items,
            string searchQuery,
            List<SearchResult> phase1Results,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var phase1ItemIds = new HashSet<int>(phase1Results.Select(r => r.Item.Id));
                var enhancedResults = new List<SearchResult>();

                // Only process items not already in phase 1 results
                foreach (var item in items.Where(i => !phase1ItemIds.Contains(i.Id)))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_hybridStrategy.Matches(item, searchQuery))
                    {
                        var score = _hybridStrategy.CalculateScore(item, searchQuery);
                        
                        // Reduce score slightly for fuzzy matches to prioritize exact matches
                        score *= 0.85;
                        
                        var matches = _hybridStrategy.GetMatches(item.Content ?? "", searchQuery).ToList();
                        enhancedResults.Add(new SearchResult(item, score, matches));
                    }
                }

                // Combine results - exact matches first, then enhanced
                var allResults = phase1Results.Concat(enhancedResults)
                    .OrderByDescending(r => r.Score)
                    .ToList();

                return new ProgressiveSearchResult
                {
                    Phase = SearchPhase.Enhanced,
                    Results = allResults,
                    IsComplete = true,
                    NeedsEnhancement = false
                };
            }, cancellationToken);
        }

        public bool ShouldEnhanceResults(List<SearchResult> exactResults, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            // Don't enhance for very short queries
            if (query.Trim().Length < 3)
                return false;

            // Don't enhance if we have enough good results
            if (exactResults.Count >= 5)
                return false;

            // Enhance if we have few or no results
            if (exactResults.Count <= 2)
                return true;

            // Enhance if query has multiple words (token search helps)
            if (query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                return true;

            return false;
        }
    }
}