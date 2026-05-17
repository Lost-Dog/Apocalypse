using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates a 512x512 white-silhouette revive-station (Reviver Hive) icon
/// in the style of The Division 2.
/// Capsule/pod body, honeycomb panel cuts, medical cross on front face,
/// two micro-drones launching from the top with rotor arcs.
/// Run via: Tools > Generate Revive Station Icon
/// </summary>
public static class GenerateReviveStationIcon
{
    private const string OutputPath =
        "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Status/ICON_Apocalypse_Status_ReviveStation_01.png";

    private const int S = 512;

    [MenuItem("Tools/Generate Revive Station Icon")]
    public static void Generate()
    {
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        Color[] px = new Color[S * S];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        tex.SetPixels(px);

        // ── Pixel helpers ─────────────────────────────────────────────────

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

        void FillRoundedRect(float rx, float ry, float rw, float rh, float rad, Color c)
        {
            if (rad * 2f > rw) rad = rw * 0.5f;
            if (rad * 2f > rh) rad = rh * 0.5f;
            FillRect(rx + rad, ry,        rw - rad * 2f, rh,           c);
            FillRect(rx,       ry + rad,  rad,           rh - rad * 2f, c);
            FillRect(rx + rw - rad, ry + rad, rad,       rh - rad * 2f, c);
            FillCircle(rx + rad,       ry + rad,       rad, c);
            FillCircle(rx + rw - rad,  ry + rad,       rad, c);
            FillCircle(rx + rad,       ry + rh - rad,  rad, c);
            FillCircle(rx + rw - rad,  ry + rh - rad,  rad, c);
        }

        void FillLine(float ax, float ay, float bx, float by, float halfThick, Color c)
        {
            float dx  = bx - ax, dy = by - ay;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) return;
            float cos = dx / len, sin = dy / len;
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

        // Arc stroke: sweeps from startDeg to endDeg (CCW), radius r, thickness t
        void StrokeArc(float cx, float cy, float r, float startDeg, float endDeg,
                       float thickness, Color c)
        {
            float rOuter = r + thickness * 0.5f;
            float rInner = r - thickness * 0.5f;
            float ro2 = rOuter * rOuter, ri2 = rInner * rInner;
            float a0 = startDeg * Mathf.Deg2Rad;
            float a1 = endDeg   * Mathf.Deg2Rad;
            // Normalise so a1 > a0
            while (a1 < a0) a1 += Mathf.PI * 2f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - rOuter - 1));
            int x1 = Mathf.Min(S - 1, Mathf.CeilToInt(cx + rOuter + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - rOuter - 1));
            int y1 = Mathf.Min(S - 1, Mathf.CeilToInt(cy + rOuter + 1));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx2 = x - cx, dy2 = y - cy;
                    float d2 = dx2 * dx2 + dy2 * dy2;
                    if (d2 < ri2 || d2 > ro2) continue;
                    float angle = Mathf.Atan2(dy2, dx2);
                    while (angle < a0) angle += Mathf.PI * 2f;
                    if (angle <= a1) SetPx(x, y, c);
                }
        }

        Color W   = Color.white;
        Color CUT = new Color(0f, 0f, 0f, 0.70f);

        // ── Layout (Y-up, origin bottom-left) ─────────────────────────────
        //
        //       [~]  [~]          ← two micro-drones with rotor arcs
        //        |    |
        //   ┌────●────●────┐      ← launch collar / top rim
        //   │   ┌─────┐   │      ← pod body (capsule, honeycomb panels)
        //   │   │  +  │   │      ← medical cross on front face
        //   │   │     │   │
        //   │   └─────┘   │
        //   └─────────────┘      ← flat base plate
        //  ══════════════════     ← ground shadow bar
        //
        //  Canvas 512×512.  Pod body centred at X=256, spanning Y 118–320.

        float midX  = 256f;

        // ── Ground shadow bar ─────────────────────────────────────────────
        FillRoundedRect(midX - 120f, 52f, 240f, 18f, 9f, W);

        // ── Base plate ────────────────────────────────────────────────────
        FillRoundedRect(midX - 130f, 72f, 260f, 44f, 12f, W);

        // ── Pod body (main capsule) ───────────────────────────────────────
        float bodyX = midX - 104f;
        float bodyY = 114f;
        float bodyW = 208f;
        float bodyH = 186f;
        float bodyR = 28f;
        FillRoundedRect(bodyX, bodyY, bodyW, bodyH, bodyR, W);

        // ── Side flanges (panel wings left & right of body) ───────────────
        FillRoundedRect(bodyX - 44f, bodyY + 28f, 52f, 106f, 10f, W);
        FillRoundedRect(bodyX + bodyW - 8f, bodyY + 28f, 52f, 106f, 10f, W);

        // ── Top launch collar ─────────────────────────────────────────────
        float collarY = bodyY + bodyH - 4f;
        FillRoundedRect(midX - 118f, collarY, 236f, 36f, 14f, W);

        // Collar inner channel cut
        FillRoundedRect(midX - 80f, collarY + 8f, 160f, 20f, 8f, CUT);

        // ── Honeycomb panel cuts on pod body ──────────────────────────────
        // Three rows of offset hexagon-like ovals, cutting into the body face
        float hcStartY = bodyY + 16f;
        float hcH      = 22f;   // cell height
        float hcW      = 38f;   // cell width
        float hcGapX   = 14f;   // gap between cells horizontally
        float hcGapY   = 10f;   // gap between rows

        for (int row = 0; row < 3; row++)
        {
            float ry    = hcStartY + row * (hcH + hcGapY);
            float offX  = (row % 2 == 1) ? (hcW + hcGapX) * 0.5f : 0f;
            float startX = bodyX + 18f + offX;
            while (startX + hcW <= bodyX + bodyW - 18f)
            {
                FillRoundedRect(startX, ry, hcW, hcH, 8f, CUT);
                startX += hcW + hcGapX;
            }
        }

        // ── Medical cross on front face ───────────────────────────────────
        float crossCX   = midX;
        float crossCY   = bodyY + 88f;   // lower half of body
        float crossArmW = 28f;           // arm width
        float crossArmL = 72f;           // arm full length
        float crossR    = 10f;

        // Vertical arm
        FillRoundedRect(crossCX - crossArmW * 0.5f,
                        crossCY - crossArmL * 0.5f,
                        crossArmW, crossArmL, crossR, W);
        // Horizontal arm
        FillRoundedRect(crossCX - crossArmL * 0.5f,
                        crossCY - crossArmW * 0.5f,
                        crossArmL, crossArmW, crossR, W);

        // ── Vertical detail lines on side flanges ─────────────────────────
        float flangeMidL = bodyX - 44f + 26f;
        float flangeMidR = bodyX + bodyW + 8f + 26f;
        FillRect(flangeMidL - 4f, bodyY + 44f, 8f, 60f, CUT);
        FillRect(flangeMidR - 4f, bodyY + 44f, 8f, 60f, CUT);

        // ── Two micro-drones launching upward ────────────────────────────
        // Drone 1 — left  (angled slightly left)
        float d1X = midX - 64f, d1Y = collarY + 40f;
        float d2X = midX + 64f, d2Y = collarY + 56f; // right, offset higher

        // Launch stems
        FillLine(midX - 40f, collarY + 6f,  d1X, d1Y - 8f, 5f, W);
        FillLine(midX + 40f, collarY + 6f,  d2X, d2Y - 8f, 5f, W);

        // Drone bodies (small rounded rect)
        FillRoundedRect(d1X - 18f, d1Y - 10f, 36f, 22f, 7f, W);
        FillRoundedRect(d2X - 18f, d2Y - 10f, 36f, 22f, 7f, W);

        // Drone rotors (arc strokes left and right of each body)
        // Drone 1 rotors
        StrokeArc(d1X - 28f, d1Y,  20f, 200f, 340f, 7f, W); // left rotor
        StrokeArc(d1X + 28f, d1Y,  20f, 200f, 340f, 7f, W); // right rotor
        // Drone 2 rotors
        StrokeArc(d2X - 28f, d2Y,  20f, 200f, 340f, 7f, W);
        StrokeArc(d2X + 28f, d2Y,  20f, 200f, 340f, 7f, W);

        // Drone sensor dots
        FillCircle(d1X, d1Y + 1f, 5f, CUT);
        FillCircle(d2X, d2Y + 1f, 5f, CUT);

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

        Debug.Log("[GenerateReviveStationIcon] Saved → " + OutputPath);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
