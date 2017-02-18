using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RecipeShelf.Common.Models;

namespace RecipeShelf.Cache.Models
{
    public sealed class RecipeFilter
    {
        [JsonProperty("collections", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Collections { get; set; }

        [JsonProperty("cuisines", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Cuisines { get; set; }

        [JsonProperty("ingredientIds", NullValueHandling = NullValueHandling.Ignore)]
        public string[] IngredientIds { get; set; }

        [JsonProperty("overnightPreparation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OvernightPreparation { get; set; }

        [JsonProperty("regions", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Regions { get; set; }

        [JsonProperty("spiceLevels", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public SpiceLevel[] SpiceLevels { get; set; }

        [JsonProperty("totalTimes", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public TotalTime[] TotalTimes { get; set; }

        [JsonProperty("vegan", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Vegan { get; set; }
    }
}