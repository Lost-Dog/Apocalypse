using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameCreator.Runtime.Characters;

/// <summary>
/// Editor tool that replaces the active CharacterKernel units on a GC2 Character component
/// with passive stubs. Use this when attaching a passive Character component to characters
/// that manage their own movement (e.g. ABC Toolkit / Tobias TPS).
///
/// What it does:
///   - m_Player  → UnitPlayerPassive  (ABC reads IsControllable during cast sequences)
///   - m_Motion  → UnitMotionPassive  (no GC2 motion; Animim reads Height, LinearSpeed, etc.)
///   - m_Facing  → UnitFacingPassive  (no GC2 facing; Animim reads PivotSpeed)
///   - m_Driver  → UnitDriverPassive  (satisfies Animim's driver dependency, never moves character)
///   - m_Animim  → UnitAnimimPassive  (writes Animator params for GC2 modules but disables
///                                     root motion and suppresses GC2 mannequin repositioning
///                                     that caused the snap-back to spawn position)
/// </summary>
public class GC2KernelStripperTool : EditorWindow
{
    private const string MENU_PATH = "Tools/Game Creator 2/Strip Character Kernel Units";

    // ─── State ───────────────────────────────────────────────────────────────

    // Targets dragged in via the ObjectField. Stored so the window survives
    // focus changes without losing the user's selection.
    private readonly List<GameObject> m_Targets = new List<GameObject>();

    // ─── Window ──────────────────────────────────────────────────────────────

    [MenuItem(MENU_PATH)]
    public static void ShowWindow()
    {
        GC2KernelStripperTool window = GetWindow<GC2KernelStripperTool>();
        window.titleContent = new GUIContent("GC2 Kernel Stripper");
        window.minSize = new Vector2(380f, 300f);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10f);
        GUILayout.Label("GC2 Character Kernel Stripper", EditorStyles.boldLabel);
        GUILayout.Space(4f);

        EditorGUILayout.HelpBox(
            "Replaces active kernel units on the Character component with passive stubs:\n\n" +
            "• Player  → UnitPlayerPassive\n" +
            "• Motion  → UnitMotionPassive\n" +
            "• Facing  → UnitFacingPassive\n" +
            "• Driver  → UnitDriverPassive (no movement)\n" +
            "• Animim  → UnitAnimimPassive (no root-motion snap, no mannequin offset)\n\n" +
            "Drag the Tobias TPS scene instance and/or its prefab asset into the list below, " +
            "then click Strip.",
            MessageType.Info
        );

        GUILayout.Space(8f);
        GUILayout.Label("Target GameObjects:", EditorStyles.boldLabel);

        // ObjectField for each existing slot + one empty slot to add more.
        for (int i = 0; i < m_Targets.Count + 1; i++)
        {
            GameObject current = i < m_Targets.Count ? m_Targets[i] : null;
            GameObject next = (GameObject)EditorGUILayout.ObjectField(current, typeof(GameObject), true);

            if (i < m_Targets.Count)
            {
                if (next == null)
                    m_Targets.RemoveAt(i--);   // removed → compact list
                else
                    m_Targets[i] = next;
            }
            else if (next != null)
            {
                m_Targets.Add(next);           // new slot filled
            }
        }

        GUILayout.Space(10f);

        bool hasTargets = m_Targets.Count > 0;
        EditorGUI.BeginDisabledGroup(!hasTargets);
        if (GUILayout.Button("Strip Kernel Units", GUILayout.Height(36f)))
        {
            // Snapshot the list — state is owned by the window, not the selection.
            StripFromList(m_Targets.ToArray());
        }
        EditorGUI.EndDisabledGroup();

