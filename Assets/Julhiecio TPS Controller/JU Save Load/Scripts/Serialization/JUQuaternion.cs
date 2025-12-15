using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// <summary>
    /// Used as bridge to serialize <see cref="Quaternion"/> values by the <see cref="Newtonsoft"/>.
    /// </summary>
    public struct JUQuaternion
    {
        /// <summary>
        /// The X axis value.
        /// </summary>
        public float x;

        /// <summary>
        /// The Y axis value.
        /// </summary>
        public float y;

        /// <summary>
        /// The Z axis value.
        /// </summary>
        public float z;

        /// <summary>
        /// The W axis value.
        /// </summary>
        public float w;

        /// <summary>
        /// Create a serializable quaternion using a <see cref="Quaternion"/> value. 
        /// </summary>
        /// <param name="quaternion">The value.</param>
        public JUQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        /// <summary>
        /// Create a serializable vector using raw values. 
        /// </summary>
        /// <param name="x">The X axis value.</param>
        /// <param name="y">The Y axis value.</param>
        /// <param name="z">The Z axis value.</param>
        /// <param name="w">The W axis value.</param>
        public JUQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}