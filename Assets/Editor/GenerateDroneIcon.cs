using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates a 512x512 white-silhouette drone icon PNG matching
/// the vGrenadeIcon sprite format (transparent background, white art).
/// Run via: Tools > Generate Drone Icon
/// </summary>
public static class GenerateDroneIcon
{
    private const string OutputPath = "Assets/Invector-3rdPersonController/Shooter/Scripts/ThrowSystem/UI/vDroneIcon.png";
    private const int Size = 512;

    [MenuItem("Tools/Generate Drone Icon")]
    public static void Generate()
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);

        // Fill fully transparent
        Color[] pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        tex.SetPixels(pixels);

        // ── Drawing helpers ──────────────────────────────────────────────

        // Filled circle
        void FillCircle(float cx, float cy, float r, Color col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r));
            int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + r));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r));
            int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + r));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float t = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                    if (t > 0f)
                    {
                        Color existing = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, Color.Lerp(existing, col, t));
                    }
                }
        }

        // Ring (hollow circle)
        void DrawRing(float cx, float cy, float r, float thickness, Color col)
        {
            float outer = r + thickness * 0.5f;
            float inner = r - thickness * 0.5f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - outer - 1));
            int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + outer + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - outer - 1));
            int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + outer + 1));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(Mathf.Min(dist - inner, outer - dist));
                    if (t > 0f)
                    {
                        Color existing = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, Color.Lerp(existing, col, t));
                    }
                }
        }

        // Filled axis-aligned rectangle
        void FillRect(float rx, float ry, float rw, float rh, Color col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(rx));
            int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(rx + rw));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(ry));
            int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(ry + rh));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    tex.SetPixel(x, y, col);
        }

        // Filled ellipse (axis-aligned)
        void FillEllipse(float cx, float cy, float rx, float ry, Color col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - rx));
            int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + rx));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - ry));
            int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + ry));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(1f - dist) / 0.02f;
                    t = Mathf.Clamp01(t);
                    if (dist < 1f)
                    {
                        Color existing = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, Color.Lerp(existing, col, Mathf.Clamp01((1f - dist) * 50f)));
                    }
                }
        }

        // Rotated rectangle drawn as filled quad (uses pixel-level rotation test)
        void FillRotatedRect(float cx, float cy, float halfW, float halfH, float angleDeg, Color col)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            int margin = Mathf.CeilToInt(Mathf.Max(halfW, halfH)) + 2;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - margin));
            int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + margin));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - margin));
            int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + margin));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float lx =  cos * dx + sin * dy;
                    float ly = -sin * dx + cos * dy;
                    if (Mathf.Abs(lx) <= halfW && Mathf.Abs(ly) <= halfH)
                        tex.SetPixel(x, y, col);
                }
        }

        // ── Icon layout (Y-up texture space, origin bottom-left) ─────────

        Color w = Color.white;

        // Rotor positions (top-left, top-right, bottom-left, bottom-right)
        Vector2[] rotorCenters = {
            new Vector2(138f, 370f),
            new Vector2(374f, 370f),
            new Vector2(138f, 142f),
            new Vector2(374f, 142f),
        };

        float rotorRadius    = 52f;
        float ringThickness  = 9f;
        float bladeHalf      = 42f;
        float bladeWidth     = 11f;
        float hubRadius      = 10f;
        float armHalfLen     = 55f;
        float armHalfW       = 7f;

        // Body center
        float bx = 256f, by = 256f;

        // --- Arms (rotated rects from body center toward each rotor) ---
        foreach (var rc in rotorCenters)
        {
            Vector2 dir = (rc - new Vector2(bx, by));
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float midX = (bx + rc.x) * 0.5f;
            float midY = (by + rc.y) * 0.5f;
            float len  = dir.magnitude * 0.5f;
            FillRotatedRect(midX, midY, len, armHalfW, angle, w);
        }

        // --- Rotors ---
        foreach (var rc in rotorCenters)
        {
            // Guard ring
            DrawRing(rc.x, rc.y, rotorRadius, ringThickness, w);
            // Blades (cross)
            FillEllipse(rc.x, rc.y, bladeHalf, bladeWidth, w);
            FillEllipse(rc.x, rc.y, bladeWidth, bladeHalf, w);
            // Hub
            FillCircle(rc.x, rc.y, hubRadius, w);
        }

        // --- Central body (rounded rect approximated with rect + 4 corner circles) ---
        float bodyW = 90f, bodyH = 70f, bodyR = 12f;
        FillRect(bx - bodyW * 0.5f + bodyR, by - bodyH * 0.5f, bodyW - bodyR * 2f, bodyH, w);
        FillRect(bx - bodyW * 0.5f, by - bodyH * 0.5f + bodyR, bodyW, bodyH - bodyR * 2f, w);
        FillCircle(bx - bodyW * 0.5f + bodyR, by - bodyH * 0.5f + bodyR, bodyR, w);
        FillCircle(bx + bodyW * 0.5f - bodyR, by - bodyH * 0.5f + bodyR, bodyR, w);
        FillCircle(bx - bodyW * 0.5f + bodyR, by + bodyH * 0.5f - bodyR, bodyR, w);
        FillCircle(bx + bodyW * 0.5f - bodyR, by + bodyH * 0.5f - bodyR, bodyR, w);

        // --- Camera pod (below body) ---
        FillEllipse(bx, by - bodyH * 0.5f - 14f, 20f, 14f, w);
        // Lens cutout
        FillCircle(bx, by - bodyH * 0.5f - 14f, 7f, new Color(0, 0, 0, 1f));

        // --- Antenna (above body) ---
        FillRect(bx - 6f, by + bodyH * 0.5f, 12f, 20f, w);
        FillCircle(bx, by + bodyH * 0.5f + 24f, 7f, w);

        // ── Apply & save ─────────────────────────────────────────────────
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", "/"), OutputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);

        // Match vGrenadeIcon importer settings
        var importer = AssetImporter.GetAtPath(OutputPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePivot         = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100f;
            importer.alphaSource         = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.filterMode          = FilterMode.Bilinear;
            importer.anisoLevel          = 16;
            importer.wrapMode            = TextureWrapMode.Clamp;
            importer.mipmapEnabled       = true;
            importer.maxTextureSize      = 512;
            importer.textureCompression  = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }

        Debug.Log($"[GenerateDroneIcon] Saved to {OutputPath}");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
    }
}
