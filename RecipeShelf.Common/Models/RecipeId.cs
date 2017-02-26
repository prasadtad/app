using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RecipeShelf.Common.Models
{
    public struct RecipeId
    {
        private readonly string Value;

        public RecipeId(string value)
        {
            Value = value;
        }

        public static implicit operator RecipeId(string id)
        {
            return id != null ? new RecipeId(id) : null;
        }

        public static implicit operator string(RecipeId id)
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
            return obj is RecipeId ? ((RecipeId)obj).Value.Equals(Value)
                            : obj is string ? ((string)obj).Equals(Value)
                                    : false;
        }
    }

    public class RecipeIdConverter : JsonConverter
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
            else if (value is RecipeId)
                serializer.Serialize(writer, ((RecipeId)value).ToString());
            else if (value is RecipeId[])
                serializer.Serialize(writer, ((RecipeId[])value).ToStrings());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize(reader);
            if (id == null) return null;
            if (id is string) return new RecipeId((string)id);
            var idArray = (JArray)id;
            var ids = new RecipeId[idArray.Count];
            for (var i = 0; i < ids.Length; i++)
                ids[i] = new RecipeId(idArray[i].Value<string>());
            return ids;
        }
    }
}
