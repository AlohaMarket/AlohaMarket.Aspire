using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aloha.LocationService.Models.Responses
{
    public class LocationValidationResult
    {
        public bool IsValid { get; set; }
        public string? ProvinceText { get; set; }
        public string? DistrictText { get; set; }
        public string? WardText { get; set; }
    }
}