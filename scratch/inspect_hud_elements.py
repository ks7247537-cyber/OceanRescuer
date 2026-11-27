with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re

blocks = text.split('--- !u!')
target_names = ['UndoButton', 'PowerUpDock', 'DeepOceanSonarHUD', 'DeepOceanPressureHUD', 'CreatureAbilityHUD', 'RestartButton']

for b in blocks:
    for name in target_names:
        if f'm_Name: {name}\r\n' in b or f'm_Name: {name}\n' in b:
            header = b.strip().splitlines()[0]
            go_id = header.split('&')[-1].strip()
            print(f"================ {name} (GO: {go_id}) ================")
            # find RectTransform of this GO
            for b2 in blocks:
                if f'm_GameObject: {{fileID: {go_id}}}' in b2 and ('RectTransform:' in b2 or 'Transform:' in b2):
                    lines = b2.strip().splitlines()
                    for l in lines[:25]:
                        if any(k in l for k in ['AnchorMin', 'AnchorMax', 'AnchoredPosition', 'SizeDelta', 'Pivot', 'Father', 'LocalScale']):
                            print('  ' + l.strip())
            break
