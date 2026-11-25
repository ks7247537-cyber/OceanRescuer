import re

with open('Assets/Scenes/LevelSelectionScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Map GameObjects
gos = {}
for m in re.finditer(r'--- !u!1 &(\d+)\nGameObject:(.*?)(?=\n---|\Z)', text, re.DOTALL):
    gid = m.group(1)
    block = m.group(2)
    name_m = re.search(r'm_Name: (.*)', block)
    name = name_m.group(1).strip() if name_m else "Unnamed"
    comp_ids = re.findall(r'- component: {fileID: (\d+)}', block)
    gos[gid] = {'name': name, 'components': comp_ids}

# Map RectTransforms
rts = {}
for m in re.finditer(r'--- !u!224 &(\d+)\nRectTransform:(.*?)(?=\n---|\Z)', text, re.DOTALL):
    rtid = m.group(1)
    block = m.group(2)
    gid_m = re.search(r'm_GameObject: {fileID: (\d+)}', block)
    gid = gid_m.group(1) if gid_m else None
    parent_m = re.search(r'm_Father: {fileID: (\d+)}', block)
    parent_id = parent_m.group(1) if parent_m else None
    
    amin = re.search(r'm_AnchorMin: {x: ([\d.-]+), y: ([\d.-]+)}', block)
    amax = re.search(r'm_AnchorMax: {x: ([\d.-]+), y: ([\d.-]+)}', block)
    pos = re.search(r'm_AnchoredPosition: {x: ([\d.-]+), y: ([\d.-]+)}', block)
    size = re.search(r'm_SizeDelta: {x: ([\d.-]+), y: ([\d.-]+)}', block)
    pivot = re.search(r'm_Pivot: {x: ([\d.-]+), y: ([\d.-]+)}', block)
    
    rts[rtid] = {
        'rtid': rtid,
        'gid': gid,
        'parent_rtid': parent_id,
        'name': gos.get(gid, {}).get('name', 'Unknown'),
        'amin': (float(amin.group(1)), float(amin.group(2))) if amin else (0,0),
        'amax': (float(amax.group(1)), float(amax.group(2))) if amax else (0,0),
        'pos': (float(pos.group(1)), float(pos.group(2))) if pos else (0,0),
        'size': (float(size.group(1)), float(size.group(2))) if size else (0,0),
        'pivot': (float(pivot.group(1)), float(pivot.group(2))) if pivot else (0.5, 0.5)
    }

print("=== Canvas and Root Hierarchy in LevelSelectionScene ===")
for rtid, data in rts.items():
    if data['parent_rtid'] == '0' or data['parent_rtid'] is None:
        print(f"Root: {data['name']} (RT &{rtid})")

def print_tree(rtid, depth=0):
    d = rts.get(rtid)
    if not d: return
    indent = "  " * depth
    print(f"{indent}[{d['name']}] (RT &{rtid}) AnchorMin={d['amin']} AnchorMax={d['amax']} Pos={d['pos']} Size={d['size']} Pivot={d['pivot']}")
    # find children
    for child_id, child_data in rts.items():
        if child_data['parent_rtid'] == rtid:
            print_tree(child_id, depth + 1)

for rtid, data in rts.items():
    if 'canvas' in data['name'].lower():
        print(f"\nCanvas Tree for {data['name']}:")
        print_tree(rtid)
