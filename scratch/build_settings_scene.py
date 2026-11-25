import re

SCENE_PATH = "Assets/Scenes/MainMenuScene.unity"

with open(SCENE_PATH, "r", encoding="utf-8") as f:
    scene_text = f.read()

# Sprite GUIDs
GUID_PANEL_BG = "6838991699c64244fb03ce0e2e7b5289" # setting panel.png
FILEID_PANEL_BG = "-7846336255137442416"
GUID_BANNER = "5f1f30bfe5b1ac3409dd1251923b09d5"   # banner_coral_reef.png
FILEID_BANNER = "-1975562576801453055"
GUID_BTN_EXIT = "49372c0887693594d8c1732bfa758465" # button_exit.png
FILEID_BTN_EXIT = "757748561783155231"
GUID_BTN_GREEN = "4c57dc607b7cb2340a6f7d597ab50efa" # button_green_blank.png
FILEID_BTN_GREEN = "-2674774105910015051"
GUID_BTN_ORANGE = "95a258957f0b2d345a6b1218b90719ba" # button_orange_blank.png
FILEID_BTN_ORANGE = "7890239773570327735"
GUID_SOUND_ON = "e3429685d5c2d554bb921b39feb76c53" # icon_sound_on.png
FILEID_SOUND_ON = "21300000"
GUID_SOUND_OFF = "8eca980a4f53b73409b10f486d8081c1" # icon_sound_off.png
FILEID_SOUND_OFF = "21300000"
GUID_TMP_FONT = "8f586378b4e144a9851e7b34d9b748ee"
FILEID_TMP_FONT = "11400000"
FILEID_TMP_MAT = "2180264"
GUID_UI_FEEDBACK = "b4e4ba9c6fca65b4c95d90e964177ee1" # UIButtonFeedback

# Clean up any existing 1920000xxx blocks and old settings children
parts = scene_text.split("--- !u!")
header = parts[0]
filtered = []
old_fids = {
    "2127324656", "2127324657", "2127324658", "2127324659",
    "1964641475", "1964641476", "1964641477", "1964641478", "1964641479",
    "3301001001", "3301001002", "3301001003", "3301001004", "3301001005",
    "3301002001", "3301002002", "3301002003", "3301002004", "3301002005",
}
for p in parts[1:]:
    first_line = p.split('\n', 1)[0]
    fid = first_line.split('&')[1].strip()
    if fid.startswith("19200") or fid in old_fids:
        continue
    filtered.append(p)

scene_text = header + "--- !u!" + "--- !u!".join(filtered)

# Generator with exact ID tracking
id_counter = 1920000000
def next_id():
    global id_counter
    id_counter += 1
    return id_counter

# IDs
ID_BG_GO = next_id()
ID_BG_RT = next_id()
ID_BG_CR = next_id()
ID_BG_IMG = next_id()

ID_FRAME_GO = next_id()
ID_FRAME_RT = next_id()
ID_FRAME_CR = next_id()
ID_FRAME_IMG = next_id()

ID_HEADER_GO = next_id()
ID_HEADER_RT = next_id()
ID_HEADER_BG_GO = next_id()
ID_HEADER_BG_RT = next_id()
ID_HEADER_BG_CR = next_id()
ID_HEADER_BG_IMG = next_id()
ID_HEADER_TXT_GO = next_id()
ID_HEADER_TXT_RT = next_id()
ID_HEADER_TXT_CR = next_id()
ID_HEADER_TXT_TMP = next_id()

ID_CLOSE_GO = next_id()
ID_CLOSE_RT = next_id()
ID_CLOSE_CR = next_id()
ID_CLOSE_IMG = next_id()
ID_CLOSE_BTN = next_id()
ID_CLOSE_FB = next_id()
ID_CLOSE_ICON_GO = next_id()
ID_CLOSE_ICON_RT = next_id()
ID_CLOSE_ICON_CR = next_id()

