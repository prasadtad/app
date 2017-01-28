using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using System.Text;

namespace RecipeShelf.Site
{
    internal static class MarkdownGenerator
    {
        internal static string GenerateMarkdown(this Recipe recipe)
        {
            var sb = new StringBuilder();
            sb.AppendLine("+++");
            AppendArray(sb, "collections", recipe.Collections);
            AppendArray(sb, "cuisine", new[] { recipe.Cuisine });
            AppendValue(sb, "date", recipe.LastModified);
            AppendValue(sb, "title", recipe.Names[0]);
            AppendValue(sb, "description", recipe.Description);
            AppendValue(sb, "servings", recipe.Servings);
            if (!string.IsNullOrEmpty(recipe.ImageId))
                AppendValue(sb, "image_id", recipe.ImageId);
            AppendValue(sb, "total_time", "PT" + recipe.TotalTimeInMinutes + "M");
            AppendValue(sb, "total_time_pretty", recipe.TotalTimeInMinutes + " minutes");
            AppendValue(sb, "chef", "Sirisha Tadimalla");
            AppendValue(sb, "spice_level", (int)recipe.SpiceLevel);
            AppendValue(sb, "overnight_preparation", recipe.OvernightPreparation ? 1 : 0);
            AppendValue(sb, "ingredients_count", recipe.IngredientIds.Length);
            if (recipe.AccompanimentIds != null && recipe.AccompanimentIds.Length > 0)
                AppendArray(sb, "accompaniment_ids", recipe.AccompanimentIds.ToStrings());
            AppendIngredientsHtml(sb, recipe.Ingredients);
            AppendStepsHtml(sb, recipe.Steps);
            sb.AppendLine("+++");
            return sb.ToString();
        }

        private static void AppendArray(StringBuilder sb, string name, string[] array)
        {
            sb.Append(name);
            sb.Append(" = ");
            if (array != null)
                sb.AppendLine(JsonConvert.SerializeObject(array));
            else
                sb.AppendLine("[]");
        }

        private static void AppendValue(StringBuilder sb, string name, object value)
        {
            sb.Append(name);
            sb.Append(" = ");
            sb.AppendLine(JsonConvert.SerializeObject(value));
        }

        private static void AppendIngredientsHtml(StringBuilder sb, RecipeItem[] items)
        {
            sb.Append("ingredients_html = \"<ul style='padding-left: 0; list-style: none;'>");
            foreach (var item in items)
            {
                if (item.Decorator == Decorator.Heading)
                {
                    sb.Append("<li style='margin: 8px 0px;padding: 8px 0px;'><span style='font-size: medium; color: #f78153;'>");
                    sb.Append(item.Text.Replace("\"", "\\\""));
                    sb.Append("</span></li>");
                }
                else
                {
                    sb.Append("<li itemprop='recipeIngredient' style='margin: 8px 0px;padding: 8px 0px;'>");
                    sb.Append(item.Text.Replace("\"", "\\\""));
                    sb.Append("</li>");
                }
            }
            sb.AppendLine("</ul>\"");
        }

        private static void AppendStepsHtml(StringBuilder sb, RecipeItem[] items)
        {
            sb.Append("steps_html = \"<ol style='list-style: none inside; padding-left: 0px;'>");
            var headingOpen = false;
            foreach (var item in items)
            {
                if (item.Decorator == Decorator.Heading)
                {
                    if (headingOpen)
                    {
                        sb.Append("</ol></li>");
                        headingOpen = false;
                    }
                    sb.Append("<li style='list-style: none; margin: 8px 0px;padding: 8px 0px;'><span style='font-size: medium; color: #f78153;'>");
                    sb.Append(item.Text.Replace("\"", "\\\""));
                    sb.Append("</span><ol style='list-style: none inside; padding-left: 0px;'>");
                    headingOpen = true;
                }
                else if (item.Decorator == Decorator.Quote)
                {
                    sb.Append("<blockquote>");
                    sb.Append(item.Text.Replace("\"", "\\\""));
                    sb.Append("</blockquote>");
                }
                else
                {
                    sb.Append("<li style='padding-bottom: 10px;'><i class='step-track-icon fa fa-square-o'></i><span class='step-text' itemprop='recipeInstructions'>");
                    sb.Append(item.Text.Replace("\"", "\\\""));
                    sb.Append("</span></li>");
                }
            }
            if (headingOpen)
            {
                sb.Append("</ol></li>");
                headingOpen = false;
            }
            sb.AppendLine("</ol>\"");
        }
    }
}
