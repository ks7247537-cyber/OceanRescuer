#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// Targeted pipe prefab scale fixer.
/// Sets localScale on each pipe prefab so it displays at TileSize=1.1 units.
/// Also assigns proper individual pipe sprites from the sprite sheets.
/// </summary>
public static class PipePrefabScaleFixer
{
    // Target visual size in world units (matches GridManager.TileSize)
    const float TARGET_SIZE = 1.0f;

    [MenuItem("OceanRescuer/Fix Pipe Prefab Scales")]
    public static void FixPipeScales()
    {
        bool anyChanged = false;

        // Map: prefabName -> sprite asset path + sprite name pattern
        var configs = new System.Collections.Generic.Dictionary<string, (string artPath, string spritePattern, PipeTile.PipeType pipeType)>
        {
            ["Pipe_Straight"] = ("Assets/Art/Pipes/straight pipe.png",    "straight pipe", PipeTile.PipeType.Straight),
            ["Pipe_Elbow"]    = ("Assets/Art/Pipes/curved pipe.png",       "curved pipe",   PipeTile.PipeType.Elbow),
            ["Pipe_T"]        = ("Assets/Art/Pipes/t  pipe .png",          "t  pipe",        PipeTile.PipeType.TJunction),
            ["Pipe_Cross"]    = ("Assets/Art/Pipes/cross pipe.png",        "cross pipe",    PipeTile.PipeType.Cross),
        };

        foreach (var kv in configs)
        {
            string prefabPath = $"Assets/Resources/Prefabs/{kv.Key}.prefab";
            var (artPath, _, pipeType) = kv.Value;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[PipeFix] Prefab not found: {prefabPath}");
                continue;
            }

            // Unity 6 compatible prefab editing
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            bool changed = false;

            // 1. Fix scale
            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Vector2 spriteWorldSize = sr.sprite.bounds.size;
                Debug.Log($"[PipeFix] {kv.Key} sprite world size: {spriteWorldSize}");

                float scaleX = TARGET_SIZE / Mathf.Max(spriteWorldSize.x, 0.01f);
                float scaleY = TARGET_SIZE / Mathf.Max(spriteWorldSize.y, 0.01f);
                float uniformScale = Mathf.Min(scaleX, scaleY);

                Vector3 newScale = new Vector3(uniformScale, uniformScale, 1f);
                if (Vector3.Distance(root.transform.localScale, newScale) > 0.001f)
                {
                    root.transform.localScale = newScale;
                    changed = true;
                    Debug.Log($"[PipeFix] {kv.Key} scale set to {newScale}");
                }
            }
            else if (sr != null && sr.sprite == null)
            {
                Sprite sp = TryLoadBestSprite(artPath, kv.Key);
                if (sp != null)
                {
                    sr.sprite = sp;
                    changed = true;
                    Debug.Log($"[PipeFix] Assigned sprite to {kv.Key}: {sp.name}");
                    Vector2 spriteWorldSize = sp.bounds.size;
                    float uniformScale = TARGET_SIZE / Mathf.Max(spriteWorldSize.x, spriteWorldSize.y, 0.01f);
                    root.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                    Debug.Log($"[PipeFix] {kv.Key} scale set after sprite assign: {uniformScale}");
                }
                else
                {
                    sr.color = new Color(0.3f, 0.7f, 1f);
                    root.transform.localScale = Vector3.one;
                    changed = true;
                }
            }

            // 2. Fix BoxCollider2D
            var col = root.GetComponent<BoxCollider2D>();
            if (col == null) { col = root.AddComponent<BoxCollider2D>(); changed = true; }
            var sr2 = root.GetComponent<SpriteRenderer>();
            if (sr2 != null && sr2.sprite != null)
            {
                if (Vector2.Distance(col.size, sr2.sprite.bounds.size) > 0.01f)
                {
                    col.size = sr2.sprite.bounds.size;
                    changed = true;
                }
            }
            else if (col.size != new Vector2(1f, 1f))
            {
                col.size = new Vector2(1f, 1f);
                changed = true;
            }

            // 3. Set PipeTile type
            var pt = root.GetComponent<PipeTile>();
            if (pt != null && pt.pipeType != pipeType) { pt.pipeType = pipeType; changed = true; }

            if (changed) { PrefabUtility.SaveAsPrefabAsset(root, prefabPath); anyChanged = true; }
            PrefabUtility.UnloadPrefabContents(root);
        }

        if (anyChanged)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[PipeFix] ✅ All pipe prefab scales fixed!");
        }
        else
        {
            Debug.Log("[PipeFix] ✅ Pipe prefabs already look correct.");
        }
    }

    static Sprite TryLoadBestSprite(string primaryPath, string prefabName)
    {
        // Try primary path first
        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(primaryPath);
        if (sp != null) return sp;

        // Try all sprites in Assets/Art/Pipes/
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Art/Pipes" });
        foreach (var g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (sp != null) return sp;
        }
        return null;
    }
}
#endif
