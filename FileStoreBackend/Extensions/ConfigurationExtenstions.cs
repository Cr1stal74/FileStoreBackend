using System.Text.RegularExpressions;

namespace FileStoreBackend.Extensions;

public static class ConfigurationExtenstions
{
    private static readonly Regex _chunkSizeRegex = new Regex(@"([\d]+)([a-zA-Z]+)", RegexOptions.Compiled);

    public static int GetChunkSize(this IConfiguration configuration)
    {
        var sizeStr = configuration.GetValue<string>("ChunkSize");
        var result = _chunkSizeRegex.Match(sizeStr);

        int count = int.Parse(result.Groups[1].Value);
        var unit = result.Groups[2].Value;

        return unit switch
        {
            "B" => count,
            "KB" => count * 1024,
            "MB" => count * 1024 * 1024,
            _ => 64 * 1024 // 64KB
        };
    }
}
