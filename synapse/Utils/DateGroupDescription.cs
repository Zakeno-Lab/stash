using System;
using System.ComponentModel;
using System.Globalization;
using synapse.Models;

namespace synapse.Utils
{
    /// <summary>
    /// Custom GroupDescription for date-based grouping of clipboard items
    /// </summary>
    public class DateGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is ClipboardItem clipboardItem)
            {
                // Calculate grouping properties directly instead of relying on model properties
                var groupHeader = DateGroupHelper.GetGroupHeader(clipboardItem.Timestamp);
                var groupDate = DateGroupHelper.GetGroupDate(clipboardItem.Timestamp);
                var sortPriority = DateGroupHelper.GetGroupSortPriority(groupDate);
                
                // Return a custom group key that includes both header and sort priority
                return new DateGroupKey(groupHeader, sortPriority);
            }
            return null;
        }
    }

    /// <summary>
    /// Custom group key that allows proper sorting of date groups
    /// </summary>
    public class DateGroupKey : IComparable<DateGroupKey>
    {
        public string Header { get; }
        public int SortPriority { get; }

        public DateGroupKey(string header, int sortPriority)
        {
            Header = header;
            SortPriority = sortPriority;
        }

        public int CompareTo(DateGroupKey other)
        {
            if (other == null) return 1;
            return SortPriority.CompareTo(other.SortPriority);
        }

        public override string ToString()
        {
            return Header;
        }

        public override bool Equals(object obj)
        {
            return obj is DateGroupKey other && Header == other.Header && SortPriority == other.SortPriority;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Header, SortPriority);
        }
    }
} 