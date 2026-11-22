// Demonstrates the FIPE SDK against the live API: lists car brands,
// drills into VW Amarok models and years, prints the current price,
// and looks up a license plate (first argument, default "ABC1234").
// Set FIPE_TOKEN to use a subscription token.
using Fipe.Api.Br;

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var token = Environment.GetEnvironmentVariable("FIPE_TOKEN");
var client = new FipeClient(string.IsNullOrWhiteSpace(token) ? null : token);

var brands = await client.GetBrandsAsync(VehicleType.Cars, cancellationToken: cts.Token);
Console.WriteLine($"{brands.Count} car brands available. First 5:");
foreach (var b in brands.Take(5))
{
    Console.WriteLine($"  {b.Code}: {b.Name}");
}

var vw = brands.FirstOrDefault(b => b.Name.Contains("VolksWagen"))
    ?? throw new InvalidOperationException("VolksWagen not found in brand list");

var models = await client.GetModelsAsync(VehicleType.Cars, vw.Code, cancellationToken: cts.Token);
var amarok = models.FirstOrDefault(m => m.Name.Contains("AMAROK"))
    ?? throw new InvalidOperationException("AMAROK not found in model list");
Console.WriteLine($"\nModel: {amarok.Name} (code {amarok.Code})");

var years = await client.GetYearsAsync(VehicleType.Cars, vw.Code, amarok.Code, cancellationToken: cts.Token);
Console.WriteLine($"Years available: {years.Count}");

var vehicle = await client.GetVehicleAsync(VehicleType.Cars, vw.Code, amarok.Code, years[0].Code, cancellationToken: cts.Token);
Console.WriteLine($"\n{vehicle.Model} {vehicle.ModelYear} ({vehicle.Fuel})");
Console.WriteLine($"FIPE code: {vehicle.CodeFipe}");
Console.WriteLine($"Price: {vehicle.Price} ({vehicle.ReferenceMonth})");

var plate = args.Length > 0 ? args[0] : "ABC1234";
try
{
    var plateVehicle = await client.GetVehicleByPlateAsync(plate, cts.Token);
    Console.WriteLine($"\nPlate {plateVehicle.Plate}: {plateVehicle.Brand} {plateVehicle.Model} {plateVehicle.Year}");
    Console.WriteLine($"Color: {plateVehicle.Color}, fuel: {plateVehicle.Fuel}, registered in {plateVehicle.City}/{plateVehicle.State}");
    foreach (var price in plateVehicle.FipePrices)
    {
        Console.WriteLine($"  {price.CodeFipe} {price.Model}: {price.Price}");
    }
    Console.WriteLine($"Plate quota: {plateVehicle.Quota.Remaining}/{plateVehicle.Quota.Limit} left this month");
}
catch (FipeApiException ex)
{
    Console.WriteLine($"\nPlate lookup for {plate} failed: {ex.Message}");
}
