using FuzzySharp;
using synapse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace synapse.Services.SearchStrategies
{
    public class WordTokenSearchStrategy : BaseSearchStrategy
    {
        private readonly int _minimumTokenScore;
        private static readonly Regex WordBoundaryRegex = new Regex(@"\b\w+\b", RegexOptions.Compiled);
        
        public WordTokenSearchStrategy() : this(new SearchConfiguration())
        {
        }
        
        public WordTokenSearchStrategy(SearchConfiguration configuration)
        {
            _minimumTokenScore = configuration?.TokenMinimumScore ?? 70;
        }
        
        public override string Name => "Word Token";
        
        public override double CalculateScore(ClipboardItem item, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return 1.0;
                
            var searchableContent = GetSearchableContent(item);
            
            if (string.IsNullOrWhiteSpace(searchableContent))
                return 0.0;
            
            // Tokenize both search query and content
            var queryTokens = TokenizeText(searchQuery.ToLower());
            var contentTokens = TokenizeText(searchableContent.ToLower());
            
            if (!queryTokens.Any() || !contentTokens.Any())
                return 0.0;
            
            // Calculate token-based scores
            var tokenScores = new List<double>();
            
            // Use FuzzySharp's token-based algorithms
            tokenScores.Add(Fuzz.TokenSortRatio(searchQuery, searchableContent) / 100.0);
            tokenScores.Add(Fuzz.TokenSetRatio(searchQuery, searchableContent) / 100.0);
            
            // Also check if all query tokens exist in content (with fuzzy matching)
            var allTokensMatched = true;
            var individualTokenScores = new List<double>();
            
            foreach (var queryToken in queryTokens)
            {
                var bestTokenScore = 0.0;
                foreach (var contentToken in contentTokens)
                {
                    var score = Fuzz.Ratio(queryToken, contentToken) / 100.0;
                    bestTokenScore = Math.Max(bestTokenScore, score);
                }
                
                individualTokenScores.Add(bestTokenScore);
                if (bestTokenScore < _minimumTokenScore / 100.0)
                {
                    allTokensMatched = false;
                }
            }
            
            // If all tokens matched well, boost the score
            if (allTokensMatched && individualTokenScores.Any())
            {
                tokenScores.Add(individualTokenScores.Average());
            }
            
            return tokenScores.Any() ? tokenScores.Max() : 0.0;
        }
        
        public override IEnumerable<TextMatch> GetMatches(string text, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchQuery))
                yield break;
            
            var queryTokens = TokenizeText(searchQuery.ToLower());
            var matches = new List<TextMatch>();
            
            // Find each token in the text
            foreach (var queryToken in queryTokens)
            {
                var wordMatches = WordBoundaryRegex.Matches(text);
                
                foreach (Match wordMatch in wordMatches)
                {
                    var word = wordMatch.Value;
                    var score = Fuzz.Ratio(queryToken, word.ToLower()) / 100.0;
                    
                    if (score >= _minimumTokenScore / 100.0)
                    {
                        matches.Add(new TextMatch(wordMatch.Index, wordMatch.Length, score));
                    }
                }
            }
            
            // Merge overlapping matches
            var mergedMatches = MergeOverlappingMatches(matches);
            foreach (var match in mergedMatches)
            {
                yield return match;
            }
        }
        
        private List<string> TokenizeText(string text)
        {
            return WordBoundaryRegex.Matches(text)
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(token => token.Length > 1) // Ignore single character tokens
                .Distinct()
                .ToList();
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
                    // Merge matches
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
            return _minimumTokenScore / 100.0;
        }
    }
}