# Sound Section
ID_SOUND_SEC_GO = next_id()
ID_SOUND_SEC_RT = next_id()

ID_SOUND_ICON_GO = next_id()
ID_SOUND_ICON_RT = next_id()
ID_SOUND_ICON_CR = next_id()
ID_SOUND_ICON_IMG = next_id()

ID_SOUND_TXT_GO = next_id()
ID_SOUND_TXT_RT = next_id()
ID_SOUND_TXT_CR = next_id()
ID_SOUND_TXT_TMP = next_id()

ID_SOUND_ON_GO = next_id()
ID_SOUND_ON_RT = next_id()
ID_SOUND_ON_CR = next_id()
ID_SOUND_ON_IMG = next_id()
ID_SOUND_ON_BTN = next_id()
ID_SOUND_ON_FB = next_id()
ID_SOUND_ON_TXT_GO = next_id()
ID_SOUND_ON_TXT_RT = next_id()
ID_SOUND_ON_TXT_CR = next_id()
ID_SOUND_ON_TXT_TMP = next_id()

ID_SOUND_OFF_GO = next_id()
ID_SOUND_OFF_RT = next_id()
ID_SOUND_OFF_CR = next_id()
ID_SOUND_OFF_IMG = next_id()
ID_SOUND_OFF_BTN = next_id()
ID_SOUND_OFF_FB = next_id()
ID_SOUND_OFF_TXT_GO = next_id()
ID_SOUND_OFF_TXT_RT = next_id()
ID_SOUND_OFF_TXT_CR = next_id()
ID_SOUND_OFF_TXT_TMP = next_id()

# Music Section
ID_MUSIC_SEC_GO = next_id()
ID_MUSIC_SEC_RT = next_id()

ID_MUSIC_ICON_GO = next_id()
ID_MUSIC_ICON_RT = next_id()
ID_MUSIC_ICON_CR = next_id()
ID_MUSIC_ICON_IMG = next_id()

ID_MUSIC_TXT_GO = next_id()
ID_MUSIC_TXT_RT = next_id()
ID_MUSIC_TXT_CR = next_id()
ID_MUSIC_TXT_TMP = next_id()

ID_MUSIC_ON_GO = next_id()
ID_MUSIC_ON_RT = next_id()
ID_MUSIC_ON_CR = next_id()
ID_MUSIC_ON_IMG = next_id()
ID_MUSIC_ON_BTN = next_id()
ID_MUSIC_ON_FB = next_id()
ID_MUSIC_ON_TXT_GO = next_id()
ID_MUSIC_ON_TXT_RT = next_id()
ID_MUSIC_ON_TXT_CR = next_id()
ID_MUSIC_ON_TXT_TMP = next_id()

ID_MUSIC_OFF_GO = next_id()
ID_MUSIC_OFF_RT = next_id()
ID_MUSIC_OFF_CR = next_id()
ID_MUSIC_OFF_IMG = next_id()
ID_MUSIC_OFF_BTN = next_id()
ID_MUSIC_OFF_FB = next_id()
ID_MUSIC_OFF_TXT_GO = next_id()
ID_MUSIC_OFF_TXT_RT = next_id()
ID_MUSIC_OFF_TXT_CR = next_id()
ID_MUSIC_OFF_TXT_TMP = next_id()

# Decorations
ID_DECOR_GO = next_id()
ID_DECOR_RT = next_id()

# Update SettingsPanel RectTransform (1323268635)
settings_rt_replacement = f"""--- !u!224 &1323268635
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1323268634}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_BG_RT}}}
  - {{fileID: {ID_FRAME_RT}}}
  m_Father: {{fileID: 2058329671}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}"""

scene_text = re.sub(r'--- !u!224 &1323268635\b.*?(?=--- !u|\Z)', settings_rt_replacement + "\n", scene_text, flags=re.DOTALL)

