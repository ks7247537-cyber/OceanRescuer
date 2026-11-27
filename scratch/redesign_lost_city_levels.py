import os
import random
import yaml

# Directions: 0=Up, 1=Right, 2=Down, 3=Left
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

def construct_level_grid(cols, rows, paths, source_cell, sink_cell):
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

    # Validate that NO interior cell has len(dirs) < 2
    for cell, dirs in cell_dirs.items():
        if cell == source_cell or cell == sink_cell:
            continue
        if len(dirs) < 2:
            raise ValueError(f"DEAD END DETECTED at {cell} with dirs {dirs}! Every interior pipe MUST connect at least 2 ways!")

    pipe_types = [-1] * (cols * rows)
    solution_rotations = [0] * (cols * rows)

    for cell, dirs in cell_dirs.items():
        c, r = cell
        idx = r * cols + c
        ptype, rot = get_pipe_and_rot(dirs)
        pipe_types[idx] = ptype
        solution_rotations[idx] = rot

    initial_rotations = [0] * (cols * rows)
    for r in range(rows):
        for c in range(cols):
            idx = r * cols + c
            ptype = pipe_types[idx]
            if ptype >= 0:
                s_rot = solution_rotations[idx]
                if (c, r) == source_cell or (c, r) == sink_cell:
                    initial_rotations[idx] = s_rot
                else:
                    # Scramble with non-zero rotation offset
                    initial_rotations[idx] = (s_rot + random.choice([1, 2, 3])) % 4
            else:
                initial_rotations[idx] = 0

    return pipe_types, solution_rotations, initial_rotations, cell_dirs

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

    for step in range(25):
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

