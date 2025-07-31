using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using synapse.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace synapse.Utils
{
    /// <summary>
    /// Attached property behavior for highlighting search terms in TextBlock content
    /// </summary>
    public static class TextHighlightBehavior
    {
        private static Brush HighlightBackground
        {
            get
            {
                var resource = Application.Current?.FindResource("AccentFillColorSecondaryBrush");
                if (resource is Brush brush)
                    return brush;
                return new SolidColorBrush(Color.FromRgb(0, 120, 215));
            }
        }

        private static Brush HighlightForeground => new SolidColorBrush(Colors.White);
        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.RegisterAttached(
                "HighlightText",
                typeof(string),
                typeof(TextHighlightBehavior),
                new PropertyMetadata(null, OnHighlightTextChanged));

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.RegisterAttached(
                "SearchTerm",
                typeof(string),
                typeof(TextHighlightBehavior),
                new PropertyMetadata(null, OnSearchTermChanged));

        public static string GetHighlightText(DependencyObject obj)
        {
            return (string)obj.GetValue(HighlightTextProperty);
        }

        public static void SetHighlightText(DependencyObject obj, string value)
        {
            obj.SetValue(HighlightTextProperty, value);
        }

        public static string GetSearchTerm(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchTermProperty);
        }

        public static void SetSearchTerm(DependencyObject obj, string value)
        {
            obj.SetValue(SearchTermProperty, value);
        }

        private static void OnHighlightTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                UpdateHighlighting(textBlock);
            }
        }

        private static void OnSearchTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                UpdateHighlighting(textBlock);
            }
        }


        private static void UpdateHighlighting(TextBlock textBlock)
        {
            // Null safety checks
            if (textBlock == null) return;
            
            var text = GetHighlightText(textBlock);
            var searchTerm = GetSearchTerm(textBlock);

            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(text))
                return;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                textBlock.Inlines.Add(new Run(text));
                return;
            }

            // Always start with exact matching for performance
            var processedSearchTerm = searchTerm.Trim();
            
            // Try exact match first
            var exactMatches = new List<(int start, int length)>();
            var searchIndex = 0;
            
            while ((searchIndex = text.IndexOf(processedSearchTerm, searchIndex, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                exactMatches.Add((searchIndex, processedSearchTerm.Length));
                searchIndex += processedSearchTerm.Length;
            }
            
            // If we found exact matches, use them
            if (exactMatches.Any())
            {
                HighlightRanges(textBlock, text, exactMatches);
                return;
            }
            
            // No exact matches - try word-based highlighting for fuzzy results
            var searchWords = processedSearchTerm.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var highlightRanges = new List<(int start, int length)>();

            // Find all matches for each search word
            foreach (var word in searchWords)
            {
                var index = 0;
                while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    highlightRanges.Add((index, word.Length));
                    index += word.Length;
                }
            }

            if (highlightRanges.Any())
            {
                // Sort and merge overlapping ranges
                highlightRanges = highlightRanges.OrderBy(r => r.start).ToList();
                var mergedRanges = MergeOverlappingRanges(highlightRanges);
                HighlightRanges(textBlock, text, mergedRanges);
            }
            else
            {
                // No matches found, just add the text
                textBlock.Inlines.Add(new Run(text));
            }
        }

        private static List<(int start, int length)> MergeOverlappingRanges(List<(int start, int length)> ranges)
        {
            var mergedRanges = new List<(int start, int length)>();
            
            foreach (var range in ranges)
            {
                if (mergedRanges.Count == 0 || mergedRanges.Last().start + mergedRanges.Last().length < range.start)
                {
                    // No overlap, add as new range
                    mergedRanges.Add(range);
                }
                else
                {
                    // Overlap detected, merge with last range
                    var last = mergedRanges[mergedRanges.Count - 1];
                    var newEnd = Math.Max(last.start + last.length, range.start + range.length);
                    mergedRanges[mergedRanges.Count - 1] = (last.start, newEnd - last.start);
                }
            }
            
            return mergedRanges;
        }

        private static void HighlightRanges(TextBlock textBlock, string text, List<(int start, int length)> ranges)
        {
            var currentIndex = 0;
            foreach (var (start, length) in ranges)
            {
                // Add text before the highlight
                if (start > currentIndex)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(currentIndex, start - currentIndex)));
                }

                // Add highlighted text
                var highlightedRun = new Run(text.Substring(start, length))
                {
                    FontWeight = FontWeights.Bold,
                    Background = HighlightBackground,
                    Foreground = HighlightForeground
                };
                textBlock.Inlines.Add(highlightedRun);

                currentIndex = start + length;
            }

            // Add any remaining text
            if (currentIndex < text.Length)
            {
                textBlock.Inlines.Add(new Run(text.Substring(currentIndex)));
            }
        }
    }
}