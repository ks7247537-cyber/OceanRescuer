import re

with open('Assets/Scenes/LevelSelectionScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Exact calculated pos and size
targets = {
    '1087879200': {'lvl': 1, 'pos': (-560.8, 297.5), 'size': (185.5, 245.9)},
    '685915689':  {'lvl': 2, 'pos': (-218.5, 290.5), 'size': (180.5, 237.9)},
    '1913153268': {'lvl': 3, 'pos': (106.3, 286.5),  'size': (178.5, 239.9)},
    '1403391770': {'lvl': 4, 'pos': (624.7, 272.0),  'size': (186.5, 242.9)},
    '593214139':  {'lvl': 5, 'pos': (-619.0, 7.5),   'size': (181.1, 244.9)},
    '967069231':  {'lvl': 6, 'pos': (-280.2, -0.5),  'size': (187.0, 245.4)},
    '2033979885': {'lvl': 7, 'pos': (239.2, -18.5),  'size': (192.7, 242.4)},
    '1894854931': {'lvl': 8, 'pos': (563.0, -23.5),  'size': (191.1, 245.4)},
    '909502708':  {'lvl': 9, 'pos': (-679.6, -306.5), 'size': (182.2, 237.8)},
    '700314514':  {'lvl': 10, 'pos': (-140.8, -324.5), 'size': (181.0, 233.5)},
    '1675537783': {'lvl': 11, 'pos': (181.0, -318.5), 'size': (187.0, 233.5)},
    '1143820066': {'lvl': 12, 'pos': (504.8, -316.5), 'size': (196.6, 233.5)}
}

new_text = text

for rtid, data in targets.items():
    # Find RectTransform block
    pattern = rf'(--- !u!224 &{rtid}\nRectTransform:.*?m_AnchorMin: )\{{x: [\d.-]+, y: [\d.-]+\}}(\n  m_AnchorMax: )\{{x: [\d.-]+, y: [\d.-]+\}}(\n  m_AnchoredPosition: )\{{x: [\d.-]+, y: [\d.-]+\}}(\n  m_SizeDelta: )\{{x: [\d.-]+, y: [\d.-]+\}}'
    
    rep = rf'\g<1>{{x: 0.5, y: 0.5}}\g<2>{{x: 0.5, y: 0.5}}\g<3>{{x: {data["pos"][0]}, y: {data["pos"][1]}}}\g<4>{{x: {data["size"][0]}, y: {data["size"][1]}}}'
    
    new_text, count = re.subn(pattern, rep, new_text, flags=re.DOTALL)
    if count == 1:
        print(f"Level {data['lvl']} (RT &{rtid}): Successfully updated anchors, pos, size.")
    else:
        print(f"ERROR: Level {data['lvl']} (RT &{rtid}) matched {count} times!")

with open('Assets/Scenes/LevelSelectionScene.unity', 'w', encoding='utf-8') as f:
    f.write(new_text)

print("Saved updated LevelSelectionScene.unity successfully.")
