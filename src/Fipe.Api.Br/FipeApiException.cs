using System;
using System.Net;

namespace Fipe.Api.Br
{
    /// <summary>Thrown when the FIPE API responds with a non-success status code.</summary>
    public sealed class FipeApiException : Exception
    {
        /// <summary>Creates the exception for a failed API response.</summary>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="body">The raw response body.</param>
        public FipeApiException(HttpStatusCode statusCode, string body)
            : base($"FIPE API returned {(int)statusCode} ({statusCode}){(string.IsNullOrEmpty(body) ? "" : ": " + body)}")
        {
            StatusCode = statusCode;
            Body = body;
        }

        /// <summary>The HTTP status code returned by the API (e.g. 404, 429, 500).</summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>The raw response body, if any.</summary>
        public string Body { get; }

        /// <summary>True when the requested resource does not exist (HTTP 404).</summary>
        public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

        /// <summary>True when the client is being rate limited (HTTP 429).</summary>
        public bool IsRateLimited => (int)StatusCode == 429;
    }
}
