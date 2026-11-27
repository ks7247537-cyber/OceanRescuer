import sys, os
sys.path.append('scratch')
from build_sonar_hud_prefab import generate_sonar_hud_yaml

scene_path = 'Assets/Scenes/GameplayScene.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add DeepOceanSonarHUD RectTransform to SafeAreaContainer m_Children
safe_area_children_target = '''  m_Children:
  - {fileID: 777200011}
  - {fileID: 777300002}
  - {fileID: 777400002}'''

safe_area_children_replacement = '''  m_Children:
  - {fileID: 777200011}
  - {fileID: 777300002}
  - {fileID: 777400002}
  - {fileID: 777500002}'''

if safe_area_children_target not in content:
    raise ValueError("Could not find SafeAreaContainer m_Children target in GameplayScene.unity")

content = content.replace(safe_area_children_target, safe_area_children_replacement, 1)

# 2. Add deepOceanSonarHUD field to GameplayHUD
hud_target = '''  deepOceanPressureHUD: {fileID: 777400005}'''
hud_replacement = '''  deepOceanPressureHUD: {fileID: 777400005}
  deepOceanSonarHUD: {fileID: 777500004}'''

if hud_target not in content:
    raise ValueError("Could not find deepOceanPressureHUD in GameplayHUD MonoBehaviour in GameplayScene.unity")

content = content.replace(hud_target, hud_replacement, 1)

# 3. Generate DeepOceanSonarHUD Scene YAML and append to content
sonar_scene_yaml = generate_sonar_hud_yaml(is_scene=True, root_father_id='777200002', root_pos_x='35', root_pos_y='22')

if not content.endswith('\n'):
    content += '\n'

content += sonar_scene_yaml

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("DeepOceanSonarHUD successfully injected into GameplayScene.unity!")
