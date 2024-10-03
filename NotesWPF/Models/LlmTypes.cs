using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NotesWPF.Models
{
    internal class LlmPromptTemplate
    {
        public string? System { get; init; }
        public string? User { get; init; }
        public string? Assistant { get; init; }
        public string[]? Stop { get; init; }
    }

    internal class LlmMessage
    {
        public string Content { get; set; }
        public LlmMessageRole Role { get; set; }

        public LlmMessage(string content, LlmMessageRole role)
        {
            Content = content;
            Role = role;
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<LlmMessageRole>))]
    internal enum LlmMessageRole
    {
        System,
        User,
        Assistant
    }
}