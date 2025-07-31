using synapse.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace synapse.Services.SearchStrategies
{
    public class HybridSearchStrategy : BaseSearchStrategy
    {
        private readonly ExactSearchStrategy _exactStrategy;
        private readonly FuzzySearchStrategy _fuzzyStrategy;
        private readonly WordTokenSearchStrategy _tokenStrategy;
        private readonly SearchConfiguration _configuration;

        public HybridSearchStrategy() : this(new SearchConfiguration())
        {
        }

        public HybridSearchStrategy(SearchConfiguration configuration)
        {
            _configuration = configuration;
            _exactStrategy = new ExactSearchStrategy();
            _fuzzyStrategy = new FuzzySearchStrategy(configuration);
            _tokenStrategy = new WordTokenSearchStrategy(configuration);
        }

        public override string Name => "Hybrid";

        public override double CalculateScore(ClipboardItem item, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return 1.0;

            // Calculate scores from all strategies
            var exactScore = _exactStrategy.CalculateScore(item, searchQuery);
            var fuzzyScore = _fuzzyStrategy.CalculateScore(item, searchQuery);
            var tokenScore = _tokenStrategy.CalculateScore(item, searchQuery);

            // Prioritize exact matches
            if (exactScore > 0)
            {
                return 1.0; // Exact matches always get top score
            }

            // For non-exact matches, use weighted combination
            // Fuzzy and token scores are already normalized (0-1)
            var combinedScore = Math.Max(fuzzyScore * 0.9, tokenScore * 0.85);

            return combinedScore;
        }

        public override IEnumerable<TextMatch> GetMatches(string text, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchQuery))
                yield break;

            // Try exact matches first
            var exactMatches = _exactStrategy.GetMatches(text, searchQuery).ToList();
            if (exactMatches.Any())
            {
                foreach (var match in exactMatches)
                {
                    yield return match;
                }
                yield break;
            }

            // If no exact matches, combine fuzzy and token matches
            var allMatches = new List<TextMatch>();
            
            allMatches.AddRange(_fuzzyStrategy.GetMatches(text, searchQuery));
            allMatches.AddRange(_tokenStrategy.GetMatches(text, searchQuery));

            // Remove duplicates and overlapping matches
            var uniqueMatches = MergeOverlappingMatches(allMatches);
            foreach (var match in uniqueMatches)
            {
                yield return match;
            }
        }

        private IEnumerable<TextMatch> MergeOverlappingMatches(List<TextMatch> matches)
        {
            if (!matches.Any())
                yield break;

            // Sort by start index
            var sortedMatches = matches.OrderBy(m => m.StartIndex).ToList();

            var currentMatch = sortedMatches[0];

            for (int i = 1; i < sortedMatches.Count; i++)
            {
                var nextMatch = sortedMatches[i];

                // Check if matches overlap
                if (currentMatch.StartIndex + currentMatch.Length >= nextMatch.StartIndex)
                {
                    // Merge matches, keeping the best score
                    var endIndex = Math.Max(
                        currentMatch.StartIndex + currentMatch.Length,
                        nextMatch.StartIndex + nextMatch.Length
                    );

                    currentMatch = new TextMatch(
                        currentMatch.StartIndex,
                        endIndex - currentMatch.StartIndex,
                        Math.Max(currentMatch.Score, nextMatch.Score)
                    );
                }
                else
                {
                    // No overlap, yield current match and move to next
                    yield return currentMatch;
                    currentMatch = nextMatch;
                }
            }

            // Don't forget the last match
            yield return currentMatch;
        }

        protected override double GetMinimumScoreThreshold()
        {
            // Use a lower threshold since we're combining strategies
            return 0.3;
        }
    }
}