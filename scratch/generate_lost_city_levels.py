import os
import random
import uuid

# Openings definitions
# 0=Up, 1=Right, 2=Down, 3=Left
delta = {0: (0, 1), 1: (1, 0), 2: (0, -1), 3: (-1, 0)}
opp = {0: 2, 1: 3, 2: 0, 3: 1}

base_openings = {
    0: [False, True, False, True],   # Straight (H at 0)
    1: [False, False, True, True],   # Elbow (D & L at 0)
    2: [True, True, False, True],    # TJunction (U, R, L at 0)
    3: [True, True, True, True],     # Cross
}

def has_op(pipe_type, rot, direction):
    if pipe_type < 0 or pipe_type > 3:
        return False
    return base_openings[pipe_type][(direction - rot) % 4]

def get_pipe_and_rot(dirs):
    dset = set(dirs)
    if len(dset) == 2:
        if dset == {1, 3}: return 0, 0
        if dset == {0, 2}: return 0, 1
        if dset == {2, 3}: return 1, 0
        if dset == {0, 3}: return 1, 1
        if dset == {0, 1}: return 1, 2
        if dset == {1, 2}: return 1, 3
    elif len(dset) == 3:
        for rot in range(4):
            openings = {d for d in range(4) if has_op(2, rot, d)}
            if dset.issubset(openings):
                return 2, rot
    elif len(dset) == 4:
        return 3, 0
    elif len(dset) == 1:
        d = list(dset)[0]
        return (0, 0) if d in (1, 3) else (0, 1)
    raise ValueError(f"Invalid direction set: {dirs}")

def construct_level_grid(cols, rows, paths, source_cell, sink_cell, deco_pipes=None):
    cell_dirs = {}
    for path in paths:
        for i in range(len(path)):
            curr = path[i]
            if curr not in cell_dirs:
                cell_dirs[curr] = set()
            if i > 0:
                prev = path[i-1]
                for d, (dx, dy) in delta.items():
                    if (curr[0] + dx, curr[1] + dy) == prev:
                        cell_dirs[curr].add(d)
            if i < len(path) - 1:
                nxt = path[i+1]
                for d, (dx, dy) in delta.items():
                    if (curr[0] + dx, curr[1] + dy) == nxt:
                        cell_dirs[curr].add(d)

    pipe_types = [-1] * (cols * rows)
    solution_rotations = [0] * (cols * rows)

    for cell, dirs in cell_dirs.items():
        c, r = cell
        idx = r * cols + c
        ptype, rot = get_pipe_and_rot(dirs)
        pipe_types[idx] = ptype
        solution_rotations[idx] = rot

    # Remove deco_pipes: All pipes on the grid belong to real puzzle paths so no floating/disconnected artifacts appear!

    initial_rotations = [0] * (cols * rows)
    for r in range(rows):
        for c in range(cols):
            idx = r * cols + c
            ptype = pipe_types[idx]
            if ptype >= 0:
                s_rot = solution_rotations[idx]
                # Locked source and sink must be at solution rotation so water connects
                if (c, r) == source_cell or (c, r) == sink_cell:
                    initial_rotations[idx] = s_rot
                else:
                    scramble_offset = random.choice([1, 2, 3])
                    initial_rotations[idx] = (s_rot + scramble_offset) % 4
            else:
                initial_rotations[idx] = 0

    return pipe_types, solution_rotations, initial_rotations

