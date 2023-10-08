using FileStoreBackend.Data.Models;
using FileStoreBackend.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileStoreBackend.Data.Providers;

public class PostgresProvider
{
    private AppDbContext _context;
    private IConfiguration _configuration;

    public PostgresProvider(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    #region FileModel

    public async Task<FileModel> GetFileModelAsync(Guid id)
    {
        return await _context.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<FileDto>> GetAllFileModelsAsync()
    {
        var files = await _context.Files.AsNoTracking().ToListAsync();
        return files.Select(x => new FileDto
        {
            Id = x.Id,
            Name = x.Name,
            Size = x.IsInDatabase ? GetOverallLength(x) : (new FileInfo(Path.Combine(_configuration.GetValue<string>("DatafilesFolder"), x.Id.ToString("N")))).Length,
            IsInDatabase = x.IsInDatabase,
            IsDone = x.IsDone
        }).ToList();
    }

    public async Task AddFileModelAsync(FileModel file)
    {
        await _context.Files.AddAsync(file);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFileModelAsync(FileModel file)
    {
        _context.Files.Update(file);
        await _context.SaveChangesAsync();
    }

    #endregion
    
    #region Chunks

    public Chunk GetChunk(FileModel file, int index)
    {
        return _context.Chunks.FirstOrDefault(x => x.FileId == file.Id && x.Index == index);
    }

    public async Task AddChunkAsync(Chunk chunk)
    {
        await _context.Chunks.AddAsync(chunk);
    }

    public long GetOverallLength(FileModel file)
    {
        return _context.Chunks.Where(x => x.FileId == file.Id).Sum(x => (long)x.Length);
    }

    public int GetChunksCount(FileModel file)
    {
        return _context.Chunks.Where(x => x.FileId == file.Id).Count();
    }

    public async Task AddFileAsync(FileModel file, Stream stream)
    {
        await AddFileModelAsync(file);

        var chunkSize = _configuration.GetChunkSize();
        var buffer = new byte[chunkSize];
        int index = 0;

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, chunkSize)) > 0)
        {
            var data = new byte[bytesRead];
            Array.Copy(buffer, 0, data, 0, bytesRead);
            var chunk = new Chunk
            {
                Id = Guid.NewGuid(),
                FileId = file.Id,
                Index = index++,
                Length = bytesRead,
                Data = data
            };
            await AddChunkAsync(chunk);
            if(index % 10 == 0)
            {
                await _context.SaveChangesAsync();
            }
        }
        file.IsDone = true;
        await _context.SaveChangesAsync();
    }

    #endregion
}
