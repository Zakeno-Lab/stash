using synapse.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace synapse.Services
{
    public interface IDataService
    {
        Task AddClipboardItemAsync(ClipboardItem item);
        Task<List<ClipboardItem>> GetClipboardHistoryAsync();
    }
} 