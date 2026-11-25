import re

with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

for match in re.finditer(r'--- !u!1 &(\d+)\nGameObject:.*?\n  m_Name: ([^\n]+)', text):
    goid, name = match.group(1), match.group(2)
    if any(k.lower() in name.lower() for k in ['coin', 'gem', 'topbar', 'win', 'star', 'nextlevel', 'ability']):
        # Find father and recttransform
        mrt = re.search(r'--- !u!224 &(\d+)\nRectTransform:.*?\n  m_GameObject: \{fileID: ' + goid + r'\}(.*?)(?=\n--- !)', text, re.DOTALL)
        pos = ""
        father = ""
        scale = ""
        if mrt:
            fmatch = re.search(r'm_Father: \{fileID: (\d+)\}', mrt.group(0))
            if fmatch: father = fmatch.group(1)
            smatch = re.search(r'm_LocalScale: \{x: ([^,]+), y: ([^,]+), z: ([^\}]+)\}', mrt.group(0))
            if smatch: scale = f"{smatch.group(1)}, {smatch.group(2)}, {smatch.group(3)}"
            apmatch = re.search(r'm_AnchoredPosition: \{x: ([^,]+), y: ([^\}]+)\}', mrt.group(0))
            if apmatch: pos = f"pos=({apmatch.group(1)}, {apmatch.group(2)})"
            amin = re.search(r'm_AnchorMin: \{x: ([^,]+), y: ([^\}]+)\}', mrt.group(0))
            amax = re.search(r'm_AnchorMax: \{x: ([^,]+), y: ([^\}]+)\}', mrt.group(0))
            if amin and amax: pos += f" min=({amin.group(1)},{amin.group(2)}) max=({amax.group(1)},{amax.group(2)})"
        print(f"GO {goid} : '{name}' (Father RT: {father}) scale=[{scale}] {pos}")
