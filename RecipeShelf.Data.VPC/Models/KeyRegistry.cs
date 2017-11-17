using RecipeShelf.Common.Models;

namespace RecipeShelf.Data.VPC.Models
{
    public sealed class RecipeKeys
    {
        public const KeyType Type = KeyType.Recipe;

        public readonly string Names = Type.Append("Names");
        public readonly string ChefId = Type.Append("ChefId");
        public readonly string Collection = Type.Append("Collection");
        public readonly string Cuisine = Type.Append("Cuisine");
        public readonly string IngredientId = Type.Append("IngredientId");
        public readonly string OvernightPreparation = Type.Append("OvernightPreparation");
        public readonly string Region = Type.Append("Region");
        public readonly string SpiceLevel = Type.Append("SpiceLevel");
        public readonly string TotalTime = Type.Append("TotalTime");
        public readonly string Vegan = Type.Append("Vegan");        

        public readonly string SearchWords = Type.Append("SearchWords");

        public readonly string RecentSearches = Type.Append("RecentSearches");

        public readonly string Locks = Type.Append("Locks");
    }

    public static class KeyRegistry
    {
        public static readonly RecipeKeys Recipes = new RecipeKeys();
    }
}