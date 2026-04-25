using Microsoft.SemanticKernel;
using MudBlazor.Services;
using SmartDocAgent.Components;
using SmartDocAgent.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Config things
var groqApiKey = builder.Configuration["Groq:ApiKey"]!;
var groqChatModel = builder.Configuration["Groq:ChatModel"]!;
var embeddingModel = builder.Configuration["Ollama:EmbeddingModel"]!;
var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"]!;
var qdrantEndpoint = builder.Configuration["Qdrant:Endpoint"]!;
var qdrantPort = int.Parse(builder.Configuration["Qdrant:Port"]!);
var collectionName = builder.Configuration["Qdrant:CollectionName"]!;

builder.Services.AddKernel();

builder.Services.AddSingleton<QdrantService>(sp =>
    new QdrantService(qdrantEndpoint, qdrantPort, collectionName));

builder.Services.AddSingleton<EmbeddingService>(sp =>
    new EmbeddingService(ollamaEndpoint, embeddingModel));

builder.Services.AddSingleton<DocumentIngester>();

builder.Services.AddScoped<DocumentAgentService>(sp =>
    new DocumentAgentService(
        groqApiKey,
        groqChatModel,
        sp.GetRequiredService<QdrantService>(),
        sp.GetRequiredService<EmbeddingService>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
