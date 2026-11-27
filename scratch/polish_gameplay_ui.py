import re

scene_path = 'Assets/Scenes/GameplayScene.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Update UndoButton (GO: 94527101, RT: 94527102)
# Reset localScale to 1, 1, 1, set anchor to bottom-left (0, 0), pivot (0, 0), pos (40, 195), size (64, 64)
old_undo_rt = '''224 &94527102
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 94527101}
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 0.3, y: 2.1, z: 0.5}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 700314514}
  m_Father: {fileID: 963312468}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0.02, y: 0.01}
  m_AnchorMax: {x: 0.3, y: 0.09}
  m_AnchoredPosition: {x: -196, y: 52}
  m_SizeDelta: {x: 0, y: 0}
  m_Pivot: {x: 0.5, y: 0.5}'''

new_undo_rt = '''224 &94527102
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 94527101}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 700314514}
  m_Father: {fileID: 963312468}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 0, y: 0}
  m_AnchoredPosition: {x: 40, y: 195}
  m_SizeDelta: {x: 64, y: 64}
  m_Pivot: {x: 0, y: 0}'''

if old_undo_rt in text:
    text = text.replace(old_undo_rt, new_undo_rt, 1)
    print("Updated UndoButton RectTransform.")
else:
    print("Warning: old_undo_rt not matched directly, using regex replacement.")
    text = re.sub(
        r'224 &94527102\r?\nRectTransform:.*?(?=\n--- !u!|\Z)',
        new_undo_rt.replace('\n', '\r\n'),
        text,
        count=1,
        flags=re.DOTALL
    )

# Also fix UndoButton child Label (RT: 700314514) local scale to 1, 1, 1
text = re.sub(
    r'(224 &700314514\r?\nRectTransform:.*?m_LocalScale: ){x: [^,]+, y: [^,]+, z: [^}]+}',
    r'\g<1>{x: 1, y: 1, z: 1}',
    text,
    count=1,
    flags=re.DOTALL
)

# 2. Update RestartButton (GO: 239041177, RT: 239041178)
# Reset localScale to 1, 1, 1, anchor to bottom-center (0.5, 0), pivot (0.5, 0), pos (0, 22), size (190, 58)
text = re.sub(
    r'224 &239041178\r?\nRectTransform:.*?(?=\n--- !u!|\Z)',
    '''224 &239041178\r
RectTransform:\r
  m_ObjectHideFlags: 0\r
  m_CorrespondingSourceObject: {fileID: 0}\r
  m_PrefabInstance: {fileID: 0}\r
  m_PrefabAsset: {fileID: 0}\r
  m_GameObject: {fileID: 239041177}\r
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\r
  m_LocalPosition: {x: 0, y: 0, z: 0}\r
  m_LocalScale: {x: 1, y: 1, z: 1}\r
  m_ConstrainProportionsScale: 0\r
  m_Children:\r
  - {fileID: 464611724}\r
  m_Father: {fileID: 963312468}\r
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\r
  m_AnchorMin: {x: 0.5, y: 0}\r
  m_AnchorMax: {x: 0.5, y: 0}\r
  m_AnchoredPosition: {x: 0, y: 22}\r
  m_SizeDelta: {x: 190, y: 58}\r
  m_Pivot: {x: 0.5, y: 0}''',
    text,
    count=1,
    flags=re.DOTALL
)
print("Updated RestartButton RectTransform.")

# 3. Update DeepOceanSonarHUD (RT: 777500002)
# Set anchor to (0, 0), pos (40, 22), size (270, 74)
text = re.sub(
    r'(224 &777500002\r?\nRectTransform:.*?m_AnchoredPosition: ){x: [^,]+, y: [^}]+}(.*?m_SizeDelta: ){x: [^,]+, y: [^}]+}',
    r'\g<1>{x: 40, y: 22}\g<2>{x: 270, y: 74}',
    text,
    count=1,
    flags=re.DOTALL
)

# Set SonarIcon (Image: 777500043) sprite to sonar_scan_icon.png (guid: 89ad5f02a2ef48b3bfca51e66a3cce53)
text = re.sub(
    r'(114 &777500043\r?\nImage:.*?m_Sprite: ){fileID: [^,]+, guid: [^,]+, type: \d+}',
    r'\g<1>{fileID: 21300000, guid: 89ad5f02a2ef48b3bfca51e66a3cce53, type: 3}',
    text,
    count=1,
    flags=re.DOTALL
)
# If m_Sprite was {fileID: 0}:
text = re.sub(
    r'(114 &777500043\r?\nImage:.*?m_Sprite: ){fileID: 0}',
    r'\g<1>{fileID: 21300000, guid: 89ad5f02a2ef48b3bfca51e66a3cce53, type: 3}',
    text,
    count=1,
    flags=re.DOTALL
)

