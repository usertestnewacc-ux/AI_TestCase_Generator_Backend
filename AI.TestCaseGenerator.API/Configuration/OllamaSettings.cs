namespace AI.TestCaseGenerator.API.Configuration
{
    public class OllamaSettings
    {
        public bool Enabled { get; set; } = false;
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string ChatModel { get; set; } = "llama3.2";
        public string EmbeddingModel { get; set; } = "nomic-embed-text";
    }
}
