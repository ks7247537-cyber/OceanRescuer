import os
import uuid

# Openings definition for (pipeType, rotationStep):
# Directions: 0=Up, 1=Right, 2=Down, 3=Left
def get_openings(p_type, rot):
    rot = rot % 4
    if p_type == 0: # Straight
        return {1, 3} if rot % 2 == 0 else {0, 2}
    elif p_type == 1: # Elbow
        mapping = {0: {2, 3}, 1: {0, 3}, 2: {0, 1}, 3: {1, 2}}
        return mapping[rot]
    elif p_type == 2: # TJunction
        mapping = {0: {0, 1, 3}, 1: {0, 1, 2}, 2: {1, 2, 3}, 3: {0, 2, 3}}
        return mapping[rot]
    elif p_type == 3: # Cross
        return {0, 1, 2, 3}
    return set()

OPPOSITE = {0: 2, 1: 3, 2: 0, 3: 1}
DIR_OFFSETS = {0: (0, 1), 1: (1, 0), 2: (0, -1), 3: (-1, 0)}

def verify_path(grid_types, solution_rots, source, sink):
    queue = [source]
    visited = {source}
    parent = {}

    while queue:
        curr = queue.pop(0)
        cx, cy = curr
        if curr == sink:
            path = []
            node = sink
            while node in parent:
                path.append(node)
                node = parent[node]
            path.append(source)
            path.reverse()
            return True, path

        c_idx = cy * 9 + cx
        c_type = grid_types[c_idx]
        c_rot = solution_rots[c_idx]
        c_open = get_openings(c_type, c_rot)

        for d in c_open:
            dx, dy = DIR_OFFSETS[d]
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < 9 and 0 <= ny < 5 and (nx, ny) not in visited:
                n_idx = ny * 9 + nx
                n_type = grid_types[n_idx]
                if n_type != -1:
                    n_rot = solution_rots[n_idx]
                    n_open = get_openings(n_type, n_rot)
                    if OPPOSITE[d] in n_open:
                        visited.add((nx, ny))
                        parent[(nx, ny)] = curr
                        queue.append((nx, ny))

    return False, []

def calc_min_moves(init_rots, sol_rots, path_cells):
    moves = 0
    for cx, cy in path_cells:
        idx = cy * 9 + cx
        diff = (sol_rots[idx] - init_rots[idx]) % 4
        moves += diff
    return moves

def build_level_data(level_num, name, creature, difficulty, path_nodes, valves=None, gates=None):
    grid_types = [-1] * 45
    sol_rots = [0] * 45
    init_rots = [0] * 45

    n = len(path_nodes)
    for i in range(n):
        cx, cy = path_nodes[i]
        c_idx = cy * 9 + cx
        
        needed_dirs = set()
        if i == 0:
            next_x, next_y = path_nodes[i + 1]
            for d, (dx, dy) in DIR_OFFSETS.items():
                if cx + dx == next_x and cy + dy == next_y:
                    needed_dirs.add(d)
            needed_dirs.add(3)
        elif i == n - 1:
            prev_x, prev_y = path_nodes[i - 1]
            for d, (dx, dy) in DIR_OFFSETS.items():
                if cx + dx == prev_x and cy + dy == prev_y:
                    needed_dirs.add(d)
            needed_dirs.add(1)
        else:
            prev_x, prev_y = path_nodes[i - 1]
            next_x, next_y = path_nodes[i + 1]
            for d, (dx, dy) in DIR_OFFSETS.items():
                if (cx + dx == prev_x and cy + dy == prev_y) or (cx + dx == next_x and cy + dy == next_y):
                    needed_dirs.add(d)

        matched = False
        for r in range(4):
            if get_openings(0, r) == needed_dirs:
                grid_types[c_idx] = 0
                sol_rots[c_idx] = r
                matched = True
                break
        if not matched:
            for r in range(4):
                if get_openings(1, r) == needed_dirs:
                    grid_types[c_idx] = 1
                    sol_rots[c_idx] = r
                    matched = True
                    break
        if not matched:
            for r in range(4):
                if needed_dirs.issubset(get_openings(2, r)):
                    grid_types[c_idx] = 2
                    sol_rots[c_idx] = r
                    matched = True
                    break
        if not matched:
            grid_types[c_idx] = 3
            sol_rots[c_idx] = 0

        scramble = ((i * 2 + 1) % 3) + 1
        init_rots[c_idx] = (sol_rots[c_idx] + scramble) % 4

    for y in range(5):
        for x in range(9):
            idx = y * 9 + x
            if grid_types[idx] == -1 and (x + y) % 2 == 0:
                grid_types[idx] = (x + y) % 3
                init_rots[idx] = (x * 3 + y) % 4
                sol_rots[idx] = (y * 2 + x) % 4

    source = path_nodes[0]
    sink = path_nodes[-1]

    solvable, verified_path = verify_path(grid_types, sol_rots, source, sink)
    assert solvable, f"Level {level_num} failed path verification!"

    min_moves = calc_min_moves(init_rots, sol_rots, path_nodes)
    max_moves = min_moves + 8
    target_3_stars = min_moves + 2

    return {
        "level_num": level_num,
        "name": name,
        "creature": creature,
        "difficulty": difficulty,
        "source": source,
        "sink": sink,
        "min_moves": min_moves,
        "max_moves": max_moves,
        "target_3_stars": target_3_stars,
        "grid_types": grid_types,
        "init_rots": init_rots,
        "sol_rots": sol_rots,
        "valves": valves or [],
        "gates": gates or []
    }

