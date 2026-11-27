import os
import re

print("=======================================================================")
print("          PHASE 1 & 2: PROJECT STRUCTURE & SCENE INTEGRITY AUDIT       ")
print("=======================================================================")

# 1. Inspect EditorBuildSettings
build_settings_path = "ProjectSettings/EditorBuildSettings.asset"
if os.path.exists(build_settings_path):
    with open(build_settings_path, "r", encoding="utf-8") as f:
        c = f.read()
    scenes_in_build = re.findall(r'path: ([^\n]+)', c)
    enabled_states = re.findall(r'enabled: ([0-9]+)', c)
    print("\n[Build Settings Scenes]")
    for sc, en in zip(scenes_in_build, enabled_states):
        print(f"  - {sc} (enabled: {en})")
else:
    print(f"\n[Build Settings] {build_settings_path} NOT FOUND!")

# 2. Inspect All Assets by Category
categories = {
    'Scenes': [f for f in os.listdir('Assets/Scenes') if f.endswith('.unity')],
    'Scripts': [f for f in os.listdir('Assets/Script') if f.endswith('.cs')],
    'Prefabs': [],
    'Worlds': [f for f in os.listdir('Assets/Resources/Worlds') if f.endswith('.asset')],
    'Audio': [],
    'Sprites_Resources': []
}

for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab'):
            categories['Prefabs'].append(os.path.join(root, f))
        elif f.endswith('.wav') or f.endswith('.mp3') or f.endswith('.ogg'):
            categories['Audio'].append(os.path.join(root, f))

print(f"\n[Asset Category Inventory]")
print(f"  - Scenes: {len(categories['Scenes'])}")
print(f"  - Scripts: {len(categories['Scripts'])}")
print(f"  - Prefabs: {len(categories['Prefabs'])}")
print(f"  - World Configs: {len(categories['Worlds'])}")
print(f"  - Audio Clips: {len(categories['Audio'])}")

# 3. Check for Missing Meta Files
missing_meta = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if not f.endswith('.meta'):
            p = os.path.join(root, f)
            if not os.path.exists(p + '.meta'):
                missing_meta.append(p)

print(f"\n[Meta File Verification]")
print(f"  - Total files missing .meta: {len(missing_meta)}")
if missing_meta:
    for m in missing_meta[:10]:
        print(f"    * Missing meta: {m}")

# 4. Check Scene File Integrity & Missing References (GUIDs in scenes)
guid_pattern = re.compile(r'guid: ([0-9a-f]{32})')
all_project_metas = {}
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.meta'):
            p = os.path.join(root, f)
            with open(p, 'r', encoding='utf-8', errors='ignore') as mf:
                mfc = mf.read()
                m = re.search(r'guid: ([0-9a-f]{32})', mfc)
                if m:
                    all_project_metas[m.group(1)] = p[:-5]

core_scenes = [
    'Assets/Scenes/SplashScene.unity',
    'Assets/Scenes/MainMenuScene.unity',
    'Assets/Scenes/WorldSelectionScene.unity',
    'Assets/Scenes/LevelSelectionScene.unity',
    'Assets/Scenes/GameplayScene.unity'
]

print(f"\n[Core Scene Dependency Audit]")
for sc in core_scenes:
    if not os.path.exists(sc):
        print(f"  - {sc}: MISSING SCENE FILE!")
        continue
    with open(sc, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    scene_guids = set(guid_pattern.findall(content))
    # Exclude default built-in GUIDs (e.g. 0000000000000000e000000000000000 or unity packages)
    unresolved_guids = []
    for g in scene_guids:
        if g not in all_project_metas:
            # Check if default unity builtin
            if not g.startswith('0000000000000000'):
                unresolved_guids.append(g)
    print(f"  - {os.path.basename(sc)}: {len(scene_guids)} referenced GUIDs, Unresolved outside project: {len(unresolved_guids)}")
    if unresolved_guids:
        for ug in unresolved_guids[:5]:
            print(f"      * Note unresolved GUID: {ug}")

