using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapFogOfWar : MonoBehaviour
    {
        public Material fogPlaneMaterial;
        public Shader painterShader;

        [Header("Fog Settings")]
        [Range(0.01f, 0.5f)] public float radius = 0.1f;
        [Range(0.01f, 0.5f)] public float softness = 0.05f;
        public int textureResolution = 512;
        public int updateRate = 30;

        private RenderTexture[] buffers = new RenderTexture[2];
        private int bufferIndex = 0;
        private Vector3 lastPlayerPos;
        private Material painterMat;
        private bl_MiniMapPlaneBase miniMapPlane;
        // Cache Property IDs for speed
        private static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");
        private static readonly int RadiusID = Shader.PropertyToID("_Radius");
        private static readonly int SoftnessID = Shader.PropertyToID("_Softness");
        private static readonly int MaskTexID = Shader.PropertyToID("_MaskTex");

        void Start()
        {
            painterMat = new Material(painterShader);

            for (int i = 0; i < 2; i++)
            {
                buffers[i] = new RenderTexture(textureResolution, textureResolution, 0, RenderTextureFormat.R8);
                buffers[i].filterMode = FilterMode.Bilinear;
                buffers[i].wrapMode = TextureWrapMode.Clamp;
                buffers[i].Create();

                // Clear buffers to black
                Graphics.SetRenderTarget(buffers[i]);
                GL.Clear(false, true, Color.clear);
            }
        }

        public void Setup(bl_MiniMapPlaneBase mapPlane)
        {
            miniMapPlane = mapPlane;
            if (miniMapPlane.MiniMap == null) return;
            updateRate = miniMapPlane.MiniMap.fogOfWarUpdateRate;
            radius = miniMapPlane.MiniMap.fogOfWarRadius;
            softness = miniMapPlane.MiniMap.fogOfWarSoftness;
            fogPlaneMaterial.SetColor("_FogColor", miniMapPlane.MiniMap.fogOfWarColor);
            gameObject.SetActive(miniMapPlane.MiniMap.hasFogOfWar);
        }

        void Update()
        {
            if (Time.frameCount % updateRate != 0) return;
            if (miniMapPlane == null || miniMapPlane.MiniMap == null || miniMapPlane.MiniMap.Target == null) return;
            if (miniMapPlane.MiniMap.hasFogOfWar == false) return;

            // fast distance check
            if ((miniMapPlane.MiniMap.Target.position - lastPlayerPos).sqrMagnitude < 0.001f) return;

            lastPlayerPos = miniMapPlane.MiniMap.Target.position;
            // Efficient coordinate mapping
            Vector3 localPos = transform.InverseTransformPoint(miniMapPlane.MiniMap.Target.position);
            Vector4 uvPos = new Vector4(
                Mathf.InverseLerp(5f, -5f, localPos.x),
                Mathf.InverseLerp(5f, -5f, localPos.z),
                0, 0
            );

            // Update properties using IDs (faster than strings)
            painterMat.SetVector(PlayerPosID, uvPos);
            painterMat.SetFloat(RadiusID, radius);
            painterMat.SetFloat(SoftnessID, softness);

            // Double Buffering: Ping-pong between textures
            int src = bufferIndex;
            int dst = 1 - bufferIndex;

            Graphics.Blit(buffers[src], buffers[dst], painterMat);

            // Final material displays the newly written frame
            fogPlaneMaterial.SetTexture(MaskTexID, buffers[dst]);

            bufferIndex = dst; // Swap
        }

        /// <summary>
        /// Save the current fog of war state to a PNG file
        /// </summary>
        /// <param name="SavePath" >The file path where the PNG will be saved.</param>
        public void SaveFog(string SavePath)
        {
            var maskTexture = buffers[bufferIndex];
            // 1. Create a temporary Texture2D to read from the GPU
            Texture2D tex = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.R8, false);

            RenderTexture.active = maskTexture;
            tex.ReadPixels(new Rect(0, 0, maskTexture.width, maskTexture.height), 0, 0);
            tex.Apply();

            // check if the path is a folder or file (if folder, use default file name)
            if (Directory.Exists(SavePath))
            {
                SavePath = Path.Combine(SavePath, $"{SceneManager.GetActiveScene().name}_Reveal.png");
            }

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(SavePath, bytes);

            Destroy(tex); // Clean up memory
            RenderTexture.active = null;
        }

        /// <summary>
        /// Load the fog of war state from a PNG file
        /// </summary>
        /// <param name="SavePath"></param>
        public void LoadFog(string SavePath)
        {
            if (Directory.Exists(SavePath))
            {
                SavePath = Path.Combine(SavePath, $"{SceneManager.GetActiveScene().name}_Reveal.png");
            }

            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("Fog of War save file not found: " + SavePath);
                return;
            }

            byte[] bytes = File.ReadAllBytes(SavePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            var maskTexture = buffers[bufferIndex];
            // Copy the loaded texture back into the RenderTexture
            Graphics.Blit(tex, maskTexture);

            Destroy(tex);
        }

        private void OnDestroy()
        {
            if (buffers[0] != null) buffers[0].Release();
            if (buffers[1] != null) buffers[1].Release();
        }
    }
}