levels_data = []

# Level 1: Intro to Deep Ocean (Gentle S-Curve)
p1 = [(0,1), (1,1), (1,2), (2,2), (3,2), (3,1), (4,1), (5,1), (5,2), (6,2), (7,2), (7,1), (8,1)]
levels_data.append(build_level_data(1, "Abyssal Gate", 0, "Medium", p1))

# Level 2: On/Off Valve Intro
p2 = [(0,0), (1,0), (1,1), (2,1), (3,1), (4,1), (4,2), (5,2), (6,2), (6,1), (7,1), (8,1)]
v2 = [{"gridPos": {"x": 4, "y": 1}, "valveType": 0, "isInitiallyOpen": 0, "allowedDirection": 1}]
levels_data.append(build_level_data(2, "Bioluminescent Flow", 0, "Medium", p2, valves=v2))

# Level 3: Directional Valve Intro
p3 = [(0,2), (1,2), (2,2), (2,3), (3,3), (4,3), (5,3), (5,2), (6,2), (7,2), (8,2)]
v3 = [{"gridPos": {"x": 4, "y": 3}, "valveType": 1, "isInitiallyOpen": 1, "allowedDirection": 1}]
levels_data.append(build_level_data(3, "Subsea Current", 1, "Medium", p3, valves=v3))

# Level 4: Breakable Rock Intro (Turtle)
p4 = [(0,0), (1,0), (1,1), (2,1), (3,1), (3,2), (4,2), (5,2), (6,2), (7,2), (8,2)]
g4 = [{"gridPos": {"x": 3, "y": 2}, "gateType": 3, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}}]
levels_data.append(build_level_data(4, "Obsidian Barrier", 1, "Medium", p4, gates=g4))

# Level 5: Locked Gate with Linked Valve
p5 = [(0,1), (1,1), (2,1), (2,2), (3,2), (4,2), (5,2), (6,2), (6,1), (7,1), (8,1)]
v5 = [{"gridPos": {"x": 2, "y": 2}, "valveType": 0, "isInitiallyOpen": 0, "allowedDirection": 1}]
g5 = [{"gridPos": {"x": 5, "y": 2}, "gateType": 0, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": 2, "y": 2}}]
levels_data.append(build_level_data(5, "Sunken Vault", 1, "Medium", p5, valves=v5, gates=g5))

# Level 6: Electric Gate Intro (Jellyfish)
p6 = [(0,2), (1,2), (2,2), (3,2), (3,3), (4,3), (5,3), (6,3), (6,2), (7,2), (8,2)]
g6 = [{"gridPos": {"x": 4, "y": 3}, "gateType": 2, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}}]
levels_data.append(build_level_data(6, "Electric Shallows", 3, "Medium", p6, gates=g6))

# Level 7: Inked Hazard Intro (Octopus)
p7 = [(0,1), (1,1), (1,2), (2,2), (3,2), (4,2), (4,1), (5,1), (6,1), (7,1), (8,1)]
g7 = [{"gridPos": {"x": 4, "y": 1}, "gateType": 4, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}}]
levels_data.append(build_level_data(7, "Octopus Trench", 2, "Medium", p7, gates=g7))