        if (!hasTargets)
        {
            EditorGUILayout.HelpBox(
                "Drag at least one GameObject into the list above.",
                MessageType.Warning
            );
        }
    }

    // ─── Core Logic ──────────────────────────────────────────────────────────

    private static void StripFromList(GameObject[] targets)
    {
        int stripped = 0;
        int skipped  = 0;

        foreach (GameObject go in targets)
        {
            if (go == null) continue;

            bool isPrefabAsset = !go.scene.IsValid();

            if (isPrefabAsset)
            {
                stripped += StripPrefabAsset(go) ? 1 : 0;
            }
            else
            {
                Character character = go.GetComponent<Character>();
                if (character == null) { skipped++; continue; }
                stripped += StripSceneInstance(character) ? 1 : 0;
            }
        }

        AssetDatabase.SaveAssets();

        string msg = $"Stripped kernel units from {stripped} Character component(s).";
        if (skipped > 0)
            msg += $"\n{skipped} GameObject(s) had no Character component and were skipped.";

        Debug.Log($"[GC2 Kernel Stripper] {msg}");
        EditorUtility.DisplayDialog("Done", msg, "OK");
    }

    /// <summary>
    /// Strips kernel units from a prefab asset by loading its contents, modifying in place,
    /// and saving back via PrefabUtility.
    /// </summary>
    private static bool StripPrefabAsset(GameObject prefabAsset)
    {
        string path = AssetDatabase.GetAssetPath(prefabAsset);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find asset path for '{prefabAsset.name}'.");
            return false;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        Character character = contents.GetComponent<Character>();

        if (character == null)
        {
            Debug.LogWarning($"[GC2 Kernel Stripper] No Character component on prefab '{prefabAsset.name}'.");
            PrefabUtility.UnloadPrefabContents(contents);
            return false;
        }

        bool changed = ApplyKernelStrip(character);

        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);

        if (changed)
            Debug.Log($"[GC2 Kernel Stripper] Stripped kernel units on prefab '{prefabAsset.name}'.");

        return changed;
    }

    /// <summary>
    /// Strips kernel units from a scene instance and records the override so the change
    /// is tracked against the source prefab if applicable.
    /// </summary>
    private static bool StripSceneInstance(Character character)
    {
        bool changed = ApplyKernelStrip(character);

        if (changed)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(character);
            EditorUtility.SetDirty(character);
            Debug.Log($"[GC2 Kernel Stripper] Stripped kernel units on scene instance '{character.gameObject.name}'.");
        }

        return changed;
    }

    /// <summary>
    /// Replaces the five kernel unit managed-reference fields:
    ///   Player  → UnitPlayerPassive
    ///   Motion  → UnitMotionPassive
    ///   Facing  → UnitFacingPassive
    ///   Driver  → UnitDriverPassive
    ///   Animim  → UnitAnimimPassive wired to the character's own Animator
    /// Returns true if any field changed.
    /// </summary>
    private static bool ApplyKernelStrip(Character character)
    {
        SerializedObject so = new SerializedObject(character);
        const string kernelBase = "m_Kernel";
        bool anyChanged = false;

        // Verify the kernel field exists.
        SerializedProperty kernelProp = so.FindProperty(kernelBase);
        if (kernelProp == null)
        {
            Debug.LogWarning($"[GC2 Kernel Stripper] Property '{kernelBase}' not found on " +
                             $"'{character.gameObject.name}'. Is this a GC2 Character component?");
            return false;
        }

        Debug.Log($"[GC2 Kernel Stripper] Found kernel on '{character.gameObject.name}' " +
                  $"(type={kernelProp.managedReferenceFullTypename})");

        // ── Replace all five kernel units with passive stubs ──────────────────
        // None of the five units can be left null:
        //   m_Player  → ABC reads Character.Player.IsControllable to toggle control during casts
        //   m_Motion  → UnitAnimimKinematic.OnUpdate reads Height, LinearSpeed, StandLevel, etc.
        //   m_Facing  → UnitAnimimKinematic.OnUpdate reads PivotSpeed
        //   m_Driver  → UnitAnimimKinematic.OnUpdate reads LocalMoveDirection, IsGrounded, SkinWidth
        //   m_Animim  → AnimimGraph.OnStartup reads Animim.Animator (handled separately below)

        string playerPath = $"{kernelBase}.m_Player";
        SerializedProperty playerProp = so.FindProperty(playerPath);
        if (playerProp == null)
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find '{playerPath}' — skipping.");
        else
        {
            Debug.Log($"[GC2 Kernel Stripper] '{playerPath}' = {playerProp.managedReferenceFullTypename ?? "null"}");
            if (playerProp.managedReferenceValue is not UnitPlayerPassive)
            {
                playerProp.managedReferenceValue = new UnitPlayerPassive();
                anyChanged = true;
                Debug.Log($"[GC2 Kernel Stripper] Replaced player with UnitPlayerPassive.");
            }
        }

        string motionPath = $"{kernelBase}.m_Motion";
        SerializedProperty motionProp = so.FindProperty(motionPath);
        if (motionProp == null)
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find '{motionPath}' — skipping.");
        else
        {
            Debug.Log($"[GC2 Kernel Stripper] '{motionPath}' = {motionProp.managedReferenceFullTypename ?? "null"}");
            if (motionProp.managedReferenceValue is not UnitMotionPassive)
            {
                motionProp.managedReferenceValue = new UnitMotionPassive();
                anyChanged = true;
                Debug.Log($"[GC2 Kernel Stripper] Replaced motion with UnitMotionPassive.");
            }
        }

        string facingPath = $"{kernelBase}.m_Facing";
        SerializedProperty facingProp = so.FindProperty(facingPath);
        if (facingProp == null)
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find '{facingPath}' — skipping.");
        else
        {
            Debug.Log($"[GC2 Kernel Stripper] '{facingPath}' = {facingProp.managedReferenceFullTypename ?? "null"}");
            if (facingProp.managedReferenceValue is not UnitFacingPassive)
            {
                facingProp.managedReferenceValue = new UnitFacingPassive();
                anyChanged = true;
                Debug.Log($"[GC2 Kernel Stripper] Replaced facing with UnitFacingPassive.");
            }
        }

        // ── Replace driver with passive stub ─────────────────────────────────
        string driverPath = $"{kernelBase}.m_Driver";
        SerializedProperty driverProp = so.FindProperty(driverPath);
        if (driverProp == null)
        {
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find property '{driverPath}' — skipping.");
        }
        else
        {
            Debug.Log($"[GC2 Kernel Stripper] '{driverPath}' = {driverProp.managedReferenceFullTypename ?? "null"}");
            if (driverProp.managedReferenceValue is not UnitDriverPassive)
            {
                driverProp.managedReferenceValue = new UnitDriverPassive();
                anyChanged = true;
                Debug.Log($"[GC2 Kernel Stripper] Replaced driver with UnitDriverPassive.");
            }
        }

        // ── Wire animim to the character's own Animator ───────────────────────
        // m_Animator is a [SerializeField] on TUnitAnimim — we cannot reliably traverse
        // two nested [SerializeReference] layers (m_Kernel → m_Animim → m_Animator) via
        // FindProperty. Instead, set it via reflection on the C# object before assigning
        // managedReferenceValue so the value is baked into the serialised snapshot.
        string animimPath = $"{kernelBase}.m_Animim";
        SerializedProperty animimProp = so.FindProperty(animimPath);
        if (animimProp == null)
        {
            Debug.LogWarning($"[GC2 Kernel Stripper] Could not find property '{animimPath}' — skipping.");
        }
        else
        {
            Debug.Log($"[GC2 Kernel Stripper] '{animimPath}' = {animimProp.managedReferenceFullTypename ?? "null"}");

            Animator animator = character.GetComponentInChildren<Animator>();
            Debug.Log($"[GC2 Kernel Stripper] Found Animator on '{animator?.gameObject.name ?? "NONE"}'.");

            if (animator == null)
            {
                Debug.LogWarning($"[GC2 Kernel Stripper] No Animator found on '{character.gameObject.name}' " +
                                 "or its children. Assign the Animator manually in the Character's Animim unit.");
            }

            // Always rebuild the animim unit to pick up any class changes (e.g. base class swap
            // from UnitAnimimKinematic → TUnitAnimim). The cost is negligible — it's a single
            // managed reference assignment.
            bool needsRebuild = true;
            if (animimProp.managedReferenceValue is UnitAnimimPassive existing)
            {
                Debug.Log($"[GC2 Kernel Stripper] Existing UnitAnimimPassive.Animator = " +
                          $"'{existing.Animator?.gameObject.name ?? "null"}'");
            }

            if (needsRebuild)
            {
                var kinematic = new UnitAnimimPassive();

                System.Reflection.FieldInfo animatorField =
                    typeof(TUnitAnimim).GetField(
                        "m_Animator",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (animatorField != null && animator != null)
                {
                    animatorField.SetValue(kinematic, animator);
                    Debug.Log($"[GC2 Kernel Stripper] Set m_Animator via reflection → '{animator.gameObject.name}'.");
                }
                else
                {
                    Debug.LogWarning($"[GC2 Kernel Stripper] animatorField={animatorField != null}, " +
                                     $"animator={animator != null} — m_Animator NOT set.");
                }

                animimProp.managedReferenceValue = kinematic;
                anyChanged = true;
                Debug.Log($"[GC2 Kernel Stripper] Assigned new UnitAnimimPassive as m_Animim.");
            }
        }

        if (anyChanged)
            so.ApplyModifiedPropertiesWithoutUndo();

        return anyChanged;
    }
}
