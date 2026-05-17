using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates a 512x512 white-silhouette turret-on-tripod icon (Division 2 style).
/// Run via: Tools > Generate Turret Icon
/// </summary>
public static class GenerateTurretIcon
{
    private const string OutputPath =
        "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Status/ICON_Apocalypse_Status_Turret_01.png";

    private const int S = 512;

    [MenuItem("Tools/Generate Turret Icon")]
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

        void FillRoundedRect(float rx, float ry, float rw, float rh, float rad, Color c)
        {
            FillRect(rx + rad, ry, rw - rad * 2f, rh, c);
            FillRect(rx, ry + rad, rad, rh - rad * 2f, c);
            FillRect(rx + rw - rad, ry + rad, rad, rh - rad * 2f, c);
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
        Color CUT = new Color(0f, 0f, 0f, 0.72f);

        // ── Layout ────────────────────────────────────────────────────────
        //
        //    [~] ← antenna nub
        //   [sens]
        //  ┌──────────────────┐
        //  │  turret body     │═══════[■]  ← barrel + muzzle brake
        //  └──────────────────┘
        //          │                       ← mounting column
        //         (●)                      ← swivel joint
        //        / | \                     ← three tripod legs
        //       /  |  \
        //      [_][_][_]                   ← foot pads with ankle joints

        float apexX = 250f, apexY = 192f;

        // ── Three tripod legs ─────────────────────────────────────────────

        float lfx = 114f, lfy = 68f;
        FillLine(apexX, apexY, lfx, lfy, 13f, W);

        float rfx = 388f, rfy = 68f;
        FillLine(apexX, apexY, rfx, rfy, 13f, W);

        float cfx = 250f, cfy = 60f;
        FillLine(apexX, apexY, cfx, cfy, 8f, W);

        // Ankle joints
        float lkx = apexX + (lfx - apexX) * 0.52f, lky = apexY + (lfy - apexY) * 0.52f;
        float rkx = apexX + (rfx - apexX) * 0.52f, rky = apexY + (rfy - apexY) * 0.52f;
        FillCircle(lkx, lky, 14f, W);
        FillCircle(rkx, rky, 14f, W);

        // Foot pads
        FillLine(lfx - 28f, lfy, lfx + 28f, lfy, 11f, W);
        FillLine(rfx - 28f, rfy, rfx + 28f, rfy, 11f, W);
        FillLine(cfx - 18f, cfy, cfx + 18f, cfy, 9f,  W);

        // Junction hub
        FillCircle(apexX, apexY, 22f, W);

        // ── Mounting column ───────────────────────────────────────────────
        float colTopY = 288f;
        FillRect(apexX - 17f, apexY, 34f, (colTopY - apexY) * 0.55f, W);
        FillRect(apexX - 13f, apexY + (colTopY - apexY) * 0.55f,
                 26f, (colTopY - apexY) * 0.45f, W);

        // Swivel joint
        FillCircle(apexX, colTopY, 25f, W);

        // ── Turret body ───────────────────────────────────────────────────
        float bodyX = 136f, bodyY = 286f, bodyW = 192f, bodyH = 130f;
        FillRoundedRect(bodyX, bodyY, bodyW, bodyH, 20f, W);

        // ── Gun barrel ───────────────────────────────────────────────────
        float barrelCY  = bodyY + bodyH * 0.42f;
        float barrelX   = bodyX + bodyW - 8f;
        float barrelLen = 152f, barrelH = 24f;
        FillRoundedRect(barrelX, barrelCY - barrelH * 0.5f, barrelLen, barrelH, 6f, W);

        // Muzzle brake
        float muzzX = barrelX + barrelLen - 2f, muzzH = 40f;
        FillRoundedRect(muzzX, barrelCY - muzzH * 0.5f, 26f, muzzH, 8f, W);

        // Barrel support strut
        FillLine(barrelX + 14f, bodyY + 8f,
                 barrelX + 14f, barrelCY - barrelH * 0.5f, 5f, W);

        // ── Sensor block on top of body ───────────────────────────────────
        float sensorX = bodyX + 22f, sensorY = bodyY + bodyH - 2f;
        float sensorW = 88f, sensorH = 36f;
        FillRoundedRect(sensorX, sensorY, sensorW, sensorH, 10f, W);

        // Lens
        FillCircle(sensorX + sensorW - 16f, sensorY + sensorH * 0.5f, 11f, W);

        // Antenna
        FillRect(sensorX + 20f, sensorY + sensorH, 10f, 24f, W);
        FillCircle(sensorX + 25f, sensorY + sensorH + 28f, 8f, W);

        // ── Detail cuts ───────────────────────────────────────────────────
        float slotX = bodyX + 24f, slotW = bodyW - 52f;
        FillRect(slotX, bodyY + 30f, slotW, 9f,  CUT);
        FillRect(slotX, bodyY + 50f, slotW, 9f,  CUT);
        FillRect(slotX, bodyY + 70f, slotW, 9f,  CUT);

        FillRect(barrelX + 22f, barrelCY - barrelH * 0.5f, 5f, barrelH, CUT);
        FillRect(barrelX + 60f, barrelCY - barrelH * 0.5f, 5f, barrelH, CUT);

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

        Debug.Log("[GenerateTurretIcon] Saved → " + OutputPath);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