# Update SonarIcon RectTransform (RT: 777500042) to size (54, 54), pos (36, 0)
text = re.sub(
    r'(224 &777500042\r?\nRectTransform:.*?m_AnchoredPosition: ){x: [^,]+, y: [^}]+}(.*?m_SizeDelta: ){x: [^,]+, y: [^}]+}',
    r'\g<1>{x: 36, y: 0}\g<2>{x: 54, y: 54}',
    text,
    count=1,
    flags=re.DOTALL
)

# Update ButtonLabel (TMP: 777500053) font size and color to bright cyan
text = re.sub(
    r'(114 &777500053\r?\nMonoBehaviour:.*?m_fontSize: )\d+(\.?\d*)',
    r'\g<1>18',
    text,
    count=1,
    flags=re.DOTALL
)

# 4. Update PowerUpDock (RT: 777300002)
# Set anchor to (0, 0), pos (40, 104), size (270, 80)
text = re.sub(
    r'(224 &777300002\r?\nRectTransform:.*?m_AnchoredPosition: ){x: [^,]+, y: [^}]+}(.*?m_SizeDelta: ){x: [^,]+, y: [^}]+}',
    r'\g<1>{x: 40, y: 104}\g<2>{x: 270, y: 80}',
    text,
    count=1,
    flags=re.DOTALL
)
print("Updated PowerUpDock RectTransform.")

# Hide text labels under power up buttons to give big icons room
for name_id in ['777300041', '777300101', '777300161']:
    text = re.sub(
        r'(1 &'+name_id+r'\r?\nGameObject:.*?m_IsActive: )\d+',
        r'\g<1>0',
        text,
        count=1,
        flags=re.DOTALL
    )

# 5. Update DeepOceanPressureHUD (RT: 777400002)
# Set anchor to (1, 0), pos (-40, 104), size (290, 80), pivot (1, 0)
text = re.sub(
    r'(224 &777400002\r?\nRectTransform:.*?m_AnchoredPosition: ){x: [^,]+, y: [^}]+}(.*?m_SizeDelta: ){x: [^,]+, y: [^}]+}',
    r'\g<1>{x: -40, y: 104}\g<2>{x: 290, y: 80}',
    text,
    count=1,
    flags=re.DOTALL
)

# Set GaugeBackground (Image: 777400027) sprite to pressure_gauge_bg.png (guid: 2a255f478e3c4b8faf6fb9952893098b)
text = re.sub(
    r'(114 &777400027\r?\nMonoBehaviour:.*?m_Sprite: ){fileID: 0}',
    r'\g<1>{fileID: 21300000, guid: 2a255f478e3c4b8faf6fb9952893098b, type: 3}',
    text,
    count=1,
    flags=re.DOTALL
)
# If m_Sprite already had something:
text = re.sub(
    r'(114 &777400027\r?\nMonoBehaviour:.*?m_Sprite: ){fileID: [^,]+, guid: [^,]+, type: \d+}',
    r'\g<1>{fileID: 21300000, guid: 2a255f478e3c4b8faf6fb9952893098b, type: 3}',
    text,
    count=1,
    flags=re.DOTALL
)
# Make GaugeBackground color white so sprite renders with true colors
text = re.sub(
    r'(114 &777400027\r?\nMonoBehaviour:.*?m_Color: ){r: [^,]+, g: [^,]+, b: [^,]+, a: [^}]+}',
    r'\g<1>{r: 1, g: 1, b: 1, a: 1}',
    text,
    count=1,
    flags=re.DOTALL
)

# 6. Update CompanionAbilityHUD (RT: 777200014)
# Set anchor to (1, 0), pos (-40, 22), size (290, 74), pivot (1, 0)
text = re.sub(
    r'(224 &777200014\r?\nRectTransform:.*?m_AnchoredPosition: ){x: [^,]+, y: [^}]+}(.*?m_SizeDelta: ){x: [^,]+, y: [^}]+}',
    r'\g<1>{x: -40, y: 22}\g<2>{x: 290, y: 74}',
    text,
    count=1,
    flags=re.DOTALL
)

# Hide overlapping abilityDescriptionText (GO: 777200081)
text = re.sub(
    r'(1 &777200081\r?\nGameObject:.*?m_IsActive: )\d+',
    r'\g<1>0',
    text,
    count=1,
    flags=re.DOTALL
)

# Set iconGlow or companionIconContainer frame to ability_badge_frame.png (guid: 2b44465dc66749d78e944e132778591b)
# Let's check iconGlow Image 777200043
text = re.sub(
    r'(114 &777200043\r?\nMonoBehaviour:.*?m_Sprite: ){fileID: 0}',
    r'\g<1>{fileID: 21300000, guid: 2b44465dc66749d78e944e132778591b, type: 3}',
    text,
    count=1,
    flags=re.DOTALL
)
text = re.sub(
    r'(114 &777200043\r?\nMonoBehaviour:.*?m_Color: ){r: [^,]+, g: [^,]+, b: [^,]+, a: [^}]+}',
    r'\g<1>{r: 1, g: 1, b: 1, a: 1}',
    text,
    count=1,
    flags=re.DOTALL
)

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("GameplayScene.unity UI layout successfully updated and polished!")
