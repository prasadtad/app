using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RecipeShelf.Common.Models
{
    public sealed class Ingredient
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(IdConverter))]
        public Id Id { get; set; }

        [JsonProperty("lastModified")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModified { get; set; }

        [JsonProperty("names")]
        public string[] Names { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("vegan")]
        public bool Vegan { get; set; }
    }
}