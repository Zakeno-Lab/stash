using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using synapse.Models;

namespace synapse.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ClipboardItem> ClipboardItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var synapseDataFolder = Path.Combine(appDataFolder, "Synapse");
            Directory.CreateDirectory(synapseDataFolder); // Ensures the folder exists

            var dbPath = Path.Combine(synapseDataFolder, "synapse.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
} 