using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace synapse.Models
{
    public class ClipboardItem
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; } // "Text" or "Image"
        public DateTime Timestamp { get; set; }
        public string SourceApplication { get; set; }
        public string? WindowTitle { get; set; }
        public string? ApplicationExecutablePath { get; set; }
        public int? WordCount { get; set; }
        public int? CharacterCount { get; set; }
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }
        public string? UrlDomain { get; set; }
        
        /// <summary>
        /// Search relevance score (not persisted to database)
        /// Used for sorting search results by relevance
        /// </summary>
        [NotMapped]
        public double SearchScore { get; set; } = 0.0;
        
        // Simple computed properties for display (pragmatic approach)
        // Grouping logic moved to DateGroupDescription to maintain MVVM principles
        
        /// <summary>
        /// Gets content formatted for display (removes line breaks)
        /// </summary>
        public string DisplayContent => Content.Replace("\r", " ").Replace("\n", " ").Trim();
        
        /// <summary>
        /// Gets the timestamp converted to local time and formatted for display
        /// </summary>
        public string LocalTimestamp => Timestamp.ToLocalTime().ToString("MMM d, yyyy h:mm tt");
        
        /// <summary>
        /// Gets the display title (window title if available, otherwise source application)
        /// </summary>
        public string DisplayTitle => !string.IsNullOrEmpty(WindowTitle) ? WindowTitle : SourceApplication;
        
        /// <summary>
        /// Gets formatted word count for display
        /// </summary>
        public string? FormattedWordCount
        {
            get
            {
                if (ContentType == "Text" && WordCount.HasValue)
                {
                    return $"{WordCount.Value:N0}";
                }
                return null;
            }
        }
        
        /// <summary>
        /// Gets formatted character count for display
        /// </summary>
        public string? FormattedCharacterCount
        {
            get
            {
                if (ContentType == "Text" && CharacterCount.HasValue)
                {
                    return $"{CharacterCount.Value:N0}";
                }
                return null;
            }
        }
        
        /// <summary>
        /// Gets formatted image dimensions for display
        /// </summary>
        public string? FormattedImageDimensions
        {
            get
            {
                if (ContentType == "Image" && ImageWidth.HasValue && ImageHeight.HasValue)
                {
                    return $"{ImageWidth.Value:N0} × {ImageHeight.Value:N0}";
                }
                return null;
            }
        }
        
        /// <summary>
        /// Gets the URL domain for display
        /// </summary>
        public string? FormattedUrlDomain
        {
            get
            {
                if (ContentType == "URL")
                {
                    return !string.IsNullOrEmpty(UrlDomain) ? UrlDomain : "Unknown Domain";
                }
                return null;
            }
        }
    }
} 