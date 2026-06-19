using System;

public static class PrefabClassificationUtility
{
    public enum PrefabCategory
    {
        Buildings,
        StructuralModules,
        RoadsOverpass,
        PropsDeco
    }

    public enum PrefabPack
    {
        Unknown,
        CityPack5,
        MegaVRse,
        MegaVRse2
    }

    public static PrefabCategory Classify(string fileName)
    {
        string n = fileName.ToLowerInvariant();

        if (ContainsAny(n,
            "road", "street", "overpass", "roundabout", "ramp", "traffic", "busstation", "cross"))
        {
            return PrefabCategory.RoadsOverpass;
        }

        if (ContainsAny(n,
            "building", "shop", "market", "tower", "house", "church", "garage", "reception", "pharmacy", "barber", "newsstand", "gshop", "coffehouse", "bank", "hotel"))
        {
            return PrefabCategory.Buildings;
        }

        if (ContainsAny(n,
            "block", "wall", "roof", "floor", "column", "stairs", "gate", "fence", "window", "fill", "canopy", "pannel", "panel", "segment", "enterance", "entrance", "glass"))
        {
            return PrefabCategory.StructuralModules;
        }

        return PrefabCategory.PropsDeco;
    }

    public static PrefabPack IdentifyPack(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return PrefabPack.Unknown;

        string normalized = assetPath.Replace('\\', '/');
        if (normalized.StartsWith("Assets/CityPack5/", StringComparison.OrdinalIgnoreCase)) return PrefabPack.CityPack5;
        if (normalized.StartsWith("Assets/MegaVRse 2/", StringComparison.OrdinalIgnoreCase)) return PrefabPack.MegaVRse2;
        if (normalized.StartsWith("Assets/MegaVRse/", StringComparison.OrdinalIgnoreCase)) return PrefabPack.MegaVRse;

        return PrefabPack.Unknown;
    }

    public static string GetCategoryFolderName(PrefabCategory category)
    {
        switch (category)
        {
            case PrefabCategory.Buildings:
                return "Buildings";
            case PrefabCategory.StructuralModules:
                return "Structural Modules";
            case PrefabCategory.RoadsOverpass:
                return "Roads and Overpass";
            default:
                return "Props and Deco";
        }
    }

    public static string ToLabelSuffix(PrefabCategory category)
    {
        switch (category)
        {
            case PrefabCategory.Buildings:
                return "buildings";
            case PrefabCategory.StructuralModules:
                return "structural-modules";
            case PrefabCategory.RoadsOverpass:
                return "roads-overpass";
            default:
                return "props-deco";
        }
    }

    public static string GetPackDisplayName(PrefabPack pack)
    {
        switch (pack)
        {
            case PrefabPack.CityPack5:
                return "CityPack5";
            case PrefabPack.MegaVRse:
                return "MegaVRse";
            case PrefabPack.MegaVRse2:
                return "MegaVRse 2";
            default:
                return "Unknown";
        }
    }

    public static bool TryInferCategoryFromAssetPath(string assetPath, string fallbackName, out PrefabCategory category)
    {
        category = PrefabCategory.PropsDeco;
        if (string.IsNullOrEmpty(assetPath))
        {
            category = Classify(fallbackName ?? string.Empty);
            return false;
        }

        string normalized = assetPath.Replace('\\', '/').ToLowerInvariant();

        if (normalized.Contains("/source/buildings/"))
        {
            category = PrefabCategory.Buildings;
            return true;
        }

        if (normalized.Contains("/source/roads/") || normalized.Contains("/source/signs/"))
        {
            category = PrefabCategory.RoadsOverpass;
            return true;
        }

        if (normalized.Contains("/source/props/") ||
            normalized.Contains("/source/foliage/") ||
            normalized.Contains("/source/decals/") ||
            normalized.Contains("/source/cars/") ||
            normalized.Contains("/source/lights/") ||
            normalized.Contains("/source/sky/") ||
            normalized.Contains("/source/terrain/") ||
            normalized.Contains("/textures/") ||
            normalized.Contains("/materials/") ||
            normalized.Contains("/light data/"))
        {
            category = PrefabCategory.PropsDeco;
            return true;
        }

        category = Classify(fallbackName ?? normalized);
        return false;
    }

    private static bool ContainsAny(string source, params string[] needles)
    {
        for (int i = 0; i < needles.Length; i++)
        {
            if (source.Contains(needles[i]))
            {
                return true;
            }
        }

        return false;
    }
}