# Update SettingsManager component (3301003001)
settings_mgr_replacement = f"""--- !u!114 &3301003001
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1323268634}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: b392f3acf9ec00643bbcec3444924945, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  soundOnButton: {{fileID: {ID_SOUND_ON_BTN}}}
  soundOffButton: {{fileID: {ID_SOUND_OFF_BTN}}}
  musicOnButton: {{fileID: {ID_MUSIC_ON_BTN}}}
  musicOffButton: {{fileID: {ID_MUSIC_OFF_BTN}}}
  closeButton: {{fileID: {ID_CLOSE_BTN}}}
  musicButton: {{fileID: 0}}
  soundButton: {{fileID: 0}}
  musicOnSprite: {{fileID: {FILEID_SOUND_ON}, guid: {GUID_SOUND_ON}, type: 3}}
  musicOffSprite: {{fileID: {FILEID_SOUND_OFF}, guid: {GUID_SOUND_OFF}, type: 3}}
  soundOnSprite: {{fileID: {FILEID_SOUND_ON}, guid: {GUID_SOUND_ON}, type: 3}}
  soundOffSprite: {{fileID: {FILEID_SOUND_OFF}, guid: {GUID_SOUND_OFF}, type: 3}}
  musicText: {{fileID: {ID_MUSIC_TXT_TMP}}}
  soundText: {{fileID: {ID_SOUND_TXT_TMP}}}
  soundIcon: {{fileID: {ID_SOUND_ICON_IMG}}}
  musicIcon: {{fileID: {ID_MUSIC_ICON_IMG}}}
  soundToggleIndicator: {{fileID: 0}}
  musicToggleIndicator: {{fileID: 0}}
  soundToggleText: {{fileID: 0}}
  musicToggleText: {{fileID: 0}}
  onColor: {{r: 0.18, g: 0.85, b: 0.35, a: 1}}
  offColor: {{r: 0.85, g: 0.22, b: 0.22, a: 1}}
  inactiveDimColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.45}}"""

scene_text = re.sub(r'--- !u!114 &3301003001\b.*?(?=--- !u|\Z)', settings_mgr_replacement + "\n", scene_text, flags=re.DOTALL)

# Build Blocks
blocks = []

# 1. Background
blocks.append(f"""--- !u!1 &{ID_BG_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_BG_RT}}}
  - component: {{fileID: {ID_BG_CR}}}
  - component: {{fileID: {ID_BG_IMG}}}
  m_Layer: 5
  m_Name: Background
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_BG_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_BG_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 1323268635}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_BG_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_BG_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_BG_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_BG_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.01, g: 0.05, b: 0.12, a: 0.8}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 0}}
  m_Type: 0
  m_PreserveAspect: 0""")

# 2. PanelFrame
blocks.append(f"""--- !u!1 &{ID_FRAME_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_FRAME_RT}}}
  - component: {{fileID: {ID_FRAME_CR}}}
  - component: {{fileID: {ID_FRAME_IMG}}}
  m_Layer: 5
  m_Name: PanelFrame
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_FRAME_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_FRAME_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_HEADER_RT}}}
  - {{fileID: {ID_CLOSE_RT}}}
  - {{fileID: {ID_SOUND_SEC_RT}}}
  - {{fileID: {ID_MUSIC_SEC_RT}}}
  - {{fileID: {ID_DECOR_RT}}}
  m_Father: {{fileID: 1323268635}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 750, y: 500}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_FRAME_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_FRAME_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_FRAME_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_FRAME_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_PANEL_BG}, guid: {GUID_PANEL_BG}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0""")

