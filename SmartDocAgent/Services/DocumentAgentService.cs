using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace SmartDocAgent.Services;

public class DocumentAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ChatHistory _chatHistory;

    public DocumentAgentService(
        string groqApiKey,
        string chatModel,
        QdrantService qdrantService,
        EmbeddingService embeddingService)
    {
        // Build Kernel with Groq
        // Groq is OpenAI API compatible!
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: chatModel,
            apiKey: groqApiKey,
            endpoint: new Uri("https://api.groq.com/openai/v1")
        );

        // Register RAG Plugin
        builder.Plugins.AddFromObject(
            new RAGPlugin(qdrantService, embeddingService),
            "RAGPlugin"
        );

        _kernel = builder.Build();
        _chatService = _kernel
            .GetRequiredService<IChatCompletionService>();

        _chatHistory = new ChatHistory();
        _chatHistory.AddSystemMessage(
            "You are a helpful document assistant. " +
            "Always use the SearchDocuments tool to find " +
            "relevant information before answering. " +
            "Always mention the source filename in your answer."
        );
    }

    public async Task<AgentResponse> AskAsync(string question)
    {
        _chatHistory.AddUserMessage(question);

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var response = await _chatService
            .GetChatMessageContentAsync(
                _chatHistory,
                executionSettings: settings,
                kernel: _kernel
            );

        _chatHistory.AddAssistantMessage(response.Content!);

        return new AgentResponse
        {
            Answer = response.Content!,
            Sources = ExtractSources(response.Content!)
        };
    }

    public void ClearHistory()
    {
        _chatHistory.RemoveRange(1, _chatHistory.Count - 1);
    }

    private List<string> ExtractSources(string content)
    {
        var sources = new List<string>();
        var words = content.Split(' ');
        foreach (var word in words)
        {
            if (word.EndsWith(".pdf") || word.EndsWith(".txt"))
                sources.Add(word.Trim('.', ',', ')', '('));
        }
        return sources.Distinct().ToList();
    }
}

public class AgentResponse
{
    public string Answer { get; set; } = "";
    public List<string> Sources { get; set; } = new();
}

public class RAGPlugin
{
    private readonly QdrantService _qdrantService;
    private readonly EmbeddingService _embeddingService;

    public RAGPlugin(
        QdrantService qdrantService,
        EmbeddingService embeddingService)
    {
        _qdrantService = qdrantService;
        _embeddingService = embeddingService;
    }

    [KernelFunction("SearchDocuments")]
    [Description("Search uploaded documents to find relevant information for a query")]
    public async Task<string> SearchDocumentsAsync(
        [Description("The search query")] string query)
    {
        var embedding = await _embeddingService
            .GetEmbeddingAsync(query);

        var results = await _qdrantService
            .SearchAsync(embedding.ToArray());

        if (!results.Any())
            return "No relevant documents found.";

        return string.Join("\n\n", results.Select(r =>
            $"[Source: {r.FileName} | Score: {r.Score:F2}]\n{r.Text}"
        ));
    }
}