using System.Collections.Generic;

namespace AI.TestCaseGenerator.API.DTOs.Ollama
{
    public class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<OllamaMessage> Messages { get; set; } = new();
        public bool Stream { get; set; } = false;
    }
}
