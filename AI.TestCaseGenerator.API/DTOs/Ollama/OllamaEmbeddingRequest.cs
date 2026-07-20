namespace AI.TestCaseGenerator.API.DTOs.Ollama
{
    public class OllamaEmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
