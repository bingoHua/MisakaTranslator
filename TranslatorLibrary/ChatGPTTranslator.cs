using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Policy;
using System.Linq;

/*
 * ChatGPT translator integration
 * Author: bychv
 * API version: v1
 */
namespace TranslatorLibrary
{
    public class ChatGPTTranslator : ITranslator

    {
        public static readonly string SIGN_UP_URL = "https://platform.openai.com";
        public static readonly string BILL_URL = "https://platform.openai.com/account/usage";
        public static readonly string DOCUMENT_URL = "https://platform.openai.com/docs/introduction/overview";
        private string openai_model = "gpt-3.5-turbo";

        private string apiKey; //ChatGPT翻译API的密钥
        private string apiUrl; //ChatGPT翻译API的URL
        private string errorInfo; //错误信息

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            string q = sourceText;

            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            string retString;
            string jsonParam = $"{{\"model\": \"{openai_model}\",\"messages\": [{{\"role\": \"system\", \"content\": \"你是一个翻译引擎。根据原文逐行翻译，将每行原文翻译为简体中文。保留每行文本的原始格式，并根据所需格式输出翻译后的文本。在翻译文本时，请严格注意以下几个方面：首先，一些完整的文本可能会被分成不同的行。请严格按照每行的原始文本进行翻译，不要偏离原文。其次，无论句子的长度如何，每行都是一个独立的句子，确保不要将多行合并成一个翻译。第三，在每行文本中，转义字符（例如\, \\r, 和\\n）或非日语内容（例如数字、英文字母、特殊符号等）不需要翻译或更改，应保持原样。\"}},{{\"role\": \"user\", \"content\": \"{q}\"}}]}}";
            var hc = CommonFunction.GetHttpClient();
            var req = new StringContent(jsonParam, null, "application/json");
            hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            try
            {
                retString = await (await hc.PostAsync(apiUrl, req)).Content.ReadAsStringAsync();
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            finally
            {
                req.Dispose();
            }

            ChatResponse oinfo;

            try
            {
                oinfo = JsonSerializer.Deserialize<ChatResponse>(retString, CommonFunction.JsonOP);
            }
            catch
            {
                errorInfo = "JsonConvert Error";
                return null;
            }
            
            try
            {
                return oinfo.choices[0].message.content;
            }
            catch
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ChatResErr>(retString, CommonFunction.JsonOP);
                    errorInfo = err.error.message;
                    return null;
                }
                catch
                {
                    errorInfo = "Unknown error";
                    return null;
                }
                return null;
            }
        }

        public void TranslatorInit(string param1, string param2)
        {
            apiKey = param1;
            apiUrl = param2;
        }
    }

#pragma warning disable 0649
    public struct ChatResponse
    {
        public string id;
        public string _object;
        public int created;
        public string model;
        public ChatUsage usage;
        public ChatChoice[] choices;
    }

    public struct ChatUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    public struct ChatChoice
    {
        public ChatMessage message;
        public string finish_reason;
        public int index;
    }

    public struct ChatMessage
    {
        public string role;
        public string content;
    }

    public struct ChatResErr
    {
        public ChatError error;
    }

    public struct ChatError
    {
        public string message;
        public string type;
        public object param;
        public object code;
    }
}
