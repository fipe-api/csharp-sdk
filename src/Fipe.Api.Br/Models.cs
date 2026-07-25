using System.Collections.Generic;
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
}
