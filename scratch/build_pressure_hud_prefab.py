# DeepOceanPressureHUD Prefab & Scene Generator
import os

def generate_pressure_hud_yaml(is_scene=False, root_father_id='0', root_pos_x='-30', root_pos_y='106'):
    font_guid = '8f586378b4e144a9851e7b34d9b748ee'
    panel_bg_guid = '536fcf00d0381504f9bb36651f388cd6'
    panel_bg_id = '-3238952746588262579'
    bar_guid = '0b25e985b6ef61f4389bd6a9910732f1'
    bar_id = '-7597184693866071952'
    bubble_guid = '0cb6ba5a8d2265144938c988fab7bac2'
    bubble_id = '2556029130322487988'
    badge_guid = '4fe76dde415a2d04ba4dc186e35b3811'
    badge_id = '-3570497144541376017'
    btn_bg_guid = '55d3222c578b6fd4ea91367c711b307b'
    btn_bg_id = '-5353219563556483298'

    hud_script_guid = 'f2a3b4c5d6e7f8a9b0123456789abcde'
    mgr_script_guid = 'e1f2a3b4c5d6e7f8a9b0123456789abc'
    pulse_script_guid = '6bbc7f42cb97f18428b8b5d89118acba'

    base = 777400000 if is_scene else 3000000000
    def ID(offset):
        return str(base + offset)

    yaml = []
    if not is_scene:
        yaml.append('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:')

    # 1. Root DeepOceanPressureHUD GameObject
    yaml.append(f'''--- !u!1 &{ID(1)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(2)}}}
  - component: {{fileID: {ID(3)}}}
  - component: {{fileID: {ID(4)}}}
  - component: {{fileID: {ID(5)}}}
  - component: {{fileID: {ID(6)}}}
  m_Layer: 5
  m_Name: DeepOceanPressureHUD
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1''')

    # 2. Root RectTransform
    yaml.append(f'''--- !u!224 &{ID(2)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID(12)}}}
  - {{fileID: {ID(22)}}}
  - {{fileID: {ID(52)}}}
  - {{fileID: {ID(62)}}}
  - {{fileID: {ID(72)}}}
  m_Father: {{fileID: {root_father_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0}}
  m_AnchorMax: {{x: 1, y: 0}}
  m_AnchoredPosition: {{x: {root_pos_x}, y: {root_pos_y}}}
  m_SizeDelta: {{x: 270, y: 68}}
  m_Pivot: {{x: 1, y: 0}}''')

    # 3. CanvasRenderer
    yaml.append(f'''--- !u!222 &{ID(3)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_CullTransparentMesh: 1''')

    # 4. CanvasGroup
    yaml.append(f'''--- !u!225 &{ID(4)}
CanvasGroup:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_Enabled: 1
  m_Alpha: 1
  m_Interactable: 1
  m_BlocksRaycasts: 1
  m_IgnoreParentGroups: 0''')

    # 5. DeepOceanPressureHUD Component
    yaml.append(f'''--- !u!114 &{ID(5)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {hud_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressurePanel: {{fileID: {ID(2)}}}
  panelBackground: {{fileID: {ID(14)}}}
  pressureGauge: {{fileID: {ID(22)}}}
  gaugeBackground: {{fileID: {ID(26)}}}
  gaugeFill: {{fileID: {ID(30)}}}
  pressureIndicator: {{fileID: {ID(34)}}}
  pressureValueText: {{fileID: {ID(42)}}}
  pressureStateText: {{fileID: {ID(54)}}}
  pressureWarningIcon: {{fileID: {ID(64)}}}
  pressureFeedbackEffect: {{fileID: {ID(71)}}}
  lowColor: {{r: 0.15, g: 0.75, b: 1, a: 1}}
  optimalColor: {{r: 0.2, g: 1, b: 0.55, a: 1}}
  highColor: {{r: 1, g: 0.7, b: 0.15, a: 1}}
  warningColor: {{r: 1, g: 0.25, b: 0.25, a: 1}}
  smoothSpeed: 5
  gaugeTrackWidth: 140''')

    # 6. DeepOceanPressureManager Component
    yaml.append(f'''--- !u!114 &{ID(6)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {mgr_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  basePressure: 25
  pressurePerPipe: 3.5
  pressurePerBend: 4
  pressurePerObstruction: 15
  maxGaugePressure: 100
  lowThreshold: 35
  optimalThreshold: 75
  warningThreshold: 88''')

    # ─────────────────────────────────────────────────────────────────────────
    # 7. PressurePanel (11-14)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(11)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(12)}}}
  - component: {{fileID: {ID(13)}}}
  - component: {{fileID: {ID(14)}}}
  m_Layer: 5
  m_Name: PressurePanel
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(12)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(11)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(13)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(11)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(14)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(11)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.04, g: 0.14, b: 0.28, a: 0.9}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {panel_bg_id}, guid: {panel_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1''')

    # ─────────────────────────────────────────────────────────────────────────
    # 8. PressureGauge (21-44)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(21)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(22)}}}
  m_Layer: 5
  m_Name: PressureGauge
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(22)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(21)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID(25)}}}
  - {{fileID: {ID(29)}}}
  - {{fileID: {ID(33)}}}
  - {{fileID: {ID(41)}}}
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: -20, y: 8}}
  m_SizeDelta: {{x: 140, y: 20}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!1 &{ID(24)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(25)}}}
  - component: {{fileID: {ID(26)}}}
  - component: {{fileID: {ID(27)}}}
  m_Layer: 5
  m_Name: GaugeBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(25)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(24)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(22)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(26)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(24)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(27)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(24)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.08, g: 0.18, b: 0.3, a: 0.95}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {bar_id}, guid: {bar_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{ID(28)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(29)}}}
  - component: {{fileID: {ID(30)}}}
  - component: {{fileID: {ID(31)}}}
  m_Layer: 5
  m_Name: GaugeFill
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(29)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(28)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(22)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: -4, y: -4}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(30)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(28)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(31)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(28)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.2, g: 1, b: 0.55, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {bar_id}, guid: {bar_guid}, type: 3}}
  m_Type: 3
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 0
  m_FillAmount: 0.45
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{ID(32)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(33)}}}
  - component: {{fileID: {ID(34)}}}
  - component: {{fileID: {ID(35)}}}
  m_Layer: 5
  m_Name: PressureIndicator
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(33)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(32)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(22)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 22, y: 22}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(34)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(32)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(35)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(32)}}}
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
  m_Sprite: {{fileID: {bubble_id}, guid: {bubble_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{ID(40)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(41)}}}
  - component: {{fileID: {ID(42)}}}
  - component: {{fileID: {ID(43)}}}
  m_Layer: 5
  m_Name: PressureValueText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(41)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(40)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(22)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: 42, y: 0}}
  m_SizeDelta: {{x: 60, y: 20}}
  m_Pivot: {{x: 0, y: 0.5}}
--- !u!222 &{ID(42)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(40)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(43)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(40)}}}
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
  m_text: "45 <size=70%>PSI</size>"
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {font_guid}, type: 2}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 12
  m_fontSizeBase: 12
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 9
  m_fontSizeMax: 15
  m_fontStyle: 1
  m_HorizontalAlignment: 1
  m_VerticalAlignment: 512
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 0''')

    # ─────────────────────────────────────────────────────────────────────────
    # 9. PressureStateText (51-54)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(51)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(52)}}}
  - component: {{fileID: {ID(53)}}}
  - component: {{fileID: {ID(54)}}}
  m_Layer: 5
  m_Name: PressureStateText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(52)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(51)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0}}
  m_AnchorMax: {{x: 0.5, y: 0}}
  m_AnchoredPosition: {{x: -20, y: 12}}
  m_SizeDelta: {{x: 180, y: 16}}
  m_Pivot: {{x: 0.5, y: 0}}
