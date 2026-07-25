# FIPE C# SDK

[![CI](https://github.com/fipe-api/csharp-sdk/actions/workflows/ci.yml/badge.svg)](https://github.com/fipe-api/csharp-sdk/actions/workflows/ci.yml)

A .NET client for the [FIPE API](https://fipe.api.br) (`/api/v2`), which provides average vehicle prices in the Brazilian market from Fundação Instituto de Pesquisas Econômicas (FIPE). Prices are updated monthly.

Targets `netstandard2.0` — works on .NET Core, .NET 5+, and .NET Framework 4.6.2+.

## Install

```sh
dotnet add package Fipe.Api.Br
```

## Quick start

```csharp
using Fipe.Api.Br;

var client = new FipeClient();

var brands = await client.GetBrandsAsync(VehicleType.Cars);
foreach (var brand in brands)
{
    Console.WriteLine($"{brand.Code} {brand.Name}");
}
```

Vehicle types: `VehicleType.Cars`, `VehicleType.Motorcycles`, `VehicleType.Trucks`.

## Authentication

The free tier works without a token but is rate limited. With a subscription token:

```csharp
var client = new FipeClient(subscriptionToken: "your-token");
```

In DI scenarios, pass an `HttpClient` from `IHttpClientFactory`:

```csharp
services.AddHttpClient<FipeClient>();
// or
var client = new FipeClient(httpClientFactory.CreateClient(), subscriptionToken: "your-token");
```

## Endpoints

Drill down brand → model → year → price:

```csharp
var brands  = await client.GetBrandsAsync(VehicleType.Cars);                          // GET /cars/brands
var models  = await client.GetModelsAsync(VehicleType.Cars, "59");                    // GET /cars/brands/59/models
var years   = await client.GetYearsAsync(VehicleType.Cars, "59", "5940");             // GET /cars/brands/59/models/5940/years
var vehicle = await client.GetVehicleAsync(VehicleType.Cars, "59", "5940", "2014-3"); // GET /cars/brands/59/models/5940/years/2014-3

Console.WriteLine($"{vehicle.Model}: {vehicle.Price}");
```

Browse by year:

```csharp
var years  = await client.GetYearsByBrandAsync(VehicleType.Cars, "59");                  // GET /cars/brands/59/years
var models = await client.GetModelsByBrandYearAsync(VehicleType.Cars, "59", "2014-3");   // GET /cars/brands/59/years/2014-3/models
```

Look up by FIPE code:

```csharp
var years   = await client.GetYearsByFipeCodeAsync(VehicleType.Cars, "005340-6");                  // GET /cars/005340-6/years
var vehicle = await client.GetVehicleByFipeCodeAsync(VehicleType.Cars, "005340-6", "2014-3");      // GET /cars/005340-6/years/2014-3
var history = await client.GetHistoryByFipeCodeAsync(VehicleType.Cars, "005340-6", "2014-3");      // GET /cars/005340-6/years/2014-3/history

foreach (var entry in history.PriceHistory)
{
    Console.WriteLine($"{entry.Month}: {entry.Price}");
}
```

### Reference months

Prices are published per monthly reference table. Every endpoint accepts an optional `reference` to query a past table:

```csharp
var references = await client.GetReferencesAsync(); // GET /references — e.g. Code "308", Month "abril de 2024"

var brands = await client.GetBrandsAsync(VehicleType.Cars, reference: 308);
```

## Error handling

Non-success responses throw `FipeApiException` with the status code and body:

```csharp
try
{
    var vehicle = await client.GetVehicleAsync(VehicleType.Cars, "59", "5940", "1900-1");
}
catch (FipeApiException ex) when (ex.IsNotFound)
{
    // unknown brand/model/year
}
catch (FipeApiException ex) when (ex.IsRateLimited)
{
    // rate limited — back off or use a subscription token
}
catch (FipeApiException ex)
{
    Console.WriteLine($"API returned {(int)ex.StatusCode}: {ex.Body}");
}
```

## Example program

A runnable example that lists brands and prints an Amarok price from the live API:

```sh
dotnet run --project examples/Fipe.Examples
```

## License

MIT