# 12 Masterfully Designed Levels
levels_data = [
    # Level 1: Sunken Promenade
    # Clean, beautiful single-arch aqueduct
    {
        'levelNumber': 1,
        'levelName': "Sunken Promenade",
        'difficulty': "Easy",
        'activeCreature': 0, # Clownfish
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 24,
        'targetMovesFor3Stars': 14,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2), (7, 2), (8, 2)]
        ],
        'gates': [],
        'runes': [],
        'relics': [],
        'seals': []
    },

    # Level 2: Rune of the Tides
    # Elegant stepped temple aqueduct rising to the Rune at the apex (4, 3). No open chimney!
    {
        'levelNumber': 2,
        'levelName': "Rune of the Tides",
        'difficulty': "Easy",
        'activeCreature': 0,
        'sourceCell': (0, 1),
        'sinkCell': (8, 1),
        'maxMoves': 26,
        'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (3, 3), (4, 3), (5, 3), (5, 2), (6, 2), (6, 1), (7, 1), (8, 1)]
        ],
        'gates': [],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Rune of the Tides"}],
        'relics': [],
        'seals': []
    },

    # Level 3: Sanctuary Portal
    # Upper arch powers Rune 0 at (4, 3), which unseals Temple Door at (4, 2) on the lower canal!
    {
        'levelNumber': 3,
        'levelName': "Sanctuary Portal",
        'difficulty': "Easy",
        'activeCreature': 1, # Turtle
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 28,
        'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Portal Rune"}],
        'relics': [],
        'seals': []
    },

    # Level 4: Pedestal of the Sun
    # Lower canal routes water to Solar Seal at (4, 1), unsealing Temple Door at (4, 2)!
    {
        'levelNumber': 4,
        'levelName': "Pedestal of the Sun",
        'difficulty': "Easy",
        'activeCreature': 1,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 28,
        'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1}],
        'runes': [],
        'relics': [],
        'seals': [{'gridPos': (4, 1), 'linkedDoorPos': (4, 2), 'sealName': "Solar Seal"}]
    },

    # Level 5: Chamber of Relics
    # 3-tier grand reservoir: Upper arch powers Rune 0 (4, 3) to unlock door (4, 2); lower canal yields Golden Trident Relic (4, 1)!
    {
        'levelNumber': 5,
        'levelName': "Chamber of Relics",
        'difficulty': "Medium",
        'activeCreature': 2, # Octopus
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 32,
        'targetMovesFor3Stars': 22,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Relic Rune"}],
        'relics': [{'gridPos': (4, 1), 'bonusMoves': 3, 'relicName': "Sunken Golden Trident"}],
        'seals': []
    },

    # Level 6: The Aqueduct Chasm
    # Lower chasm canal energizes Bridge Key Rune at (4, 1), forming Ancient Stone Bridge across the chasm at (4, 2)!
    {
        'levelNumber': 6,
        'levelName': "The Aqueduct Chasm",
        'difficulty': "Medium",
        'activeCreature': 2,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 30,
        'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 1), 'runeId': 0, 'runeName': "Bridge Key Rune"}],
        'relics': [],
        'seals': []
    },

    # Level 7: Twin Rune Corridor
    # Chamber 1 upper loop energizes Rune 0 (3, 3) -> unseals Door 0 (3, 2); Chamber 2 lower loop energizes Rune 1 (5, 1) -> unseals Door 1 (5, 2)!
    {
        'levelNumber': 7,
        'levelName': "Twin Rune Corridor",
        'difficulty': "Medium",
        'activeCreature': 3, # Jellyfish
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 34,
        'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (5, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (3, 3), 'runeId': 0, 'runeName': "First Seal Rune"},
            {'gridPos': (5, 1), 'runeId': 1, 'runeName': "Second Seal Rune"}
        ],
        'relics': [],
        'seals': []
    },

    # Level 8: Sunken Vault
    # Multi-tier vault: Lower canal activates Solar Seal (4, 1) unlocking Temple Door (4, 2); upper vault yields Sacred Chalice Relic (4, 3)!
    {
        'levelNumber': 8,
        'levelName': "Sunken Vault",
        'difficulty': "Medium",
        'activeCreature': 3,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 34,
        'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1}],
        'runes': [],
        'relics': [{'gridPos': (4, 3), 'bonusMoves': 3, 'relicName': "Sacred Chalice"}],
        'seals': [{'gridPos': (4, 1), 'linkedDoorPos': (4, 2), 'sealName': "Vault Seal"}]
    },

    # Level 9: Bridge of the Ancients
    # Double chamber: Chamber 1 upper arch activates Bridge Rune 0 (3, 3) -> restores Stone Bridge (3, 2); Chamber 2 lower canal activates Solar Seal (5, 1) -> unseals Vault Door (5, 2)!
    {
        'levelNumber': 9,
        'levelName': "Bridge of the Ancients",
        'difficulty': "Medium",
        'activeCreature': 1,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 36,
        'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (5, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (3, 3), 'runeId': 0, 'runeName': "Aqueduct Rune"}],
        'relics': [],
        'seals': [{'gridPos': (5, 1), 'linkedDoorPos': (5, 2), 'sealName': "Archive Seal"}]
    },

    # Level 10: Temple of the Ancients
    # Triple interconnected temple courtyards: Chamber 1 Rune 0 (3, 3) opens Door (3, 2); Chamber 2 holds Orb of Tides Relic (4, 3); Chamber 3 Seal (5, 1) opens Sanctum Door (6, 2)!
    {
        'levelNumber': 10,
        'levelName': "Temple of the Ancients",
        'difficulty': "Hard",
        'activeCreature': 2,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 38,
        'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (6, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (3, 3), 'runeId': 0, 'runeName': "Guardian Rune"}],
        'relics': [{'gridPos': (5, 3), 'bonusMoves': 3, 'relicName': "Orb of Tides"}],
        'seals': [{'gridPos': (5, 1), 'linkedDoorPos': (6, 2), 'sealName': "Sanctum Seal"}]
    },

    # Level 11: Celestial Aqueduct
    # Multi-level celestial aqueduct: Bridge Rune 0 (2, 3) restores Stone Bridge (3, 2); Upper Relic (5, 3) gives bonus moves; Lower Rune 1 (5, 1) unlocks Sanctuary Door (7, 2)!
    {
        'levelNumber': 11,
        'levelName': "Celestial Aqueduct",
        'difficulty': "Hard",
        'activeCreature': 3,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 40,
        'targetMovesFor3Stars': 30,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(3, 2), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 3), 'runeId': 0, 'runeName': "Abyss Bridge Rune"},
            {'gridPos': (5, 1), 'runeId': 1, 'runeName': "Sanctuary Door Rune"}
        ],
        'relics': [
            {'gridPos': (5, 3), 'bonusMoves': 3, 'relicName': "Upper Relic"}
        ],
        'seals': []
    },

    # Level 12: The Grand Sanctuary (Grand Finale)
    # Masterpiece double-loop temple layout:
    # Rune 0 (2, 3) restores Stone Bridge (3, 2);
    # Crown of the Sunken Empire (4, 3) awards 3 moves;
    # Solar Seal (4, 1) unlocks Inner Temple Door (5, 2);
    # Apex Rune 1 (6, 3) unlocks Grand Sanctuary Door (7, 2)!
    {
        'levelNumber': 12,
        'levelName': "The Grand Sanctuary",
        'difficulty': "Hard",
        'activeCreature': 0,
        'sourceCell': (0, 2),
        'sinkCell': (8, 2),
        'maxMoves': 44,
        'targetMovesFor3Stars': 32,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (5, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 3), 'runeId': 0, 'runeName': "Aqueduct Rune"},
            {'gridPos': (6, 3), 'runeId': 1, 'runeName': "Apex Temple Rune"}
        ],
        'relics': [
            {'gridPos': (4, 3), 'bonusMoves': 3, 'relicName': "Crown of the Sunken Empire"}
        ],
        'seals': [
            {'gridPos': (4, 1), 'linkedDoorPos': (5, 2), 'sealName': "Royal Seal"}
        ]
    }
]

