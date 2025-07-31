using FuzzySharp;
using synapse.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace synapse.Services.SearchStrategies
{
    public class FuzzySearchStrategy : BaseSearchStrategy
    {
        private readonly int _minimumScore;
        private readonly int _highQualityScore;
        
        public FuzzySearchStrategy() : this(new SearchConfiguration())
        {
        }
        
        public FuzzySearchStrategy(SearchConfiguration configuration)
        {
            _minimumScore = configuration?.FuzzyMinimumScore ?? 60;
            _highQualityScore = configuration?.FuzzyHighQualityScore ?? 80;
        }
        
        public override string Name => "Fuzzy";
        
        public override double CalculateScore(ClipboardItem item, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return 1.0;
                
            var searchableContent = GetSearchableContent(item);
            
            if (string.IsNullOrWhiteSpace(searchableContent))
                return 0.0;
            
            // Optimized: Use only the most effective algorithms
            int bestScore = 0;
            
            // For short queries, use PartialRatio (best for substring matching)
            if (searchQuery.Length <= 20)
            {
                bestScore = Math.Max(bestScore, Fuzz.PartialRatio(searchQuery, searchableContent));
                
                // Check content separately for better accuracy
                if (!string.IsNullOrEmpty(item.Content) && item.Content.Length <= 1000)
                {
                    bestScore = Math.Max(bestScore, Fuzz.PartialRatio(searchQuery, item.Content));
                }
            }
            else
            {
                // For longer queries, use TokenSetRatio (handles word order variations)
                bestScore = Math.Max(bestScore, Fuzz.TokenSetRatio(searchQuery, searchableContent));
            }
            
            // Quick check on application name (exact match is common)
            if (!string.IsNullOrEmpty(item.SourceApplication))
            {
                if (item.SourceApplication.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bestScore = Math.Max(bestScore, 95); // Boost for exact substring match
                }
            }
            
            return bestScore / 100.0; // Normalize to 0-1 range
        }
        
        public override IEnumerable<TextMatch> GetMatches(string text, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchQuery))
                yield break;
            
            // Optimized: Simple approach for better performance
            var queryLower = searchQuery.ToLowerInvariant();
            var textLower = text.ToLowerInvariant();
            
            // First, try to find exact substring matches (fastest)
            int index = 0;
            while ((index = textLower.IndexOf(queryLower, index)) != -1)
            {
                yield return new TextMatch(index, searchQuery.Length, 1.0);
                index += searchQuery.Length;
            }
            
            // If no exact matches found, look for word-based fuzzy matches
            if (!textLower.Contains(queryLower))
            {
                var words = text.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                var queryWords = searchQuery.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                int currentPos = 0;
                foreach (var word in words)
                {
                    var wordIndex = text.IndexOf(word, currentPos, StringComparison.OrdinalIgnoreCase);
                    if (wordIndex >= 0)
                    {
                        currentPos = wordIndex + word.Length;
                        
                        // Only check first query word for performance
                        if (queryWords.Length > 0)
                        {
                            var score = Fuzz.Ratio(queryWords[0].ToLowerInvariant(), word.ToLowerInvariant());
                            if (score >= _highQualityScore)
                            {
                                yield return new TextMatch(wordIndex, word.Length, score / 100.0);
                            }
                        }
                    }
                }
            }
        }
        
        protected override double GetMinimumScoreThreshold()
        {
            return _minimumScore / 100.0;
        }
    }
}