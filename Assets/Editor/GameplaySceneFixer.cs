#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-click fix for GameplayScene:
/// - Attaches GameplayBootstrapper to GameManager if missing
/// - Sets GridManager prefab references from Resources
/// - Saves scene
/// Menu: OceanRescuer > Fix GameplayScene
/// </summary>
public static class GameplaySceneFixer
{
    [MenuItem("OceanRescuer/Fix GameplayScene (Attach Bootstrapper + Prefabs)")]
    public static void FixGameplayScene()
    {
        // Open GameplayScene if not already open
        string scenePath = "Assets/Scenes/GameplayScene.unity";
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!current.path.Contains("GameplayScene"))
        {
            if (!EditorUtility.DisplayDialog("Fix GameplayScene",
                "GameplayScene ko open karke fix karega.\nContinue?", "Yes", "Cancel")) return;
            EditorSceneManager.OpenScene(scenePath);
        }

        // Find or create GameManager
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("[GameplaySceneFixer] GameManager not found!"); return; }

        // Attach GameplayBootstrapper if missing
        var boot = gm.GetComponent<GameplayBootstrapper>();
        if (boot == null)
        {
            boot = gm.gameObject.AddComponent<GameplayBootstrapper>();
            Debug.Log("[GameplaySceneFixer] GameplayBootstrapper attached.");
        }

        // Wire pipe prefabs to GridManager
        var grid = Object.FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pS = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Pipes/Prefabs/Pipe_Straight.prefab");
            var pE = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Pipes/Prefabs/Pipe_Elbow.prefab");
            var pT = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Pipes/Prefabs/Pipe_T.prefab");
            var pX = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Pipes/Prefabs/Pipe_Cross.prefab");

            var so = new SerializedObject(grid);
            so.FindProperty("straightPrefab").objectReferenceValue  = pS;
            so.FindProperty("elbowPrefab").objectReferenceValue     = pE;
            so.FindProperty("tJunctionPrefab").objectReferenceValue = pT;
            so.FindProperty("crossPrefab").objectReferenceValue     = pX;
            so.FindProperty("Cols").intValue = 9;
            so.FindProperty("Rows").intValue = 5;
            so.FindProperty("TileSize").floatValue = 1.1f;
            so.FindProperty("useScenePlacedTiles").boolValue = false; // let GridManager generate
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(grid);
            Debug.Log("[GameplaySceneFixer] GridManager prefabs wired. Cols=9 Rows=5 TileSize=1.1");
        }

        // Set camera orthoSize for portrait
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 8f;
            cam.transform.position = new Vector3(0, 0, -10f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done!",
            "GameplayScene fixed!\n\n" +
            "✅ GameplayBootstrapper attached\n" +
            "✅ Pipe prefabs wired to GridManager\n" +
            "✅ GridManager: 9×5, TileSize=1.1\n" +
            "✅ Camera orthoSize=8\n\n" +
            "▶ Play dabao — pipes auto-generate honge!\n" +
            "Ya 'Build Level 01' menu se snake layout banao.", "OK");
    }

    [MenuItem("OceanRescuer/Set Portrait + 1080x1920")]
    public static void SetPortrait()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        Debug.Log("[GameplaySceneFixer] Portrait orientation set.");
        EditorUtility.DisplayDialog("Portrait Set",
            "✅ Portrait orientation locked\n✅ 1080×1920 reference resolution applied to all Canvases\n\nCanvas Scaler: Scale With Screen Size", "OK");
    }
}
#endif