def run_tests_and_generate():
    cols, rows = 9, 5
    out_dir = "Assets/Resources/Levels/LostCity"
    os.makedirs(out_dir, exist_ok=True)

    print("==================================================")
    print("TESTING & GENERATING REDESIGNED LOST CITY LEVELS")
    print("==================================================")

    all_passed = True

    for data in levels_data:
        lvl = data['levelNumber']
        name = data['levelName']
        src = data['sourceCell']
        snk = data['sinkCell']
        paths = data['paths']
        gates = data['gates']
        runes = data['runes']
        relics = data['relics']
        seals = data['seals']

        # 1. Construct Grid & check no dead ends
        try:
            ptypes, srots, irots, cell_dirs = construct_level_grid(cols, rows, paths, src, snk)
        except Exception as e:
            print(f"FAILED on Level {lvl} ({name}) Grid Construction: {e}")
            all_passed = False
            continue

        # 2. Verify Solvability
        is_solved, connected, a_runes, a_seals, c_relics = verify_level_solution(
            cols, rows, ptypes, srots, src, snk, gates, runes, relics, seals
        )

        pipe_count = sum(1 for p in ptypes if p >= 0)
        dead_ends = sum(1 for cell, dirs in cell_dirs.items() if cell not in (src, snk) and len(dirs) < 2)

        status_str = "PASSED [SOLVED]" if is_solved else "FAILED [NOT SOLVED]"
        print(f"Level {lvl:02d}: {name:<26} | Pipes: {pipe_count:2d} | DeadEnds: {dead_ends} | {status_str}")

        if not is_solved:
            all_passed = False
            print(f"   Sink {snk} not reached! Connected: {len(connected)} tiles")

        # 3. Output Unity YAML Asset
        asset_path = os.path.join(out_dir, f"Level_{lvl:02d}.asset")
        write_unity_asset(asset_path, data, ptypes, irots, srots)

    if all_passed:
        print("\nALL 12 LOST CITY LEVELS REDESIGNED, VERIFIED & SAVED SUCCESSFULLY!")
    else:
        print("\nERRORS OCCURRED DURING LEVEL REDESIGN.")

