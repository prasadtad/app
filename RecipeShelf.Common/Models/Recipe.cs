using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RecipeShelf.Common.Models
{
    public sealed class Recipe
    {
        [JsonProperty("id")]
        public RecipeId Id { get; set; }

        [JsonProperty("lastModified")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModified { get; set; }

        [JsonProperty("names")]
        public string[] Names { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("steps")]
        public RecipeItem[] Steps { get; set; }

        [JsonProperty("totalTimeInMinutes")]
        public int TotalTimeInMinutes { get; set; }

        [JsonIgnore]
        public TotalTime TotalTime
        {
            get
            {
                if (TotalTimeInMinutes <= 30) return TotalTime.Quick;
                if (TotalTimeInMinutes <= 60) return TotalTime.Regular;
                return TotalTime.Slow;
            }
        }

        [JsonProperty("servings")]
        public string Servings { get; set; }

        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("spiceLevel")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SpiceLevel SpiceLevel { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        [JsonProperty("chefId")]
        public string ChefId { get; set; }

        [JsonProperty("imageId", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageId { get; set; }

        [JsonProperty("ingredients")]
        public RecipeItem[] Ingredients { get; set; }

        [JsonProperty("cuisine", NullValueHandling = NullValueHandling.Ignore)]
        public string Cuisine { get; set; }

        [JsonProperty("ingredientIds")]
        public IngredientId[] IngredientIds { get; set; }

        [JsonProperty("overnightPreparation")]
        public bool OvernightPreparation { get; set; }

        [JsonProperty("accompanimentIds", NullValueHandling = NullValueHandling.Ignore)]
        public RecipeId[] AccompanimentIds { get; set; }

        [JsonProperty("collections", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Collections { get; set; }
    }

    public class RecipeItem
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("decorator")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Decorator Decorator { get; set; }
    }
}