# 3. SettingsHeader
blocks.append(f"""--- !u!1 &{ID_HEADER_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_HEADER_RT}}}
  m_Layer: 5
  m_Name: SettingsHeader
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_HEADER_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_HEADER_BG_RT}}}
  - {{fileID: {ID_HEADER_TXT_RT}}}
  m_Father: {{fileID: {ID_FRAME_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 1}}
  m_AnchorMax: {{x: 0.5, y: 1}}
  m_AnchoredPosition: {{x: 0, y: -20}}
  m_SizeDelta: {{x: 360, y: 90}}
  m_Pivot: {{x: 0.5, y: 0.5}}

# HeaderBackground
--- !u!1 &{ID_HEADER_BG_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_HEADER_BG_RT}}}
  - component: {{fileID: {ID_HEADER_BG_CR}}}
  - component: {{fileID: {ID_HEADER_BG_IMG}}}
  m_Layer: 5
  m_Name: HeaderBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_HEADER_BG_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_BG_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_HEADER_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_HEADER_BG_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_BG_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_HEADER_BG_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_BG_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BANNER}, guid: {GUID_BANNER}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0

# TitleText
--- !u!1 &{ID_HEADER_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_HEADER_TXT_RT}}}
  - component: {{fileID: {ID_HEADER_TXT_CR}}}
  - component: {{fileID: {ID_HEADER_TXT_TMP}}}
  m_Layer: 5
  m_Name: TitleText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_HEADER_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_HEADER_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 5}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_HEADER_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_HEADER_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_HEADER_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 0.95, b: 0.6, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: SETTINGS
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 0.95, b: 0.6, a: 1}}
  m_fontSize: 34
  m_fontSizeBase: 34
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 514""")

# 4. CloseButton
blocks.append(f"""--- !u!1 &{ID_CLOSE_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_CLOSE_RT}}}
  - component: {{fileID: {ID_CLOSE_CR}}}
  - component: {{fileID: {ID_CLOSE_IMG}}}
  - component: {{fileID: {ID_CLOSE_BTN}}}
  - component: {{fileID: {ID_CLOSE_FB}}}
  m_Layer: 5
  m_Name: CloseButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_CLOSE_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_CLOSE_ICON_RT}}}
  m_Father: {{fileID: {ID_FRAME_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 1}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: -25, y: -25}}
  m_SizeDelta: {{x: 65, y: 65}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_CLOSE_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_CLOSE_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BTN_EXIT}, guid: {GUID_BTN_EXIT}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
--- !u!114 &{ID_CLOSE_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 0
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.96, g: 0.96, b: 0.96, a: 1}}
    m_PressedColor: {{r: 0.78, g: 0.78, b: 0.78, a: 1}}
    m_SelectedColor: {{r: 0.96, g: 0.96, b: 0.96, a: 1}}
    m_DisabledColor: {{r: 0.78, g: 0.78, b: 0.78, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID_CLOSE_IMG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: 3301003001}}
        m_TargetAssemblyTypeName: SettingsManager, Assembly-CSharp
        m_MethodName: CloseSettings
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
--- !u!114 &{ID_CLOSE_FB}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_UI_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  duration: 0.1
  enableBlink: 1
  blinkColor: {{r: 1, g: 1, b: 1, a: 1}}
  blinkDuration: 0.12

# CloseIcon
--- !u!1 &{ID_CLOSE_ICON_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_CLOSE_ICON_RT}}}
  - component: {{fileID: {ID_CLOSE_ICON_CR}}}
  m_Layer: 5
  m_Name: CloseIcon
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_CLOSE_ICON_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_ICON_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_CLOSE_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_CLOSE_ICON_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_CLOSE_ICON_GO}}}
  m_CullTransparentMesh: 1""")

