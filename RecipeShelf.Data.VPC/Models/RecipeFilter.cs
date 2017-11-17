using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RecipeShelf.Common.Models;

namespace RecipeShelf.Data.VPC.Models
{
    public struct RecipeFilter
    {
        [JsonProperty("collections", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Collections { get; }

        [JsonProperty("cuisines", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Cuisines { get; }

        [JsonProperty("ingredientIds", NullValueHandling = NullValueHandling.Ignore)]
        public string[] IngredientIds { get; }

        [JsonProperty("overnightPreparation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OvernightPreparation { get; }

        [JsonProperty("regions", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Regions { get; }

        [JsonProperty("spiceLevels", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public SpiceLevel[] SpiceLevels { get; }

        [JsonProperty("totalTimes", NullValueHandling = NullValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public TotalTime[] TotalTimes { get; }

        [JsonProperty("vegan", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Vegan { get; }

        public RecipeFilter(string[] collections, string[] cuisines, string[] ingredientIds, bool? overnightPreparation, string[] regions, SpiceLevel[] spiceLevels, TotalTime[] totalTimes, bool? vegan)
        {
            Collections = collections;
            Cuisines = cuisines;
            IngredientIds = ingredientIds;
            OvernightPreparation = overnightPreparation;
            Regions = regions;
            SpiceLevels = spiceLevels;
            TotalTimes = totalTimes;
            Vegan = vegan;
        }
    }
}