using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class LocationValidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public string ProvinceText { get; set; } = string.Empty;
        public string DistrictText { get; set; } = string.Empty;
        public string WardText { get; set; } = string.Empty;
    }

    public class LocationInvalidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}