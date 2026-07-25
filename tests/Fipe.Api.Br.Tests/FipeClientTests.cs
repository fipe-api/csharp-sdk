using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fipe.Api.Br.Tests
{
    public sealed class StubHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public string Body { get; set; } = "[]";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(Status)
            {
                Content = new StringContent(Body),
            };
            return Task.FromResult(response);
        }
    }

    public class FipeClientTests
    {
        private const string VehicleJson = @"{
            ""brand"": ""VW - VolksWagen"",
            ""codeFipe"": ""005340-6"",
            ""fuel"": ""Diesel"",
            ""fuelAcronym"": ""D"",
            ""model"": ""AMAROK High.CD 2.0 16V TDI 4x4 Dies. Aut"",
            ""modelYear"": 2014,
            ""price"": ""R$ 10.000,00"",
            ""referenceMonth"": ""abril de 2024"",
            ""vehicleType"": 1
        }";

        private static (FipeClient Client, StubHandler Handler) NewClient(string? token = null)
        {
            var handler = new StubHandler();
            var client = new FipeClient(new HttpClient(handler), token, "http://test.local/api/v2");
            return (client, handler);
        }

        [Theory]
        [InlineData("References", "/api/v2/references")]
        [InlineData("Brands", "/api/v2/cars/brands")]
        [InlineData("Models", "/api/v2/cars/brands/23/models")]
        [InlineData("Years", "/api/v2/cars/brands/23/models/5585/years")]
        [InlineData("YearsByBrand", "/api/v2/motorcycles/brands/23/years")]
        [InlineData("ModelsByBrandYear", "/api/v2/cars/brands/23/years/2022-3/models")]
        [InlineData("YearsByFipeCode", "/api/v2/cars/005340-6/years")]
        public async Task ListEndpoints_BuildCorrectPath(string operation, string expectedPath)
        {
            var (client, handler) = NewClient();

            Task<object?> Call() => operation switch
            {
                "References" => client.GetReferencesAsync().ContinueWith(t => (object?)t.Result),
                "Brands" => client.GetBrandsAsync(VehicleType.Cars).ContinueWith(t => (object?)t.Result),
                "Models" => client.GetModelsAsync(VehicleType.Cars, "23").ContinueWith(t => (object?)t.Result),
                "Years" => client.GetYearsAsync(VehicleType.Cars, "23", "5585").ContinueWith(t => (object?)t.Result),
                "YearsByBrand" => client.GetYearsByBrandAsync(VehicleType.Motorcycles, "23").ContinueWith(t => (object?)t.Result),
                "ModelsByBrandYear" => client.GetModelsByBrandYearAsync(VehicleType.Cars, "23", "2022-3").ContinueWith(t => (object?)t.Result),
                "YearsByFipeCode" => client.GetYearsByFipeCodeAsync(VehicleType.Cars, "005340-6").ContinueWith(t => (object?)t.Result),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };

            await Call();

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(expectedPath, handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
            Assert.Contains("application/json", handler.LastRequest.Headers.Accept.ToString());
        }

        [Theory]
        [InlineData("Vehicle", "/api/v2/cars/brands/23/models/5585/years/2022-3")]
        [InlineData("VehicleByFipeCode", "/api/v2/cars/005340-6/years/2022-3")]
        [InlineData("HistoryByFipeCode", "/api/v2/cars/005340-6/years/2022-3/history")]
        public async Task VehicleEndpoints_BuildCorrectPathAndDeserialize(string operation, string expectedPath)
        {
            var (client, handler) = NewClient();
            handler.Body = VehicleJson;

            var vehicle = operation switch
            {
                "Vehicle" => await client.GetVehicleAsync(VehicleType.Cars, "23", "5585", "2022-3"),
                "VehicleByFipeCode" => await client.GetVehicleByFipeCodeAsync(VehicleType.Cars, "005340-6", "2022-3"),
                "HistoryByFipeCode" => await client.GetHistoryByFipeCodeAsync(VehicleType.Cars, "005340-6", "2022-3"),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };

            Assert.Equal(expectedPath, handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal("VW - VolksWagen", vehicle.Brand);
            Assert.Equal("005340-6", vehicle.CodeFipe);
            Assert.Equal(2014, vehicle.ModelYear);
            Assert.Equal("R$ 10.000,00", vehicle.Price);
        }

        [Fact]
        public async Task Reference_IsSentAsQueryParameter()
        {
            var (client, handler) = NewClient();

            await client.GetBrandsAsync(VehicleType.Trucks, reference: 308);

            Assert.Equal("?reference=308", handler.LastRequest!.RequestUri!.Query);
        }

        [Fact]
        public async Task Reference_OmittedByDefault()
        {
            var (client, handler) = NewClient();

            await client.GetBrandsAsync(VehicleType.Cars);

            Assert.Equal("", handler.LastRequest!.RequestUri!.Query);
        }

        [Fact]
        public async Task SubscriptionToken_SentWhenConfigured()
        {
            var (client, handler) = NewClient(token: "secret");

            await client.GetReferencesAsync();

            Assert.Equal("secret", Assert.Single(handler.LastRequest!.Headers.GetValues("X-Subscription-Token")));
        }

        [Fact]
        public async Task SubscriptionToken_OmittedWhenNotConfigured()
        {
            var (client, handler) = NewClient();

            await client.GetReferencesAsync();

            Assert.False(handler.LastRequest!.Headers.Contains("X-Subscription-Token"));
        }

        [Fact]
        public async Task ListResponse_Deserializes()
        {
            var (client, handler) = NewClient();
            handler.Body = @"[{""code"": ""23"", ""name"": ""VW - VolksWagen""}]";

            var brands = await client.GetBrandsAsync(VehicleType.Cars);

            var brand = Assert.Single(brands);
            Assert.Equal("23", brand.Code);
            Assert.Equal("VW - VolksWagen", brand.Name);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, true, false)]
        [InlineData((HttpStatusCode)429, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, false, false)]
        public async Task ErrorStatus_ThrowsFipeApiException(HttpStatusCode status, bool isNotFound, bool isRateLimited)
        {
            var (client, handler) = NewClient();
            handler.Status = status;
            handler.Body = "error body";

            var ex = await Assert.ThrowsAsync<FipeApiException>(() => client.GetBrandsAsync(VehicleType.Cars));

            Assert.Equal(status, ex.StatusCode);
            Assert.Equal("error body", ex.Body);
            Assert.Equal(isNotFound, ex.IsNotFound);
            Assert.Equal(isRateLimited, ex.IsRateLimited);
        }

        [Fact]
        public async Task InvalidJson_ThrowsJsonException()
        {
            var (client, handler) = NewClient();
            handler.Body = "not json";

            await Assert.ThrowsAsync<JsonException>(() => client.GetReferencesAsync());
        }

        [Fact]
        public async Task PriceHistory_Deserializes()
        {
            var (client, handler) = NewClient();
            handler.Body = @"{
                ""brand"": ""VW - VolksWagen"",
                ""priceHistory"": [{""month"": ""abril de 2024"", ""price"": ""R$ 10.000,00"", ""reference"": ""308""}]
            }";

            var vehicle = await client.GetHistoryByFipeCodeAsync(VehicleType.Cars, "005340-6", "2022-3");

            var entry = Assert.Single(vehicle.PriceHistory);
            Assert.Equal("abril de 2024", entry.Month);
            Assert.Equal("R$ 10.000,00", entry.Price);
            Assert.Equal("308", entry.Reference);
        }

        [Fact]
        public void NullHttpClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FipeClient((HttpClient)null!));
        }
    }
}
