using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace synapse.Utils
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Replaces all items in the collection with new items in a single operation
        /// to minimize UI update notifications
        /// </summary>
        public static void ReplaceAll<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (newItems == null) throw new ArgumentNullException(nameof(newItems));

            var newItemsList = newItems as IList<T> ?? newItems.ToList();
            
            // If both are empty, nothing to do
            if (collection.Count == 0 && newItemsList.Count == 0)
                return;

            // Clear and add all items
            collection.Clear();
            
            foreach (var item in newItemsList)
            {
                collection.Add(item);
            }
        }
    }
}