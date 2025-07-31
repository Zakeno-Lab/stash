using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace synapse.Models
{
    /// <summary>
    /// Represents a group of clipboard items organized by date
    /// </summary>
    public partial class ClipboardGroup : ObservableObject
    {
        /// <summary>
        /// The display name for this group (e.g., "Today", "Yesterday", "July 2025")
        /// </summary>
        [ObservableProperty]
        private string _header;

        /// <summary>
        /// The date key used for grouping (for sorting and comparison)
        /// </summary>
        public DateTime GroupDate { get; }

        /// <summary>
        /// The collection of clipboard items in this group
        /// </summary>
        public ObservableCollection<ClipboardItem> Items { get; }

        /// <summary>
        /// Indicates whether this group has any items
        /// </summary>
        public bool HasItems => Items.Count > 0;

        public ClipboardGroup(string header, DateTime groupDate)
        {
            Header = header;
            GroupDate = groupDate;
            Items = new ObservableCollection<ClipboardItem>();
            
            // Subscribe to collection changes to update HasItems
            Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasItems));
        }

        /// <summary>
        /// Adds an item to this group, maintaining chronological order (newest first)
        /// </summary>
        public void AddItem(ClipboardItem item)
        {
            // Insert at the correct position to maintain chronological order
            var insertIndex = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                if (item.Timestamp > Items[i].Timestamp)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = i + 1;
            }
            
            Items.Insert(insertIndex, item);
        }

        /// <summary>
        /// Removes an item from this group
        /// </summary>
        public bool RemoveItem(ClipboardItem item)
        {
            return Items.Remove(item);
        }
    }
} 