namespace AI.TestCaseGenerator.API.DTOs.Document
{
    public class DocumentDownloadDto
    {
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream";
    }
}