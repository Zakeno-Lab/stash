using synapse.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace synapse.Services.SearchStrategies
{
    public class ExactSearchStrategy : BaseSearchStrategy
    {
        public override string Name => "Exact";
        
        public override double CalculateScore(ClipboardItem item, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return 1.0;
                
            var searchableContent = GetSearchableContent(item);
            
            if (searchableContent.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                return 1.0;
                
            return 0.0;
        }
        
        public override IEnumerable<TextMatch> GetMatches(string text, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchQuery))
                yield break;
                
            var pattern = Regex.Escape(searchQuery);
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            
            foreach (Match match in regex.Matches(text))
            {
                yield return new TextMatch(match.Index, match.Length, 1.0);
            }
        }
        
        protected override double GetMinimumScoreThreshold()
        {
            return 0.5;
        }
    }
}