using Microsoft.EntityFrameworkCore;
using synapse.Data;
using synapse.Models;
using System.Threading.Tasks;

namespace synapse.Services
{
    public class DataService : IDataService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DataService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddClipboardItemAsync(ClipboardItem item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.ClipboardItems.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task<List<ClipboardItem>> GetClipboardHistoryAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ClipboardItems
                .OrderByDescending(item => item.Timestamp)
                .ToListAsync();
        }
    }
} 