using Newtonsoft.Json;
using OPP.AspNetCore.Common.Extensions;
using System;

namespace OPP.Entities.PM
{
    public class FuelEntity
    {
        public int VehicleId { get; set; }
        public string VehicleCaption { get; set; }
        public bool? VehicleIcon { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }

        public long DateTicks
        {
            get
            {
                return Date.ConvertUTCToUnix();
            }
        }

        public decimal Price { get; set; }
        public decimal Unit { get; set; }
        public FuelType FuelType { get; set; }
        public string CreatedBy { get; set; }
        public decimal? Odometer { get; set; }
    }

    public enum FuelType
    {
        Petrol = 1,
        Gasoline = 2,
        CNG = 3,
        LPG = 4
    }
}