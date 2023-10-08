using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreBackend.Data.Models;

[Table("files")]
public class FileModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name", TypeName = "varchar(50)")]
    public string Name { get; set; }

    [Column("is_in_database")]
    public bool IsInDatabase { get; set; }

    [Column("is_done")]
    public bool IsDone { get; set; }
}
