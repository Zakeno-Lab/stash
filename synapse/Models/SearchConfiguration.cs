namespace synapse.Models
{
    public class SearchConfiguration
    {
        /// <summary>
        /// Minimum score (0-100) for fuzzy matches to be considered valid
        /// </summary>
        public int FuzzyMinimumScore { get; set; } = 60;
        
        /// <summary>
        /// Score threshold (0-100) for high-quality fuzzy matches
        /// </summary>
        public int FuzzyHighQualityScore { get; set; } = 80;
        
        /// <summary>
        /// Minimum score (0-100) for word token matches to be considered valid
        /// </summary>
        public int TokenMinimumScore { get; set; } = 70;
    }
}