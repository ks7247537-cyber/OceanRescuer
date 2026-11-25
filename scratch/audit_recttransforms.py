import re

def inspect_scene_elements(scene_name):
    path = f'Assets/Scenes/{scene_name}.unity'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    print(f"==================================================")
    print(f"SCENE: {scene_name}")
    print(f"==================================================")
    
    # Map GameObjects ID -> Name
    gos = {}
    for m in re.finditer(r'--- !u!1 &(\d+)\nGameObject:(.*?)(?=\n---|\Z)', content, re.DOTALL):
        gid = m.group(1)
        block = m.group(2)
        name_m = re.search(r'm_Name: (.*)', block)
        name = name_m.group(1).strip() if name_m else "Unnamed"
        gos[gid] = name

    # Map RectTransforms
    rts = {}
    for m in re.finditer(r'--- !u!224 &(\d+)\nRectTransform:(.*?)(?=\n---|\Z)', content, re.DOTALL):
        rtid = m.group(1)
        block = m.group(2)
        gid_m = re.search(r'm_GameObject: {fileID: (\d+)}', block)
        gid = gid_m.group(1) if gid_m else None
        
        amin = re.search(r'm_AnchorMin: {x: ([\d.-]+), y: ([\d.-]+)}', block)
        amax = re.search(r'm_AnchorMax: {x: ([\d.-]+), y: ([\d.-]+)}', block)
        pos = re.search(r'm_AnchoredPosition: {x: ([\d.-]+), y: ([\d.-]+)}', block)
        size = re.search(r'm_SizeDelta: {x: ([\d.-]+), y: ([\d.-]+)}', block)
        pivot = re.search(r'm_Pivot: {x: ([\d.-]+), y: ([\d.-]+)}', block)
        
        rts[rtid] = {
            'gid': gid,
            'name': gos.get(gid, "Unknown"),
            'amin': (float(amin.group(1)), float(amin.group(2))) if amin else (0,0),
            'amax': (float(amax.group(1)), float(amax.group(2))) if amax else (0,0),
            'pos': (float(pos.group(1)), float(pos.group(2))) if pos else (0,0),
            'size': (float(size.group(1)), float(size.group(2))) if size else (0,0),
            'pivot': (float(pivot.group(1)), float(pivot.group(2))) if pivot else (0.5, 0.5)
        }

    # Print interesting UI elements
    keywords = ['panel', 'hud', 'topbar', 'button', 'coin', 'gem', 'energy', 'star', 'header', 'title', 'level', 'play', 'shop', 'settings', 'pause', 'win', 'fail', 'card', 'bg', 'background']
    for rtid, data in rts.items():
        name_lower = data['name'].lower()
        if any(kw in name_lower for kw in keywords):
            amin, amax = data['amin'], data['amax']
            pos, size = data['pos'], data['size']
            anchor_type = "Point" if amin == amax else ("StretchX" if amin[1] == amax[1] else ("StretchY" if amin[0] == amax[0] else "FullStretch"))
            print(f"  [{data['name']}] (Anchor: {anchor_type} min={amin} max={amax} | Pos={pos} Size={size})")

for s in ['SplashScene', 'MainMenuScene', 'WorldSelectionScene', 'LevelSelectionScene', 'GameplayScene']:
    inspect_scene_elements(s)