def write_unity_asset(file_path, data, pipe_types, initial_rots, sol_rots):
    # Formats Unity YAML exactly as expected by Unity 6
    content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: a1b2c3d4e5f60718293a4b5c6d7e8f90, type: 3}}
  m_Name: Level_{data['levelNumber']:02d}
  m_EditorClassIdentifier: 
  levelNumber: {data['levelNumber']}
  levelName: {data['levelName']}
  worldName: Lost City
  difficulty: {data['difficulty']}
  activeCreature: {data['activeCreature']}
  abilityUses: 2
  cols: 9
  rows: 5
  sourceCell: {{x: {data['sourceCell'][0]}, y: {data['sourceCell'][1]}}}
  sinkCell: {{x: {data['sinkCell'][0]}, y: {data['sinkCell'][1]}}}
  maxMoves: {data['maxMoves']}
  targetMovesFor3Stars: {data['targetMovesFor3Stars']}
  pipeTypes:
"""
    for pt in pipe_types:
        content += f"  - {pt}\n"

    content += "  initialRotations:\n"
    for ir in initial_rots:
        content += f"  - {ir}\n"

    content += "  solutionRotations:\n"
    for sr in sol_rots:
        content += f"  - {sr}\n"

    content += "  valves: []\n"

    # Gates (TempleDoor, StoneBridge)
    if data['gates']:
        content += "  gates:\n"
        for g in data['gates']:
            content += f"  - gridPos: {{x: {g['gridPos'][0]}, y: {g['gridPos'][1]}}}\n"
            content += f"    gateType: {g['gateType']}\n"
            content += f"    isOpen: false\n"
            content += f"    linkedValvePos: {{x: -1, y: -1}}\n"
            content += f"    requiredPressure: 0\n"
            content += f"    requiredRuneId: {g.get('requiredRuneId', 0)}\n"
    else:
        content += "  gates: []\n"

    content += "  obstacles: []\n"
    content += "  pressureLocks: []\n"
    content += "  oxygenLeaks: []\n"
    content += "  currentBoosters: []\n"
    content += "  routeDiversions: []\n"
    content += "  abyssalTremors: []\n"
    content += "  abyssalDarknessZones: []\n"
    content += "  abyssalBeacons: []\n"

    # Ancient Runes
    if data['runes']:
        content += "  ancientRunes:\n"
        for r in data['runes']:
            content += f"  - gridPos: {{x: {r['gridPos'][0]}, y: {r['gridPos'][1]}}}\n"
            content += f"    runeId: {r['runeId']}\n"
            content += f"    runeName: {r['runeName']}\n"
    else:
        content += "  ancientRunes: []\n"

    # Energy Relics
    if data['relics']:
        content += "  energyRelics:\n"
        for el in data['relics']:
            content += f"  - gridPos: {{x: {el['gridPos'][0]}, y: {el['gridPos'][1]}}}\n"
            content += f"    relicName: {el['relicName']}\n"
            content += f"    bonusMoves: {el['bonusMoves']}\n"
    else:
        content += "  energyRelics: []\n"

    # Ancient Seals
    if data['seals']:
        content += "  ancientSeals:\n"
        for s in data['seals']:
            content += f"  - gridPos: {{x: {s['gridPos'][0]}, y: {s['gridPos'][1]}}}\n"
            content += f"    linkedDoorPos: {{x: {s['linkedDoorPos'][0]}, y: {s['linkedDoorPos'][1]}}}\n"
            content += f"    sealName: {s['sealName']}\n"
    else:
        content += "  ancientSeals: []\n"

    content += "  requiresSonar: false\n"
    content += "  ambientPressure: 0\n"

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

if __name__ == "__main__":
    run_tests_and_generate()
