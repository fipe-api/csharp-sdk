using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fipe.Api.Br
{
    /// <summary>
    /// Client for the FIPE API (https://fipe.api.br), which provides
    /// average vehicle prices from Brazil's Tabela FIPE.
    /// </summary>
    public sealed class FipeClient
    {
        /// <summary>The production endpoint of the FIPE API.</summary>
        public const string DefaultBaseUrl = "https://fipe.api.br/api/v2";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string? _subscriptionToken;

        /// <summary>Creates a client using its own <see cref="HttpClient"/>.</summary>
        public FipeClient(string? subscriptionToken = null)
            : this(new HttpClient(), subscriptionToken)
        {
        }

        /// <summary>
        /// Creates a client using the given <see cref="HttpClient"/> — pass one from
        /// IHttpClientFactory in DI scenarios.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for requests. Not disposed by this class.</param>
        /// <param name="subscriptionToken">
        /// Optional X-Subscription-Token sent on every request. The free tier works
        /// without a token but has stricter rate limits.
        /// </param>
        /// <param name="baseUrl">Overrides the API base URL (e.g. for testing).</param>
        public FipeClient(HttpClient httpClient, string? subscriptionToken = null, string? baseUrl = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _subscriptionToken = subscriptionToken;
            _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
        }

        /// <summary>Lists the FIPE monthly reference tables, newest first.</summary>
        public Task<IReadOnlyList<Reference>> GetReferencesAsync(CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Reference>>("/references", null, cancellationToken);

        /// <summary>Lists the brands available for a vehicle type.</summary>
        public Task<IReadOnlyList<Brand>> GetBrandsAsync(VehicleType vehicleType, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Brand>>($"/{vehicleType.ToPathSegment()}/brands", reference, cancellationToken);

        /// <summary>Lists the models of a brand.</summary>
        public Task<IReadOnlyList<Model>> GetModelsAsync(VehicleType vehicleType, string brandId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Model>>($"/{vehicleType.ToPathSegment()}/brands/{Escape(brandId)}/models", reference, cancellationToken);

        /// <summary>Lists the model-year variants of a model.</summary>
        public Task<IReadOnlyList<Year>> GetYearsAsync(VehicleType vehicleType, string brandId, string modelId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Year>>($"/{vehicleType.ToPathSegment()}/brands/{Escape(brandId)}/models/{Escape(modelId)}/years", reference, cancellationToken);

        /// <summary>Returns the FIPE price details for a brand, model and year.</summary>
        public Task<Vehicle> GetVehicleAsync(VehicleType vehicleType, string brandId, string modelId, string yearId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<Vehicle>($"/{vehicleType.ToPathSegment()}/brands/{Escape(brandId)}/models/{Escape(modelId)}/years/{Escape(yearId)}", reference, cancellationToken);

        /// <summary>Lists all model years available for a brand.</summary>
        public Task<IReadOnlyList<Year>> GetYearsByBrandAsync(VehicleType vehicleType, string brandId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Year>>($"/{vehicleType.ToPathSegment()}/brands/{Escape(brandId)}/years", reference, cancellationToken);

        /// <summary>Lists the models of a brand available for a given year.</summary>
        public Task<IReadOnlyList<Model>> GetModelsByBrandYearAsync(VehicleType vehicleType, string brandId, string yearId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Model>>($"/{vehicleType.ToPathSegment()}/brands/{Escape(brandId)}/years/{Escape(yearId)}/models", reference, cancellationToken);

        /// <summary>Lists the model-year variants of a vehicle by its FIPE code (e.g. "005340-6").</summary>
        public Task<IReadOnlyList<Year>> GetYearsByFipeCodeAsync(VehicleType vehicleType, string fipeCode, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<IReadOnlyList<Year>>($"/{vehicleType.ToPathSegment()}/{Escape(fipeCode)}/years", reference, cancellationToken);

        /// <summary>Returns the FIPE price details for a vehicle by its FIPE code and year.</summary>
        public Task<Vehicle> GetVehicleByFipeCodeAsync(VehicleType vehicleType, string fipeCode, string yearId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<Vehicle>($"/{vehicleType.ToPathSegment()}/{Escape(fipeCode)}/years/{Escape(yearId)}", reference, cancellationToken);

        /// <summary>Returns the vehicle details including its price history across reference months.</summary>
        public Task<Vehicle> GetHistoryByFipeCodeAsync(VehicleType vehicleType, string fipeCode, string yearId, int? reference = null, CancellationToken cancellationToken = default) =>
            GetAsync<Vehicle>($"/{vehicleType.ToPathSegment()}/{Escape(fipeCode)}/years/{Escape(yearId)}/history", reference, cancellationToken);

        private static string Escape(string segment) => Uri.EscapeDataString(segment);

        private async Task<T> GetAsync<T>(string path, int? reference, CancellationToken cancellationToken)
        {
            var url = _baseUrl + path;
            if (reference.HasValue)
            {
                url += "?reference=" + reference.Value;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(_subscriptionToken))
                {
                    request.Headers.Add("X-Subscription-Token", _subscriptionToken);
                }

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new FipeApiException(response.StatusCode, body.Trim());
                    }

                    return JsonSerializer.Deserialize<T>(body, JsonOptions)
                        ?? throw new FipeApiException(response.StatusCode, "empty response body");
                }
            }
        }
    }
}
