using Microsoft.ML.OnnxRuntimeGenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace NotesWPF.Models
{

    internal class GenAIModel : IDisposable
    {
        private const string TEMPLATE_PLACEHOLDER = "{{CONTENT}}";

        private Model? _model;
        private Tokenizer? _tokenizer;
        private LlmPromptTemplate? _template;
        private static SemaphoreSlim _createSemaphore = new SemaphoreSlim(1, 1);

        // Search Options
        public int TopK { get; set; } = 50;
        public float TopP { get; set; } = 0.9f;
        public float Temperature { get; set; } = 1;
        public int MinLength { get; set; }
        public int MaxLength { get; set; } = 1024;
        public bool DoSample { get; set; }

        public event EventHandler? ModelLoaded;

        public int MaxTextLength => 2048;

        private GenAIModel()
        {
        }

        public static async Task<GenAIModel?> CreateAsync(string modelDir, LlmPromptTemplate? template = null, CancellationToken cancellationToken = default)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var model = new GenAIModel();
#pragma warning restore CA2000 // Dispose objects before losing scope

            var lockAcquired = false;
            try
            {
                // ensure we call CreateAsync one at a time to avoid fun issues
                await _createSemaphore.WaitAsync(cancellationToken);
                lockAcquired = true;
                cancellationToken.ThrowIfCancellationRequested();
                await model.InitializeAsync(modelDir, cancellationToken);
            }
            catch
            {
                model?.Dispose();
                return null;
            }
            finally
            {
                if (lockAcquired)
                {
                    _createSemaphore.Release();
                }
            }

            model._template = template;
            return model;
        }

        [MemberNotNullWhen(true, nameof(_model), nameof(_tokenizer))]
        public bool IsReady => _model != null && _tokenizer != null;

        public void Dispose()
        {
            _model?.Dispose();
            _tokenizer?.Dispose();
        }

        private string GetPrompt(string userMsg, string? systemMsg = null)
        {
            string prompt = string.Empty;

            if (_template != null)
            {
                if (!string.IsNullOrWhiteSpace(systemMsg))
                {
                    if (!string.IsNullOrWhiteSpace(_template.System))
                    {
                        prompt = _template.System.Replace(TEMPLATE_PLACEHOLDER, systemMsg);
                    }
                    else
                    {
                        userMsg = $"{systemMsg} {userMsg}";
                    }
                }

                if (!string.IsNullOrWhiteSpace(userMsg))
                {
                    if (!string.IsNullOrWhiteSpace(_template.User))
                    {
                        prompt += _template.User.Replace(TEMPLATE_PLACEHOLDER, userMsg);
                    }
                    else
                    {
                        prompt += userMsg;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_template.Assistant))
                {
                    var substringIndex = _template.Assistant.IndexOf(TEMPLATE_PLACEHOLDER, StringComparison.InvariantCulture);
                    prompt += _template.Assistant.Substring(0, substringIndex);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(systemMsg))
                {
                    prompt = systemMsg + " ";
                }

                prompt += userMsg;
            }

            return prompt;
        }

        private string GetPrompt(List<LlmMessage> history)
        {
            if (history.Count == 0)
            {
                return string.Empty;
            }

            if (_template == null)
            {
                return string.Join(". ", history.Select(h => h.Content));
            }

            string prompt = string.Empty;

            string systemMsgWithoutSystemTemplate = string.Empty;

            for (var i = 0; i < history.Count; i++)
            {
                var message = history[i];
                switch (message.Role)
                {
                    case LlmMessageRole.System:
                        if (i > 0)
                        {
                            throw new ArgumentException("Only first message can be a system message");
                        }

                        if (string.IsNullOrWhiteSpace(_template.System))
                        {
                            systemMsgWithoutSystemTemplate = message.Content;
                        }
                        else
                        {
                            prompt += _template.System.Replace(TEMPLATE_PLACEHOLDER, message.Content);
                        }

                        break;
                    case LlmMessageRole.User:
                        string msgText = message.Content;
                        if (i == 1 && !string.IsNullOrWhiteSpace(systemMsgWithoutSystemTemplate))
                        {
                            msgText = $"{systemMsgWithoutSystemTemplate} {msgText}";
                        }

                        if (string.IsNullOrWhiteSpace(_template.User))
                        {
                            prompt += msgText;
                        }
                        else
                        {
                            prompt += _template.User.Replace(TEMPLATE_PLACEHOLDER, msgText);
                        }

                        break;
                    case LlmMessageRole.Assistant:

                        if (string.IsNullOrWhiteSpace(_template.Assistant))
                        {
                            prompt += message.Content;
                        }
                        else
                        {
                            prompt += _template.Assistant.Replace(TEMPLATE_PLACEHOLDER, message.Content);
                        }

                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(_template.Assistant))
            {
                var substringIndex = _template.Assistant.IndexOf(TEMPLATE_PLACEHOLDER, StringComparison.InvariantCulture);
                prompt += _template.Assistant.Substring(0, substringIndex);
            }

            return prompt;
        }

        // TODO, this is a bit of a mess, need to clean up
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

        public IAsyncEnumerable<string> InferStreaming(string systemPrompt, string userPrompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var prompt = GetPrompt(userPrompt, systemPrompt);
            return InferStreaming(prompt, false, ct);
        }

        public IAsyncEnumerable<string> InferStreaming(string systemPrompt, string userPrompt, bool useCustomParameters, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var prompt = GetPrompt(userPrompt, systemPrompt);
            return InferStreaming(prompt, useCustomParameters, ct);
        }

        public IAsyncEnumerable<string> InferStreaming(List<LlmMessage> history, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var prompt = GetPrompt(history);
            return InferStreaming(prompt, false, ct);
        }

        public IAsyncEnumerable<string> InferStreaming(List<LlmMessage> history, string systemPrompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var historyWithSystemPrompt = history.ToList();
            historyWithSystemPrompt.Insert(0, new LlmMessage(systemPrompt, LlmMessageRole.System));

            return InferStreaming(historyWithSystemPrompt, ct);
        }

        public IAsyncEnumerable<string> InferStreaming(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            return InferStreaming(prompt, false, ct);
        }

        public async IAsyncEnumerable<string> InferStreaming(string prompt, bool useCustomParameters, [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException("Model is not ready");
            }

            using var generatorParams = new GeneratorParams(_model);

            using var sequences = _tokenizer.Encode(prompt);

            if (useCustomParameters)
            {
                generatorParams.SetSearchOption("top_k", TopK);
                generatorParams.SetSearchOption("top_p", TopP);
                generatorParams.SetSearchOption("temperature", Temperature);
                generatorParams.SetSearchOption("min_length", MinLength);
                generatorParams.SetSearchOption("max_length", MaxLength);
                generatorParams.SetSearchOption("do_sample", DoSample);
            }

            generatorParams.SetInputSequences(sequences);
            generatorParams.TryGraphCaptureWithMaxBatchSize(1);

            using var tokenizerStream = _tokenizer.CreateStream();
            using var generator = new Generator(_model, generatorParams);
            StringBuilder stringBuilder = new();
            bool stopTokensAvailable = _template != null && _template.Stop != null && _template.Stop.Length > 0;
            while (!generator.IsDone())
            {
                string part;
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Delay(0, ct).ConfigureAwait(false);

                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    part = tokenizerStream.Decode(generator.GetSequence(0)[^1]);

                    if (ct.IsCancellationRequested)
                    {
                        part = "<|end|>";
                    }

                    stringBuilder.Append(part);

                    if (stopTokensAvailable)
                    {
                        var str = stringBuilder.ToString();
                        if (_template!.Stop!.Any(s => str.Contains(s)))
                        {
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }

                yield return part;
            }
        }
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

        private Task InitializeAsync(string modelDir, CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    _model = new Model(modelDir);
                    _tokenizer = new Tokenizer(_model);
                    ModelLoaded?.Invoke(this, EventArgs.Empty);
                },
                cancellationToken);
        }
    }
}