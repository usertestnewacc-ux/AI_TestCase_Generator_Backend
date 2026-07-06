namespace AI.TestCaseGenerator.API.DTOs.Export
{
    public class ExportFileDto
    {
        /// <summary>
        /// File content as byte array.
        /// </summary>
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Name of the exported file.
        /// Example: Login_TestCases.xlsx
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME type.
        /// Excel:
        /// application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        ///
        /// PDF:
        /// application/pdf
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize => FileBytes.LongLength;
    }
}