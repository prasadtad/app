using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RecipeShelf.Common.Models
{
    public struct Recipe : IEquatable<Recipe>
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

        [JsonProperty("steps")]
        public RecipeItem[] Steps { get; }

        [JsonProperty("totalTimeInMinutes")]
        public int TotalTimeInMinutes { get; }

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
        public string Servings { get; }

        [JsonProperty("approved")]
        public bool Approved { get; }

        [JsonProperty("spiceLevel")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SpiceLevel SpiceLevel { get; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; }

        [JsonProperty("chefId")]
        public string ChefId { get; }

        [JsonProperty("imageId", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageId { get; }

        [JsonProperty("ingredients")]
        public RecipeItem[] Ingredients { get; }

        [JsonProperty("cuisine", NullValueHandling = NullValueHandling.Ignore)]
        public string Cuisine { get; }

        [JsonProperty("ingredientIds")]
        public string[] IngredientIds { get; }

        [JsonProperty("overnightPreparation")]
        public bool OvernightPreparation { get; }

        [JsonProperty("accompanimentIds", NullValueHandling = NullValueHandling.Ignore)]
        public string[] AccompanimentIds { get; }

        [JsonProperty("collections", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Collections { get; }

        public Recipe(string id, 
                      DateTime lastModified, 
                      string[] names, 
                      string description, 
                      RecipeItem[] steps, 
                      int totalTimeInMinutes, 
                      string servings, 
                      bool approved, 
                      SpiceLevel spiceLevel, 
                      string region, 
                      string chefId,
                      string imageId,
                      RecipeItem[] ingredients,
                      string cuisine,
                      string[] ingredientIds,
                      bool overnightPreparation,
                      string[] accompanimentIds,
                      string[] collections)
        {
            Id = id;
            LastModified = lastModified;
            Names = names;
            Description = description;
            Steps = steps;
            TotalTimeInMinutes = totalTimeInMinutes;
            Servings = servings;
            Approved = approved;
            SpiceLevel = spiceLevel;
            Region = region;
            ChefId = chefId;
            ImageId = imageId;
            Ingredients = ingredients;
            Cuisine = cuisine;
            IngredientIds = ingredientIds;
            OvernightPreparation = overnightPreparation;
            AccompanimentIds = accompanimentIds;
            Collections = collections;
        }

        public bool Equals(Recipe other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Recipe)) return false;

            var other = (Recipe)obj;
            return Id == other.Id &&
                   LastModified == other.LastModified &&
                   Names.EqualsAll(other.Names) &&
                   Description == other.Description &&
                   Steps.EqualsAll(other.Steps) &&
                   TotalTimeInMinutes == other.TotalTimeInMinutes &&
                   Servings == other.Servings &&
                   Approved == other.Approved &&
                   SpiceLevel == other.SpiceLevel &&
                   Region == other.Region &&
                   ChefId == other.ChefId &&
                   ImageId == other.ImageId &&
                   Ingredients.EqualsAll(other.Ingredients) &&
                   Cuisine == other.Cuisine &&
                   IngredientIds.EqualsAll(other.IngredientIds) &&
                   OvernightPreparation == other.OvernightPreparation &&
                   AccompanimentIds.EqualsAll(other.AccompanimentIds) &&
                   Collections.EqualsAll(other.Collections);
        }

        public override int GetHashCode()
        {
            return Extensions.GetHashCode(Id, 
                                          LastModified, 
                                          Names, 
                                          Description,
                                          Steps,
                                          TotalTimeInMinutes,
                                          Servings,
                                          Approved,
                                          SpiceLevel,
                                          Region,
                                          ChefId,
                                          ImageId,
                                          Ingredients,
                                          Cuisine,
                                          IngredientIds,
                                          OvernightPreparation,
                                          AccompanimentIds,
                                          Collections);
        }

        public static bool operator ==(Recipe r1, Recipe r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(Recipe r1, Recipe r2)
        {
            return !r1.Equals(r2);
        }
    }

    public struct RecipeItem : IEquatable<RecipeItem>
    {
        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("decorator")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Decorator Decorator { get; }

        public RecipeItem(string text, Decorator decorator)
        {
            Text = text;
            Decorator = decorator;
        }

        public bool Equals(RecipeItem other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RecipeItem)) return false;

            var other = (RecipeItem)obj;
            return Text == other.Text &&
                   Decorator == other.Decorator;
        }

        public override int GetHashCode()
        {
            return Extensions.GetHashCode(Text, Decorator);
        }

        public static bool operator ==(RecipeItem ri1, RecipeItem ri2)
        {
            return ri1.Equals(ri2);
        }

        public static bool operator !=(RecipeItem ri1, RecipeItem ri2)
        {
            return !ri1.Equals(ri2);
        }
    }
}