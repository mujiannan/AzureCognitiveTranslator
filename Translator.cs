using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveTranslator
{
    public class Translator
    {
        private readonly string _host;
        private readonly string _subscriptionKey;
        public Translator(string baseUrl, string key)//baseUrl should be string like "api.cognitive.microsofttranslator.com", determined by the Region of your AzureCognitiveService
        {
            _host = "https://" + baseUrl;
            _subscriptionKey = key;
        }
        //A perfect content for translation will be built, each List<string> must be limited by _limitedItemsCoun and _limitedCharactersCount
        private List<List<object>> _perfectContentForTranslation = new List<List<object>> { new List<object>() };
        public List<List<object>> PerfectContentForTranslation { get => _perfectContentForTranslation; set => _perfectContentForTranslation = value; }
        private int _CharactersCount = 0;
        //SetContensForTranslation
        private const int _limitedCharactersCount = 5000;//api 3.0
        private const int _limitedItemsCount = 100;//api 3.0
        public void AddContent(string newContent)
        {
            if (newContent.Length > _limitedCharactersCount)
            {
                throw new Exception("InputError:OutOfCharactersCount");
            }
            PerfectContentForTranslation[PerfectContentForTranslation.Count - 1].Add(new { Text = newContent });//先验证是否超过100毫无意义，因为就算没超过100，也可能在添加后导致长度达到5000……
            _CharactersCount += newContent.Length;
            if (PerfectContentForTranslation[PerfectContentForTranslation.Count - 1].Count > _limitedItemsCount || _CharactersCount > _limitedCharactersCount)//each List<string> must be limited to one hundred item
            {
                PerfectContentForTranslation[PerfectContentForTranslation.Count - 1].RemoveAt(PerfectContentForTranslation[PerfectContentForTranslation.Count - 1].Count - 1);//that's complex, but only one line
                PerfectContentForTranslation.Add(new List<object>());
                PerfectContentForTranslation[PerfectContentForTranslation.Count - 1].Add(new { Text = newContent });
                _CharactersCount = newContent.Length;//a new List<string>, a new _CharactersCount
            }
        }
        public List<string> Contents//user naturally set the contents, ignore all limits
        {
            set
            {
                _perfectContentForTranslation.Clear();
                _perfectContentForTranslation.Add(new List<object>());
                for (int i = 0; i < value.Count; i++)
                {
                    AddContent(value[i]);
                }
            }
        }

        //jsonAcceptLanguage
        private string _acceptLanguages = string.Empty;//不要在AcceptLanguages之外使用这个字段
        private string AcceptLanguages
        {
            get
            {
                if (_acceptLanguages == string.Empty)
                {
                    string route = "/languages?api-version=3.0";
                    using (HttpClient httpClient = new HttpClient())
                    {
                        using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(_host + route)))
                        {
                            httpRequestMessage.Headers.Add("Accept-Language", "en");
                            _acceptLanguages = httpClient.SendAsync(httpRequestMessage).Result.Content.ReadAsStringAsync().Result;
                        }
                    }

                }
                return _acceptLanguages;
            }
        }

        //GetTranslableLanguage
        public struct Language
        {
            public string name, nativeName, dir;
        }
        private static Dictionary<string, Language> _translatableLanguages = null;
        public Dictionary<string, Language> TranslatableLanguages//code as key, struct Language as value
        {
            get
            {
                if (_translatableLanguages == null)
                {
                    try
                    {
                        string translation = JsonConvert.DeserializeObject<JObject>(AcceptLanguages)["translation"].ToString();//extract "translation" from jsonAcceptLanguage
                        _translatableLanguages = JsonConvert.DeserializeObject<Dictionary<string, Language>>(translation);//如果字段为null，则先获取再设置，最后总是返回该字段
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    
                }
                return _translatableLanguages;
            }
        }

        //This is a useless TranslatingProgressReporter(Perhaps it'll be useful when translating more than one million characters at one time, which I've never done)
        public class TranslatingEventArgs : EventArgs
        {
            public TranslatingEventArgs(double newProgress)
            {
                if (newProgress >= 0 & newProgress <= 1)
                {
                    NewProgress = newProgress;
                }
                else
                {
                    throw new Exception("InnerError:InvalidNewProgress");
                }
            }
            public double NewProgress { get; }
        }
        public delegate void DTranslating(object sender, TranslatingEventArgs e);
        public event DTranslating ProgressChange;
        private void ChangeProgress(double newProgress)
        {
            DTranslating dTranslating = ProgressChange;
            if (dTranslating != null)
            {
                ProgressChange(this, new TranslatingEventArgs(newProgress));
            }
        }

        //Main method for translation
        public async Task<List<string>> TranslateAsync(string toLanguageCode)
        {
            if (PerfectContentForTranslation.Count==0)
            {
                throw new Exception("NoContentToTranslate");
            }
            Task<List<string>>[] tasks = new Task<List<string>>[PerfectContentForTranslation.Count];
            HttpClient translateClient = new HttpClient();
            for (int i = 0; i < PerfectContentForTranslation.Count; i++)
            {
                tasks[i] = TranslateAsync(PerfectContentForTranslation[i], toLanguageCode,translateClient);
            }
            List<string> results = new List<string>();
            for (int i = 0; i < tasks.Length; i++)
            {
                results.AddRange(await tasks[i]);
                ChangeProgress((double)(i + 1) / tasks.Length);
            }
            translateClient.Dispose();
            return results;
        }
        //TranslateAsync will call this method
        private async Task<List<string>> TranslateAsync(List<object> contentsForTranslation, string toLanguageCode,HttpClient httpClient)
        {
            if (!TranslatableLanguages.ContainsKey(toLanguageCode))
            {
                throw new Exception("InputError:UnexpectedLanguageCode");
            }
            string route = "/translate?api-version=3.0&to=" + toLanguageCode;
            string requestBody = JsonConvert.SerializeObject(contentsForTranslation);
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_host + route),
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            List<string> results = new List<string>();
            try
            {
                Newtonsoft.Json.Linq.JArray result = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(responseBody);
                for (int i = 0; i < result.Count; i++)
                {
                    results.Add(result[i]["translations"][0]["text"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            request.Dispose();
            return results;
        }
    }
}
