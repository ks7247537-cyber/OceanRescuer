# Injects DeepOceanPressureHUD into GameplayScene.unity cleanly
import sys, os
sys.path.append('scratch')
from build_pressure_hud_prefab import generate_pressure_hud_yaml

scene_path = 'Assets/Scenes/GameplayScene.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add DeepOceanPressureHUD RectTransform to SafeAreaContainer m_Children
safe_area_children_target = '''  m_Children:
  - {fileID: 777200011}
  - {fileID: 777300002}'''

safe_area_children_replacement = '''  m_Children:
  - {fileID: 777200011}
  - {fileID: 777300002}
  - {fileID: 777400002}'''

if safe_area_children_target not in content:
    raise ValueError("Could not find SafeAreaContainer m_Children target in GameplayScene.unity")

content = content.replace(safe_area_children_target, safe_area_children_replacement, 1)

# 2. Add deepOceanPressureHUD field to GameplayHUD
hud_target = '''  powerUpDock: {fileID: 777300005}'''
hud_replacement = '''  powerUpDock: {fileID: 777300005}
  deepOceanPressureHUD: {fileID: 777400005}'''

if hud_target not in content:
    raise ValueError("Could not find powerUpDock in GameplayHUD MonoBehaviour in GameplayScene.unity")

content = content.replace(hud_target, hud_replacement, 1)

# 3. Generate DeepOceanPressureHUD Scene YAML and append to content
pressure_scene_yaml = generate_pressure_hud_yaml(is_scene=True, root_father_id='777200002')

if not content.endswith('\n'):
    content += '\n'

content += pressure_scene_yaml

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("GameplayScene.unity updated with DeepOceanPressureHUD successfully!")
