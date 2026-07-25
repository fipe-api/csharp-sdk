// Demonstrates the FIPE SDK against the live API: lists car brands,
// drills into VW Amarok models and years, and prints the current price.
using Fipe.Api.Br;

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var client = new FipeClient();

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
