import os

def generate_sonar_hud_yaml(is_scene=False, root_father_id='0', root_pos_x='232', root_pos_y='35'):
    font_guid = '8f586378b4e144a9851e7b34d9b748ee'
    btn_bg_guid = '55d3222c578b6fd4ea91367c711b307b'
    btn_bg_id = '-5353219563556483298'
    sonar_icon_guid = '89ad5f02a2ef48b3bfca51e66a3cce53'
    sonar_icon_id = '21300000'
    dock_bg_guid = '536fcf00d0381504f9bb36651f388cd6'
    dock_bg_id = '-3238952746588262579'
    bubble_guid = '9c065ebcc4144aa599b25bc5db80eac5'
    bubble_id = '1649192393020575799'
    
    hud_script_guid = '4e9f4aca2d7049a2b83087657bd1c52c'
    pulse_script_guid = 'bba85e35281b430f8de43b840441fc9a'

    base = 777500000 if is_scene else 6000000000

    def ID(offset):
        return str(base + offset)

    yaml = []
    if not is_scene:
        yaml.append('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:')

    # 1. Root DeepOceanSonarHUD GameObject
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
  m_Layer: 5
  m_Name: DeepOceanSonarHUD
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
  - {{fileID: {ID(11)}}}
  - {{fileID: {ID(21)}}}
  - {{fileID: {ID(91)}}}
  m_Father: {{fileID: {root_father_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 0, y: 0}}
  m_AnchoredPosition: {{x: {root_pos_x}, y: {root_pos_y}}}
  m_SizeDelta: {{x: 268, y: 78}}
  m_Pivot: {{x: 0, y: 0}}''')

    # 3. CanvasRenderer
    yaml.append(f'''--- !u!222 &{ID(3)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_CullTransparentMesh: 1''')

    # 4. DeepOceanSonarHUD MonoBehaviour
    yaml.append(f'''--- !u!114 &{ID(4)}
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
  sonarButton: {{fileID: {ID(24)}}}
  buttonBackground: {{fileID: {ID(33)}}}
  sonarIcon: {{fileID: {ID(43)}}}
  buttonLabel: {{fileID: {ID(53)}}}
  cooldownText: {{fileID: {ID(63)}}}
  cooldownFill: {{fileID: {ID(73)}}}
  activationFeedback: {{fileID: {ID(80)}}}
  feedbackText: {{fileID: {ID(83)}}}
  pulseEffect: {{fileID: {ID(94)}}}
  cooldownDuration: 8
  pingDuration: 5
  readyColor: {{r: 0, g: 0.85, b: 1, a: 1}}
  cooldownColor: {{r: 0.35, g: 0.45, b: 0.55, a: 0.7}}
  activeEchoColor: {{r: 0.2, g: 1, b: 0.6, a: 1}}''')

    # 10. SonarPanel (Background Frame)
    yaml.append(f'''--- !u!1 &{ID(10)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(11)}}}
  - component: {{fileID: {ID(12)}}}
  - component: {{fileID: {ID(13)}}}
  m_Layer: 5
  m_Name: SonarPanel
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(11)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(10)}}}
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
--- !u!222 &{ID(12)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(10)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(13)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(10)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.05, g: 0.15, b: 0.25, a: 0.75}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {dock_bg_id}, guid: {dock_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1''')

    # 20. SonarButton
    yaml.append(f'''--- !u!1 &{ID(20)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(21)}}}
  - component: {{fileID: {ID(22)}}}
  - component: {{fileID: {ID(23)}}}
  - component: {{fileID: {ID(24)}}}
  m_Layer: 5
  m_Name: SonarButton
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(21)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(20)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ID(31)}}}
  - {{fileID: {ID(41)}}}
  - {{fileID: {ID(51)}}}
  - {{fileID: {ID(61)}}}
  - {{fileID: {ID(71)}}}
  - {{fileID: {ID(81)}}}
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 252, y: 62}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(22)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(20)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(23)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(20)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 0}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 0}}
  m_Type: 0
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{ID(24)}
Button:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(20)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {ID(33)}}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9, g: 1, b: 1, a: 1}}
    m_PressedColor: {{r: 0.7, g: 0.9, b: 1, a: 1}}
    m_SelectedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.6}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []''')

    # 30. ButtonBackground
    yaml.append(f'''--- !u!1 &{ID(30)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(31)}}}
  - component: {{fileID: {ID(32)}}}
  - component: {{fileID: {ID(33)}}}
  m_Layer: 5
  m_Name: ButtonBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(31)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(30)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(32)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(30)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(33)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(30)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0, g: 0.85, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {btn_bg_id}, guid: {btn_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1''')

    # 40. SonarIcon
    yaml.append(f'''--- !u!1 &{ID(40)}
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
  m_Name: SonarIcon
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
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: 34, y: 0}}
  m_SizeDelta: {{x: 44, y: 44}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(42)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(40)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(43)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(40)}}}
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
  m_Sprite: {{fileID: {sonar_icon_id}, guid: {sonar_icon_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1''')

    # 50. ButtonLabel
    yaml.append(f'''--- !u!1 &{ID(50)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(51)}}}
  - component: {{fileID: {ID(52)}}}
  - component: {{fileID: {ID(53)}}}
  m_Layer: 5
  m_Name: ButtonLabel
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(51)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(50)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 26, y: 6}}
  m_SizeDelta: {{x: 150, y: 24}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(52)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(50)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(53)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(50)}}}
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
  m_text: SONAR SCAN
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
  m_fontSize: 16
  m_fontSizeBase: 16
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 12
  m_fontSizeMax: 20
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

    # 60. CooldownText
    yaml.append(f'''--- !u!1 &{ID(60)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(61)}}}
  - component: {{fileID: {ID(62)}}}
  - component: {{fileID: {ID(63)}}}
  m_Layer: 5
  m_Name: CooldownText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(61)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(60)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 26, y: -14}}
  m_SizeDelta: {{x: 150, y: 18}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(62)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(60)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(63)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(60)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.2, g: 0.9, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: READY
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {font_guid}, type: 2}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 0.2, g: 0.9, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 12
  m_fontSizeBase: 12
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 10
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

    # 70. CooldownFill
    yaml.append(f'''--- !u!1 &{ID(70)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(71)}}}
  - component: {{fileID: {ID(72)}}}
  - component: {{fileID: {ID(73)}}}
  m_Layer: 5
  m_Name: CooldownFill
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{ID(71)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(70)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(72)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(70)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(73)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(70)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.1, g: 0.15, b: 0.22, a: 0.75}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {btn_bg_id}, guid: {btn_bg_guid}, type: 3}}
  m_Type: 3
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 0
  m_FillAmount: 0
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1''')

    # 80. ActivationFeedback
    yaml.append(f'''--- !u!1 &{ID(80)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(81)}}}
  - component: {{fileID: {ID(82)}}}
  - component: {{fileID: {ID(83)}}}
  m_Layer: 5
  m_Name: ActivationFeedback
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{ID(81)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(80)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(21)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 1}}
  m_AnchorMax: {{x: 0.5, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 55}}
  m_SizeDelta: {{x: 320, y: 35}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(82)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(80)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(83)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(80)}}}
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
  m_text: ''
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
  m_fontSize: 14
  m_fontSizeBase: 14
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 11
  m_fontSizeMax: 18
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

    # 90. SonarPulseEffect
    yaml.append(f'''--- !u!1 &{ID(90)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(91)}}}
  - component: {{fileID: {ID(92)}}}
  - component: {{fileID: {ID(93)}}}
  - component: {{fileID: {ID(94)}}}
  m_Layer: 5
  m_Name: SonarPulseEffect
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{ID(91)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(90)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 0.1, y: 0.1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 100, y: 100}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{ID(92)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(90)}}}
  m_CullTransparentMesh: 1
--- !u!114 &{ID(93)}
Image:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(90)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.1, g: 1, b: 1, a: 0}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {sonar_icon_id}, guid: {sonar_icon_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 0
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{ID(94)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(90)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {pulse_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pulseDuration: 1.2
  maxScale: 4.5
  waveColor: {{r: 0.1, g: 1, b: 1, a: 0.75}}
  uiPulseRing: {{fileID: {ID(93)}}}
  worldPulseRing: {{fileID: 0}}''')

    return '\n'.join(yaml) + '\n'

if __name__ == '__main__':
    yaml_content = generate_sonar_hud_yaml(is_scene=False)
    os.makedirs('Assets/Prefab/UI', exist_ok=True)
    os.makedirs('Assets/Resources/Prefabs/UI', exist_ok=True)

    with open('Assets/Prefab/UI/DeepOceanSonarHUD.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    with open('Assets/Resources/Prefabs/UI/DeepOceanSonarHUD.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    print('DeepOceanSonarHUD prefabs generated successfully!')
