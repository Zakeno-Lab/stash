using System;

namespace synapse.Utils
{
    /// <summary>
    /// Utility class for date-based grouping of clipboard items
    /// </summary>
    public static class DateGroupHelper
    {
        /// <summary>
        /// Gets the group header text for a given date
        /// </summary>
        public static string GetGroupHeader(DateTime date)
        {
            var today = DateTime.Today;
            var itemDate = date.ToLocalTime().Date; // Convert UTC to local time for comparison

            if (itemDate == today)
            {
                return "Today";
            }
            else if (itemDate == today.AddDays(-1))
            {
                return "Yesterday";
            }
            else
            {
                // For older dates, show month and year (use local time)
                return itemDate.ToString("MMMM yyyy");
            }
        }

        /// <summary>
        /// Gets the group date key for sorting and comparison
        /// </summary>
        public static DateTime GetGroupDate(DateTime date)
        {
            var today = DateTime.Today;
            var itemDate = date.ToLocalTime().Date; // Convert UTC to local time for comparison

            if (itemDate == today)
            {
                return today;
            }
            else if (itemDate == today.AddDays(-1))
            {
                return today.AddDays(-1);
            }
            else
            {
                // For older dates, use the first day of the month (local time)
                return new DateTime(itemDate.Year, itemDate.Month, 1);
            }
        }

        /// <summary>
        /// Determines if two dates belong to the same group
        /// </summary>
        public static bool AreInSameGroup(DateTime date1, DateTime date2)
        {
            return GetGroupDate(date1) == GetGroupDate(date2);
        }

        /// <summary>
        /// Gets the sort priority for a group (lower numbers appear first)
        /// </summary>
        public static int GetGroupSortPriority(DateTime groupDate)
        {
            var today = DateTime.Today;
            
            if (groupDate == today)
            {
                return 0; // Today comes first
            }
            else if (groupDate == today.AddDays(-1))
            {
                return 1; // Yesterday comes second
            }
            else
            {
                // Older dates: more recent months have lower priority numbers
                var monthsAgo = ((today.Year - groupDate.Year) * 12) + (today.Month - groupDate.Month);
                return 2 + monthsAgo;
            }
        }
    }
} 