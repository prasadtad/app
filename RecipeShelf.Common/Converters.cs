using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RecipeShelf.Common.Models;
using System;

namespace RecipeShelf.Common
{
    public sealed class IdConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) ||
                   objectType == typeof(string[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize(reader);
            if (id == null) return null;
            if (id is string) return new Id((string)id);
            var idArray = (JArray)id;
            var ids = new Id[idArray.Count];
            for (var i = 0; i < ids.Length; i++)
                ids[i] = new Id(idArray[i].Value<string>());
            return ids;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                if (serializer.NullValueHandling == NullValueHandling.Ignore)
                    return;
                serializer.Serialize(writer, null);
            }
            else if (value is Id)
                serializer.Serialize(writer, ((Id)value).Value);
            else if (value is Id[])
                serializer.Serialize(writer, ((Id[])value).ToStrings());
        }
    }
}