--- !u!222 &{ID(53)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(51)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(54)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(51)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.2, g: 1, b: 0.55, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: "<color=#33FF88>PRESSURE STABLE ⚓</color>"
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {font_guid}, type: 2}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 0.2, g: 1, b: 0.55, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 10
  m_fontSizeBase: 10
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 8
  m_fontSizeMax: 14
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 0''')

    # ─────────────────────────────────────────────────────────────────────────
    # 10. PressureWarningIcon (61-65)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(61)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(62)}}}
  - component: {{fileID: {ID(63)}}}
  - component: {{fileID: {ID(64)}}}
  - component: {{fileID: {ID(65)}}}
  m_Layer: 5
  m_Name: PressureWarningIcon
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{ID(62)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(61)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0.5}}
  m_AnchorMax: {{x: 1, y: 0.5}}
  m_AnchoredPosition: {{x: -22, y: 0}}
  m_SizeDelta: {{x: 26, y: 26}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(63)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(61)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(64)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(61)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 0.25, b: 0.25, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {badge_id}, guid: {badge_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{ID(65)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(61)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {pulse_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  minScale: 0.95
  maxScale: 1.15
  pulseSpeed: 3.5
  pulseAlpha: 1
  minAlpha: 0.4
  maxAlpha: 1''')

    # ─────────────────────────────────────────────────────────────────────────
    # 11. PressureFeedbackEffect (71-75)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(71)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(72)}}}
  - component: {{fileID: {ID(73)}}}
  - component: {{fileID: {ID(74)}}}
  - component: {{fileID: {ID(75)}}}
  m_Layer: 5
  m_Name: PressureFeedbackEffect
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{ID(72)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(71)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 6, y: 6}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(73)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(71)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(74)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(71)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 0.3, b: 0.2, a: 0.4}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {panel_bg_id}, guid: {panel_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 0
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{ID(75)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(71)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {pulse_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  minScale: 0.98
  maxScale: 1.05
  pulseSpeed: 3
  pulseAlpha: 1
  minAlpha: 0.2
  maxAlpha: 0.6''')

    return '\n'.join(yaml) + '\n'

if __name__ == '__main__':
    prefab_yaml = generate_pressure_hud_yaml(is_scene=False)
    os.makedirs('Assets/Prefab/UI', exist_ok=True)
    os.makedirs('Assets/Resources/Prefabs/UI', exist_ok=True)

    with open('Assets/Prefab/UI/DeepOceanPressureHUD.prefab', 'w', encoding='utf-8') as f:
        f.write(prefab_yaml)

    with open('Assets/Resources/Prefabs/UI/DeepOceanPressureHUD.prefab', 'w', encoding='utf-8') as f:
        f.write(prefab_yaml)

    print('DeepOceanPressureHUD prefabs created successfully!')
