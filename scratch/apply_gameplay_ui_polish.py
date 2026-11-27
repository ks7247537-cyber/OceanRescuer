import re

with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Add ResponsiveGameplayCamera to Main Camera (1369692145)
if 'ResponsiveGameplayCamera' not in text:
    cam_go_target = """--- !u!1 &1369692145
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 1369692147}
  - component: {fileID: 1369692146}
  - component: {fileID: 1369692148}"""

    cam_go_replacement = """--- !u!1 &1369692145
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 1369692147}
  - component: {fileID: 1369692146}
  - component: {fileID: 1369692148}
  - component: {fileID: 1369692149}"""

    responsive_cam_block = """--- !u!114 &1369692149
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1369692145}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: f053986560fea594289e590836cedf79, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  referenceAspect: 1.7777778
  referenceOrthoSize: 3.2
  minHorizontalHalfWidth: 5.68889
"""

    if cam_go_target in text:
        text = text.replace(cam_go_target, cam_go_replacement)
        text += "\n" + responsive_cam_block
        print("ResponsiveGameplayCamera added to Main Camera.")

# 2. Wire Pause Menu buttons in GameplayHUD (779360231)
hud_target = """  pausePanel: {fileID: 1992876803}
  resumeButton: {fileID: 0}
  pauseRestartBtn: {fileID: 0}
  pauseMenuBtn: {fileID: 0}"""

hud_replacement = """  pausePanel: {fileID: 1992876803}
  resumeButton: {fileID: 1984576996}
  pauseRestartBtn: {fileID: 59704717}
  pauseMenuBtn: {fileID: 947624641}"""

if hud_target in text:
    text = text.replace(hud_target, hud_replacement)
    print("Pause buttons wired in GameplayHUD.")

# 3. Add UISafeAreaHandler to TopBar (610871075)
if '775e55954238a694c92c006d4db6f4cc' not in text:
    topbar_go_target = """--- !u!1 &610871075
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 610871076}
  - component: {fileID: 610871078}
  - component: {fileID: 610871077}"""

    topbar_go_replacement = """--- !u!1 &610871075
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 610871076}
  - component: {fileID: 610871078}
  - component: {fileID: 610871077}
  - component: {fileID: 610871079}"""

    safe_area_block = """--- !u!114 &610871079
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 610871075}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 775e55954238a694c92c006d4db6f4cc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  applyLeft: 1
  applyRight: 1
  applyTop: 1
  applyBottom: 0
  simulateInEditor: 0
  simulatedLeftCutout: 80
  simulatedRightCutout: 80
"""

    if topbar_go_target in text:
        text = text.replace(topbar_go_target, topbar_go_replacement)
        text += "\n" + safe_area_block
        print("UISafeAreaHandler added to TopBar.")

with open('Assets/Scenes/GameplayScene.unity', 'w', encoding='utf-8') as f:
    f.write(text)

print("GameplayScene UI updates saved.")
