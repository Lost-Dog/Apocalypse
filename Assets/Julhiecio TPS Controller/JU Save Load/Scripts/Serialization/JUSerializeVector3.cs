using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// Used by <see cref="Newtonsoft"/> to serialize <see cref="Vector3"/>.
    public class JUSerializeVector3 : CustomCreationConverter<Vector3>
    {
        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override Vector3 Create(Type objectType)
        {
            return default;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Vector3 vec3)
                serializer.Serialize(writer, new JUVector3(vec3));
        }
    }
}