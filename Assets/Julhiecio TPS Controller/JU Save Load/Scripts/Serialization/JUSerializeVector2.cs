using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// Used by <see cref="Newtonsoft"/> to serialize <see cref="Vector2"/>.
    public class JUSerializeVector2 : CustomCreationConverter<Vector2>
    {
        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override Vector2 Create(Type objectType)
        {
            return default;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Vector2 vec2)
                serializer.Serialize(writer, new JUVector2(vec2));
        }
    }
}