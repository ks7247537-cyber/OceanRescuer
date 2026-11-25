#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LevelSelectionVisualCapture
{
    [MenuItem("OceanRescuer/Verification/Capture Before Screenshot")]
    public static void CaptureBefore()
    {
        CaptureToFile("scratch/level_selection_before.png");
    }

    [MenuItem("OceanRescuer/Verification/Capture After Screenshot")]
    public static void CaptureAfter()
    {
        CaptureToFile("scratch/level_selection_after.png");
    }

    public static void CaptureToFile(string relativePath)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/LevelSelectionScene.unity", OpenSceneMode.Single);
        
        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            var go = new GameObject("CaptureCam");
            cam = go.AddComponent<Camera>();
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        RenderMode originalMode = canvas != null ? canvas.renderMode : RenderMode.ScreenSpaceOverlay;
        Camera originalCam = canvas != null ? canvas.worldCamera : null;

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
        }

        int width = 1920;
        int height = 1080;

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        
        Canvas.ForceUpdateCanvases();
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        if (canvas != null)
        {
            canvas.renderMode = originalMode;
            canvas.worldCamera = originalCam;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        string dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"[LevelSelectionVisualCapture] Screenshot successfully saved to {fullPath} ({bytes.Length} bytes)");
    }
}
#endif
