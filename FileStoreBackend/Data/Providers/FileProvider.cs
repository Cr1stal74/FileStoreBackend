using FileStoreBackend.Data.Models;

namespace FileStoreBackend.Data.Providers;

public class FileProvider
{
    private IConfiguration _configuration;

    public FileProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Stream GetDataStream(FileModel file)
    {
        var path = Path.Combine(_configuration.GetValue<string>("DatafilesFolder"), file.Id.ToString("N"));
        return new FileStream(path, FileMode.Open);
    }

    public Stream CreateDataStream(FileModel file)
    {
        var path = Path.Combine(_configuration.GetValue<string>("DatafilesFolder"), file.Id.ToString("N"));
        return new FileStream(path, FileMode.Create);
    }
}
