# 🤖 SmartDocAgent — Agentic RAG Application

A locally running **Agentic RAG (Retrieval-Augmented Generation)** application built with **.NET 8**, **Blazor Server**, **Semantic Kernel**, **Groq AI**, **Ollama**, and **Qdrant** vector database. Upload any PDF or text document and ask natural language questions — the AI agent intelligently searches your documents and answers with cited sources.

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![Blazor](https://img.shields.io/badge/Blazor-Server-blue)
![SemanticKernel](https://img.shields.io/badge/Semantic%20Kernel-Latest-green)
![MudBlazor](https://img.shields.io/badge/MudBlazor-Latest-red)
![Qdrant](https://img.shields.io/badge/Qdrant-Vector%20DB-orange)

---

## 📸 Screenshots

> Upload documents and chat with your AI agent in real time.

```
┌──────────────┬─────────────────────────────────────┐
│  📁 Documents │        🤖 AI Document Assistant      │
│               │                                     │
│  • policy.pdf │  You: What is the refund policy?    │
│  • manual.txt │                                     │
│               │  Agent: Based on policy.pdf, the    │
│  [Browse Files│  refund policy allows 30-day        │
│   ]           │  returns...                         │
│               │                                     │
│               │  📌 Sources: policy.pdf             │
└──────────────┴─────────────────────────────────────┘
```

---

## ✨ Features

- 📄 **Document Upload** — Upload PDF and TXT files via drag & drop
- 🧠 **Agentic RAG** — AI agent decides when and how to search documents
- ⚡ **Groq AI** — Ultra-fast LLM inference via Groq cloud
- 🔍 **Semantic Search** — Vector similarity search via Qdrant
- 📌 **Source Citations** — Every answer includes the source filename
- 💬 **Conversation Memory** — Agent remembers context across turns
- 🗄️ **Persistent Storage** — Qdrant stores vectors across restarts
- 🎨 **Modern UI** — Built with MudBlazor Material Design components

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                  BLAZOR SERVER UI                    │
│         MudBlazor Components (Material Design)       │
│   MudFileUpload │ Chat Interface │ Source Citations  │
└───────────────────────┬─────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────┐
│              APPLICATION LAYER (.NET 8)              │
│                                                     │
│  DocumentIngester    │    DocumentAgentService       │
│  ┌────────────────┐  │  ┌──────────────────────────┐│
│  │ Parse PDF/TXT  │  │  │   Semantic Kernel Agent  ││
│  │ Chunk Text     │  │  │   ReAct Loop             ││
│  │ Embed Chunks   │  │  │   RAGPlugin (Tool)       ││
│  │ Store Vectors  │  │  │   SearchDocuments()      ││
│  └────────────────┘  │  └──────────────────────────┘│
└──────────┬───────────┴──────────────┬───────────────┘
           │                          │
    ┌──────▼──────┐           ┌───────▼───────┐
    │   QDRANT    │           │  GROQ  │OLLAMA│
    │ Vector DB   │           │  LLM   │Embed │
    │ Port: 6333  │           │ Cloud  │Local │
    │ Port: 6334  │           └───────────────┘
    │  (Docker)   │
    └─────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | .NET 8, Blazor Server |
| **UI Components** | MudBlazor (Material Design) |
| **AI Orchestration** | Microsoft Semantic Kernel |
| **LLM (Chat)** | Groq AI — llama-3.1-8b-instant |
| **Embeddings** | Ollama — nomic-embed-text (local) |
| **Vector Database** | Qdrant (Docker) |
| **PDF Parsing** | PdfPig |

---

## 📋 Prerequisites

Before running this project, make sure you have:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Ollama](https://ollama.com/download)
- [Groq API Key](https://console.groq.com) (free tier available)

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/SmartDocAgent.git
cd SmartDocAgent
```

### 2. Pull Ollama Embedding Model

```bash
ollama pull nomic-embed-text
```

### 3. Start Qdrant with Docker

```bash
cd SmartDocAgent
docker-compose up -d
```

Verify Qdrant is running at: `http://localhost:6333/dashboard`

### 4. Configure API Keys

Open `appsettings.json` and add your Groq API key:

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text"
  },
  "Groq": {
    "ApiKey": "gsk_your_groq_api_key_here",
    "ChatModel": "llama-3.1-8b-instant"
  },
  "Qdrant": {
    "Endpoint": "localhost",
    "Port": 6334,
    "CollectionName": "documents"
  }
}
```

### 5. Run the Application

```bash
dotnet run
```

Open your browser at: `https://localhost:5001`

---

## 📁 Project Structure

```
SmartDocAgent/
│
├── Components/
│   ├── Pages/
│   │   └── Home.razor            # Main chat + upload page
│   ├── Layout/
│   │   └── MainLayout.razor      # MudBlazor layout
│   ├── _Imports.razor
│   └── App.razor
│
├── Services/
│   ├── QdrantService.cs          # Vector DB operations
│   ├── EmbeddingService.cs       # Ollama embeddings
│   ├── DocumentIngester.cs       # PDF/TXT ingestion pipeline
│   └── DocumentAgentService.cs   # SK Agent + RAG Plugin
│
├── docker-compose.yml            # Qdrant container
├── appsettings.json
└── Program.cs
```

---

## 🔄 How It Works

### Document Ingestion Flow
```
Upload PDF/TXT
     │
     ▼
Parse Text (PdfPig)
     │
     ▼
Chunk into 500-word pieces with 50-word overlap
     │
     ▼
Generate Embeddings (Ollama: nomic-embed-text)
     │
     ▼
Store in Qdrant Vector DB
```

### Agentic Query Flow
```
User Question
     │
     ▼
Semantic Kernel Agent (ReAct Loop)
     │
     ├── THINK: "I need to search documents"
     │
     ├── ACT: Call SearchDocuments() tool
     │
     ├── OBSERVE: Retrieved chunks from Qdrant
     │
     └── ANSWER: Generate response with citations
```

---

## 🧩 Key Concepts Demonstrated

| Concept | Implementation |
|---|---|
| **Agentic AI** | SK Agent with auto function calling |
| **RAG Pattern** | Retrieve → Augment → Generate |
| **Vector Search** | Cosine similarity in Qdrant |
| **Text Chunking** | 500-word chunks with 50-word overlap |
| **Tool Calling** | KernelFunction attribute on SearchDocuments |
| **Embeddings** | nomic-embed-text via Ollama |

---

## ⚙️ Configuration Options

| Setting | Description | Default |
|---|---|---|
| `Groq:ChatModel` | LLM model for chat | llama-3.1-8b-instant |
| `Ollama:EmbeddingModel` | Embedding model | nomic-embed-text |
| `Qdrant:CollectionName` | Vector collection name | documents |
| `Qdrant:Port` | gRPC port for Qdrant client | 6334 |

---

## 🐳 Docker Services

```yaml
services:
  qdrant:
    image: qdrant/qdrant
    ports:
      - "6333:6333"   # REST/Dashboard
      - "6334:6334"   # gRPC
```

---

## 📦 NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.SemanticKernel` | AI agent orchestration |
| `Microsoft.SemanticKernel.Connectors.Ollama` | Ollama integration |
| `Microsoft.SemanticKernel.Connectors.Qdrant` | Qdrant integration |
| `MudBlazor` | UI components |
| `Qdrant.Client` | Qdrant gRPC client |
| `UglyToad.PdfPig` | PDF text extraction |

---

## 🔮 Future Enhancements

- [ ] Azure OpenAI integration
- [ ] Multi-collection support
- [ ] Document management (delete/update)
- [ ] Streaming responses
- [ ] Authentication
- [ ] Export chat history

---

## 🤝 Contributing

Pull requests are welcome! For major changes, please open an issue first.

---

## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

Built with ❤️ using .NET 8, Semantic Kernel, and Groq AI.
