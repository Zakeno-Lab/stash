using synapse.Models;
using System.Collections.Generic;

namespace synapse.Services
{
    public interface ISearchService
    {
        bool ExactMatchOnly { get; set; }
        
        bool MatchesSearch(ClipboardItem item, string searchQuery);
        
        IEnumerable<SearchResult> Search(IEnumerable<ClipboardItem> items, string searchQuery);
        
        IEnumerable<TextMatch> GetTextMatches(string text, string searchQuery);
    }
    
    public class SearchResult
    {
        public ClipboardItem Item { get; set; }
        public double Score { get; set; }
        public IEnumerable<TextMatch> Matches { get; set; }
        
        public SearchResult(ClipboardItem item, double score, IEnumerable<TextMatch>? matches = null)
        {
            Item = item;
            Score = score;
            Matches = matches ?? new List<TextMatch>();
        }
    }
    
    public class TextMatch
    {
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public double Score { get; set; }
        
        public TextMatch(int startIndex, int length, double score = 1.0)
        {
            StartIndex = startIndex;
            Length = length;
            Score = score;
        }
    }
}