# 5. SoundSection
blocks.append(f"""--- !u!1 &{ID_SOUND_SEC_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_SEC_RT}}}
  m_Layer: 5
  m_Name: SoundSection
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_SEC_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_SEC_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_SOUND_ICON_RT}}}
  - {{fileID: {ID_SOUND_TXT_RT}}}
  - {{fileID: {ID_SOUND_ON_RT}}}
  - {{fileID: {ID_SOUND_OFF_RT}}}
  m_Father: {{fileID: {ID_FRAME_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 35}}
  m_SizeDelta: {{x: 580, y: 80}}
  m_Pivot: {{x: 0.5, y: 0.5}}

# SoundIcon
--- !u!1 &{ID_SOUND_ICON_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_ICON_RT}}}
  - component: {{fileID: {ID_SOUND_ICON_CR}}}
  - component: {{fileID: {ID_SOUND_ICON_IMG}}}
  m_Layer: 5
  m_Name: SoundIcon
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_ICON_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ICON_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_SOUND_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: 35, y: 0}}
  m_SizeDelta: {{x: 50, y: 50}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_ICON_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ICON_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_ICON_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ICON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_SOUND_ON}, guid: {GUID_SOUND_ON}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1

# SoundText
--- !u!1 &{ID_SOUND_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_TXT_RT}}}
  - component: {{fileID: {ID_SOUND_TXT_CR}}}
  - component: {{fileID: {ID_SOUND_TXT_TMP}}}
  m_Layer: 5
  m_Name: SoundText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_SOUND_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: 190, y: 0}}
  m_SizeDelta: {{x: 230, y: 60}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: Sound Effects
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 28
  m_fontSizeBase: 28
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 1
  m_VerticalAlignment: 512
  m_textAlignment: 513

# SoundOnButton
--- !u!1 &{ID_SOUND_ON_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_ON_RT}}}
  - component: {{fileID: {ID_SOUND_ON_CR}}}
  - component: {{fileID: {ID_SOUND_ON_IMG}}}
  - component: {{fileID: {ID_SOUND_ON_BTN}}}
  - component: {{fileID: {ID_SOUND_ON_FB}}}
  m_Layer: 5
  m_Name: SoundOnButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_ON_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_SOUND_ON_TXT_RT}}}
  m_Father: {{fileID: {ID_SOUND_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: -145, y: 0}}
  m_SizeDelta: {{x: 90, y: 52}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_ON_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_ON_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BTN_GREEN}, guid: {GUID_BTN_GREEN}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
--- !u!114 &{ID_SOUND_ON_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 0
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_PressedColor: {{r: 0.8, g: 0.8, b: 0.8, a: 1}}
    m_SelectedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID_SOUND_ON_IMG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: 3301003001}}
        m_TargetAssemblyTypeName: SettingsManager, Assembly-CSharp
        m_MethodName: OnSoundOnClicked
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
--- !u!114 &{ID_SOUND_ON_FB}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_UI_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  duration: 0.1
  enableBlink: 1
  blinkColor: {{r: 1, g: 1, b: 1, a: 1}}
  blinkDuration: 0.12

# SoundOnButton Text
--- !u!1 &{ID_SOUND_ON_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_ON_TXT_RT}}}
  - component: {{fileID: {ID_SOUND_ON_TXT_CR}}}
  - component: {{fileID: {ID_SOUND_ON_TXT_TMP}}}
  m_Layer: 5
  m_Name: Text
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_ON_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_SOUND_ON_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_ON_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_ON_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_ON_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: ON
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 24
  m_fontSizeBase: 24
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 514

# SoundOffButton
--- !u!1 &{ID_SOUND_OFF_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_OFF_RT}}}
  - component: {{fileID: {ID_SOUND_OFF_CR}}}
  - component: {{fileID: {ID_SOUND_OFF_IMG}}}
  - component: {{fileID: {ID_SOUND_OFF_BTN}}}
  - component: {{fileID: {ID_SOUND_OFF_FB}}}
  m_Layer: 5
  m_Name: SoundOffButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_OFF_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_SOUND_OFF_TXT_RT}}}
  m_Father: {{fileID: {ID_SOUND_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: -45, y: 0}}
  m_SizeDelta: {{x: 90, y: 52}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_OFF_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_OFF_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BTN_ORANGE}, guid: {GUID_BTN_ORANGE}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
--- !u!114 &{ID_SOUND_OFF_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 0
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_PressedColor: {{r: 0.8, g: 0.8, b: 0.8, a: 1}}
    m_SelectedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID_SOUND_OFF_IMG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: 3301003001}}
        m_TargetAssemblyTypeName: SettingsManager, Assembly-CSharp
        m_MethodName: OnSoundOffClicked
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
--- !u!114 &{ID_SOUND_OFF_FB}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_UI_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  duration: 0.1
  enableBlink: 1
  blinkColor: {{r: 1, g: 1, b: 1, a: 1}}
  blinkDuration: 0.12

# SoundOffButton Text
--- !u!1 &{ID_SOUND_OFF_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_SOUND_OFF_TXT_RT}}}
  - component: {{fileID: {ID_SOUND_OFF_TXT_CR}}}
  - component: {{fileID: {ID_SOUND_OFF_TXT_TMP}}}
  m_Layer: 5
  m_Name: Text
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_SOUND_OFF_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_SOUND_OFF_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_SOUND_OFF_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_SOUND_OFF_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_SOUND_OFF_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: OFF
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 24
  m_fontSizeBase: 24
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 514""")