def verify_level_solution(cols, rows, pipe_types, rotations, source, sink, gates=None, runes=None, relics=None, seals=None):
    grid = {}
    for r in range(rows):
        for c in range(cols):
            idx = r * cols + c
            grid[(c, r)] = (pipe_types[idx], rotations[idx])

    active_runes = set()
    active_seals = set()
    charged_relics = set()
    connected = set()

    for step in range(15):
        new_flow = False
        queue = [source]
        seen = {source}
        connected.add(source)

        while queue:
            curr = queue.pop(0)
            c, r = curr
            ptype, rot = grid.get(curr, (-1, 0))
            if ptype < 0:
                continue

            if runes:
                for rc in runes:
                    if rc['gridPos'] == curr:
                        active_runes.add(rc['runeId'])

            if seals:
                for sc in seals:
                    if sc['gridPos'] == curr:
                        active_seals.add(sc.get('linkedDoorPos', (-1, -1)))

            if relics:
                for ec in relics:
                    if ec['gridPos'] == curr:
                        charged_relics.add(curr)

            for d in range(4):
                if not has_op(ptype, rot, d):
                    continue
                dx, dy = delta[d]
                nxt = (c + dx, r + dy)
                if nxt[0] < 0 or nxt[0] >= cols or nxt[1] < 0 or nxt[1] >= rows:
                    continue

                if gates:
                    is_blocked = False
                    for gc in gates:
                        if gc['gridPos'] == nxt:
                            if gc['gateType'] == 'TempleDoor':
                                req_rune = gc.get('requiredRuneId', 0)
                                if req_rune not in active_runes and nxt not in active_seals:
                                    is_blocked = True
                            elif gc['gateType'] == 'StoneBridge':
                                req_rune = gc.get('requiredRuneId', 0)
                                if req_rune not in active_runes:
                                    is_blocked = True
                    if is_blocked:
                        continue

                nxt_ptype, nxt_rot = grid.get(nxt, (-1, 0))
                if has_op(nxt_ptype, nxt_rot, opp[d]):
                    if nxt not in seen:
                        seen.add(nxt)
                        queue.append(nxt)
                        if nxt not in connected:
                            connected.add(nxt)
                            new_flow = True

        if not new_flow:
            break

    is_solved = sink in connected
    return is_solved, connected, active_runes, active_seals, charged_relics

