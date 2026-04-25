using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace SmartDocAgent.Services
{
    public class QdrantService
    {
        private readonly QdrantClient _client;
        private readonly string _collectionName;

        public QdrantService(string endpoint, int port, string collectionName)
        {
            _client = new QdrantClient(endpoint, port:6334);
            _collectionName = collectionName;
        }

        public async Task EnsureCollectionExistsAsync()
        {
            var collections = await _client.ListCollectionsAsync();
            var exists = collections.Any(c => c == _collectionName);

            if (!exists)
            {
                await _client.CreateCollectionAsync(_collectionName,
                new VectorParams
                {
                    Size = 768,
                    Distance = Distance.Cosine
                });
            }
        }

        public async Task UpsertVectorsAsync(List<DocumentChunk> chunks)
        {
            var points = chunks.Select((chunk, index) => new PointStruct
            {
                Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                Vectors = chunk.Embedding.ToArray(),
                Payload =
            {
                ["text"] = chunk.Text,
                ["filename"] = chunk.FileName,
                ["chunkIndex"] = chunk.ChunkIndex
            }
            }).ToList();

            await _client.UpsertAsync(_collectionName, points);
        }

        public async Task<List<SearchResult>> SearchAsync(
        float[] queryVector, int topK = 3)
        {
            var results = await _client.SearchAsync(
                _collectionName, queryVector, limit: (ulong)topK);

            return results.Select(r => new SearchResult
            {
                Text = r.Payload["text"].StringValue,
                FileName = r.Payload["filename"].StringValue,
                Score = r.Score
            }).ToList();
        }
    }
}

public class DocumentChunk
{
    public string Text { get; set; } = "";
    public string FileName { get; set; } = "";
    public int ChunkIndex { get; set; }
    public List<float> Embedding { get; set; } = new();
}

public class SearchResult
{
    public string Text { get; set; } = "";
    public string FileName { get; set; } = "";
    public float Score { get; set; }
}