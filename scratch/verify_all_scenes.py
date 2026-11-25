import re, os

def check_scene(path):
    print(f"\n==================== SCENE: {os.path.basename(path)} ====================")
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()

    # 1. Canvas Scaler
    cs_match = re.search(r'm_ReferenceResolution: {x: (\d+), y: (\d+)}', text)
    if cs_match:
        print(f"  [Canvas Scaler] Reference Resolution: {cs_match.group(1)}x{cs_match.group(2)}")
    else:
        print("  [Canvas Scaler] None found")

    # 2. Buttons
    buttons = re.findall(r'm_Name: (.*Button.*?)\n', text)
    print(f"  [Buttons Found] ({len(buttons)}): {', '.join(set(buttons))}")

    # 3. Missing scripts check
    missing = len(re.findall(r'm_Script: {fileID: 0}', text))
    print(f"  [Missing Scripts] Count: {missing}")

    # 4. Check for duplicate IDs
    ids = re.findall(r'--- !u!\d+ &(\d+)', text)
    dups = [x for x in set(ids) if ids.count(x) > 1]
    print(f"  [YAML Duplicate IDs] Count: {len(dups)}")

check_scene('Assets/Scenes/MainMenuScene.unity')
check_scene('Assets/Scenes/LevelSelectionScene.unity')
check_scene('Assets/Scenes/GameplayScene.unity')
check_scene('Assets/Scenes/WorldSelectionScene.unity')
check_scene('Assets/Scenes/SplashScene.unity')
