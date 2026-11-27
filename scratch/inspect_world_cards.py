with open('Assets/Scenes/WorldSelectionScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re

blocks = text.split('--- !u!')
objects = {}

for b in blocks:
    if not b.strip(): continue
    lines = b.splitlines()
    header = lines[0].strip() # e.g. "1 &934252258" or "224 &934252259"
    parts = header.split('&')
    if len(parts) != 2: continue
    type_id = parts[0].strip()
    file_id = parts[1].strip()
    
    obj_type = lines[1].split(':')[0].strip() if len(lines) > 1 else ''
    
    data = {'type_id': type_id, 'file_id': file_id, 'obj_type': obj_type, 'body': b}
    
    name_m = re.search(r'm_Name:\s*([^\n\r]+)', b)
    if name_m: data['name'] = name_m.group(1).strip()
    
    go_m = re.search(r'm_GameObject:\s*{fileID:\s*(\d+)}', b)
    if go_m: data['go'] = go_m.group(1)
    
    act_m = re.search(r'm_IsActive:\s*(\d+)', b)
    if act_m: data['active'] = act_m.group(1)
    
    ch_m = re.search(r'm_Children:\s*\n((?:\s+-\s*{fileID:\s*\d+}\s*\n)*)', b)
    if ch_m:
        data['children'] = re.findall(r'{fileID:\s*(\d+)}', ch_m.group(1))
    else:
        data['children'] = []
        
    objects[file_id] = data

def get_go_of_tf(tf_id):
    if tf_id in objects:
        return objects[tf_id].get('go')
    return None

def get_tf_of_go(go_id):
    for fid, obj in objects.items():
        if obj.get('obj_type') in ['Transform', 'RectTransform'] and obj.get('go') == go_id:
            return fid
    return None

def print_tree(go_id, indent=0):
    go = objects.get(go_id)
    if not go: return
    name = go.get('name', 'Unnamed')
    active = go.get('active', '?')
    print('  '*indent + f'GO: {go_id} | Name: "{name}" | Active: {active}')
    tf_id = get_tf_of_go(go_id)
    if tf_id and tf_id in objects:
        for ch_tf in objects[tf_id].get('children', []):
            ch_go = get_go_of_tf(ch_tf)
            if ch_go:
                print_tree(ch_go, indent + 1)

print('=== WorldCard_0 ===')
print_tree('487027632')
print('\n=== WorldCard_1 ===')
print_tree('934252258')
print('\n=== WorldCard_2 ===')
print_tree('1047165217')
