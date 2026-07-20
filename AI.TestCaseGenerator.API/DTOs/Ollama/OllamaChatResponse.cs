namespace AI.TestCaseGenerator.API.DTOs.Ollama
{
    public class OllamaChatResponse
    {
        public string Model { get; set; } = string.Empty;
        public OllamaMessage? Message { get; set; }
        public bool Done { get; set; }
    }
}
