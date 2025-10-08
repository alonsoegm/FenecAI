# 🦊 FenecAI — Azure-Powered Generative AI Platform

> _“Merging technology and consciousness to build meaningful AI solutions.”_

FenecAI is a **comprehensive Generative AI API** built with **.NET 8** and **Azure OpenAI**, aligned with the **Microsoft Certified: Azure AI Engineer Associate (AI-102)** objectives.

This project demonstrates expert-level integration of multiple Azure Cognitive Services — including **OpenAI, AI Search, Blob Storage, and Content Safety** — to deliver end-to-end intelligent capabilities such as:

- 🧠 **Chat Completions** (GPT-4)
- 🧩 **RAG (Retrieval-Augmented Generation)**
- 🔍 **Embeddings & Semantic Similarity**
- 🎨 **Image Generation (DALL·E 3)**
- 🛡️ **Content Safety (Text & Image)**
- 📊 **Token Usage & Cost Metrics**

---

## 🧭 Architecture Overview

```mermaid
graph TD
A[User Request] --> B[ASP.NET Core API]
B --> C1[Azure OpenAI (Chat, DALL·E, Embeddings)]
B --> C2[Azure Blob Storage]
B --> C3[Azure AI Search (Vector Index)]
B --> C4[Azure Content Safety]
B --> C5[Application Insights]
C2 --> C3
C3 --> C1
The system follows a modular service-oriented design:

Each capability (Chat, Image, RAG, Safety, Metrics) lives in an isolated service.

All configuration values are injected via Dependency Injection.

Asynchronous programming ensures scalability and high throughput.

```
