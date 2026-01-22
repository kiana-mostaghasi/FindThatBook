**Find That Book**
==================

An intelligent search engine for books
--------------------------------------

Traditional book APIs are very literal. If you make a typo ("The Gret Gatsby") or search for a plot point ("scary clown in sewer"), they return **zero results**.

**Find That Book** solves this by placing an AI Interceptor before the database. It translates human intent into structured search parameters, tolerates messy input, and explains _why_ the results match the query.

**How it Works**
----------------

The system follows a strict **Clean Architecture** pipeline to ensure reliability and safety.

### **1\. The AI Interceptor (Gemini Flash)**

Before reaching the Open Library API, the user's query is sanitized (stripping delimiters and limiting length) and passed to Google Gemini. Using **Few-Shot Prompting**, the AI extracts the user's _Intent_:

*   **Correction:** "the gret gatsby" → Title: "The Great Gatsby"
    
*   **Inference:** "scary book about a clown in a sewer" → Title: "It"
    
*   **Synthesis:** "kids playing at school" → Keywords: \["kids", "playing", "school"\]
    

### **2\. Cascading Search Strategy**

1.  If the AI identifies a specific book, we search by Title AND Author.
    
    *   _Why Author?_ Searching for "It" (Stephen King) by title alone returns hundreds of irrelevant results (e.g., _"Five Children and It"_). Adding the author is critical for common words.
        
2.  If the specific search fails, or the query is vague, we switch to a keyword-based search (q=...) and rely on Open Library's relevance sorting.
    
3.  We strictly limit results to 1 for title search and 5 for keyword search.
    

### **3\. Contextual Explanation**

Once results are found, they are fed back into Gemini to generate a one-sentence explanation connecting the book to the user's original vague query.

**Quality Assurance**
---------------------

LLMs are inherently non-deterministic. To ensure consistent performance, this project implements a custom **Regression Evaluator**.

The evaluation\_dataset.json contains a suite of edge cases, covering **typos, vague plot descriptions, and safety constraints.**

Iterative prompt engineering achieved a 100% pass rate, ensuring that optimizations for complex queries do not regress basic functionality.

**Tech Stack**
--------------

*   **Architecture:** Clean Architecture (Separation of Domain, Application, Infrastructure, API)
    
*   **Backend:** .NET 8 Web API
    
*   **Frontend:** React + Vite
    
*   **AI Model:** Google gemini-flash-latest
    
*   **Data Source:** Open Library Search API
    

**Getting Started**
-------------------

### **Prerequisites**

*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
    
*   [Node.js](https://nodejs.org/) (v18+)
    
*   [Google Gemini API Key](https://ai.google.dev/)
    

### **1\. Backend Setup**
```bash
cd FindThatBook.API

dotnet user-secrets init

dotnet user-secrets set "Gemini:ApiKey" "YOUR\_GEMINIL\_API\_KEY\_HERE"

dotnet run --launch-profile http
```
_The API will start at http://localhost:5063_

### **2\. Frontend Setup**

```bash

cd FindThatBook.Client

npm install

npm run start
```
_The UI will start at http://localhost:5173_

**Roadmap & Production Considerations**
---------------------------------------

While the current implementation is functional and architecturally sound, taking this to production would involve:

*   **Semantic Re-ranking**: Currently, we rely on Open Library's default sort order. A production upgrade would fetch a larger candidate set (e.g., top 50 results) and apply a custom Re-ranking Step to score them against the user's specific intent before displaying the top 5.

*   **Search Index Analysis**: Conducting rigorous A/B testing to determine the optimal keyword strategy for the Open Library API. We need to verify if the q= parameter consistently indexes author and title fields across their entire dataset to prevent "false negatives" on broad queries.

*   **Advanced Evaluation:** Using LLM-as-a-Judge or human experts to evaluate the quality of the generated explanations, not only the search accuracy.
    
*   **Observability:** Measuring search latency (User Perceived Latency) vs. Explanation generation time. We could optimize this by loading the explanation asynchronously.
    
*   **Cost Analysis:** Monitoring token usage per query to ensure that cost per query makes sense.
    
*   **Caching:** Implementing Redis to cache frequent queries (e.g., "Harry Potter") to save both API and AI costs.
    
*   **Rate Limiting:** Protecting the backend from abuse.
