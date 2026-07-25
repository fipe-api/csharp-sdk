using System;

namespace Fipe.Api.Br
{
    /// <summary>Category of vehicle being queried.</summary>
    public enum VehicleType
    {
        /// <summary>Passenger cars.</summary>
        Cars,

        /// <summary>Motorcycles.</summary>
        Motorcycles,

        /// <summary>Trucks.</summary>
        Trucks,
    }

    internal static class VehicleTypeExtensions
    {
        public static string ToPathSegment(this VehicleType vehicleType) => vehicleType switch
        {
            VehicleType.Cars => "cars",
            VehicleType.Motorcycles => "motorcycles",
            VehicleType.Trucks => "trucks",
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null),
        };
    }
}