# Level 8: Pressure Gate Intro
p8 = [(0,0), (1,0), (2,0), (2,1), (3,1), (4,1), (5,1), (6,1), (6,2), (7,2), (8,2)]
g8 = [{"gridPos": {"x": 6, "y": 1}, "gateType": 1, "isInitiallyOpen": 0, "requiredPressure": 6, "linkedValvePos": {"x": -1, "y": -1}}]
levels_data.append(build_level_data(8, "Hydrostatic Rift", 3, "Medium", p8, gates=g8))

# Level 9: Dual Valves (On/Off + Directional)
p9 = [(0,2), (1,2), (1,3), (2,3), (3,3), (4,3), (5,3), (5,2), (6,2), (7,2), (8,2)]
v9 = [
    {"gridPos": {"x": 3, "y": 3}, "valveType": 0, "isInitiallyOpen": 0, "allowedDirection": 1},
    {"gridPos": {"x": 6, "y": 2}, "valveType": 1, "isInitiallyOpen": 1, "allowedDirection": 1}
]
levels_data.append(build_level_data(9, "Twisting Caverns", 1, "Medium", p9, valves=v9))

# Level 10: Breakable Rock + Inked Hazard Combo
p10 = [(0,0), (1,0), (2,0), (2,1), (3,1), (4,1), (5,1), (5,2), (6,2), (7,2), (8,2)]
g10 = [
    {"gridPos": {"x": 3, "y": 1}, "gateType": 3, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}},
    {"gridPos": {"x": 5, "y": 2}, "gateType": 4, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}}
]
levels_data.append(build_level_data(10, "Coral Labyrinth", 2, "Medium", p10, gates=g10))

# Level 11: Electric Gate + Pressure Gate Combo
p11 = [(0,1), (1,1), (2,1), (3,1), (3,2), (4,2), (5,2), (6,2), (7,2), (7,1), (8,1)]
g11 = [
    {"gridPos": {"x": 4, "y": 2}, "gateType": 2, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}},
    {"gridPos": {"x": 7, "y": 2}, "gateType": 1, "isInitiallyOpen": 0, "requiredPressure": 7, "linkedValvePos": {"x": -1, "y": -1}}
]
levels_data.append(build_level_data(11, "Midnight Abyss", 3, "Medium", p11, gates=g11))