# Level Specifications
levels_data = [
    # Level 1: Sunken Promenade
    {
        'levelNumber': 1,
        'levelName': "Sunken Promenade",
        'difficulty': "Easy",
        'activeCreature': 0, # Clownfish
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 22,
        'targetMovesFor3Stars': 12,
        'paths': [
            [(0, 2), (1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [],
        'runes': [],
        'relics': [],
        'seals': [],
        'deco': {(2, 1): (0, 0), (3, 1): (1, 2), (4, 1): (0, 0), (6, 3): (1, 0)}
    },
    # Level 2: Rune of the Tides (introduces Ancient Rune #0)
    {
        'levelNumber': 2,
        'levelName': "Rune of the Tides",
        'difficulty': "Easy",
        'activeCreature': 0,
        'sourceCell': (0, 1),
        'sinkCell': (8, 1),
        'maxMoves': 24,
        'targetMovesFor3Stars': 14,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (4, 2), (4, 3)],
            [(4, 2), (5, 2), (6, 2), (6, 1), (7, 1), (8, 1)]
        ],
        'gates': [],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Rune of the Tides"}],
        'relics': [],
        'seals': [],
        'deco': {(3, 3): (0, 0), (5, 3): (1, 1)}
    },
    # Level 3: Sanctuary Portal (introduces Temple Door)
    {
        'levelNumber': 3,
        'levelName': "Sanctuary Portal",
        'difficulty': "Easy",
        'activeCreature': 1, # Turtle
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 25,
        'targetMovesFor3Stars': 15,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 3), (3, 3)],
            [(2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [{'gridPos': (6, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (3, 3), 'runeId': 0, 'runeName': "Portal Rune"}],
        'relics': [],
        'seals': [],
        'deco': {(1, 1): (1, 2), (2, 1): (0, 0), (5, 3): (1, 0)}
    },
    # Level 4: Pedestal of the Sun (introduces Ancient Seal)
    {
        'levelNumber': 4,
        'levelName': "Pedestal of the Sun",
        'difficulty': "Easy",
        'activeCreature': 1,
        'sourceCell': (0, 3),
        'sinkCell': (8, 3),
        'maxMoves': 26,
        'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 3), (1, 3), (1, 2), (2, 2), (3, 2), (3, 1)],
            [(3, 2), (4, 2), (4, 3), (5, 3), (6, 3), (7, 3), (8, 3)]
        ],
        'gates': [{'gridPos': (5, 3), 'gateType': 'TempleDoor', 'requiredRuneId': -1}],
        'runes': [],
        'relics': [],
        'seals': [{'gridPos': (3, 1), 'linkedDoorPos': (5, 3), 'sealName': "Solar Seal"}],
        'deco': {(2, 1): (0, 0), (4, 1): (1, 1), (6, 2): (0, 1)}
    },
    # Level 5: Chamber of Relics (introduces Energy Relic)
    {
        'levelNumber': 5,
        'levelName': "Chamber of Relics",
        'difficulty': "Medium",
        'activeCreature': 2, # Octopus
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 26,
        'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 3)],
            [(2, 2), (3, 2), (4, 2), (4, 1)],
            [(4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [{'gridPos': (6, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (2, 3), 'runeId': 0, 'runeName': "Relic Rune"}],
        'relics': [{'gridPos': (4, 1), 'bonusMoves': 3, 'relicName': "Sunken Golden Trident"}],
        'seals': [],
        'deco': {(3, 1): (1, 2), (5, 1): (1, 0), (7, 3): (0, 0)}
    },
    # Level 6: The Aqueduct Chasm (introduces Ancient Stone Bridge)
    {
        'levelNumber': 6,
        'levelName': "The Aqueduct Chasm",
        'difficulty': "Medium",
        'activeCreature': 2,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 28,
        'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 1), (3, 1), (3, 0)],
            [(2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [{'gridPos': (5, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (3, 0), 'runeId': 0, 'runeName': "Bridge Key Rune"}],
        'relics': [],
        'seals': [],
        'deco': {(1, 1): (0, 0), (4, 1): (1, 3), (6, 3): (1, 1)}
    },
    # Level 7: Twin Rune Corridor
    {
        'levelNumber': 7,
        'levelName': "Twin Rune Corridor",
        'difficulty': "Medium",
        'activeCreature': 3, # Jellyfish
        'sourceCell': (0, 3),
        'sinkCell': (8, 2),
        'maxMoves': 30,
        'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2), (5, 2)],
            [(5, 2), (5, 1)],
            [(5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [
            {'gridPos': (4, 3), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 3), 'runeId': 0, 'runeName': "First Seal Rune"},
            {'gridPos': (5, 1), 'runeId': 1, 'runeName': "Second Seal Rune"}
        ],
        'relics': [],
        'seals': [],
        'deco': {(2, 2): (0, 1), (3, 1): (1, 2), (6, 3): (0, 0)}
    },
    # Level 8: Sunken Vault
    {
        'levelNumber': 8,
        'levelName': "Sunken Vault",
        'difficulty': "Medium",
        'activeCreature': 3,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 30,
        'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (1, 1), (2, 1), (3, 1), (4, 1), (4, 2)],
            [(4, 2), (4, 3), (5, 3)],
            [(4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [{'gridPos': (6, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (5, 3), 'runeId': 0, 'runeName': "Vault Keystone"}],
        'relics': [{'gridPos': (2, 1), 'bonusMoves': 3, 'relicName': "Sacred Chalice"}],
        'seals': [],
        'deco': {(2, 3): (1, 2), (3, 3): (0, 0), (7, 1): (1, 0)}
    },
    # Level 9: Bridge of the Ancients
    {
        'levelNumber': 9,
        'levelName': "Bridge of the Ancients",
        'difficulty': "Medium",
        'activeCreature': 1,
        'sourceCell': (0, 2),
        'sinkCell': (8, 1),
        'maxMoves': 32,
        'targetMovesFor3Stars': 22,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 3)],
            [(2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (6, 3)],
            [(6, 2), (6, 1), (7, 1), (8, 1)]
        ],
        'gates': [
            {'gridPos': (4, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (7, 1), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (2, 3), 'runeId': 0, 'runeName': "Aqueduct Rune"}],
        'relics': [],
        'seals': [{'gridPos': (6, 3), 'linkedDoorPos': (7, 1), 'sealName': "Archive Seal"}],
        'deco': {(3, 1): (0, 0), (4, 3): (1, 1), (5, 1): (1, 3)}
    },
    # Level 10: Temple of the Ancients
    {
        'levelNumber': 10,
        'levelName': "Temple of the Ancients",
        'difficulty': "Hard",
        'activeCreature': 2,
        'sourceCell': (0, 3),
        'sinkCell': (8, 3),
        'maxMoves': 34,
        'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 3), (1, 3), (1, 2), (1, 1)],
            [(1, 3), (2, 3), (2, 4)],
            [(2, 3), (3, 3), (4, 3), (5, 3), (5, 2), (5, 1), (5, 0)],
            [(5, 3), (6, 3), (7, 3), (8, 3)]
        ],
        'gates': [
            {'gridPos': (4, 3), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 3), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (2, 4), 'runeId': 0, 'runeName': "Guardian Rune"}],
        'relics': [{'gridPos': (1, 1), 'bonusMoves': 3, 'relicName': "Orb of Tides"}],
        'seals': [{'gridPos': (5, 0), 'linkedDoorPos': (7, 3), 'sealName': "Sanctum Seal"}],
        'deco': {(3, 2): (0, 1), (4, 1): (1, 2), (6, 2): (0, 0)}
    },
    # Level 11: Celestial Aqueduct
    {
        'levelNumber': 11,
        'levelName': "Celestial Aqueduct",
        'difficulty': "Hard",
        'activeCreature': 3,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 36,
        'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 2), (1, 2), (1, 3), (1, 4)],
            [(1, 2), (2, 2), (3, 2), (4, 2)],
            [(4, 2), (4, 3), (5, 3), (5, 4)],
            [(4, 2), (4, 1), (5, 1), (5, 0)],
            [(4, 2), (5, 2), (6, 2), (6, 3)],
            [(6, 2), (7, 2), (8, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (1, 4), 'runeId': 0, 'runeName': "Abyss Bridge Rune"},
            {'gridPos': (6, 3), 'runeId': 1, 'runeName': "Sanctuary Door Rune"}
        ],
        'relics': [
            {'gridPos': (5, 4), 'bonusMoves': 3, 'relicName': "Upper Relic"},
            {'gridPos': (5, 0), 'bonusMoves': 3, 'relicName': "Lower Relic"}
        ],
        'seals': [],
        'deco': {(2, 3): (0, 0), (3, 1): (1, 1), (7, 1): (0, 1)}
    },
    # Level 12: The Grand Sanctuary (Grand Finale)
    {
        'levelNumber': 12,
        'levelName': "The Grand Sanctuary",
        'difficulty': "Hard",
        'activeCreature': 0,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 38,
        'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 1), (2, 0)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (3, 4)],
            [(2, 2), (3, 2), (4, 2), (5, 2), (5, 3), (5, 4)],
            [(5, 2), (6, 2), (6, 1), (6, 0)],
            [(6, 2), (7, 2), (8, 2)]
        ],
        'gates': [
            {'gridPos': (4, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (6, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 0), 'runeId': 0, 'runeName': "Aqueduct Rune"},
            {'gridPos': (6, 0), 'runeId': 1, 'runeName': "Apex Temple Rune"}
        ],
        'relics': [
            {'gridPos': (3, 4), 'bonusMoves': 3, 'relicName': "Crown of the Sunken Empire"}
        ],
        'seals': [
            {'gridPos': (5, 4), 'linkedDoorPos': (6, 2), 'sealName': "Grand Seal"}
        ],
        'deco': {(3, 1): (0, 0), (4, 3): (1, 2), (7, 3): (0, 1)}
    }
]

# Write out YAML asset for each level
output_dir = "Assets/Resources/Levels/LostCity"
os.makedirs(output_dir, exist_ok=True)

for lcfg in levels_data:
    lvl_num = lcfg['levelNumber']
    cols, rows = 9, 5
    pipe_types, solution_rot, initial_rot = construct_level_grid(cols, rows, lcfg['paths'], lcfg['sourceCell'], lcfg['sinkCell'], None)

    # Verify Solvability!
    is_solved, connected, a_runes, a_seals, c_relics = verify_level_solution(
        cols, rows, pipe_types, solution_rot,
        lcfg['sourceCell'], lcfg['sinkCell'],
        lcfg['gates'], lcfg['runes'], lcfg['relics'], lcfg['seals']
    )

    if not is_solved:
        raise RuntimeError(f"FATAL: Level {lvl_num:02d} ({lcfg['levelName']}) failed solvability check! Connected cells: {len(connected)}")
    
    print(f"Level {lvl_num:02d} ({lcfg['levelName']}): 100% SOLVABLE! Path length: {len(connected)}, Runes: {len(a_runes)}, Relics: {len(c_relics)}")

    # Format Unity YAML
    yaml_lines = [
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
        f"  m_Name: Level_{lvl_num:02d}",
        "  m_EditorClassIdentifier: ",
        f"  levelNumber: {lvl_num}",
        f"  levelName: {lcfg['levelName']}",
        "  worldName: Lost City",
        f"  difficulty: {lcfg['difficulty']}",
        f"  activeCreature: {lcfg['activeCreature']}",
        "  abilityUses: 2",
        f"  cols: {cols}",
        f"  rows: {rows}",
        f"  sourceCell: {{x: {lcfg['sourceCell'][0]}, y: {lcfg['sourceCell'][1]}}}",
        f"  sinkCell: {{x: {lcfg['sinkCell'][0]}, y: {lcfg['sinkCell'][1]}}}",
        f"  maxMoves: {lcfg['maxMoves']}",
        f"  targetMovesFor3Stars: {lcfg['targetMovesFor3Stars']}",
        "  pipeTypes:"
    ]
    for pt in pipe_types:
        yaml_lines.append(f"  - {pt}")

    yaml_lines.append("  initialRotations:")
    for ir in initial_rot:
        yaml_lines.append(f"  - {ir}")

    yaml_lines.append("  solutionRotations:")
    for sr in solution_rot:
        yaml_lines.append(f"  - {sr}")

    yaml_lines.append("  valves: []")
    
    # Gates (TempleDoor, StoneBridge)
    if lcfg['gates']:
        yaml_lines.append("  gates:")
        for g in lcfg['gates']:
            gt_num = 6 if g['gateType'] == 'TempleDoor' else 7
            yaml_lines.append(f"  - gridPos: {{x: {g['gridPos'][0]}, y: {g['gridPos'][1]}}}")
            yaml_lines.append(f"    gateType: {gt_num}")
            yaml_lines.append("    isInitiallyOpen: 0")
            yaml_lines.append("    requiredPressure: 0")
            yaml_lines.append("    linkedValvePos: {x: -1, y: -1}")
            yaml_lines.append("    isConcealed: 0")
            yaml_lines.append("    minPressure: 0")
            yaml_lines.append("    maxPressure: 0")
            yaml_lines.append(f"    requiredRuneId: {g.get('requiredRuneId', 0)}")
    else:
        yaml_lines.append("  gates: []")

    yaml_lines.append("  boosters: []")
    yaml_lines.append("  oxygenLeaks: []")
    yaml_lines.append("  abyssalDarkness: 0")
    yaml_lines.append("  tremors: []")
    yaml_lines.append("  diversions: []")
    yaml_lines.append("  beacons: []")

    # Runes
    if lcfg['runes']:
        yaml_lines.append("  ancientRunes:")
        for r in lcfg['runes']:
            yaml_lines.append(f"  - gridPos: {{x: {r['gridPos'][0]}, y: {r['gridPos'][1]}}}")
            yaml_lines.append(f"    runeName: {r['runeName']}")
            yaml_lines.append(f"    runeId: {r['runeId']}")
    else:
        yaml_lines.append("  ancientRunes: []")

    # Relics
    if lcfg['relics']:
        yaml_lines.append("  energyRelics:")
        for rel in lcfg['relics']:
            yaml_lines.append(f"  - gridPos: {{x: {rel['gridPos'][0]}, y: {rel['gridPos'][1]}}}")
            yaml_lines.append(f"    relicName: {rel['relicName']}")
            yaml_lines.append(f"    bonusMoves: {rel['bonusMoves']}")
    else:
        yaml_lines.append("  energyRelics: []")

    # Seals
    if lcfg['seals']:
        yaml_lines.append("  ancientSeals:")
        for s in lcfg['seals']:
            ld = s['linkedDoorPos']
            yaml_lines.append(f"  - gridPos: {{x: {s['gridPos'][0]}, y: {s['gridPos'][1]}}}")
            yaml_lines.append(f"    linkedDoorPos: {{x: {ld[0]}, y: {ld[1]}}}")
            yaml_lines.append(f"    sealName: {s['sealName']}")
    else:
        yaml_lines.append("  ancientSeals: []")

    file_path = f"{output_dir}/Level_{lvl_num:02d}.asset"
    with open(file_path, "w", encoding="utf-8") as f:
        f.write("\n".join(yaml_lines) + "\n")

    meta_path = file_path + ".meta"
    if not os.path.exists(meta_path):
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\nNativeFormatImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

print("\nAll 12 Lost City levels generated and validated with 100% solvability!")
