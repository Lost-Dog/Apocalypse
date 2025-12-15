using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// Used by <see cref="Newtonsoft"/> to serialize <see cref="Quaternion"/>.
    public class JUSerializeQuaternion : CustomCreationConverter<Quaternion>
    {
        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override Quaternion Create(Type objectType)
        {
            return default;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Quaternion quat)
                serializer.Serialize(writer, new JUQuaternion(quat));
        }
    }
}