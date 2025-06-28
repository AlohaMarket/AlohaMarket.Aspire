using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aloha.PostService.Models.Enums
{
    public enum LocationLevel
    {
        Province,
        District,
        Ward
    }
    public static class LocationLevelExtensions
    {
        public static string ToFriendlyString(this LocationLevel level)
        {
            return level switch
            {
                LocationLevel.Province => "province",
                LocationLevel.District => "district",
                LocationLevel.Ward => "ward",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
    }
}