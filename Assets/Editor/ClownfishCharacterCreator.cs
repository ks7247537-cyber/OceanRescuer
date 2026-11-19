#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates the playable Fish_Character (Clownfish) under CharacterContainer
/// in GameplayScene. Loads Clownfish_0 sub-sprite directly from the spritesheet.
/// Menu: OceanRescuer > Create Clownfish Character
/// </summary>
public static class ClownfishCharacterCreator
{
    private const string SCENE_PATH   = "Assets/Scenes/GameplayScene.unity";
    private const string SPRITE_GUID  = "41048f0c2901b0347ba1a68b7bbd3952";
    private const long   SPRITE_FILEID = -3040762595238011028;  // Clownfish_0

    // Grid config (must match GridManager in scene)
    private const int   COLS     = 9;
    private const int   ROWS     = 5;
    private const float TILESIZE = 1.1f;

    [MenuItem("OceanRescuer/Create Clownfish Character")]
    public static void Run()
    {
        // ── 1. Open GameplayScene ────────────────────────────────────────────
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "GameplayScene")
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            activeScene = SceneManager.GetActiveScene();
        }

        // ── 2. Load Clownfish_0 sprite ───────────────────────────────────────
        string texPath = AssetDatabase.GUIDToAssetPath(SPRITE_GUID);
        Sprite fishSprite = null;

        // Try loading the specific sub-sprite by fileID
        if (!string.IsNullOrEmpty(texPath))
        {
            // Load all sprites from the asset
            Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(texPath);
            foreach (var obj in allSprites)
            {
                if (obj is Sprite sp)
                {
                    // Pick Clownfish_0 (first sprite, cleanest frame)
                    if (sp.name == "Clownfish_0")
                    {
                        fishSprite = sp;
                        break;
                    }
                }
            }

            // Fallback: use any sprite from the sheet
            if (fishSprite == null)
            {
                foreach (var obj in allSprites)
                {
                    if (obj is Sprite sp)
                    {
                        fishSprite = sp;
                        Debug.Log($"[ClownfishCreator] Using fallback sprite: {sp.name}");
                        break;
                    }
                }
            }
        }

        if (fishSprite == null)
        {
            Debug.LogError("[ClownfishCreator] Could not load any Clownfish sprite! Check Assets/Art/Characters/Clownfish.png import settings.");
            return;
        }

        Debug.Log($"[ClownfishCreator] Loaded sprite: {fishSprite.name} from {texPath}");

        // ── 3. Remove existing Fish_Character if present ─────────────────────
        var existing = GameObject.Find("Fish_Character");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[ClownfishCreator] Removed existing Fish_Character.");
        }

        // ── 4. Find or create CharacterContainer ────────────────────────────
        GameObject container = GameObject.Find("CharacterContainer");
        if (container == null)
        {
            container = new GameObject("CharacterContainer");
            container.transform.position = Vector3.zero;
            Debug.Log("[ClownfishCreator] Created CharacterContainer.");
        }

        // ── 5. Create Fish_Character ─────────────────────────────────────────
        GameObject fishGO = new GameObject("Fish_Character");
        fishGO.transform.SetParent(container.transform);

        // Position at grid cell (0,0) = water source
        float ox = -(COLS - 1) * TILESIZE * 0.5f;  // -4.4
        float oy = -(ROWS - 1) * TILESIZE * 0.5f;  // -2.2
        fishGO.transform.position = new Vector3(ox, oy, -1f);
        fishGO.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        fishGO.transform.localRotation = Quaternion.identity;

        // ── 6. Add and configure SpriteRenderer ─────────────────────────────
        var sr = fishGO.AddComponent<SpriteRenderer>();
        sr.sprite       = fishSprite;
        sr.sortingOrder = 10;           // above pipes (order ~0)
        sr.color        = Color.white;
        sr.enabled      = true;
        Debug.Log($"[ClownfishCreator] SpriteRenderer set: sprite={sr.sprite.name}, sortingOrder={sr.sortingOrder}");

        // ── 7. Add CharacterMover ────────────────────────────────────────────
        var mover = fishGO.AddComponent<CharacterMover>();
        mover.startCell     = new Vector2Int(0, 0);
        mover.moveSpeed     = 3f;
        mover.rotationSpeed = 180f;
        mover.idleBob       = true;
        mover.bobSpeed      = 1.5f;
        mover.bobAmount     = 0.08f;

        // Wire FlowManager and GridManager
        var flowMgr = Object.FindObjectOfType<FlowManager>();
        var gridMgr = Object.FindObjectOfType<GridManager>();
        mover.flowManager = flowMgr;
        mover.gridManager = gridMgr;

        if (flowMgr == null) Debug.LogWarning("[ClownfishCreator] FlowManager not found in scene — assign manually.");
        if (gridMgr == null) Debug.LogWarning("[ClownfishCreator] GridManager not found in scene — assign manually.");

        // ── 8. Wire GameManager ──────────────────────────────────────────────
        var gameMgr = Object.FindObjectOfType<GameManager>();
        if (gameMgr != null)
        {
            var so = new SerializedObject(gameMgr);
            var prop = so.FindProperty("characterMover");
            if (prop != null)
            {
                prop.objectReferenceValue = mover;
                so.ApplyModifiedProperties();
                Debug.Log("[ClownfishCreator] GameManager.characterMover wired.");
            }
        }
        else
        {
            Debug.LogWarning("[ClownfishCreator] GameManager not found in scene.");
        }

        // ── 9. Save scene ────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("[ClownfishCreator] ✅ Fish_Character created with Clownfish sprite! Scene saved.");
        Debug.Log($"[ClownfishCreator] Position: ({fishGO.transform.position.x}, {fishGO.transform.position.y}, {fishGO.transform.position.z})");
        Debug.Log($"[ClownfishCreator] Scale: {fishGO.transform.localScale}");
        Debug.Log($"[ClownfishCreator] SortingOrder: {sr.sortingOrder}");
    }
}
#endif
