import re

def verify_scene():
    with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
        lines = f.readlines()

    # Collect all IDs
    declared_ids = set()
    for l in lines:
        if l.startswith('--- !u!'):
            m = re.search(r'&([0-9]+)', l)
            if m:
                declared_ids.add(m.group(1))

    # Check key IDs
    expected_ids = [
        '777200001', '777200002', '777200003', # SafeAreaContainer
        '777200010', '777200011', '777200012', '777200013', '777200014', '777200015', '777200016', # CompanionAbilityHUD
        '777200020', '777200021', '777200022', '777200023', # AbilityPanelBackground
        '777200030', '777200031', '777200032', '777200033', # PanelDecoration
        '777200040', '777200041', # CompanionIconContainer
        '777200050', '777200051', '777200052', '777200053', '777200054', # IconGlow
        '777200060', '777200061', '777200062', '777200063', # CompanionIcon
        '777200070', '777200071', # AbilityInformation
        '777200080', '777200081', '777200082', '777200083', # AbilityNameText
        '777200090', '777200091', '777200092', '777200093', # AbilityDescriptionText
        '777200100', '777200101', '777200102', '777200103', # AbilityStatusText
        '777200110', '777200111', # ChargesContainer
        '777200120', '777200121', '777200122', '777200123', # ChargesBackground
        '777200130', '777200131', '777200132', '777200133', # ChargesIcon
        '777200140', '777200141', '777200142', '777200143', # ChargesCountText
        '777200150', '777200151', '777200152', '777200153', # AbilityAvailabilityIndicator
        '777200160', '777200161', '777200162', '777200163', '777200164', # AbilityPulseEffect
        '777200170', '777200171', '777200172', '777200173', '777200174', # AbilityFeedbackEffect
    ]

    missing = [x for x in expected_ids if x not in declared_ids]
    if missing:
        print("ERROR: Missing IDs in scene:", missing)
        return False
    else:
        print("SUCCESS: All 65 components/objects found in GameplayScene.unity!")

    # Verify HUD_Canvas children
    content = "".join(lines)
    if '- {fileID: 777200002}' in content and '- {fileID: 777100002}' not in content:
        print("SUCCESS: HUD_Canvas correctly links SafeAreaContainer (777200002) and old widget is removed!")
    else:
        print("ERROR: HUD_Canvas link mismatch!")

    if 'creatureAbilityHUD: {fileID: 777200016}' in content:
        print("SUCCESS: GameplayHUD correctly links CreatureAbilityHUD (777200016)!")
    else:
        print("ERROR: GameplayHUD missing creatureAbilityHUD link!")

    return True

verify_scene()
