using Newtonsoft.Json;

namespace Dodgeball.TrustServer.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    
    public static class QueryUtils
    {
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
        }

        public static DodgeballResponse CreateErrorResponse(Exception exc)
        {
            var category = exc.GetType().Name;
            return new DodgeballResponse
            {
                success = false,
                errors = new DodgeballError[]
                {
                    new DodgeballError(category, exc.Message)
                }
            };
        }
    }

    public class HttpQuery
    {
        public HttpQuery(string? baseUrl, string relativeUrl)
        {
            this.baseUrl = baseUrl;
            this.relativeUrl = relativeUrl;
        }


        private string baseUrl;
        private string? relativeUrl;
        private object? data;
        private object? body;
        private Dictionary<string, string?> headers = new Dictionary<string, string?>();
        private Dictionary<string, string?> parameters = new Dictionary<string, string?>();

        public async Task<DodgeballResponse> PostDodgeball()
        {
            try
            {
                var url = new Flurl.Url(this.Url).SetQueryParams(this.parameters);
                var rawResponse = await url.WithHeaders(this.headers).PostJsonAsync(
                    this.body);
                
                var responseString = await rawResponse.GetStringAsync();
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<DodgeballResponse>(
                        responseString);

                return response;
            }
            catch (Exception exc)
            {
                return QueryUtils.CreateErrorResponse(exc);
            }
        }

        public async Task<DodgeballCheckpointResponse> PostCheckpoint()
        {
            try
            {
                var url = new Flurl.Url(this.Url).SetQueryParams(this.parameters);
                /*
                 * Enable this block for deep debugging purposes
                var innerResponse = await url.WithHeaders(this.headers).PostJsonAsync(
                    this.body).ReceiveJson();
                
                Console.WriteLine(innerResponse);
                */
                var rawResponse = await url.WithHeaders(this.headers).PostJsonAsync(
                    this.body);

                var responseString = await rawResponse.GetStringAsync();
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<DodgeballCheckpointResponse>(
                    responseString);
                
                return response;
            }
            catch (Exception exc)
            {
                var internalOnly = QueryUtils.CreateErrorResponse(exc);
                return new DodgeballCheckpointResponse
                {
                    errors = internalOnly.errors,
                    success = false
                };
            }
        }

        public async Task<DodgeballCheckpointResponse> GetVerification()
        {
            try
            {
                var url = new Flurl.Url(this.Url).SetQueryParams(this.parameters);
                var responseString = await url.WithHeaders(this.headers).GetStringAsync();
                var response = JsonConvert.DeserializeObject<DodgeballCheckpointResponse>(
                    responseString);

                return response;
            }
            catch (Exception exc)
            {
                var internalOnly = QueryUtils.CreateErrorResponse(exc);
                return new DodgeballCheckpointResponse
                {
                    errors = internalOnly.errors,
                    success = false
                };
            }
        }


        public HttpQuery SetBody(object body)
        {
            this.body = body;
            return this;
        }

        public HttpQuery SetData(object data)
        {
            this.data = data;
            return this;
        }

        public HttpQuery SetHeader(string key, string? value)
        {
            this.headers[key] = value;
            return this;
        }

        public HttpQuery SetHeaders(Dictionary<string, string?> headers)
        {
            this.headers = headers;
            return this;
        }

        public HttpQuery SetParameter(string key, string? value)
        {
            this.parameters[key] = value;
            return this;
        }

        public string Url
        {
            get
            {
                string baseUrl = String.IsNullOrEmpty(this.baseUrl) ? "https://api.dodgeballhq.com" : this.baseUrl;
                if (String.IsNullOrEmpty(this.relativeUrl))
                {
                    return baseUrl;
                }

                return String.Format("{0}/{1}", this.baseUrl.TrimEnd('/'), this.relativeUrl.TrimStart('/'));

            }
        }
    }
}