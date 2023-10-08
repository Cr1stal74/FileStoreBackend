namespace FileStoreBackend.Data.Models;

public class FileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
    public bool IsInDatabase { get; set; }
    public bool IsDone { get; set; }
}
