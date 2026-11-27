# PowerUpDock Prefab & Scene Generator
import os

def generate_powerup_dock_yaml(is_scene=False, root_father_id='0', root_pos_x='35', root_pos_y='115'):
    font_guid = '8f586378b4e144a9851e7b34d9b748ee'
    hint_sprite_guid = '6554b07698d451644b20fee83de9ee36'
    hint_sprite_id = '-367820732411874062'
    wrench_sprite_guid = 'f4907d97b2936cc4595a003eff63c03f'
    wrench_sprite_id = '-4381854829442937179'
    moves_sprite_guid = '743cda273cb289d4490b5c9f31259dc8'
    moves_sprite_id = '-8986431420854970528'
    badge_sprite_guid = '4fe76dde415a2d04ba4dc186e35b3811'
    badge_sprite_id = '-3570497144541376017'
    btn_bg_guid = '55d3222c578b6fd4ea91367c711b307b'
    btn_bg_id = '-5353219563556483298'
    dock_bg_guid = '536fcf00d0381504f9bb36651f388cd6'
    dock_bg_id = '-3238952746588262579'
    
    # ID Offset
    # For scene: 777300000 series
    # For prefab: 2000000000 series
    base = 777300000 if is_scene else 2000000000

    def ID(offset):
        return str(base + offset)

    yaml = []
    if not is_scene:
        yaml.append('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:')

    # 1. Root PowerUpDock GameObject
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
  m_Layer: 5
  m_Name: PowerUpDock
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
  - {{fileID: {ID(82)}}}
  - {{fileID: {ID(142)}}}
  - {{fileID: {ID(202)}}}
  m_Father: {{fileID: {root_father_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 0, y: 0}}
  m_AnchoredPosition: {{x: {root_pos_x}, y: {root_pos_y}}}
  m_SizeDelta: {{x: 268, y: 78}}
  m_Pivot: {{x: 0, y: 0}}''')

    # 3. Root CanvasRenderer
    yaml.append(f'''--- !u!222 &{ID(3)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_CullTransparentMesh: 1''')

    # 4. Root CanvasGroup
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

    # 5. Root PowerUpDockUI
    yaml.append(f'''--- !u!114 &{ID(5)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(1)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: d0c1e2f3a4b5c6d7e8f90123456789ab, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  dockRoot: {{fileID: {ID(2)}}}
  dockBackground: {{fileID: {ID(14)}}}
  hintButton:
    powerUpId: Hint
    button: {{fileID: {ID(26)}}}
    icon: {{fileID: {ID(34)}}}
    nameText: {{fileID: {ID(44)}}}
    countText: {{fileID: {ID(64)}}}
    quantityBadge: {{fileID: {ID(51)}}}
    canvasGroup: {{fileID: {ID(24)}}}
    glowOutline: {{fileID: {ID(74)}}}
  wrenchButton:
    powerUpId: Wrench
    button: {{fileID: {ID(86)}}}
    icon: {{fileID: {ID(94)}}}
    nameText: {{fileID: {ID(104)}}}
    countText: {{fileID: {ID(124)}}}
    quantityBadge: {{fileID: {ID(111)}}}
    canvasGroup: {{fileID: {ID(84)}}}
    glowOutline: {{fileID: {ID(134)}}}
  extraMovesButton:
    powerUpId: ExtraMoves
    button: {{fileID: {ID(146)}}}
    icon: {{fileID: {ID(154)}}}
    nameText: {{fileID: {ID(164)}}}
    countText: {{fileID: {ID(184)}}}
    quantityBadge: {{fileID: {ID(171)}}}
    canvasGroup: {{fileID: {ID(144)}}}
    glowOutline: {{fileID: {ID(194)}}}
  feedbackToastText: {{fileID: {ID(205)}}}
  toastCanvasGroup: {{fileID: {ID(204)}}}
  debounceCooldown: 0.45
  disabledAlpha: 0.45
  enabledAlpha: 1
  badgeNormalColor: {{r: 0.2, g: 0.7, b: 1, a: 1}}
  badgeZeroColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.8}}''')

    # ─────────────────────────────────────────────────────────────────────────
    # DockBackground (11-14)
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
  m_Name: DockBackground
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
  m_Color: {{r: 0.04, g: 0.16, b: 0.32, a: 0.88}}
  m_RaycastTarget: 1
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

    # ─────────────────────────────────────────────────────────────────────────
    # Helper to generate Button sub-hierarchy (7 nodes per button)
    # ─────────────────────────────────────────────────────────────────────────
    def generate_button(b_offset, b_name, b_x, icon_guid, icon_id, title_str, default_count='0'):
        b_go = ID(b_offset + 1)
        b_rt = ID(b_offset + 2)
        b_cr = ID(b_offset + 3)
        b_cg = ID(b_offset + 4)
        b_im = ID(b_offset + 5)
        b_bt = ID(b_offset + 6)
        b_fb = ID(b_offset + 7)

        i_go = ID(b_offset + 11)
        i_rt = ID(b_offset + 12)
        i_cr = ID(b_offset + 13)
        i_im = ID(b_offset + 14)

        n_go = ID(b_offset + 21)
        n_rt = ID(b_offset + 22)
        n_cr = ID(b_offset + 23)
        n_tx = ID(b_offset + 24)

        q_go = ID(b_offset + 31)
        q_rt = ID(b_offset + 32)
        q_cr = ID(b_offset + 33)
        q_im = ID(b_offset + 34)

        c_go = ID(b_offset + 41)
        c_rt = ID(b_offset + 42)
        c_cr = ID(b_offset + 43)
        c_tx = ID(b_offset + 44)

        f_go = ID(b_offset + 51)
        f_rt = ID(b_offset + 52)
        f_cr = ID(b_offset + 53)
        f_im = ID(b_offset + 54)
        f_gp = ID(b_offset + 55)

        return f'''--- !u!1 &{b_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {b_rt}}}
  - component: {{fileID: {b_cr}}}
  - component: {{fileID: {b_cg}}}
  - component: {{fileID: {b_im}}}
  - component: {{fileID: {b_bt}}}
  - component: {{fileID: {b_fb}}}
  m_Layer: 5
  m_Name: {b_name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{b_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {i_rt}}}
  - {{fileID: {n_rt}}}
  - {{fileID: {q_rt}}}
  - {{fileID: {f_rt}}}
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0.5}}
  m_AnchorMax: {{x: 0, y: 0.5}}
  m_AnchoredPosition: {{x: {b_x}, y: 0}}
  m_SizeDelta: {{x: 74, y: 68}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{b_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
  m_CullTransparentMesh: 1
--- !u!225 &{b_cg}
CanvasGroup:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
  m_Enabled: 1
  m_Alpha: 1
  m_Interactable: 1
  m_BlocksRaycasts: 1
  m_IgnoreParentGroups: 0
--- !u!114 &{b_im}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
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
  m_Sprite: {{fileID: {btn_bg_id}, guid: {btn_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{b_bt}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
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
    m_HighlightedColor: {{r: 0.92, g: 0.95, b: 1, a: 1}}
    m_PressedColor: {{r: 0.75, g: 0.85, b: 0.95, a: 1}}
    m_SelectedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_DisabledColor: {{r: 0.5, g: 0.5, b: 0.5, a: 0.5}}
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
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {b_im}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
--- !u!114 &{b_fb}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {b_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 9f1d43a12b7e41988efca32104598710, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  animDuration: 0.08
  enableFlash: 1
  flashDuration: 0.12
  playClickSound: 1
  autoUpdateDisabledVisual: 1
  disabledColor: {{r: 0.55, g: 0.55, b: 0.55, a: 0.65}}
--- !u!1 &{i_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i_rt}}}
  - component: {{fileID: {i_cr}}}
  - component: {{fileID: {i_im}}}
  m_Layer: 5
  m_Name: Icon
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {b_rt}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: 0, y: 6}}
  m_SizeDelta: {{x: 42, y: 42}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{i_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{i_im}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i_go}}}
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
  m_Sprite: {{fileID: {icon_id}, guid: {icon_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{n_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {n_rt}}}
  - component: {{fileID: {n_cr}}}
  - component: {{fileID: {n_tx}}}
  m_Layer: 5
  m_Name: NameText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{n_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {n_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {b_rt}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0}}
  m_AnchorMax: {{x: 0.5, y: 0}}
  m_AnchoredPosition: {{x: 0, y: 8}}
  m_SizeDelta: {{x: 72, y: 15}}
  m_Pivot: {{x: 0.5, y: 0}}
--- !u!222 &{n_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {n_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{n_tx}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {n_go}}}
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
  m_text: "{title_str}"
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
  m_enableWordWrapping: 0
--- !u!1 &{q_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {q_rt}}}
  - component: {{fileID: {q_cr}}}
  - component: {{fileID: {q_im}}}
  m_Layer: 5
  m_Name: QuantityBadge
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{q_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {q_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {c_rt}}}
  m_Father: {{fileID: {b_rt}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 1}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: -3, y: -3}}
  m_SizeDelta: {{x: 22, y: 22}}
  m_Pivot: {{x: 1, y: 1}}
--- !u!222 &{q_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {q_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{q_im}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {q_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.2, g: 0.7, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {badge_sprite_id}, guid: {badge_sprite_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!1 &{c_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {c_rt}}}
  - component: {{fileID: {c_cr}}}
  - component: {{fileID: {c_tx}}}
  m_Layer: 5
  m_Name: CountText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{c_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {c_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {q_rt}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{c_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {c_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{c_tx}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {c_go}}}
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
  m_text: "{default_count}"
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
  m_enableWordWrapping: 0
--- !u!1 &{f_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {f_rt}}}
  - component: {{fileID: {f_cr}}}
  - component: {{fileID: {f_im}}}
  - component: {{fileID: {f_gp}}}
  m_Layer: 5
  m_Name: FeedbackEffect
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{f_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {f_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {b_rt}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 4, y: 4}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{f_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {f_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{f_im}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {f_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.3, g: 0.9, b: 1, a: 0.35}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {btn_bg_id}, guid: {btn_bg_guid}, type: 3}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 0
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{f_gp}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {f_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 6bbc7f42cb97f18428b8b5d89118acba, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  minScale: 0.98
  maxScale: 1.04
  pulseSpeed: 2.8
  pulseAlpha: 1
  minAlpha: 0.2
  maxAlpha: 0.5'''.strip()

    # Hint Button (offset 20) at x = 44
    yaml.append(generate_button(20, 'HintButton', 44, hint_sprite_guid, hint_sprite_id, 'HINT'))
    # Wrench Button (offset 80) at x = 134
    yaml.append(generate_button(80, 'WrenchButton', 134, wrench_sprite_guid, wrench_sprite_id, 'WRENCH'))
    # Extra Moves Button (offset 140) at x = 224
    yaml.append(generate_button(140, 'ExtraMovesButton', 224, moves_sprite_guid, moves_sprite_id, '+5 MOVES'))

    # ─────────────────────────────────────────────────────────────────────────
    # FeedbackToast (201-205)
    # ─────────────────────────────────────────────────────────────────────────
    yaml.append(f'''--- !u!1 &{ID(201)}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ID(202)}}}
  - component: {{fileID: {ID(203)}}}
  - component: {{fileID: {ID(204)}}}
  - component: {{fileID: {ID(205)}}}
  m_Layer: 5
  m_Name: FeedbackToast
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &{ID(202)}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(201)}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ID(2)}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 14}}
  m_SizeDelta: {{x: 0, y: 24}}
  m_Pivot: {{x: 0.5, y: 0}}
--- !u!222 &{ID(203)}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(201)}}}
  m_CullTransparentMesh: 1
--- !u!225 &{ID(204)}
CanvasGroup:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(201)}}}
  m_Enabled: 1
  m_Alpha: 0
  m_Interactable: 0
  m_BlocksRaycasts: 0
  m_IgnoreParentGroups: 0
--- !u!114 &{ID(205)}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ID(201)}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.2, g: 1, b: 1, a: 1}}
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
  m_fontColor: {{r: 0.2, g: 1, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 12
  m_fontSizeBase: 12
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 9
  m_fontSizeMax: 15
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

    return '\n'.join(yaml) + '\n'

if __name__ == '__main__':
    prefab_yaml = generate_powerup_dock_yaml(is_scene=False)
    os.makedirs('Assets/Prefab/UI', exist_ok=True)
    os.makedirs('Assets/Resources/Prefabs/UI', exist_ok=True)

    with open('Assets/Prefab/UI/PowerUpDock.prefab', 'w', encoding='utf-8') as f:
        f.write(prefab_yaml)

    with open('Assets/Resources/Prefabs/UI/PowerUpDock.prefab', 'w', encoding='utf-8') as f:
        f.write(prefab_yaml)

    print('PowerUpDock prefabs created successfully!')
