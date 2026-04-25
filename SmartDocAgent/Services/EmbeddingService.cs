using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace SmartDocAgent.Services
{
    public class EmbeddingService
    {
        private readonly string _endpoint;
        private readonly string _model;

        public EmbeddingService(string endpoint, string model)
        {
            _endpoint = endpoint;
            _model = model;
        }

        public async Task<List<float>> GetEmbeddingAsync(string text)
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOllamaEmbeddingGenerator(
                modelId: _model,
                endpoint: new Uri(_endpoint)
            );

            var kernel = builder.Build();

            var embeddingService = kernel
                .GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            var result = await embeddingService
             .GenerateAsync(new[] { text });

            return result[0].Vector.ToArray().ToList();
        }
    }
}
