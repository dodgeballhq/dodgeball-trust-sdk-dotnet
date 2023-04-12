using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace Dodgeball.TrustServer.Api;

public static class QueryUtils
{
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

public class HttpQuery{
    public HttpQuery(string? baseUrl, string relativeUrl)
    {
        this.baseUrl = baseUrl;
        this.relativeUrl = relativeUrl;
    }
    
    
    private string baseUrl;
    private string? relativeUrl;
    private object data;
    private object body;
    private Dictionary<string, string?> headers = new Dictionary<string, string?>();
    private Dictionary<string, string?> parameters = new Dictionary<string, string?>();

    public async Task<DodgeballResponse> PostDodgeball()
    {
        try
        {
            var url = new Flurl.Url(this.Url).SetQueryParams(this.parameters);
            DodgeballResponse response = await url.WithHeaders(this.headers).PostJsonAsync(
                this.body).ReceiveJson<DodgeballResponse>();

            return new DodgeballResponse
            {
                success = false,
                errors = new DodgeballError[]
                {
                    new DodgeballError("NOT_IMPLEMENTED", "Must implement")
                }
            };
        }
        catch (Exception exc)
        {
            return QueryUtils.CreateErrorResponse(exc);
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
        get{
            string baseUrl = String.IsNullOrEmpty(this.baseUrl) ? "https://api.dodgeballhq.com" : this.baseUrl;
            if (String.IsNullOrEmpty(this.relativeUrl))
            {
                return baseUrl;
            }

            return String.Format("{0}/{1}", this.baseUrl.TrimEnd('/'), this.relativeUrl.TrimStart('/'));

        }
    }
}
