using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// Used by <see cref="Newtonsoft"/> to serialize <see cref="Vector4"/>.
    public class JUSerializeVector4 : CustomCreationConverter<Vector4>
    {
        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override Vector4 Create(Type objectType)
        {
            return default;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector4);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Vector4 vec4)
                serializer.Serialize(writer, new JUVector4(vec4));
        }
    }
}