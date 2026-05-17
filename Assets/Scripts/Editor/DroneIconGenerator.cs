using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Procedurally generates a low-poly drone icon in the Synty Polygon style
/// and saves it to Assets/Tutorials/Icons/drone_icon.png.
/// Run via Tools → Generate Drone Icon.
/// </summary>
public static class DroneIconGenerator
{
    private const int Size = 512;
    private const string OutputPath = "Assets/Tutorials/Icons/drone_icon.png";

    private static Color[] _pixels;

    [MenuItem("Tools/Generate Drone Icon")]
    public static void Generate()
    {
        _pixels = new Color[Size * Size];

        // White background
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = Color.white;

        DrawDrone();

        Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.SetPixels(_pixels);
        tex.Apply();

        File.WriteAllBytes(OutputPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(OutputPath);
        AssetDatabase.Refresh();

        Debug.Log($"[DroneIconGenerator] Saved → {OutputPath}");
        EditorUtility.RevealInFinder(OutputPath);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Drawing logic — painter's algorithm (back to front)
    // ──────────────────────────────────────────────────────────────────

    private static void DrawDrone()
    {
        // ── Palette ────────────────────────────────────────────────────
        Color iceBlue    = H("#B8D4E8");
        Color steelBlue  = H("#8AAEC8");
        Color darkSteel  = H("#5C7A96");
        Color deepShadow = H("#3D5570");
        Color armTop     = H("#C5D8E6");
        Color armSide    = H("#7A9BB5");
        Color armShd     = H("#4A6880");
        Color rotorHi    = H("#D6EBEF");
        Color rotorMid   = H("#88C4CC");
        Color rotorShd   = H("#4A8D96");
        Color bladeHi    = H("#E2EEF0");
        Color bladeMid   = H("#9BBFC5");
        Color bladeDrk   = H("#7AAFC0");
        Color camBody    = H("#2A4560");
        Color camDark    = H("#1A3040");
        Color lensGlint  = H("#7EC8E3");
        Color lensHi     = H("#D6F0F8");
        Color bodyFaceA  = H("#D6EBEF");
        Color bodyFaceB  = H("#C5D8E6");
        Color bodyFaceC  = H("#9EC4D8");
        Color bodyBase   = H("#2E4560");
        Color ledRed     = H("#FF6B6B");
        Color ledHi      = H("#FFAAAA");

        // ── ARMS (rear first, then front) ──────────────────────────────

        // Rear-left arm
        Poly(new[]{ V(256,256), V(168,210), V(130,168), V(200,195) }, H("#B0C8DC"));
        Poly(new[]{ V(168,210), V(130,168), V(128,182), V(166,225) }, darkSteel);
        Poly(new[]{ V(256,256), V(168,210), V(166,225), V(254,271) }, armShd);

        // Rear-right arm
        Poly(new[]{ V(256,256), V(344,210), V(382,168), V(312,195) }, armTop);
        Poly(new[]{ V(344,210), V(382,168), V(384,182), V(346,225) }, armSide);
        Poly(new[]{ V(256,256), V(344,210), V(346,225), V(258,271) }, armShd);

        // Front-left arm
        Poly(new[]{ V(256,256), V(168,302), V(130,344), V(200,317) }, steelBlue);
        Poly(new[]{ V(168,302), V(130,344), V(128,330), V(166,287) }, armShd);
        Poly(new[]{ V(256,256), V(168,302), V(166,287), V(254,241) }, deepShadow);

        // Front-right arm
        Poly(new[]{ V(256,256), V(344,302), V(382,344), V(312,317) }, bodyFaceC);
        Poly(new[]{ V(344,302), V(382,344), V(384,330), V(346,287) }, darkSteel);
        Poly(new[]{ V(256,256), V(344,302), V(346,287), V(258,241) }, deepShadow);

        // ── ROTORS ─────────────────────────────────────────────────────
        Rotor(130, 168, rotorHi,  rotorMid, rotorShd, bladeHi,  bladeMid, -30, 60);
        Rotor(382, 168, rotorHi,  rotorMid, rotorShd, bladeHi,  bladeHi,   30,-60);
        Rotor(130, 344, rotorMid, rotorShd, rotorShd, bladeMid, bladeDrk, -30, 60);
        Rotor(382, 344, rotorMid, rotorShd, rotorShd, bladeMid, bladeDrk,  30,-60);

        // ── CENTRAL BODY ───────────────────────────────────────────────

        // Shadow base plate
        Poly(new[]{ V(256,282), V(210,268), V(196,256), V(210,244),
                    V(256,230), V(302,244), V(316,256), V(302,268) }, deepShadow);

        // Top faces (flat-shaded, lightest toward top-right)
        Poly(new[]{ V(256,234), V(302,248), V(302,264), V(256,250) }, bodyFaceA); // top-right
        Poly(new[]{ V(302,248), V(316,260), V(302,264)             }, iceBlue);   // right tip
        Poly(new[]{ V(210,248), V(256,234), V(256,250), V(210,264) }, steelBlue); // top-left
        Poly(new[]{ V(196,260), V(210,248), V(210,264)             }, darkSteel); // left tip
        Poly(new[]{ V(210,248), V(256,234), V(302,248), V(256,250) }, bodyFaceB); // top-centre
        Poly(new[]{ V(210,264), V(256,278), V(302,264), V(256,250) }, steelBlue); // bottom-centre
        Poly(new[]{ V(256,278), V(302,264), V(316,260), V(302,268),
                    V(304,284), V(258,282)                          }, bodyFaceC); // bottom-right
        Poly(new[]{ V(210,264), V(256,278), V(258,282), V(208,284),
                    V(194,270), V(196,260)                          }, darkSteel); // bottom-left

        // Side walls
        Poly(new[]{ V(316,260), V(302,268), V(304,284), V(318,276) }, armShd);
        Poly(new[]{ V(196,260), V(194,270), V(208,284), V(210,268) }, deepShadow);
        Poly(new[]{ V(302,268), V(258,282), V(260,294), V(304,284) }, darkSteel);
        Poly(new[]{ V(210,268), V(208,284), V(260,294), V(258,282) }, deepShadow);

        // Underside
        Poly(new[]{ V(208,284), V(260,294), V(304,284), V(318,276),
                    V(302,268), V(258,282), V(210,268), V(194,270) }, bodyBase);

        // ── LANDING SKIDS ──────────────────────────────────────────────
        Poly(new[]{ V(218,280), V(210,290), V(208,308), V(218,298) }, armShd);
        Poly(new[]{ V(208,308), V(218,298), V(218,310), V(208,320) }, deepShadow);
        Poly(new[]{ V(296,280), V(304,290), V(306,308), V(296,298) }, darkSteel);
        Poly(new[]{ V(306,308), V(296,298), V(296,310), V(306,320) }, armShd);
        // Skid bar
        Poly(new[]{ V(208,316), V(218,308), V(296,308), V(306,316),
                    V(296,320), V(218,320)                          }, darkSteel);
        Poly(new[]{ V(218,320), V(296,320), V(296,324), V(218,324) }, deepShadow);

        // ── CAMERA GIMBAL ──────────────────────────────────────────────
        Poly(new[]{ V(230,272), V(226,278), V(228,290), V(234,284) }, armShd);
        Poly(new[]{ V(282,272), V(286,278), V(284,290), V(278,284) }, armShd);
        Poly(new[]{ V(230,272), V(282,272), V(278,284), V(234,284) }, darkSteel);

        // Camera lens assembly
        Ellipse(256, 292, 24, 18, camBody);
        Ellipse(256, 290, 24, 18, camBody);
        Ellipse(256, 290, 18, 14, camDark);
        EllipseA(256, 290, 12,  9, lensGlint, 0.55f);
        EllipseA(250, 286,  5,  3, lensHi,    0.85f, -20f);

        // ── STATUS LED ─────────────────────────────────────────────────
        Ellipse(256, 238, 8, 5, deepShadow);
        Ellipse(256, 236, 8, 5, ledRed);
        Ellipse(254, 234, 3, 2, ledHi);

        // ── ANTENNA ────────────────────────────────────────────────────
        Poly(new[]{ V(270,240), V(274,238), V(278,218), V(274,220) }, armSide);
        Poly(new[]{ V(274,238), V(278,218), V(280,220), V(276,240) }, darkSteel);
        Ellipse(278, 217, 4, 3, rotorMid);
        Ellipse(278, 215, 4, 3, rotorHi);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Rotor helper
    // ──────────────────────────────────────────────────────────────────

    private static void Rotor(
        float cx, float cy,
        Color ringHi, Color ringMid, Color ringShd,
        Color blade1, Color blade2,
        float angle1, float angle2)
    {
        // Drop shadow
        EllipseA(cx + 4, cy + 8, 46, 28, new Color(0.15f, 0.25f, 0.35f), 0.35f);
        // Underside thickness
        Ellipse(cx, cy + 4, 46, 28, ringShd);
        // Blade shadows
        Ellipse(cx, cy + 4, 38, 10, ringShd, angle1);
        Ellipse(cx, cy + 4, 38, 10, ringShd, angle2);
        // Top ring face
        Ellipse(cx, cy, 46, 28, ringMid);
        // Blades top
        Ellipse(cx, cy, 36,  8, blade1, angle1);
        Ellipse(cx, cy, 36,  8, blade2, angle2);
        // Ring outlines
        Ring(cx, cy, 46, 28, ringHi, 7f);
        Ring(cx, cy, 46, 28, ringMid, 3.5f);
        // Hub
        Ellipse(cx, cy,     10, 7, H("#B8D4E8"));
        Ellipse(cx, cy - 2,  8, 5, H("#E2EEF0"));
    }

    // ──────────────────────────────────────────────────────────────────
    //  Rasterization primitives
    // ──────────────────────────────────────────────────────────────────

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    /// <summary>Sets a pixel, converting from SVG-space (top-left) to texture-space (bottom-left).
    /// Alpha-blends over the existing pixel.</summary>
    private static void SetPx(int x, int y, Color c)
    {
        int ty = Size - 1 - y;
        if ((uint)x >= Size || (uint)ty >= Size) return;

        int idx = ty * Size + x;
        Color bg = _pixels[idx];
        float a = c.a;
        _pixels[idx] = new Color(
            bg.r + (c.r - bg.r) * a,
            bg.g + (c.g - bg.g) * a,
            bg.b + (c.b - bg.b) * a,
            1f);
    }

    /// <summary>Scanline polygon fill.</summary>
    private static void Poly(Vector2[] pts, Color color)
    {
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in pts)
        {
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        int n = pts.Length;
        for (int y = (int)minY; y <= (int)maxY; y++)
        {
            var xs = new List<float>(4);
            for (int i = 0; i < n; i++)
            {
                Vector2 a = pts[i], b = pts[(i + 1) % n];
                if ((a.y <= y && b.y > y) || (b.y <= y && a.y > y))
                    xs.Add(a.x + (y - a.y) / (b.y - a.y) * (b.x - a.x));
            }
            xs.Sort();
            for (int i = 0; i + 1 < xs.Count; i += 2)
                for (int x = (int)xs[i]; x <= (int)xs[i + 1]; x++)
                    SetPx(x, y, color);
        }
    }

    /// <summary>Filled (optionally rotated) ellipse.</summary>
    private static void Ellipse(float cx, float cy, float rx, float ry, Color color, float angleDeg = 0f)
    {
        float cos = Mathf.Cos(angleDeg * Mathf.Deg2Rad);
        float sin = Mathf.Sin(angleDeg * Mathf.Deg2Rad);
        int r = (int)(Mathf.Max(rx, ry) + 2);

        for (int y = (int)(cy - r); y <= (int)(cy + r); y++)
        for (int x = (int)(cx - r); x <= (int)(cx + r); x++)
        {
            float dx = x - cx, dy = y - cy;
            float lx = cos * dx + sin * dy;
            float ly = -sin * dx + cos * dy;
            if (lx * lx / (rx * rx) + ly * ly / (ry * ry) <= 1f)
                SetPx(x, y, color);
        }
    }

    /// <summary>Filled ellipse with explicit alpha override.</summary>
    private static void EllipseA(float cx, float cy, float rx, float ry, Color color, float alpha, float angleDeg = 0f)
    {
        Ellipse(cx, cy, rx, ry, new Color(color.r, color.g, color.b, alpha), angleDeg);
    }

    /// <summary>Ellipse outline (ring) with a given stroke width.</summary>
    private static void Ring(float cx, float cy, float rx, float ry, Color color, float strokeWidth, float angleDeg = 0f)
    {
        float half = strokeWidth * 0.5f;
        float rxO = rx + half, ryO = ry + half;
        float rxI = rx - half, ryI = ry - half;
        float cos = Mathf.Cos(angleDeg * Mathf.Deg2Rad);
        float sin = Mathf.Sin(angleDeg * Mathf.Deg2Rad);
        int r = (int)(rxO + 2);

        for (int y = (int)(cy - r); y <= (int)(cy + r); y++)
        for (int x = (int)(cx - r); x <= (int)(cx + r); x++)
        {
            float dx = x - cx, dy = y - cy;
            float lx = cos * dx + sin * dy;
            float ly = -sin * dx + cos * dy;
            float outer = lx * lx / (rxO * rxO) + ly * ly / (ryO * ryO);
            float inner = (rxI > 0 && ryI > 0)
                ? lx * lx / (rxI * rxI) + ly * ly / (ryI * ryI)
                : 2f;
            if (outer <= 1f && inner >= 1f)
                SetPx(x, y, color);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Utility
    // ──────────────────────────────────────────────────────────────────

    private static Color H(string hex)
    {
        hex = hex.TrimStart('#');
        float r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255f;
        return new Color(r, g, b, 1f);
    }
}
