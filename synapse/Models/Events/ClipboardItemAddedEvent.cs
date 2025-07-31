using System;

namespace synapse.Models.Events
{
    /// <summary>
    /// Event raised when a new clipboard item is added to the history
    /// </summary>
    public class ClipboardItemAddedEvent
    {
        /// <summary>
        /// The clipboard item that was added
        /// </summary>
        public ClipboardItem Item { get; }

        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        public DateTime EventTimestamp { get; }

        public ClipboardItemAddedEvent(ClipboardItem item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            EventTimestamp = DateTime.UtcNow;
        }
    }
} 