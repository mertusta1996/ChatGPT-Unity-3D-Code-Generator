#nullable enable
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject]
public sealed class Message
{
    [JsonProperty("role")]
    public string messageRole { get; private set; }
        
    [JsonProperty("content")]
    public string Content { get; private set; }

    internal Message()
    {
        this.messageRole = "user";
        this.Content = string.Empty;
    }

    internal Message(string role, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(content);
        }
        this.messageRole = "user";
        this.Content = content;
    }
}

[JsonObject]
public sealed class Choice
{
    [JsonProperty("message")]
    public Message Message { get; private set; } = new Message();
}

[JsonObject]
public sealed class RequestBody
{
    [JsonProperty("model"), JsonRequired]
    public string Model { get; }

    [JsonProperty("messages"), JsonRequired]
    public List<Message> Messages { get; }

    public RequestBody(string model, List<Message> messages)
    {
        this.Model = model;
        this.Messages = messages;
    }

    public string ToJson()
        => JsonConvert.SerializeObject(
            this,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

    public static RequestBody? FromJson(string json)
        => JsonConvert.DeserializeObject<RequestBody>(json);
}

[JsonObject]
public sealed class ResponseBody
{
    [JsonProperty("id"), JsonRequired]
    public string ID { get; private set; } = string.Empty;

    [JsonProperty("object"), JsonRequired]
    public string Object { get; private set; } = string.Empty;

    [JsonProperty("created"), JsonRequired]
    public uint Created { get; private set; }

    [JsonProperty("model"), JsonRequired]
    public string Model { get; private set; } = string.Empty;

    [JsonProperty("choices"), JsonRequired]
    public Choice[] Choices { get; private set; } = Array.Empty<Choice>();

    public string ResultMessage
        => Choices.Length != 0 ? Choices[0].Message.Content : string.Empty;

    public string ToJson()
        => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static ResponseBody? FromJson(string json)
        => JsonConvert.DeserializeObject<ResponseBody>(json, new JsonSerializerSettings());
}