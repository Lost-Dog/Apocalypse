using UnityEngine;

namespace GameCreator.Runtime.Common
{
    public static class PerlinNoiseUtils
    {
        private static readonly float SCALAR = Mathf.Sqrt(2.0f);
        
        private static readonly int[] PERMUTATIONS = new int[512];
        
        static PerlinNoiseUtils()
        {
            int[] permutation = new int[256];
            
            for (int i = 0; i < 256; i++)
            {
                permutation[i] = i;
            }
            
            System.Random random = new System.Random();
            
            for (int i = permutation.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
            }
            
            for (int i = 0; i < 256; i++)
            {
                PERMUTATIONS[i] = PERMUTATIONS[i + 256] = permutation[i];
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        /// <summary>
        /// Returns a value between [-1, 1] and at integer positions the value is always 0.
        /// Values can be slightly above or below
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float Get(float x, float y)
        {
            bool xIsInt = Mathf.Abs(x - Mathf.Floor(x)) < float.Epsilon ||
                          Mathf.Abs(x - Mathf.Ceil(x)) < float.Epsilon;
            
            bool yIsInt = Mathf.Abs(y - Mathf.Floor(y)) < float.Epsilon ||
                          Mathf.Abs(y - Mathf.Ceil(y)) < float.Epsilon;

            if (xIsInt && yIsInt)
            {
                return 0f;
            }
            
            int xi = Mathf.FloorToInt(x) & 255;
            int yi = Mathf.FloorToInt(y) & 255;
            
            float xf = x - Mathf.Floor(x);
            float yf = y - Mathf.Floor(y);
            
            float u = Fade(xf);
            float v = Fade(yf);
            
            int aa = PERMUTATIONS[xi] + yi;
            int ab = PERMUTATIONS[xi] + yi + 1;
            int ba = PERMUTATIONS[xi + 1] + yi;
            int bb = PERMUTATIONS[xi + 1] + yi + 1;
            
            float dot00 = Gradient(PERMUTATIONS[aa], xf, yf);
            float dot10 = Gradient(PERMUTATIONS[ba], xf - 1.0f, yf);
            float dot01 = Gradient(PERMUTATIONS[ab], xf, yf - 1.0f);
            float dot11 = Gradient(PERMUTATIONS[bb], xf - 1.0f, yf - 1.0f);

            float lerpX1 = Lerp(dot00, dot10, u);
            float lerpX2 = Lerp(dot01, dot11, u);
            
            float result = Lerp(lerpX1, lerpX2, v);

            return result * SCALAR;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }
        
        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static float Gradient(int hash, float x, float y)
        {
            int h = hash & 7;
            
            float u = h < 4 ? x : y;
            float v = h < 4 ? y : x;
            
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}