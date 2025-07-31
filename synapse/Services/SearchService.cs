using synapse.Models;
using synapse.Services.SearchStrategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace synapse.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchStrategy _hybridStrategy;
        private readonly ISearchStrategy _exactStrategy;
        private readonly SearchConfiguration _configuration;
        
        public SearchService() : this(new SearchConfiguration())
        {
        }
        
        public SearchService(SearchConfiguration configuration)
        {
            _configuration = configuration ?? new SearchConfiguration();
            
            _hybridStrategy = new HybridSearchStrategy(_configuration);
            _exactStrategy = new ExactSearchStrategy();
        }
        
        public bool ExactMatchOnly { get; set; } = true; // Default to exact search for performance
        
        public bool MatchesSearch(ClipboardItem item, string searchQuery)
        {
            if (item == null)
                return false;
            
            var (processedQuery, useExactMatch) = ProcessSearchQuery(searchQuery);
            var strategy = useExactMatch ? _exactStrategy : GetCurrentStrategy();
            return strategy.Matches(item, processedQuery);
        }
        
        public IEnumerable<SearchResult> Search(IEnumerable<ClipboardItem> items, string searchQuery)
        {
            if (items == null)
                yield break;
            
            var (processedQuery, useExactMatch) = ProcessSearchQuery(searchQuery);
            var strategy = useExactMatch ? _exactStrategy : GetCurrentStrategy();
            
            var results = new List<SearchResult>();
            
            foreach (var item in items)
            {
                var score = strategy.CalculateScore(item, processedQuery);
                
                if (strategy.Matches(item, processedQuery))
                {
                    var matches = new List<TextMatch>();
                    
                    // Get matches from content
                    if (!string.IsNullOrEmpty(item.Content))
                    {
                        matches.AddRange(strategy.GetMatches(item.Content, processedQuery));
                    }
                    
                    results.Add(new SearchResult(item, score, matches));
                }
            }
            
            // Sort by score descending for better UX
            foreach (var result in results.OrderByDescending(r => r.Score))
            {
                yield return result;
            }
        }
        
        public IEnumerable<TextMatch> GetTextMatches(string text, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchQuery))
                return Enumerable.Empty<TextMatch>();
            
            var (processedQuery, useExactMatch) = ProcessSearchQuery(searchQuery);
            var strategy = useExactMatch ? _exactStrategy : GetCurrentStrategy();
            return strategy.GetMatches(text, processedQuery);
        }
        
        private ISearchStrategy GetCurrentStrategy()
        {
            return ExactMatchOnly ? _exactStrategy : _hybridStrategy;
        }
        
        private (string processedQuery, bool useExactMatch) ProcessSearchQuery(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return (searchQuery, false);
            
            searchQuery = searchQuery.Trim();
            
            // Check if query is wrapped in quotes
            if (searchQuery.Length >= 2 && 
                searchQuery.StartsWith("\"") && 
                searchQuery.EndsWith("\""))
            {
                // Remove quotes and use exact match
                var unquoted = searchQuery.Substring(1, searchQuery.Length - 2);
                return (unquoted, true);
            }
            
            return (searchQuery, false);
        }
    }
}