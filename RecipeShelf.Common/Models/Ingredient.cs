using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RecipeShelf.Common.Models
{
    public struct Ingredient : IEquatable<Ingredient>
    {
        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("lastModified")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModified { get; }

        [JsonProperty("names")]
        public string[] Names { get; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; }

        [JsonProperty("vegan")]
        public bool Vegan { get; }

        public Ingredient(string id, DateTime lastModified, string[] names, string description, string category, bool vegan)
        {
            Id = id;
            LastModified = lastModified;
            Names = names;
            Description = description;
            Category = category;
            Vegan = vegan;
        }

        public Ingredient With(DateTime? lastModified = null, string[] names = null, string description = null, string category = null, bool? vegan = null)
        {
            return new Ingredient(Id, lastModified ?? LastModified, names ?? Names, description ?? Description, category ?? Category, vegan ?? Vegan);
        }

        public bool Equals(Ingredient other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Ingredient)) return false;

            var other = (Ingredient)obj;
            return Id == other.Id &&
                   LastModified == other.LastModified &&
                   Names.EqualsAll(other.Names) &&
                   Description == other.Description &&
                   Category == other.Category &&
                   Vegan == other.Vegan;
        }

        public override int GetHashCode()
        {
            return Extensions.GetHashCode(Id, LastModified, Names, Description, Category, Vegan);
        }

        public static bool operator ==(Ingredient i1, Ingredient i2)
        {
            return i1.Equals(i2);
        }

        public static bool operator !=(Ingredient i1, Ingredient i2)
        {
            return !i1.Equals(i2);
        }
    }
}