using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RecipeShelf.Common.Models
{
    public struct IngredientId
    {
        private readonly string Value;

        public IngredientId(string value)
        {
            Value = value;
        }

        public static implicit operator IngredientId(string id)
        {
            return id != null ? new IngredientId(id) : null;
        }

        public static implicit operator string(IngredientId id)
        {
            return id.Value;
        }        

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IngredientId ? ((IngredientId)obj).Value.Equals(Value)
                            : obj is string ? ((string)obj).Equals(Value)
                                    : false;
        }
    }

    public class IngredientIdConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) ||
                   objectType == typeof(string[]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                if (serializer.NullValueHandling == NullValueHandling.Ignore)
                    return;
                serializer.Serialize(writer, null);
            }
            else if (value is IngredientId)
                serializer.Serialize(writer, ((IngredientId)value).ToString());
            else if (value is IngredientId[])
                serializer.Serialize(writer, ((IngredientId[])value).ToStrings());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize(reader);
            if (id == null) return null;
            if (id is string) return new IngredientId((string)id);
            var idArray = (JArray)id;
            var ids = new IngredientId[idArray.Count];
            for (var i = 0; i < ids.Length; i++)
                ids[i] = new IngredientId(idArray[i].Value<string>());
            return ids;
        }
    }
}
