using synapse.Models;
using System.Collections.Generic;

namespace synapse.Services.SearchStrategies
{
    public interface ISearchStrategy
    {
        string Name { get; }
        
        bool Matches(ClipboardItem item, string searchQuery);
        
        double CalculateScore(ClipboardItem item, string searchQuery);
        
        IEnumerable<TextMatch> GetMatches(string text, string searchQuery);
    }
}