# Level 12: Deep Ocean Grand Challenge (Valve + Locked Gate + Electric Gate)
p12 = [(0,0), (1,0), (2,0), (2,1), (3,1), (4,1), (5,1), (5,2), (6,2), (7,2), (8,2)]
v12 = [{"gridPos": {"x": 2, "y": 1}, "valveType": 0, "isInitiallyOpen": 0, "allowedDirection": 1}]
g12 = [
    {"gridPos": {"x": 4, "y": 1}, "gateType": 0, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": 2, "y": 1}},
    {"gridPos": {"x": 6, "y": 2}, "gateType": 2, "isInitiallyOpen": 0, "requiredPressure": 0, "linkedValvePos": {"x": -1, "y": -1}}
]
levels_data.append(build_level_data(12, "Abyssal Sanctuary", 3, "Medium", p12, valves=v12, gates=g12))

print(f"Constructed and verified all {len(levels_data)} Deep Ocean levels!")

def serialize_asset(ld):
    valves_yaml = "  valves: []\n"
    if ld["valves"]:
        valves_yaml = "  valves:\n"
        for v in ld["valves"]:
            valves_yaml += f"  - gridPos: {{x: {v['gridPos']['x']}, y: {v['gridPos']['y']}}}\n"
            valves_yaml += f"    valveType: {v['valveType']}\n"
            valves_yaml += f"    isInitiallyOpen: {v['isInitiallyOpen']}\n"
            valves_yaml += f"    allowedDirection: {v['allowedDirection']}\n"

    gates_yaml = "  gates: []\n"
    if ld["gates"]:
        gates_yaml = "  gates:\n"
        for g in ld["gates"]:
            gates_yaml += f"  - gridPos: {{x: {g['gridPos']['x']}, y: {g['gridPos']['y']}}}\n"
            gates_yaml += f"    gateType: {g['gateType']}\n"
            gates_yaml += f"    isInitiallyOpen: {g['isInitiallyOpen']}\n"
            gates_yaml += f"    requiredPressure: {g['requiredPressure']}\n"
            gates_yaml += f"    linkedValvePos: {{x: {g['linkedValvePos']['x']}, y: {g['linkedValvePos']['y']}}}\n"

    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!114 &11400000",
        "MonoBehaviour:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        "  m_GameObject: {fileID: 0}",
        "  m_Enabled: 1",
        "  m_EditorHideFlags: 0",
        "  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f60718293a4b5c6d7e8f90, type: 3}",
        f"  m_Name: Level_{ld['level_num']:02d}",
        "  m_EditorClassIdentifier: ",
        f"  levelNumber: {ld['level_num']}",
        f"  levelName: {ld['name']}",
        "  worldName: Deep Ocean",
        f"  difficulty: {ld['difficulty']}",
        f"  activeCreature: {ld['creature']}",
        "  abilityUses: 2",
        "  cols: 9",
        "  rows: 5",
        f"  sourceCell: {{x: {ld['source'][0]}, y: {ld['source'][1]}}}",
        f"  sinkCell: {{x: {ld['sink'][0]}, y: {ld['sink'][1]}}}",
        f"  maxMoves: {ld['max_moves']}",
        f"  targetMovesFor3Stars: {ld['target_3_stars']}",
        "  pipeTypes:"
    ]
    for pt in ld["grid_types"]:
        lines.append(f"  - {pt}")
    lines.append("  initialRotations:")
    for ir in ld["init_rots"]:
        lines.append(f"  - {ir}")
    lines.append("  solutionRotations:")
    for sr in ld["sol_rots"]:
        lines.append(f"  - {sr}")
    
    yaml_content = "\n".join(lines) + "\n" + valves_yaml + gates_yaml
    return yaml_content

meta_template = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalResources: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

folder_meta_template = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalResources: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

out_dir = "Assets/Resources/Levels/DeepOcean"
os.makedirs(out_dir, exist_ok=True)
f_meta = out_dir + ".meta"
if not os.path.exists(f_meta):
    with open(f_meta, 'w', encoding='utf-8') as f:
        f.write(folder_meta_template.format(guid=uuid.uuid4().hex))

for ld in levels_data:
    asset_name = f"Level_{ld['level_num']:02d}.asset"
    path = os.path.join(out_dir, asset_name)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(serialize_asset(ld))
    meta_path = path + ".meta"
    if not os.path.exists(meta_path):
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(meta_template.format(guid=uuid.uuid4().hex))
    print(f"Wrote {path}")

# Also create alias Level_13..Level_24 in Assets/Resources/Levels/
for ld in levels_data:
    alias_num = ld['level_num'] + 12
    asset_name = f"Level_{alias_num:02d}.asset"
    path = os.path.join("Assets/Resources/Levels", asset_name)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(serialize_asset(ld))
    meta_path = path + ".meta"
    if not os.path.exists(meta_path):
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(meta_template.format(guid=uuid.uuid4().hex))
    print(f"Wrote alias {path}")

print("All 12 Deep Ocean LevelData assets successfully generated and verified!")
