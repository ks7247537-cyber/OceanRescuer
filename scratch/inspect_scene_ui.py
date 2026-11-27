import os
import re

scenes = [
    "Assets/Scenes/SplashScene.unity",
    "Assets/Scenes/MainMenuScene.unity",
    "Assets/Scenes/WorldSelectionScene.unity",
    "Assets/Scenes/LevelSelectionScene.unity",
    "Assets/Scenes/GameplayScene.unity"
]

print("=======================================================================")
print("                   SCENE UI HIERARCHY & CANVAS INSPECTION             ")
print("=======================================================================")

for sc in scenes:
    print(f"\n--- SCENE: {os.path.basename(sc)} ---")
    if not os.path.exists(sc):
        print("  File not found!")
        continue

    with open(sc, "r", encoding="utf-8", errors="ignore") as f:
        content = f.read()

    # Find Canvas Scaler (classID 223 / MonoBehaviour CanvasScaler)
    scalers = re.findall(r'CanvasScaler:[^\n]*\n([\s\S]*?)(?=\n---|\Z)', content)
    if not scalers:
        # Check standard MonoBehaviour CanvasScaler
        scalers = re.findall(r'm_ReferenceResolution: \{x: ([0-9.]+), y: ([0-9.]+)\}[\s\S]*?m_MatchWidthOrHeight: ([0-9.]+)', content)
        if scalers:
            for s in scalers:
                print(f"  CanvasScaler: RefResolution = {s[0]}x{s[1]}, MatchWidthOrHeight = {s[2]}")
        else:
            print("  CanvasScaler: NOT FOUND or default ConstantPixelSize")
    else:
        for s in scalers:
            res = re.search(r'm_ReferenceResolution: \{x: ([0-9.]+), y: ([0-9.]+)\}', s)
            match = re.search(r'm_MatchWidthOrHeight: ([0-9.]+)', s)
            mode = re.search(r'm_UiScaleMode: ([0-9]+)', s)
            mode_str = "ConstantPixelSize" if (mode and mode.group(1) == '0') else "ScaleWithScreenSize" if (mode and mode.group(1) == '1') else "ConstantPhysicalSize"
            ref_res = f"{res.group(1)}x{res.group(2)}" if res else "None"
            m_val = match.group(1) if match else "None"
            print(f"  CanvasScaler: Mode={mode_str}, Ref={ref_res}, Match={m_val}")

    # Check Safe Area Handlers
    safe_areas = re.findall(r'UISafeAreaHandler', content)
    print(f"  UISafeAreaHandler components: {len(safe_areas)}")

    # Check GameObjects with 'UI', 'HUD', 'Panel', 'Popup', 'Button', 'Menu'
    go_names = re.findall(r'm_Name: ([^\n]+)', content)
    ui_nodes = [name for name in go_names if any(k in name.lower() for k in ['canvas', 'panel', 'popup', 'hud', 'menu', 'button', 'header', 'bar', 'dialog', 'shop', 'pause', 'complete', 'failed'])]
    print(f"  Total UI-related nodes in hierarchy: {len(ui_nodes)}")
    print("  Key UI nodes:", ui_nodes[:15])

