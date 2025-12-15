using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// <summary>
    /// Used as bridge to serialize <see cref="Vector4"/> values by the <see cref="Newtonsoft"/>.
    /// </summary>
    public struct JUVector4
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
        /// Create a serializable vector using a <see cref="Vector3"/>. 
        /// </summary>
        /// <param name="vector">The vector.</param>
        public JUVector4(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
            w = 0;
        }

        /// <summary>
        /// Create a serializable vector using a <see cref="Vector4"/>. 
        /// </summary>
        /// <param name="vector">The vector.</param>
        public JUVector4(Vector4 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
            w = vector.w;
        }

        /// <summary>
        /// Create a serializable vector using raw values. 
        /// </summary>
        /// <param name="x">The X axis value.</param>
        /// <param name="y">The Y axis value.</param>
        /// <param name="z">The Z axis value.</param>
        /// <param name="w">The W axis value.</param>
        public JUVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}