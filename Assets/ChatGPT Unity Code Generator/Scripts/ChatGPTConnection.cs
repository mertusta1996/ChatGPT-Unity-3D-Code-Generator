#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
public sealed class ChatGPTConnection
{
    private readonly IReadOnlyDictionary<string, string> headers;
    private readonly List<Message> messages;
    private readonly RequestBody requestBody;
    private static readonly HttpClient httpClient;
    private const string EndPoint = "https://api.openai.com/v1/chat/completions";

    static ChatGPTConnection()
    {
        httpClient = new HttpClient();
    }

    public ChatGPTConnection(string apiKey, string model, List<Message>? messages = null)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));
        
        this.headers = CreateHeader(apiKey);
        this.messages = messages ?? new List<Message>();
        this.requestBody = new RequestBody(model, this.messages);
    }

    public async Task<ResponseBody> CreateMessageAsync(string content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        messages.Add(new Message("user", content));

        using var requestMessage = CreateRequestMessage(headers, requestBody);
        using var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (responseMessage == null)
            throw new Exception($"HttpResponseMessage is null.");

        var responseJson = await responseMessage.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseJson))
            throw new Exception($"Response JSON is null or empty.");

        if (responseMessage.IsSuccessStatusCode)
        {
            var responseBody = ResponseBody.FromJson(responseJson);
            if (responseBody == null)
                throw new Exception($"Response body is null.");

            if (responseBody.Choices.Length == 0)
                throw new Exception($"Not found any choices in response body:{responseJson}.");

            // Record result to messages
            messages.Add(responseBody.Choices[0].Message);
            return responseBody;
        }
        else if (IsAPIError(responseMessage.StatusCode))
        {
            throw new Exception($"Error response body is null with status code:{responseMessage.StatusCode}.");
        }
        else // Another error, e.g. 5XX errors.
        {
            responseMessage.EnsureSuccessStatusCode();
            throw new Exception($"It should not be be reached with status code:{responseMessage.StatusCode}.");
        }
    }

    private static IReadOnlyDictionary<string, string> CreateHeader(string apiKey)
        => new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {apiKey}",
        };

    private static HttpRequestMessage CreateRequestMessage(
        IReadOnlyDictionary<string, string> headers,
        RequestBody requestBody)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint);
        foreach (var header in headers)
        {
            requestMessage.Headers.Add(header.Key, header.Value);
        }

        var requestContent = new StringContent(
            content: requestBody.ToJson(),
            encoding: System.Text.Encoding.UTF8,
            mediaType: "application/json");

        requestMessage.Content = requestContent;

        return requestMessage;
    }

    private static bool IsAPIError(HttpStatusCode statusCode)
        => 400 <= (int)statusCode && (int)statusCode <= 499;
}