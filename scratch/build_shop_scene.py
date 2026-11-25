import re

def build_shop():
    with open('Assets/Scenes/MainMenuScene.unity', 'r', encoding='utf-8') as f:
        scene_text = f.read()

    # Step 1: Identify and remove all old descendant blocks of ShopPanel
    old_descendants = {
        1236457279, 1236457280, 1236457281, 1236457282,
        186618666, 186618667, 186618668,
        907781928, 907781929, 907781930, 907781931, 907781932,
        644234792, 644234793, 644234794, 644234795,
        985481078, 985481079, 985481080, 985481081,
        1616272111, 1616272112, 1616272113, 1616272114, 1616272115,
        616786080, 616786081, 616786082, 616786083,
        868451694, 868451695, 868451696, 868451697,
        467756942, 467756943, 467756944, 467756945, 467756946,
        819221464, 819221465, 819221466,
        914838972, 914838973, 914838974, 914838975, 914838976,
        382816960, 382816961, 382816962, 382816963,
        1110203221, 1110203222, 1110203223, 1110203224, 1110203225,
        145253755, 145253756, 145253757, 145253758,
        1561946021, 1561946022, 1561946023, 1561946024, 1561946025,
        1958628721, 1958628722, 1958628723, 1958628724,
        1721361129, 1721361130, 1721361131, 1721361132, 1721361133,
        95842891, 95842892, 95842893, 95842894,
        883326081, 883326082, 883326083, 883326084,
        1799627082, 1799627083,
        364866565, 364866566
    }

    # Split into blocks
    blocks = re.split(r'\n(?=--- !u!)', scene_text)
    header = ""
    kept_blocks = []

    for b in blocks:
        m = re.match(r'--- !u!\d+ &(\d+)', b)
        if m:
            bid = int(m.group(1))
            if bid in old_descendants:
                continue
            kept_blocks.append(b)
        else:
            kept_blocks.append(b)

    print(f"Kept {len(kept_blocks)} blocks (removed {len(blocks) - len(kept_blocks)} old blocks)")

    # ID Allocator starting above 1910000000
    current_id = 1910000000
    def next_id():
        nonlocal current_id
        current_id += 1
        return current_id

    # Sprites definitions
    GUID_IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"
    GUID_BUTTON = "4e29b1a8efbd4b44bb3f3716e73f07ff"
    GUID_TMP = "f4688fdb7df04437aeb418b961361dc5"
    GUID_FEEDBACK = "9f1d43a12b7e41988efca32104598710"
    GUID_SHOP_MGR = "27684ea3fd56221408e62bfae44f4de3"

    SP_PANEL = (-3238952746588262579, "536fcf00d0381504f9bb36651f388cd6")
    SP_BTN_BLUE = (-5353219563556483298, "55d3222c578b6fd4ea91367c711b307b")
    SP_BTN_GREEN = (-2674774105910015051, "4c57dc607b7cb2340a6f7d597ab50efa")
    SP_BTN_PURPLE = (-3506326862288302868, "bba1f194c3dcc7948a0c88d3f93fe272")
    SP_BTN_ORANGE = (7890239773570327735, "95a258957f0b2d345a6b1218b90719ba")
    SP_BTN_CLOSE = (757748561783155231, "49372c0887693594d8c1732bfa758465")
    SP_BANNER = (-1975562576801453055, "5f1f30bfe5b1ac3409dd1251923b09d5")
    SP_COIN = (8139719423079072813, "2e79075d137a53146ba74047b1eb04a4")
    SP_GEM = (6521567286427641033, "2725cc1a28feb1849bc9df088e00e45b")
    SP_BUBBLES = (1649192393020575799, "97931138169cad44cbe47ac141c724ea")
    SP_SHELL = (21300000, "2b5f63507d394e14f8aebf04df5c8261")
    SP_STAR = (-2736927149447006642, "a7198e3788237734e9f46035e1f38fbd")

    SP_WRENCH = (-4381854829442937179, "f4907d97b2936cc4595a003eff63c03f")
    SP_FREEZE = (3971311155782116182, "f62a88886c796844093201ee9647901b")
    SP_MAGNET = (8864161598196001867, "595cc1823e1858d48abda6d85fce56a6")
    SP_HINT = (-367820732411874062, "6554b07698d451644b20fee83de9ee36")
    SP_BUBBLE = (2556029130322487988, "0cb6ba5a8d2265144938c988fab7bac2")
    SP_LIGHTNING = (5549887748833783432, "3dc4c40b1e63f4f48b5993a50d60bdc2")

    SP_CORAL_PINK = (5705340047435019398, "ed1781ba9ac20d9459ed2c89544a09fe")
    SP_CORAL_ORANGE = (4193198244372499103, "5ae220fd7c0efec4baf1054b650a5ea0")
    SP_CORAL_WEED = (-4429272246781240778, "5123c535d7e4bef498c530abfc20031d")

    new_blocks = []

    def make_go(goid, name, components, active=1):
        lines = [
            f"--- !u!1 &{goid}",
            "GameObject:",
            "  m_ObjectHideFlags: 0",
            "  m_CorrespondingSourceObject: {fileID: 0}",
            "  m_PrefabInstance: {fileID: 0}",
            "  m_PrefabAsset: {fileID: 0}",
            "  serializedVersion: 6",
            "  m_Component:"
        ]
        for c in components:
            lines.append(f"  - component: {{fileID: {c}}}")
        lines.extend([
            "  m_Layer: 5",
            f"  m_Name: {name}",
            "  m_TagString: Untagged",
            "  m_Icon: {fileID: 0}",
            "  m_NavMeshLayer: 0",
            "  m_StaticEditorFlags: 0",
            f"  m_IsActive: {active}"
        ])
        return "\n".join(lines)

    def make_rt(rtid, goid, fid, children, pos=(0,0), size=(100,100), amin=(0.5,0.5), amax=(0.5,0.5), scale=(1,1,1)):
        lines = [
            f"--- !u!224 &{rtid}",
            "RectTransform:",
            "  m_ObjectHideFlags: 0",
            "  m_CorrespondingSourceObject: {fileID: 0}",
            "  m_PrefabInstance: {fileID: 0}",
            "  m_PrefabAsset: {fileID: 0}",
            f"  m_GameObject: {{fileID: {goid}}}",
            "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}",
            "  m_LocalPosition: {x: 0, y: 0, z: 0}",
            f"  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}",
            "  m_ConstrainProportionsScale: 0",
            "  m_Children:"
        ]
        for c in children:
            lines.append(f"  - {{fileID: {c}}}")
        lines.extend([
            f"  m_Father: {{fileID: {fid}}}",
            "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}",
            f"  m_AnchorMin: {{x: {amin[0]}, y: {amin[1]}}}",
            f"  m_AnchorMax: {{x: {amax[0]}, y: {amax[1]}}}",
            f"  m_AnchoredPosition: {{x: {pos[0]}, y: {pos[1]}}}",
            f"  m_SizeDelta: {{x: {size[0]}, y: {size[1]}}}",
            "  m_Pivot: {x: 0.5, y: 0.5}"
        ])
        return "\n".join(lines)

    def make_cr(crid, goid):
        return f"""--- !u!222 &{crid}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {goid}}}
  m_CullTransparentMesh: 1"""

    def make_img(imgid, goid, sp, col=(1,1,1,1), itype=1, preserve=0, raycast=1):
        sp_line = f"{{fileID: {sp[0]}, guid: {sp[1]}, type: 3}}" if sp else "{fileID: 0}"
        return f"""--- !u!114 &{imgid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {goid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: {col[0]}, g: {col[1]}, b: {col[2]}, a: {col[3]}}}
  m_RaycastTarget: {raycast}
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {sp_line}
  m_Type: {itype}
  m_PreserveAspect: {preserve}
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1"""

    def make_btn(btnid, goid, target_img_id, target_sm_id, method_name):
        return f"""--- !u!114 &{btnid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {goid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_BUTTON}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.92, g: 0.92, b: 0.92, a: 1}}
    m_PressedColor: {{r: 0.78, g: 0.78, b: 0.78, a: 1}}
    m_SelectedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_DisabledColor: {{r: 0.75, g: 0.75, b: 0.75, a: 0.5}}
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
  m_TargetGraphic: {{fileID: {target_img_id}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: {target_sm_id}}}
        m_TargetAssemblyTypeName: ShopManager, Assembly-CSharp
        m_MethodName: {method_name}
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2"""

    def make_feedback(fbid, goid):
        return f"""--- !u!114 &{fbid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {goid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_FEEDBACK}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pressScale: 0.92
  animDuration: 0.08
  enableFlash: 1
  flashDuration: 0.12
  playClickSound: 1"""

    def make_tmp(tmpid, goid, text, font_size=24, col=(1,1,1,1), align=514, style=1):
        return f"""--- !u!114 &{tmpid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {goid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_TMP}, type: 3}}
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
  m_text: '{text}'
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2}}
  m_sharedMaterial: {{fileID: 2180264, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: {col[0]}, g: {col[1]}, b: {col[2]}, a: {col[3]}}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontColorGradient:
    topLeft: {{r: 1, g: 1, b: 1, a: 1}}
    topRight: {{r: 1, g: 1, b: 1, a: 1}}
    bottomLeft: {{r: 1, g: 1, b: 1, a: 1}}
    bottomRight: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontColorGradientPreset: {{fileID: 0}}
  m_spriteAsset: {{fileID: 0}}
  m_tintAllSprites: 0
  m_StyleSheet: {{fileID: 0}}
  m_TextStyleHashCode: -1183493901
  m_overrideHtmlColors: 0
  m_faceColor:
    serializedVersion: 2
    rgba: 4294967295
  m_fontSize: {font_size}
  m_fontSizeBase: {font_size}
  m_fontWeight: 400
  m_enableAutoSizing: 0
  m_fontSizeMin: 18
  m_fontSizeMax: 72
  m_fontStyle: {style}
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: {align}
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 1
  m_wordWrappingRatios: 0.4
  m_overflowMode: 0
  m_linkedTextComponent: {{fileID: 0}}
  parentLinkedComponent: {{fileID: 0}}
  m_enableKerning: 1
  m_ActiveFontFeatures: 6e72656b
  m_enableExtraPadding: 0
  checkPaddingRequired: 0
  m_isRichText: 1
  m_parseCtrlCharacters: 1
  m_isOrthographic: 1
  m_isCullingEnabled: 0
  m_horizontalMapping: 0
  m_verticalMapping: 0
  m_uvLineOffset: 0
  m_geometrySortingOrder: 0
  m_IsTextObjectScaleStatic: 0
  m_VertexBufferAutoSizeReduction: 0
  m_useMaxVisibleDescender: 1
  m_pageToDisplay: 1
  m_margin: {{x: 0, y: 0, z: 0, w: 0}}
  m_isUsingLegacyAnimationComponent: 0
  m_isVolumetricText: 0
  m_hasFontAssetChanged: 0
  m_baseMaterial: {{fileID: 0}}
  m_maskOffset: {{x: 0, y: 0, z: 0, w: 0}}"""

    SHOP_MGR_ID = next_id()
    SHOP_PANEL_GO = 1053006816
    SHOP_PANEL_RT = 1053006817

    WIN_GO = next_id(); WIN_RT = next_id(); WIN_CR = next_id(); WIN_IMG = next_id()
    INR_GO = next_id(); INR_RT = next_id(); INR_CR = next_id(); INR_IMG = next_id()

    DCP_GO = next_id(); DCP_RT = next_id(); DCP_CR = next_id(); DCP_IMG = next_id()
    DCW_L_GO = next_id(); DCW_L_RT = next_id(); DCW_L_CR = next_id(); DCW_L_IMG = next_id()
    DCO_GO = next_id(); DCO_RT = next_id(); DCO_CR = next_id(); DCO_IMG = next_id()
    DCW_R_GO = next_id(); DCW_R_RT = next_id(); DCW_R_CR = next_id(); DCW_R_IMG = next_id()
    DBL_GO = next_id(); DBL_RT = next_id(); DBL_CR = next_id(); DBL_IMG = next_id()
    DBR_GO = next_id(); DBR_RT = next_id(); DBR_CR = next_id(); DBR_IMG = next_id()

    BAN_GO = next_id(); BAN_RT = next_id(); BAN_CR = next_id(); BAN_IMG = next_id()
    TTL_GO = next_id(); TTL_RT = next_id(); TTL_CR = next_id(); TTL_TMP = next_id()

    CLS_GO = next_id(); CLS_RT = next_id(); CLS_CR = next_id(); CLS_IMG = next_id(); CLS_BTN = next_id(); CLS_FB = next_id()

    BAL_GO = next_id(); BAL_RT = next_id()
    CP_GO = next_id(); CP_RT = next_id(); CP_CR = next_id(); CP_IMG = next_id()
    CI_GO = next_id(); CI_RT = next_id(); CI_CR = next_id(); CI_IMG = next_id()
    CT_GO = next_id(); CT_RT = next_id(); CT_CR = next_id(); CT_TMP = next_id()

    GP_GO = next_id(); GP_RT = next_id(); GP_CR = next_id(); GP_IMG = next_id()
    GI_GO = next_id(); GI_RT = next_id(); GI_CR = next_id(); GI_IMG = next_id()
    GT_GO = next_id(); GT_RT = next_id(); GT_CR = next_id(); GT_TMP = next_id()

    TBAR_GO = next_id(); TBAR_RT = next_id()
    TI_GO = next_id(); TI_RT = next_id(); TI_CR = next_id(); TI_IMG = next_id(); TI_BTN = next_id(); TI_FB = next_id(); TI_L_GO = next_id(); TI_L_RT = next_id(); TI_L_CR = next_id(); TI_L_TMP = next_id()
    TE_GO = next_id(); TE_RT = next_id(); TE_CR = next_id(); TE_IMG = next_id(); TE_BTN = next_id(); TE_FB = next_id(); TE_L_GO = next_id(); TE_L_RT = next_id(); TE_L_CR = next_id(); TE_L_TMP = next_id()
    TR_GO = next_id(); TR_RT = next_id(); TR_CR = next_id(); TR_IMG = next_id(); TR_BTN = next_id(); TR_FB = next_id(); TR_L_GO = next_id(); TR_L_RT = next_id(); TR_L_CR = next_id(); TR_L_TMP = next_id()
    TG_GO = next_id(); TG_RT = next_id(); TG_CR = next_id(); TG_IMG = next_id(); TG_BTN = next_id(); TG_FB = next_id(); TG_L_GO = next_id(); TG_L_RT = next_id(); TG_L_CR = next_id(); TG_L_TMP = next_id()

    CA_GO = next_id(); CA_RT = next_id()
    PI_GO = next_id(); PI_RT = next_id()
    PE_GO = next_id(); PE_RT = next_id()
    PR_GO = next_id(); PR_RT = next_id()
    PG_GO = next_id(); PG_RT = next_id()

    SB_GO = next_id(); SB_RT = next_id(); SB_CR = next_id(); SB_IMG = next_id()
    ST_GO = next_id(); ST_RT = next_id(); ST_CR = next_id(); ST_TMP = next_id()

    # ShopWindow
    new_blocks.append(make_go(WIN_GO, "ShopWindow", [WIN_RT, WIN_CR, WIN_IMG]))
    new_blocks.append(make_rt(WIN_RT, WIN_GO, SHOP_PANEL_RT, [INR_RT, BAN_RT, CLS_RT, BAL_RT, TBAR_RT, CA_RT, SB_RT], pos=(0, -20), size=(1380, 860)))
    new_blocks.append(make_cr(WIN_CR, WIN_GO))
    new_blocks.append(make_img(WIN_IMG, WIN_GO, SP_PANEL, col=(0.08, 0.32, 0.52, 0.98), itype=1))

    # Inner Frame
    inner_decors = [DCP_RT, DCW_L_RT, DCO_RT, DCW_R_RT, DBL_RT, DBR_RT]
    new_blocks.append(make_go(INR_GO, "InnerFrame", [INR_RT, INR_CR, INR_IMG]))
    new_blocks.append(make_rt(INR_RT, INR_GO, WIN_RT, inner_decors, pos=(0, 0), size=(-48, -48), amin=(0,0), amax=(1,1)))
    new_blocks.append(make_cr(INR_CR, INR_GO))
    new_blocks.append(make_img(INR_IMG, INR_GO, SP_BTN_BLUE, col=(0.03, 0.12, 0.24, 0.92), itype=1))

    # Decors
    def add_decor(dgo, drt, dcr, dimg, name, sp, pos, size, amin, amax):
        new_blocks.append(make_go(dgo, name, [drt, dcr, dimg]))
        new_blocks.append(make_rt(drt, dgo, INR_RT, [], pos=pos, size=size, amin=amin, amax=amax))
        new_blocks.append(make_cr(dcr, dgo))
        new_blocks.append(make_img(dimg, dgo, sp, preserve=1, raycast=0))

    add_decor(DCP_GO, DCP_RT, DCP_CR, DCP_IMG, "Decor_CoralPink", SP_CORAL_PINK, (80, 75), (150, 150), (0,0), (0,0))
    add_decor(DCW_L_GO, DCW_L_RT, DCW_L_CR, DCW_L_IMG, "Decor_CoralWeedL", SP_CORAL_WEED, (160, 85), (120, 170), (0,0), (0,0))
    add_decor(DCO_GO, DCO_RT, DCO_CR, DCO_IMG, "Decor_CoralOrange", SP_CORAL_ORANGE, (-80, 75), (150, 150), (1,0), (1,0))
    add_decor(DCW_R_GO, DCW_R_RT, DCW_R_CR, DCW_R_IMG, "Decor_CoralWeedR", SP_CORAL_WEED, (-160, 85), (120, 170), (1,0), (1,0))
    add_decor(DBL_GO, DBL_RT, DBL_CR, DBL_IMG, "Decor_BubblesL", SP_BUBBLES, (35, 30), (100, 140), (0,0.5), (0,0.5))
    add_decor(DBR_GO, DBR_RT, DBR_CR, DBR_IMG, "Decor_BubblesR", SP_BUBBLES, (-35, 60), (100, 140), (1,0.5), (1,0.5))

    # Banner & Title
    new_blocks.append(make_go(BAN_GO, "WoodenHeaderBanner", [BAN_RT, BAN_CR, BAN_IMG]))
    new_blocks.append(make_rt(BAN_RT, BAN_GO, WIN_RT, [TTL_RT], pos=(0, 26), size=(460, 115), amin=(0.5, 1), amax=(0.5, 1)))
    new_blocks.append(make_cr(BAN_CR, BAN_GO))
    new_blocks.append(make_img(BAN_IMG, BAN_GO, SP_BANNER, itype=0, preserve=1))

    new_blocks.append(make_go(TTL_GO, "TitleText", [TTL_RT, TTL_CR, TTL_TMP]))
    new_blocks.append(make_rt(TTL_RT, TTL_GO, BAN_RT, [], pos=(0, 6), size=(0, 0), amin=(0,0), amax=(1,1)))
    new_blocks.append(make_cr(TTL_CR, TTL_GO))
    new_blocks.append(make_tmp(TTL_TMP, TTL_GO, "SHOP", font_size=52, col=(1, 0.94, 0.40, 1), style=1))

    # Close Button
    new_blocks.append(make_go(CLS_GO, "CloseButton", [CLS_RT, CLS_CR, CLS_IMG, CLS_BTN, CLS_FB]))
    new_blocks.append(make_rt(CLS_RT, CLS_GO, WIN_RT, [], pos=(-15, 15), size=(85, 85), amin=(1,1), amax=(1,1)))
    new_blocks.append(make_cr(CLS_CR, CLS_GO))
    new_blocks.append(make_img(CLS_IMG, CLS_GO, SP_BTN_CLOSE, itype=0, preserve=1))
    new_blocks.append(make_btn(CLS_BTN, CLS_GO, CLS_IMG, SHOP_MGR_ID, "CloseShop"))
    new_blocks.append(make_feedback(CLS_FB, CLS_GO))

    # Balance Bar
    new_blocks.append(make_go(BAL_GO, "BalanceBar", [BAL_RT]))
    new_blocks.append(make_rt(BAL_RT, BAL_GO, WIN_RT, [CP_RT, GP_RT], pos=(-130, -42), size=(360, 52), amin=(1,1), amax=(1,1)))

    # Coin Pill
    new_blocks.append(make_go(CP_GO, "CoinPill", [CP_RT, CP_CR, CP_IMG]))
    new_blocks.append(make_rt(CP_RT, CP_GO, BAL_RT, [CI_RT, CT_RT], pos=(-95, 0), size=(165, 48), amin=(0.5, 0.5), amax=(0.5, 0.5)))
    new_blocks.append(make_cr(CP_CR, CP_GO))
    new_blocks.append(make_img(CP_IMG, CP_GO, SP_BTN_BLUE, col=(0.04, 0.20, 0.36, 0.95), itype=1))

    new_blocks.append(make_go(CI_GO, "CoinIcon", [CI_RT, CI_CR, CI_IMG]))
    new_blocks.append(make_rt(CI_RT, CI_GO, CP_RT, [], pos=(26, 0), size=(38, 38), amin=(0, 0.5), amax=(0, 0.5)))
    new_blocks.append(make_cr(CI_CR, CI_GO))
    new_blocks.append(make_img(CI_IMG, CI_GO, SP_COIN, itype=0, preserve=1))

    new_blocks.append(make_go(CT_GO, "CoinText", [CT_RT, CT_CR, CT_TMP]))
    new_blocks.append(make_rt(CT_RT, CT_GO, CP_RT, [], pos=(25, 0), size=(100, 36), amin=(0.5, 0.5), amax=(0.5, 0.5)))
    new_blocks.append(make_cr(CT_CR, CT_GO))
    new_blocks.append(make_tmp(CT_TMP, CT_GO, "0", font_size=22, col=(1, 0.88, 0.25, 1), align=513))

    # Gem Pill
    new_blocks.append(make_go(GP_GO, "GemPill", [GP_RT, GP_CR, GP_IMG]))
    new_blocks.append(make_rt(GP_RT, GP_GO, BAL_RT, [GI_RT, GT_RT], pos=(95, 0), size=(165, 48), amin=(0.5, 0.5), amax=(0.5, 0.5)))
    new_blocks.append(make_cr(GP_CR, GP_GO))
    new_blocks.append(make_img(GP_IMG, GP_GO, SP_BTN_BLUE, col=(0.18, 0.06, 0.32, 0.95), itype=1))

    new_blocks.append(make_go(GI_GO, "GemIcon", [GI_RT, GI_CR, GI_IMG]))
    new_blocks.append(make_rt(GI_RT, GI_GO, GP_RT, [], pos=(26, 0), size=(38, 38), amin=(0, 0.5), amax=(0, 0.5)))
    new_blocks.append(make_cr(GI_CR, GI_GO))
    new_blocks.append(make_img(GI_IMG, GI_GO, SP_GEM, itype=0, preserve=1))

    new_blocks.append(make_go(GT_GO, "GemText", [GT_RT, GT_CR, GT_TMP]))
    new_blocks.append(make_rt(GT_RT, GT_GO, GP_RT, [], pos=(25, 0), size=(100, 36), amin=(0.5, 0.5), amax=(0.5, 0.5)))
    new_blocks.append(make_cr(GT_CR, GT_GO))
    new_blocks.append(make_tmp(GT_TMP, GT_GO, "0", font_size=22, col=(0.95, 0.55, 1, 1), align=513))

    # Category Tabs Bar
    tabs_children = [TI_RT, TE_RT, TR_RT, TG_RT]
    new_blocks.append(make_go(TBAR_GO, "CategoryTabsBar", [TBAR_RT]))
    new_blocks.append(make_rt(TBAR_RT, TBAR_GO, WIN_RT, tabs_children, pos=(0, -118), size=(1180, 62), amin=(0.5, 1), amax=(0.5, 1)))

    def add_tab_btn(bgo, brt, bcr, bimg, bbtn, bfb, lgo, lrt, lcr, ltmp, label, method, pos, active=0):
        new_blocks.append(make_go(bgo, f"Tab_{label}", [brt, bcr, bimg, bbtn, bfb]))
        scale = (1.05, 1.05, 1) if active else (1, 1, 1)
        new_blocks.append(make_rt(brt, bgo, TBAR_RT, [lrt], pos=pos, size=(280, 58), scale=scale))
        new_blocks.append(make_cr(bcr, bgo))
        col = (0.12, 0.70, 0.95, 1) if active else (0.08, 0.28, 0.48, 0.9)
        new_blocks.append(make_img(bimg, bgo, SP_BTN_BLUE, col=col, itype=1))
        new_blocks.append(make_btn(bbtn, bgo, bimg, SHOP_MGR_ID, method))
        new_blocks.append(make_feedback(bfb, bgo))

        new_blocks.append(make_go(lgo, "Label", [lrt, lcr, ltmp]))
        new_blocks.append(make_rt(lrt, lgo, brt, [], pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))
        new_blocks.append(make_cr(lcr, lgo))
        tcol = (1, 1, 1, 1) if active else (0.7, 0.88, 1, 0.85)
        new_blocks.append(make_tmp(ltmp, lgo, label, font_size=24, col=tcol, style=1))

    tabW = 280; tabSp = 16
    sX = -((4 * tabW + 3 * tabSp) / 2) + (tabW / 2)
    add_tab_btn(TI_GO, TI_RT, TI_CR, TI_IMG, TI_BTN, TI_FB, TI_L_GO, TI_L_RT, TI_L_CR, TI_L_TMP, "Items", "OnTabItemsClicked", (sX + 0*(tabW+tabSp), 0), active=1)
    add_tab_btn(TE_GO, TE_RT, TE_CR, TE_IMG, TE_BTN, TE_FB, TE_L_GO, TE_L_RT, TE_L_CR, TE_L_TMP, "Energy", "OnTabEnergyClicked", (sX + 1*(tabW+tabSp), 0), active=0)
    add_tab_btn(TR_GO, TR_RT, TR_CR, TR_IMG, TR_BTN, TR_FB, TR_L_GO, TR_L_RT, TR_L_CR, TR_L_TMP, "Rewards", "OnTabRewardsClicked", (sX + 2*(tabW+tabSp), 0), active=0)
    add_tab_btn(TG_GO, TG_RT, TG_CR, TG_IMG, TG_BTN, TG_FB, TG_L_GO, TG_L_RT, TG_L_CR, TG_L_TMP, "Gems", "OnTabGemsClicked", (sX + 3*(tabW+tabSp), 0), active=0)

    # Content Area
    new_blocks.append(make_go(CA_GO, "ContentArea", [CA_RT]))
    new_blocks.append(make_rt(CA_RT, CA_GO, WIN_RT, [PI_RT, PE_RT, PR_RT, PG_RT], pos=(0, -48), size=(1280, 550)))

    # Item Card Builder
    def build_card(parent_rt, card_name, title, desc, icon_sp, price_str, method_name, pos, size=(380, 240)):
        c_go = next_id(); c_rt = next_id(); c_cr = next_id(); c_img = next_id()
        t_go = next_id(); t_rt = next_id(); t_cr = next_id(); t_tmp = next_id()
        ib_go = next_id(); ib_rt = next_id(); ib_cr = next_id(); ib_img = next_id()
        i_go = next_id(); i_rt = next_id(); i_cr = next_id(); i_img = next_id()
        d_go = next_id(); d_rt = next_id(); d_cr = next_id(); d_tmp = next_id()
        b_go = next_id(); b_rt = next_id(); b_cr = next_id(); b_img = next_id(); b_btn = next_id(); b_fb = next_id()
        bt_go = next_id(); bt_rt = next_id(); bt_cr = next_id(); bt_tmp = next_id()

        card_children = [t_rt, ib_rt, d_rt, b_rt]
        new_blocks.append(make_go(c_go, card_name, [c_rt, c_cr, c_img]))
        new_blocks.append(make_rt(c_rt, c_go, parent_rt, card_children, pos=pos, size=size))
        new_blocks.append(make_cr(c_cr, c_go))
        new_blocks.append(make_img(c_img, c_go, SP_BTN_BLUE, col=(0.06, 0.22, 0.38, 0.95), itype=1))

        # Title
        new_blocks.append(make_go(t_go, "Title", [t_rt, t_cr, t_tmp]))
        new_blocks.append(make_rt(t_rt, t_go, c_rt, [], pos=(0, -22), size=(-20, 32), amin=(0,1), amax=(1,1)))
        new_blocks.append(make_cr(t_cr, t_go))
        new_blocks.append(make_tmp(t_tmp, t_go, title, font_size=22, col=(1, 1, 1, 1), style=1))

        # Icon Badge + Icon
        new_blocks.append(make_go(ib_go, "IconBadge", [ib_rt, ib_cr, ib_img]))
        new_blocks.append(make_rt(ib_rt, ib_go, c_rt, [i_rt], pos=(0, 22), size=(85, 85), amin=(0.5, 0.5), amax=(0.5, 0.5)))
        new_blocks.append(make_cr(ib_cr, ib_go))
        new_blocks.append(make_img(ib_img, ib_go, SP_BTN_PURPLE, col=(0.04, 0.16, 0.30, 0.90), itype=1))

        new_blocks.append(make_go(i_go, "Icon", [i_rt, i_cr, i_img]))
        new_blocks.append(make_rt(i_rt, i_go, ib_rt, [], pos=(0, 0), size=(-16, -16), amin=(0,0), amax=(1,1)))
        new_blocks.append(make_cr(i_cr, i_go))
        new_blocks.append(make_img(i_img, i_go, icon_sp, itype=0, preserve=1))

        # Description
        new_blocks.append(make_go(d_go, "Desc", [d_rt, d_cr, d_tmp]))
        new_blocks.append(make_rt(d_rt, d_go, c_rt, [], pos=(0, -32), size=(-20, 24), amin=(0, 0.5), amax=(1, 0.5)))
        new_blocks.append(make_cr(d_cr, d_go))
        new_blocks.append(make_tmp(d_tmp, d_go, desc, font_size=15, col=(0.68, 0.86, 0.98, 0.9), style=0))

        # Buy Button
        new_blocks.append(make_go(b_go, "BuyButton", [b_rt, b_cr, b_img, b_btn, b_fb]))
        new_blocks.append(make_rt(b_rt, b_go, c_rt, [bt_rt], pos=(0, 32), size=(240, 48), amin=(0.5, 0), amax=(0.5, 0)))
        new_blocks.append(make_cr(b_cr, b_go))
        new_blocks.append(make_img(b_img, b_go, SP_BTN_GREEN, col=(1, 1, 1, 1), itype=1))
        new_blocks.append(make_btn(b_btn, b_go, b_img, SHOP_MGR_ID, method_name))
        new_blocks.append(make_feedback(b_fb, b_go))

        # Buy Button Text
        new_blocks.append(make_go(bt_go, "Text", [bt_rt, bt_cr, bt_tmp]))
        new_blocks.append(make_rt(bt_rt, bt_go, b_rt, [], pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))
        new_blocks.append(make_cr(bt_cr, bt_go))
        new_blocks.append(make_tmp(bt_tmp, bt_go, price_str, font_size=22, col=(1, 1, 1, 1), style=1))

        return c_rt

    # Feature Card Builder (Large)
    def build_feature_card(parent_rt, card_name, title, desc, icon_sp, price_str, method_name, pos, size=(380, 440)):
        c_go = next_id(); c_rt = next_id(); c_cr = next_id(); c_img = next_id()
        t_go = next_id(); t_rt = next_id(); t_cr = next_id(); t_tmp = next_id()
        ib_go = next_id(); ib_rt = next_id(); ib_cr = next_id(); ib_img = next_id()
        i_go = next_id(); i_rt = next_id(); i_cr = next_id(); i_img = next_id()
        d_go = next_id(); d_rt = next_id(); d_cr = next_id(); d_tmp = next_id()
        b_go = next_id(); b_rt = next_id(); b_cr = next_id(); b_img = next_id(); b_btn = next_id(); b_fb = next_id()
        bt_go = next_id(); bt_rt = next_id(); bt_cr = next_id(); bt_tmp = next_id()

        card_children = [t_rt, ib_rt, d_rt, b_rt]
        new_blocks.append(make_go(c_go, card_name, [c_rt, c_cr, c_img]))
        new_blocks.append(make_rt(c_rt, c_go, parent_rt, card_children, pos=pos, size=size))
        new_blocks.append(make_cr(c_cr, c_go))
        new_blocks.append(make_img(c_img, c_go, SP_BTN_BLUE, col=(0.06, 0.22, 0.38, 0.95), itype=1))

        # Title
        new_blocks.append(make_go(t_go, "Title", [t_rt, t_cr, t_tmp]))
        new_blocks.append(make_rt(t_rt, t_go, c_rt, [], pos=(0, -28), size=(-20, 36), amin=(0,1), amax=(1,1)))
        new_blocks.append(make_cr(t_cr, t_go))
        new_blocks.append(make_tmp(t_tmp, t_go, title, font_size=24, col=(1, 1, 1, 1), style=1))

        # Icon Badge
        new_blocks.append(make_go(ib_go, "IconBadge", [ib_rt, ib_cr, ib_img]))
        new_blocks.append(make_rt(ib_rt, ib_go, c_rt, [i_rt], pos=(0, 35), size=(140, 140), amin=(0.5, 0.5), amax=(0.5, 0.5)))
        new_blocks.append(make_cr(ib_cr, ib_go))
        new_blocks.append(make_img(ib_img, ib_go, SP_BTN_PURPLE, col=(0.04, 0.16, 0.30, 0.90), itype=1))

        new_blocks.append(make_go(i_go, "Icon", [i_rt, i_cr, i_img]))
        new_blocks.append(make_rt(i_rt, i_go, ib_rt, [], pos=(0, 0), size=(-24, -24), amin=(0,0), amax=(1,1)))
        new_blocks.append(make_cr(i_cr, i_go))
        new_blocks.append(make_img(i_img, i_go, icon_sp, itype=0, preserve=1))

        # Desc
        new_blocks.append(make_go(d_go, "Desc", [d_rt, d_cr, d_tmp]))
        new_blocks.append(make_rt(d_rt, d_go, c_rt, [], pos=(0, -70), size=(-30, 50), amin=(0, 0.5), amax=(1, 0.5)))
        new_blocks.append(make_cr(d_cr, d_go))
        new_blocks.append(make_tmp(d_tmp, d_go, desc, font_size=17, col=(0.70, 0.88, 1, 0.95), style=0))

        # Buy Button
        new_blocks.append(make_go(b_go, "BuyButton", [b_rt, b_cr, b_img, b_btn, b_fb]))
        new_blocks.append(make_rt(b_rt, b_go, c_rt, [bt_rt], pos=(0, 45), size=(240, 55), amin=(0.5, 0), amax=(0.5, 0)))
        new_blocks.append(make_cr(b_cr, b_go))
        new_blocks.append(make_img(b_img, b_go, SP_BTN_GREEN, col=(1, 1, 1, 1), itype=1))
        new_blocks.append(make_btn(b_btn, b_go, b_img, SHOP_MGR_ID, method_name))
        new_blocks.append(make_feedback(b_fb, b_go))

        new_blocks.append(make_go(bt_go, "Text", [bt_rt, bt_cr, bt_tmp]))
        new_blocks.append(make_rt(bt_rt, bt_go, b_rt, [], pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))
        new_blocks.append(make_cr(bt_cr, bt_go))
        new_blocks.append(make_tmp(bt_tmp, bt_go, price_str, font_size=24, col=(1, 1, 1, 1), style=1))

        return c_rt

    # Populate Content_Items (active)
    cards_items = []
    cards_items.append(build_card(PI_RT, "Card_0", "Coin Magnet", "Pulls nearby coins", SP_MAGNET, "🪙 100", "BuyItems0", (-410, 135)))
    cards_items.append(build_card(PI_RT, "Card_1", "Freeze Timer", "Freezes level timer", SP_FREEZE, "🪙 100", "BuyItems1", (0, 135)))
    cards_items.append(build_card(PI_RT, "Card_2", "Super Wrench", "Auto-solves next pipe", SP_WRENCH, "🪙 150", "BuyItems2", (410, 135)))
    cards_items.append(build_card(PI_RT, "Card_3", "Path Hint", "Shows flow path", SP_HINT, "🪙 50", "BuyItems3", (-410, -125)))
    cards_items.append(build_card(PI_RT, "Card_4", "Bubble Shield", "Shields water pipes", SP_BUBBLE, "🪙 100", "BuyItems4", (0, -125)))
    cards_items.append(build_card(PI_RT, "Card_5", "Zap Blast", "Clears pipe obstacles", SP_LIGHTNING, "🪙 150", "BuyItems5", (410, -125)))

    new_blocks.append(make_go(PI_GO, "Content_Items", [PI_RT], active=1))
    new_blocks.append(make_rt(PI_RT, PI_GO, CA_RT, cards_items, pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))

    # Populate Content_Energy (inactive)
    cards_energy = []
    cards_energy.append(build_feature_card(PE_RT, "Card_0", "5 Energy Refill", "+5 Instant Energy", SP_STAR, "🪙 50", "BuyEnergy0", (-410, 0)))
    cards_energy.append(build_feature_card(PE_RT, "Card_1", "Full Energy Refill", "Refill to 30/30 max", SP_STAR, "💎 10", "BuyEnergy1", (0, 0)))
    cards_energy.append(build_feature_card(PE_RT, "Card_2", "1 Hr Unlimited", "Unlimited play time", SP_LIGHTNING, "💎 20", "BuyEnergy2", (410, 0)))

    new_blocks.append(make_go(PE_GO, "Content_Energy", [PE_RT], active=0))
    new_blocks.append(make_rt(PE_RT, PE_GO, CA_RT, cards_energy, pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))

    # Populate Content_Rewards (inactive)
    cards_rewards = []
    cards_rewards.append(build_feature_card(PR_RT, "Card_0", "Daily Pearl Gift", "+100 Bonus Coins Free!", SP_COIN, "CLAIM", "BuyRewards0", (-410, 0)))
    cards_rewards.append(build_feature_card(PR_RT, "Card_1", "Starter Package", "+10 Gems & 200 Coins", SP_STAR, "CLAIM", "BuyRewards1", (0, 0)))
    cards_rewards.append(build_feature_card(PR_RT, "Card_2", "Explorer Bundle", "+15 Gems + 2x Hints", SP_MAGNET, "🪙 400", "BuyRewards2", (410, 0)))

    new_blocks.append(make_go(PR_GO, "Content_Rewards", [PR_RT], active=0))
    new_blocks.append(make_rt(PR_RT, PR_GO, CA_RT, cards_rewards, pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))

    # Populate Content_Gems (inactive)
    cards_gems = []
    gW = 295; gSp = 20
    gSX = -((4 * gW + 3 * gSp) / 2) + (gW / 2)
    cards_gems.append(build_feature_card(PG_RT, "Card_0", "Small Pouch", "+5 Shiny Gems", SP_GEM, "🪙 100", "BuyGems0", (gSX + 0*(gW+gSp), 0), size=(gW, 440)))
    cards_gems.append(build_feature_card(PG_RT, "Card_1", "Medium Bag", "+15 Shiny Gems", SP_GEM, "🪙 250", "BuyGems1", (gSX + 1*(gW+gSp), 0), size=(gW, 440)))
    cards_gems.append(build_feature_card(PG_RT, "Card_2", "Large Chest", "+35 Shiny Gems", SP_GEM, "🪙 500", "BuyGems2", (gSX + 2*(gW+gSp), 0), size=(gW, 440)))
    cards_gems.append(build_feature_card(PG_RT, "Card_3", "Treasure Vault", "+80 Shiny Gems", SP_GEM, "🪙 1000", "BuyGems3", (gSX + 3*(gW+gSp), 0), size=(gW, 440)))

    new_blocks.append(make_go(PG_GO, "Content_Gems", [PG_RT], active=0))
    new_blocks.append(make_rt(PG_RT, PG_GO, CA_RT, cards_gems, pos=(0, 0), size=(0, 0), amin=(0,0), amax=(1,1)))

    # Status Feedback Banner
    new_blocks.append(make_go(SB_GO, "StatusFeedbackBanner", [SB_RT, SB_CR, SB_IMG], active=0))
    new_blocks.append(make_rt(SB_RT, SB_GO, WIN_RT, [ST_RT], pos=(0, -190), size=(560, 65), amin=(0.5, 1), amax=(0.5, 1)))
    new_blocks.append(make_cr(SB_CR, SB_GO))
    new_blocks.append(make_img(SB_IMG, SB_GO, SP_BTN_GREEN, col=(0.15, 0.85, 0.35, 0.98), itype=1))

    new_blocks.append(make_go(ST_GO, "StatusText", [ST_RT, ST_CR, ST_TMP]))
    new_blocks.append(make_rt(ST_RT, ST_GO, SB_RT, [], pos=(0, 0), size=(-40, 0), amin=(0,0), amax=(1,1)))
    new_blocks.append(make_cr(ST_CR, ST_GO))
    new_blocks.append(make_tmp(ST_TMP, ST_GO, "Purchased!", font_size=24, col=(1, 1, 1, 1), style=1))

    # ShopManager MonoBehaviour Component on ShopPanel
    sm_block = f"""--- !u!114 &{SHOP_MGR_ID}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {SHOP_PANEL_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_SHOP_MGR}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  shopCoinText: {{fileID: {CT_TMP}}}
  shopGemText: {{fileID: {GT_TMP}}}
  tabItems: {{fileID: {TI_BTN}}}
  tabEnergy: {{fileID: {TE_BTN}}}
  tabRewards: {{fileID: {TR_BTN}}}
  tabGems: {{fileID: {TG_BTN}}}
  contentItems: {{fileID: {PI_GO}}}
  contentEnergy: {{fileID: {PE_GO}}}
  contentRewards: {{fileID: {PR_GO}}}
  contentGems: {{fileID: {PG_GO}}}
  closeButton: {{fileID: {CLS_BTN}}}
  statusBanner: {{fileID: {SB_GO}}}
  statusText: {{fileID: {ST_TMP}}}
  statusImage: {{fileID: {SB_IMG}}}
  tabActiveColor: {{r: 0.12, g: 0.70, b: 0.95, a: 1}}
  tabInactiveColor: {{r: 0.08, g: 0.28, b: 0.48, a: 0.9}}
  successColor: {{r: 0.15, g: 0.85, b: 0.35, a: 0.95}}
  failureColor: {{r: 0.90, g: 0.20, b: 0.20, a: 0.95}}"""
    new_blocks.append(sm_block)

    # Update existing ShopPanel (1053006816) blocks in kept_blocks:
    for idx, b in enumerate(kept_blocks):
        if b.startswith("--- !u!1 &1053006816"):
            old_comp = "  - component: {fileID: 1053006818}"
            new_comp = f"  - component: {{fileID: 1053006818}}\n  - component: {{fileID: {SHOP_MGR_ID}}}"
            kept_blocks[idx] = b.replace(old_comp, new_comp)
        elif b.startswith("--- !u!224 &1053006817"):
            new_rt_text = f"""--- !u!224 &1053006817
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1053006816}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {WIN_RT}}}
  m_Father: {{fileID: 2058329671}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}"""
            kept_blocks[idx] = new_rt_text
        elif b.startswith("--- !u!114 &1053006819"):
            new_img_text = f"""--- !u!114 &1053006819
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1053006816}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0.01, g: 0.05, b: 0.12, a: 0.78}}
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
  m_PixelsPerUnitMultiplier: 1"""
            kept_blocks[idx] = new_img_text

    final_text = "\n".join(kept_blocks) + "\n" + "\n".join(new_blocks) + "\n"

    with open('Assets/Scenes/MainMenuScene.unity', 'w', encoding='utf-8') as out_f:
        out_f.write(final_text)

    print(f"Successfully wrote {len(new_blocks)} new blocks into Assets/Scenes/MainMenuScene.unity!")

if __name__ == '__main__':
    build_shop()
