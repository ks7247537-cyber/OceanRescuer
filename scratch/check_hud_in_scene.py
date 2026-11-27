import re

with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Find GameplayHUD MonoBehaviour block
m = re.search(r'm_Script: \{fileID: 11500000, guid: 02c3ef41870ea454a856a93fffead981, type: 3\}[\s\S]*?(?=\n---|\Z)', text)
if m:
    block = m.group(0)
    for line in block.splitlines()[:45]:
        print(line)
else:
    print("GameplayHUD block not found by GUID, searching by name...")
    # search by text
    idx = text.find("GameplayHUD")
    if idx != -1:
        print("Found text around GameplayHUD:")
        print(text[idx-100:idx+400])
