using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates a 512x512 white-silhouette seeker-mine icon (Division 2 style).
/// Sphere body with six radial propulsion pods, central sensor eye,
/// equatorial ring band, and a top emitter nub.
/// Run via: Tools > Generate Seeker Mine Icon
/// </summary>
public static class GenerateSeekerMineIcon
{
    private const string OutputPath =
        "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Status/ICON_Apocalypse_Status_SeekerMine_01.png";

    private const int S = 512;

    [MenuItem("Tools/Generate Seeker Mine Icon")]
    public static void Generate()
    {
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        Color[] px = new Color[S * S];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        tex.SetPixels(px);

        // ── Pixel helpers (Y-up, origin bottom-left) ─────────────────────

        void SetPx(int x, int y, Color c)
        {
            if (x < 0 || x >= S || y < 0 || y >= S) return;
            Color e = tex.GetPixel(x, y);
            tex.SetPixel(x, y, c.a >= 1f ? c : Color.Lerp(e, c, c.a));
        }

        void FillRect(float rx, float ry, float rw, float rh, Color c)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(rx));
            int x1 = Mathf.Min(S - 1, Mathf.CeilToInt(rx + rw));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(ry));
            int y1 = Mathf.Min(S - 1, Mathf.CeilToInt(ry + rh));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    SetPx(x, y, c);
        }

        void FillCircle(float cx, float cy, float r, Color c)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int x1 = Mathf.Min(S - 1, Mathf.CeilToInt(cx + r + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int y1 = Mathf.Min(S - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        SetPx(x, y, c);
        }

        // Annular ring (donut)
        void FillRing(float cx, float cy, float ro, float ri, Color c)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - ro - 1));
            int x1 = Mathf.Min(S - 1, Mathf.CeilToInt(cx + ro + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - ro - 1));
            int y1 = Mathf.Min(S - 1, Mathf.CeilToInt(cy + ro + 1));
            float ro2 = ro * ro, ri2 = ri * ri;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (d2 <= ro2 && d2 >= ri2)
                        SetPx(x, y, c);
                }
        }

        void FillRoundedRect(float rx, float ry, float rw, float rh, float rad, Color c)
        {
            FillRect(rx + rad, ry, rw - rad * 2f, rh, c);
            FillRect(rx, ry + rad, rad, rh - rad * 2f, c);
            FillRect(rx + rw - rad, ry + rad, rad, rh - rad * 2f, c);
            FillCircle(rx + rad,      ry + rad,      rad, c);
            FillCircle(rx + rw - rad, ry + rad,      rad, c);
            FillCircle(rx + rad,      ry + rh - rad, rad, c);
            FillCircle(rx + rw - rad, ry + rh - rad, rad, c);
        }

        void FillLine(float ax, float ay, float bx, float by, float halfThick, Color c)
        {
            float dx  = bx - ax, dy = by - ay;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) return;
            float cos     = dx / len, sin = dy / len;
            float halfLen = len * 0.5f;
            float mx = (ax + bx) * 0.5f, my = (ay + by) * 0.5f;
            int pad = Mathf.CeilToInt(halfLen + halfThick) + 2;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(mx - pad));
            int x1 = Mathf.Min(S - 1, Mathf.CeilToInt(mx + pad));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(my - pad));
            int y1 = Mathf.Min(S - 1, Mathf.CeilToInt(my + pad));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float lx =  cos * (x - mx) + sin * (y - my);
                    float ly = -sin * (x - mx) + cos * (y - my);
                    if (Mathf.Abs(lx) <= halfLen && Mathf.Abs(ly) <= halfThick)
                        SetPx(x, y, c);
                }
        }

        Color W   = Color.white;
        Color CUT = new Color(0f, 0f, 0f, 0.65f);

        // ── Layout ────────────────────────────────────────────────────────
        //
        //         ●          ← tip sphere
        //         │          ← emitter stem
        //        ╔╗          ← collar nub
        //   ◄────●────►      ← six propulsion pods (60° apart)
        //      ╔═════╗       ← equatorial ring band
        //   ◄─(  (O)  )─►   ← sphere body with sensor eye
        //      ╚═════╝
        //   ◄────●────►      ← lower pods

        float cx = 256f, cy = 222f;
        float R  = 148f;

        // ── Main sphere body ──────────────────────────────────────────────
        FillCircle(cx, cy, R, W);

        // ── Six propulsion pods around equator ────────────────────────────
        // Angles: 30°, 90°, 150°, 210°, 270°, 330° — offset so no pod
        // points directly right (keeps the eye visible)
        int   podCount = 6;
        float podLen   = 52f;
        float podHalf  = 13f;

        for (int i = 0; i < podCount; i++)
        {
            float angleDeg = 30f + i * (360f / podCount);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float dirX = Mathf.Cos(angleRad);
            float dirY = Mathf.Sin(angleRad);

            float startX = cx + dirX * (R - 10f);
            float startY = cy + dirY * (R - 10f);
            float endX   = cx + dirX * (R + podLen);
            float endY   = cy + dirY * (R + podLen);

            FillLine(startX, startY, endX, endY, podHalf, W);
            FillCircle(endX, endY, podHalf - 1f, W);
        }

        // ── Equatorial ring band ──────────────────────────────────────────
        FillRing(cx, cy, R + 4f, R - 28f, W);

        // ── Panel seam cuts ───────────────────────────────────────────────
        // Horizontal equator seam
        for (int x = Mathf.FloorToInt(cx - R + 4); x <= Mathf.CeilToInt(cx + R - 4); x++)
        {
            float d2 = (x - cx) * (x - cx);
            if (d2 > R * R) continue;
            for (int s = -4; s <= 4; s++)
                SetPx(x, Mathf.RoundToInt(cy + s), CUT);
        }

        // Vertical meridian seam
        for (int y = Mathf.FloorToInt(cy - R + 4); y <= Mathf.CeilToInt(cy + R - 4); y++)
        {
            float d2 = (y - cy) * (y - cy);
            if (d2 > R * R) continue;
            for (int s = -3; s <= 3; s++)
                SetPx(Mathf.RoundToInt(cx + s), y, CUT);
        }

        // ── Central sensor eye ────────────────────────────────────────────
        // Placed on the right face of the sphere
        float eyeX = cx + 70f, eyeY = cy;
        FillCircle(eyeX, eyeY, 42f,  W);    // outer lens
        FillCircle(eyeX, eyeY, 30f,  CUT);  // dark iris
        FillCircle(eyeX, eyeY, 11f,  W);    // pupil highlight

        // ── Top emitter nub ───────────────────────────────────────────────
        float collarY = cy + R - 8f;
        FillRoundedRect(cx - 15f, collarY,        30f, 28f, 8f, W);  // collar
        FillRoundedRect(cx - 10f, collarY + 26f,  20f, 38f, 6f, W);  // stem
        FillCircle(cx, collarY + 68f, 15f, W);                        // tip

        // ── Access panel detail (lower-front quadrant) ────────────────────
        float panX = cx + 18f, panY = cy - 62f;
        FillRoundedRect(panX,      panY,      56f, 38f, 8f, CUT);
        FillRoundedRect(panX + 5f, panY + 5f, 46f, 28f, 5f, W);

        // ── Apply & save ─────────────────────────────────────────────────
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string fullPath = Path.Combine(
            Application.dataPath.Replace("/Assets", "/"), OutputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);

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

        Debug.Log("[GenerateSeekerMineIcon] Saved → " + OutputPath);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
