using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace JU.SaveLoad.Utilities
{
    /// <summary>
    /// Has value type utilities to use with <see cref="JUSaveLoad"/>.
    /// </summary>
    public static class JUSaveLoadUtilities
    {
        /// <summary>
        /// Return true if is trying to convert a primitive type or 
        /// is trying to convert a <see cref="JObject"/>. <para/>
        /// It's not works for <see cref="UnityEngine.Object"/>
        /// </summary>
        /// <typeparam name="T">The target type to convert.</typeparam>
        /// <param name="value">The value type to convert.</param>
        /// <returns></returns>
        public static bool CanConvertTo<T>(object value)
        {
            if (value == null)
                return false;

            if (value.GetType().IsPrimitive && typeof(T).IsPrimitive)
                return true;

            if (value is JObject)
                return true;

            if (value is JArray)
                return true;

            return false;
        }

        /// <summary>
        /// Convert an generic <see cref="object"/> to a specific type if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>Return the value converted to the target type if possible, however return the default constructor if the cast fail."/></returns>
        public static T ConvertTo<T>(object value)
        {
            if (value == null)
                return default;

            try
            {
                if (value is JObject jsonObject)
                    return jsonObject.ToObject<T>();

                if (value is JArray jsonArray)
                    return jsonArray.ToObject<T>();

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"Can't convert {value.GetType()} to {typeof(T)}.");
                return default;
            }
        }
    }
}