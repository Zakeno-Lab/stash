using synapse.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace synapse.Services.SearchStrategies
{
    public abstract class BaseSearchStrategy : ISearchStrategy
    {
        public abstract string Name { get; }
        
        public virtual bool Matches(ClipboardItem item, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return true;
                
            var score = CalculateScore(item, searchQuery);
            return score > GetMinimumScoreThreshold();
        }
        
        public abstract double CalculateScore(ClipboardItem item, string searchQuery);
        
        public abstract IEnumerable<TextMatch> GetMatches(string text, string searchQuery);
        
        protected virtual double GetMinimumScoreThreshold()
        {
            return 0.0;
        }
        
        protected string GetSearchableContent(ClipboardItem item)
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(item.Content))
                parts.Add(item.Content);
                
            if (!string.IsNullOrEmpty(item.SourceApplication))
                parts.Add(item.SourceApplication);
                
            if (!string.IsNullOrEmpty(item.WindowTitle))
                parts.Add(item.WindowTitle);
                
            if (!string.IsNullOrEmpty(item.ContentType))
                parts.Add(item.ContentType);
                
            return string.Join(" ", parts);
        }
    }
}