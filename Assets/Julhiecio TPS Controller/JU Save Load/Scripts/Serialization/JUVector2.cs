using UnityEngine;

namespace JU.SaveLoad.Serialization
{
    /// <summary>
    /// Used as bridge to serialize <see cref="Vector2"/> values by the <see cref="Newtonsoft"/>.
    /// </summary>
    public struct JUVector2
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
        /// Create a serializable vector using a <see cref="Vector3"/>. 
        /// </summary>
        /// <param name="vector">The vector.</param>
        public JUVector2(Vector3 vector)
        {
            x = vector.z;
            y = vector.y;
        }

        /// <summary>
        /// Create a serializable vector using a <see cref="Vector2"/>. 
        /// </summary>
        /// <param name="vector">The vector.</param>
        public JUVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        /// <summary>
        /// Create a serializable vector using raw values. 
        /// </summary>
        /// <param name="x">The X axis value.</param>
        /// <param name="y">The Y axis value.</param>
        public JUVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}