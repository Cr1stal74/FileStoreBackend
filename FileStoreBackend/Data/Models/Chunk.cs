using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreBackend.Data.Models;

[Table("chunks")]
public class Chunk
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("file_id")]
    public Guid FileId { get; set; }

    [Column("length")]
    public int Length { get; set; }

    [Column("index")]
    public long Index { get; set; }

    [Column("data", TypeName = "bytea")]
    public byte[] Data { get; set; }
}