# 6. MusicSection
blocks.append(f"""--- !u!1 &{ID_MUSIC_SEC_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_SEC_RT}}}
  m_Layer: 5
  m_Name: MusicSection
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_SEC_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_SEC_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_MUSIC_ICON_RT}}}
  - {{fileID: {ID_MUSIC_TXT_RT}}}
  - {{fileID: {ID_MUSIC_ON_RT}}}
  - {{fileID: {ID_MUSIC_OFF_RT}}}
  m_Father: {{fileID: {ID_FRAME_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: -65}}
  m_SizeDelta: {{x: 580, y: 80}}
  m_Pivot: {{x: 0.5, y: 0.5}}

# MusicIcon
--- !u!1 &{ID_MUSIC_ICON_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_ICON_RT}}}
  - component: {{fileID: {ID_MUSIC_ICON_CR}}}
  - component: {{fileID: {ID_MUSIC_ICON_IMG}}}
  m_Layer: 5
  m_Name: MusicIcon
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_ICON_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ICON_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_MUSIC_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: 35, y: 0}}
  m_SizeDelta: {{x: 50, y: 50}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_ICON_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ICON_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_ICON_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ICON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_SOUND_ON}, guid: {GUID_SOUND_ON}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1

# MusicText
--- !u!1 &{ID_MUSIC_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_TXT_RT}}}
  - component: {{fileID: {ID_MUSIC_TXT_CR}}}
  - component: {{fileID: {ID_MUSIC_TXT_TMP}}}
  m_Layer: 5
  m_Name: MusicText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_MUSIC_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: 190, y: 0}}
  m_SizeDelta: {{x: 230, y: 60}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: Music
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 28
  m_fontSizeBase: 28
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 1
  m_VerticalAlignment: 512
  m_textAlignment: 513

# MusicOnButton
--- !u!1 &{ID_MUSIC_ON_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_ON_RT}}}
  - component: {{fileID: {ID_MUSIC_ON_CR}}}
  - component: {{fileID: {ID_MUSIC_ON_IMG}}}
  - component: {{fileID: {ID_MUSIC_ON_BTN}}}
  - component: {{fileID: {ID_MUSIC_ON_FB}}}
  m_Layer: 5
  m_Name: MusicOnButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_ON_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_MUSIC_ON_TXT_RT}}}
  m_Father: {{fileID: {ID_MUSIC_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: -145, y: 0}}
  m_SizeDelta: {{x: 90, y: 52}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_ON_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_ON_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BTN_GREEN}, guid: {GUID_BTN_GREEN}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
--- !u!114 &{ID_MUSIC_ON_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 0
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_PressedColor: {{r: 0.8, g: 0.8, b: 0.8, a: 1}}
    m_SelectedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID_MUSIC_ON_IMG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: 3301003001}}
        m_TargetAssemblyTypeName: SettingsManager, Assembly-CSharp
        m_MethodName: OnMusicOnClicked
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
--- !u!114 &{ID_MUSIC_ON_FB}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_UI_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  duration: 0.1
  enableBlink: 1
  blinkColor: {{r: 1, g: 1, b: 1, a: 1}}
  blinkDuration: 0.12

# MusicOnButton Text
--- !u!1 &{ID_MUSIC_ON_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_ON_TXT_RT}}}
  - component: {{fileID: {ID_MUSIC_ON_TXT_CR}}}
  - component: {{fileID: {ID_MUSIC_ON_TXT_TMP}}}
  m_Layer: 5
  m_Name: Text
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_ON_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_MUSIC_ON_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_ON_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_ON_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_ON_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: ON
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 24
  m_fontSizeBase: 24
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 514

# MusicOffButton
--- !u!1 &{ID_MUSIC_OFF_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_OFF_RT}}}
  - component: {{fileID: {ID_MUSIC_OFF_CR}}}
  - component: {{fileID: {ID_MUSIC_OFF_IMG}}}
  - component: {{fileID: {ID_MUSIC_OFF_BTN}}}
  - component: {{fileID: {ID_MUSIC_OFF_FB}}}
  m_Layer: 5
  m_Name: MusicOffButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_OFF_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID_MUSIC_OFF_TXT_RT}}}
  m_Father: {{fileID: {ID_MUSIC_SEC_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: -45, y: 0}}
  m_SizeDelta: {{x: 90, y: 52}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_OFF_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_OFF_IMG}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {FILEID_BTN_ORANGE}, guid: {GUID_BTN_ORANGE}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 0
--- !u!114 &{ID_MUSIC_OFF_BTN}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 0
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_PressedColor: {{r: 0.8, g: 0.8, b: 0.8, a: 1}}
    m_SelectedColor: {{r: 0.95, g: 0.95, b: 0.95, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID_MUSIC_OFF_IMG}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: 3301003001}}
        m_TargetAssemblyTypeName: SettingsManager, Assembly-CSharp
        m_MethodName: OnMusicOffClicked
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
--- !u!114 &{ID_MUSIC_OFF_FB}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_UI_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  duration: 0.1
  enableBlink: 1
  blinkColor: {{r: 1, g: 1, b: 1, a: 1}}
  blinkDuration: 0.12

# MusicOffButton Text
--- !u!1 &{ID_MUSIC_OFF_TXT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_MUSIC_OFF_TXT_RT}}}
  - component: {{fileID: {ID_MUSIC_OFF_TXT_CR}}}
  - component: {{fileID: {ID_MUSIC_OFF_TXT_TMP}}}
  m_Layer: 5
  m_Name: Text
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_MUSIC_OFF_TXT_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_TXT_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_MUSIC_OFF_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID_MUSIC_OFF_TXT_CR}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_TXT_GO}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID_MUSIC_OFF_TXT_TMP}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_MUSIC_OFF_TXT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: OFF
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: {FILEID_TMP_FONT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_sharedMaterial: {{fileID: {FILEID_TMP_MAT}, guid: {GUID_TMP_FONT}, type: 2}}
  m_fontSharedMaterials: []
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontSize: 24
  m_fontSizeBase: 24
  m_fontWeight: 700
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 514""")

# 7. UnderwaterDecorations
blocks.append(f"""--- !u!1 &{ID_DECOR_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID_DECOR_RT}}}
  m_Layer: 5
  m_Name: UnderwaterDecorations
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID_DECOR_RT}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID_DECOR_GO}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID_FRAME_RT}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}""")

scene_text += "\n" + "\n".join(blocks) + "\n"

with open(SCENE_PATH, "w", encoding="utf-8") as f:
    f.write(scene_text)

print(f"Generated {len(blocks)} blocks successfully! Total next_id={id_counter}")
