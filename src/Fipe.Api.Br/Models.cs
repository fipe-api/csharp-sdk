using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Fipe.Api.Br
{
    /// <summary>
    /// A FIPE monthly reference table. Prices are published per reference month;
    /// pass a reference code to query historical tables.
    /// </summary>
    public sealed class Reference
    {
        /// <summary>Reference table code, e.g. "308".</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        /// <summary>Reference month, e.g. "abril de 2024".</summary>
        [JsonPropertyName("month")]
        public string Month { get; set; } = "";
    }

    /// <summary>A vehicle manufacturer, e.g. "VW - VolksWagen".</summary>
    public sealed class Brand
    {
        /// <summary>Brand code, e.g. "23".</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        /// <summary>Brand name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    /// <summary>A vehicle model within a brand.</summary>
    public sealed class Model
    {
        /// <summary>Model code, e.g. "5585".</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        /// <summary>Model name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    /// <summary>A model year variant. Code combines year and fuel, e.g. "2022-3".</summary>
    public sealed class Year
    {
        /// <summary>Year code, e.g. "2022-3".</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        /// <summary>Year name, e.g. "2022 Diesel".</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    /// <summary>FIPE price details for a specific model year.</summary>
    public sealed class Vehicle
    {
        /// <summary>Brand name.</summary>
        [JsonPropertyName("brand")]
        public string Brand { get; set; } = "";

        /// <summary>Unique FIPE identifier, e.g. "005340-6".</summary>
        [JsonPropertyName("codeFipe")]
        public string CodeFipe { get; set; } = "";

        /// <summary>Fuel used by the vehicle, e.g. "Diesel".</summary>
        [JsonPropertyName("fuel")]
        public string Fuel { get; set; } = "";

        /// <summary>Fuel acronym, e.g. "D".</summary>
        [JsonPropertyName("fuelAcronym")]
        public string FuelAcronym { get; set; } = "";

        /// <summary>Model name.</summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        /// <summary>Manufacturing year of the vehicle.</summary>
        [JsonPropertyName("modelYear")]
        public int ModelYear { get; set; }

        /// <summary>Price in Brazilian Real, e.g. "R$ 10.000,00".</summary>
        [JsonPropertyName("price")]
        public string Price { get; set; } = "";

        /// <summary>Price history across reference months. Populated by the history endpoint.</summary>
        [JsonPropertyName("priceHistory")]
        public IReadOnlyList<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();

        /// <summary>Month of the price, e.g. "abril de 2024".</summary>
        [JsonPropertyName("referenceMonth")]
        public string ReferenceMonth { get; set; } = "";

        /// <summary>Numeric vehicle type code as returned by the API (1 = car).</summary>
        [JsonPropertyName("vehicleType")]
        public int VehicleTypeCode { get; set; }
    }

    /// <summary>One entry of a vehicle's price across reference months.</summary>
    public sealed class PriceHistory
    {
        /// <summary>Reference month, e.g. "abril de 2024".</summary>
        [JsonPropertyName("month")]
        public string Month { get; set; } = "";

        /// <summary>Price in Brazilian Real, e.g. "R$ 10.000,00".</summary>
        [JsonPropertyName("price")]
        public string Price { get; set; } = "";

        /// <summary>Reference table code, e.g. "308".</summary>
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = "";
    }

    /// <summary>Vehicle data and FIPE prices for a license plate.</summary>
    public sealed class PlateVehicle
    {
        /// <summary>Normalized plate, e.g. "ABC1234".</summary>
        [JsonPropertyName("plate")]
        public string Plate { get; set; } = "";

        /// <summary>Brand name, e.g. "Volkswagen".</summary>
        [JsonPropertyName("brand")]
        public string Brand { get; set; } = "";

        /// <summary>Model name, e.g. "SANTANA CG".</summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        /// <summary>Vehicle year, e.g. "1986".</summary>
        [JsonPropertyName("year")]
        public string Year { get; set; } = "";

        /// <summary>Vehicle color, e.g. "Vermelha".</summary>
        [JsonPropertyName("color")]
        public string Color { get; set; } = "";

        /// <summary>Fuel, e.g. "Álcool".</summary>
        [JsonPropertyName("fuel")]
        public string Fuel { get; set; } = "";

        /// <summary>Engine power, e.g. "94 cv".</summary>
        [JsonPropertyName("power")]
        public string Power { get; set; } = "";

        /// <summary>Vehicle segment, e.g. "Auto".</summary>
        [JsonPropertyName("segment")]
        public string Segment { get; set; } = "";

        /// <summary>Chassis number with the first characters hidden, e.g. "*****P246344".</summary>
        [JsonPropertyName("chassis")]
        public string Chassis { get; set; } = "";

        /// <summary>City of registration, e.g. "LOBATO".</summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = "";

        /// <summary>State of registration, e.g. "PR".</summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = "";

        /// <summary>True when the vehicle is imported.</summary>
        [JsonPropertyName("imported")]
        public bool Imported { get; set; }

        /// <summary>FIPE prices for the models that match the vehicle.</summary>
        [JsonPropertyName("fipePrices")]
        public IReadOnlyList<PlateFipePrice> FipePrices
        {
            get => _fipePrices;
            set => _fipePrices = value ?? new List<PlateFipePrice>();
        }

        private IReadOnlyList<PlateFipePrice> _fipePrices = new List<PlateFipePrice>();

        /// <summary>Monthly plate quota from the response headers.</summary>
        [JsonIgnore]
        public PlateQuota Quota { get; set; } = new PlateQuota();
    }

    /// <summary>A FIPE price for one model that matches a plate.</summary>
    public sealed class PlateFipePrice
    {
        /// <summary>FIPE code, e.g. "005102-0".</summary>
        [JsonPropertyName("codeFipe")]
        public string CodeFipe { get; set; } = "";

        /// <summary>Model name in the FIPE table, e.g. "Santana CS/CD/CG".</summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        /// <summary>Price in Brazilian Real, e.g. "R$ 8.848,00".</summary>
        [JsonPropertyName("price")]
        public string Price { get; set; } = "";
    }

    /// <summary>
    /// Monthly plate quota. A value is null when the API does not send the
    /// X-Plate-Quota-Limit or X-Plate-Quota-Remaining header.
    /// </summary>
    public sealed class PlateQuota
    {
        private const string LimitHeader = "X-Plate-Quota-Limit";
        private const string RemainingHeader = "X-Plate-Quota-Remaining";

        /// <summary>Number of plate lookups allowed this month.</summary>
        public int? Limit { get; set; }

        /// <summary>Number of plate lookups left this month.</summary>
        public int? Remaining { get; set; }

        internal static PlateQuota FromHeaders(HttpHeaders headers) => new PlateQuota
        {
            Limit = ReadInt(headers, LimitHeader),
            Remaining = ReadInt(headers, RemainingHeader),
        };

        private static int? ReadInt(HttpHeaders headers, string name)
        {
            if (!headers.TryGetValues(name, out var values))
            {
                return null;
            }

            return int.TryParse(values.FirstOrDefault(), out var value) ? value : (int?)null;
        }
    }
}
