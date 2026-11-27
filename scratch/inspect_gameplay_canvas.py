with open('Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
blocks = text.split('--- !u!')
objects = {}
for b in blocks:
    if not b.strip(): continue
    lines = b.splitlines()
    header = lines[0].strip()
    parts = header.split('&')
    if len(parts) != 2: continue
    fid = parts[1].strip()
    obj_type = lines[1].split(':')[0].strip() if len(lines) > 1 else ''
    name_m = re.search(r'm_Name:\s*([^\n\r]+)', b)
    name = name_m.group(1).strip() if name_m else ''
    go_m = re.search(r'm_GameObject:\s*{fileID:\s*(\d+)}', b)
    go = go_m.group(1) if go_m else ''
    ch_m = re.search(r'm_Children:\s*\n((?:\s+-\s*{fileID:\s*\d+}\s*\n)*)', b)
    children = re.findall(r'{fileID:\s*(\d+)}', ch_m.group(1)) if ch_m else []
    act_m = re.search(r'm_IsActive:\s*(\d+)', b)
    active = act_m.group(1) if act_m else '1'
    objects[fid] = {'type': obj_type, 'name': name, 'go': go, 'children': children, 'active': active}

def get_go_tf(go_id):
    for fid, obj in objects.items():
        if obj['type'] in ['RectTransform', 'Transform'] and obj['go'] == go_id:
            return fid
    return None

def print_tree(go_id, indent=0):
    go_obj = objects.get(go_id)
    if not go_obj: return
    print('  '*indent + f"[{go_obj['name']}] (ID: {go_id}, Active: {go_obj['active']})")
    tf_id = get_go_tf(go_id)
    if tf_id and tf_id in objects:
        for ch_tf in objects[tf_id]['children']:
            ch_obj = objects.get(ch_tf)
            if ch_obj and ch_obj.get('go'):
                print_tree(ch_obj['go'], indent + 1)

for fid, obj in objects.items():
    if obj['type'] == 'GameObject' and obj['name'] == 'Canvas':
        print('=== CANVAS HIERARCHY ===')
        print_tree(fid)
