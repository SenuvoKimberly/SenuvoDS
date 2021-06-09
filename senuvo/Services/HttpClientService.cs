using senuvo.Merchants.Ewallet.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace senuvo.Services.HttpClientService
{
    public interface IHttpClientService
    {
        HttpResponseMessage MakeRequestByToken(HttpRequestMessage request, string tokenType, string token);
        HttpResponseMessage MakeRequestByUsername(HttpRequestMessage request, string username, string password);
        HttpResponseMessage PostRequestByUsername(HttpRequestMessage request, string username, string password);
        HttpResponseMessage PostRequest(string apiUrl, TokenRequest request);
    }
    public class HttpClientService : IHttpClientService
    {
        public HttpResponseMessage MakeRequestByToken(HttpRequestMessage request, string tokenType, string token)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add(tokenType, token);
                    httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public HttpResponseMessage MakeRequestByUsername(HttpRequestMessage request, string username, string password)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                    httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public HttpResponseMessage PostRequestByUsername(HttpRequestMessage request, string username, string password)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                    HttpResponseMessage response = httpClient.PostAsync(request.RequestUri, request.Content).Result;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public HttpResponseMessage PostRequest(string apiUrl, TokenRequest request)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    //httpClient.DefaultRequestHeaders.Clear();
                    //httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");



                    //var content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");

                    ////byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(json);
                    ////var content = new ByteArrayContent(messageBytes);
                    ////content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    //HttpResponseMessage response = httpClient.PostAsync(apiUrl, content).Result;

                    //if (!response.IsSuccessStatusCode)
                    //{
                    //    var result = response.Content.ReadAsStringAsync().Result;
                    //    HttpResponseMessage responsem = new HttpResponseMessage { ReasonPhrase = result, StatusCode= System.Net.HttpStatusCode.ServiceUnavailable };
                    //    return responsem;
                    //}


                  //  var task = SendtokenAsync(request, apiUrl);
                    //var result = RunTask(SendtokenAsync).Result;
                    Task<HttpResponseMessage> task = Task.Run(async () => await SendtokenAsync(request, apiUrl));

                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<HttpResponseMessage> SendtokenAsync(TokenRequest request, string url)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", request.grant_type);
            dict.Add("username", request.username);
            dict.Add("password", request.password);
            dict.Add("client_id", request.client_id);
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);
            return res;
        }
    }
}
