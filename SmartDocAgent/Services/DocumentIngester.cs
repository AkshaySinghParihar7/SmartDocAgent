using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartDocAgent.Services
{
    public class DocumentIngester
    {
        private readonly QdrantService _qdrantService;
        private readonly EmbeddingService _embeddingService;
        private const int ChunkSize = 500;
        private const int ChunkOverlap = 50;

        public DocumentIngester(
            QdrantService qdrantService,
            EmbeddingService embeddingService)
        {
            _qdrantService = qdrantService;
            _embeddingService = embeddingService;
        }

        public async Task IngestDocumentAsync(
    Stream fileStream, string fileName)
        {
            // Copy to MemoryStream first (makes it seekable)
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // 1. Extract text
            var text = fileName.EndsWith(".pdf",
                StringComparison.OrdinalIgnoreCase)
                ? ExtractTextFromPdf(memoryStream)
                : await ExtractTextFromTxt(memoryStream);

            // 2. Chunk text
            var chunks = ChunkText(text, fileName);

            // 3. Ensure collection exists
            await _qdrantService.EnsureCollectionExistsAsync();

            // 4. Embed each chunk
            foreach (var chunk in chunks)
            {
                chunk.Embedding = await _embeddingService
                    .GetEmbeddingAsync(chunk.Text);
            }

            // 5. Store in Qdrant
            await _qdrantService.UpsertVectorsAsync(chunks);
        }

        private string ExtractTextFromPdf(Stream stream)
        {
            using var pdf = PdfDocument.Open(stream);
            var text = new System.Text.StringBuilder();

            foreach (Page page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }

        private async Task<string> ExtractTextFromTxt(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private List<DocumentChunk> ChunkText(
            string text, string fileName)
        {
            var words = text.Split(' ',
                StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<DocumentChunk>();
            int index = 0;

            for (int i = 0; i < words.Length; i += ChunkSize - ChunkOverlap)
            {
                var chunkWords = words
                    .Skip(i)
                    .Take(ChunkSize)
                    .ToArray();

                if (chunkWords.Length == 0) break;

                chunks.Add(new DocumentChunk
                {
                    Text = string.Join(' ', chunkWords),
                    FileName = fileName,
                    ChunkIndex = index++
                });

                if (i + ChunkSize >= words.Length) break;
            }

            return chunks;
        }
    }
}
