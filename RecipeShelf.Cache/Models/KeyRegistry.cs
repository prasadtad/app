using RecipeShelf.Common.Models;

namespace RecipeShelf.Cache.Models
{
    public sealed class IngredientKeys
    {
        public const KeyType Type = KeyType.Ingredient;

        public const string DEFAULT_CATEGORY = "Miscellaneous";

        public readonly string Names = Type.Append("Names");
        public readonly string Category = Type.Append("Category");
        public readonly string RecipeId = Type.Append("RecipeId");

        public readonly string Vegan = Type.Append("Vegan");

        public readonly string SearchWords = Type.Append("SearchWords");

        public readonly string RecentSearches = Type.Append("RecentSearches");
    }

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
    }

    public static class KeyRegistry
    {
        public static readonly IngredientKeys Ingredients = new IngredientKeys();

        public static readonly RecipeKeys Recipes = new RecipeKeys();
    }
}