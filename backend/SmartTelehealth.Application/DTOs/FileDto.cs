public class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = false;
} 