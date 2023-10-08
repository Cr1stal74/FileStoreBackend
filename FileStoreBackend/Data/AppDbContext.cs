using FileStoreBackend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStoreBackend.Data;

public class AppDbContext : DbContext
{
    public DbSet<FileModel> Files { get; set; }
    public DbSet<Chunk> Chunks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
        
    }
}
