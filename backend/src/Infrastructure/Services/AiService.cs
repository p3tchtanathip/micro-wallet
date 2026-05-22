using System.Net.Http.Json;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Responses;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AiService : IAiService

{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CategorizeTransactionAsync(string? description, string type, decimal amount, CancellationToken ct)
    {
        var prompt = $"""
        You are a finance transaction categorizer.

        Categorize this transaction into ONE category only.

        Possible categories:
        - Food
        - Transport
        - Shopping
        - Entertainment
        - Bills
        - Salary
        - Health
        - Education
        - Travel
        - Other

        Transaction:
        Description: {description}
        Type: {type}
        Amount: {amount}

        Return ONLY the category name.
        """;

        return (await SendPromptAsync(prompt, ct)) ?? "Other";
    }

    public async Task<string> GenerateSpendingInsightsAsync(Dictionary<string, decimal> categoryTotals, CancellationToken ct)
    {
        var breakdown = string.Join("\n", categoryTotals
            .OrderByDescending(kv => kv.Value)
            .Select(kv => $"- {kv.Key}: {kv.Value:C}"));

        var prompt = $"""
        You are a personal finance analyst. Based on the user's spending breakdown by category,
        provide a brief insight (2-3 sentences). Mention the top spending category, any notable
        patterns, and one practical suggestion.

        Spending breakdown:
        {breakdown}
        """;

        return (await SendPromptAsync(prompt, ct)) ?? "No spending data available.";
    }

    public async Task<string> AnswerQueryAsync(string query, List<TransactionInfo> transactions, CancellationToken ct)
    {
        var transactionLines = string.Join("\n", transactions.Select(t =>
        {
            var cat = t.Category ?? "Uncategorized";
            var desc = string.IsNullOrWhiteSpace(t.Description) ? "(no description)" : $"\"{t.Description}\"";
            return $"- {t.Amount:C} | {cat} | {desc} | {t.Type} | {t.CreatedAt:yyyy-MM-dd}";
        }));

        var prompt = $"""
        You are a financial data assistant. The user has the following transactions.
        Answer their question concisely based ONLY on the data provided.
        If the data does not contain enough information to answer, say so.

        Transactions:
        {transactionLines}

        Question: {query}
        """;

        return (await SendPromptAsync(prompt, ct)) ?? "Could not generate an answer.";
    }

    private async Task<string?> SendPromptAsync(string prompt, CancellationToken ct)
    {
        var model = _config["Groq:Model"];
        var apiKey = _config["Groq:ApiKey"];

        var endpoint = "openai/v1/chat/completions";
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, ct);
        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: ct);

        return result?.Choices
            ?.FirstOrDefault()
            ?.Message
            ?.Content
            ?.Trim();
    }
}