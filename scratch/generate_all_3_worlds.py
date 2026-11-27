import os
import random

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
                    initial_rotations[idx] = (s_rot + random.choice([1, 2, 3])) % 4
            else:
                initial_rotations[idx] = 0

    return pipe_types, solution_rotations, initial_rotations, cell_dirs

def verify_level_solution(cols, rows, pipe_types, rotations, source, sink, gates=None, valves=None, runes=None, relics=None, seals=None):
    grid = {}
    for r in range(rows):
        for c in range(cols):
            idx = r * cols + c
            grid[(c, r)] = (pipe_types[idx], rotations[idx])

    active_valves = set()
    if valves:
        for v in valves:
            active_valves.add(v['gridPos'])

    active_runes = set()
    active_seals = set()
    charged_relics = set()
    connected = set()

    for step in range(30):
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
                            gtype = gc.get('gateType', '')
                            if gtype == 'TempleDoor':
                                req_rune = gc.get('requiredRuneId', 0)
                                if req_rune not in active_runes and nxt not in active_seals:
                                    is_blocked = True
                            elif gtype == 'StoneBridge':
                                req_rune = gc.get('requiredRuneId', 0)
                                if req_rune not in active_runes:
                                    is_blocked = True
                            elif gtype == 'LockedGate':
                                lvp = gc.get('linkedValvePos', (-1, -1))
                                if lvp not in active_valves:
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
    return is_solved, connected

# ==========================================
# WORLD 0: CORAL REEF (Sinuous Organic Waves)
# ==========================================
coral_reef_levels = [
    # Level 1: Sunken Shallows (Easy) - Gentle straight coral baseline
    {
        'levelNumber': 1, 'levelName': "Sunken Shallows", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 20, 'targetMovesFor3Stars': 8,
        'paths': [[(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]],
        'valves': [], 'gates': [], 'runes': [], 'relics': [], 'seals': []
    },
    # Level 2: Clownfish Cove (Easy) - Gentle 1-crest wave
    {
        'levelNumber': 2, 'levelName': "Clownfish Cove", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 1), 'sinkCell': (8, 1),
        'maxMoves': 22, 'targetMovesFor3Stars': 10,
        'paths': [[(0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (6, 1), (7, 1), (8, 1)]],
        'valves': [], 'gates': [], 'runes': [], 'relics': [], 'seals': []
    },
    # Level 3: Turtle Shoals (Easy) - Smooth S-curve with Red Water Valve
    {
        'levelNumber': 3, 'levelName': "Turtle Shoals", 'difficulty': "Easy",
        'activeCreature': 1, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 24, 'targetMovesFor3Stars': 12,
        'paths': [[(0, 1), (1, 1), (2, 1), (3, 1), (3, 2), (4, 2), (5, 2), (5, 3), (6, 3), (7, 3), (8, 3)]],
        'valves': [{'gridPos': (4, 2), 'valveType': 'OnOff', 'isInitiallyOpen': False}],
        'gates': [], 'runes': [], 'relics': [], 'seals': []
    },
    # Level 4: Anemone Path (Easy) - Gentle double-crest wave with Valve
    {
        'levelNumber': 4, 'levelName': "Anemone Path", 'difficulty': "Easy",
        'activeCreature': 1, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 26, 'targetMovesFor3Stars': 14,
        'paths': [[(0, 2), (1, 2), (2, 2), (2, 3), (3, 3), (4, 3), (4, 2), (4, 1), (5, 1), (6, 1), (6, 2), (7, 2), (8, 2)]],
        'valves': [{'gridPos': (4, 2), 'valveType': 'OnOff', 'isInitiallyOpen': False}],
        'gates': [], 'runes': [], 'relics': [], 'seals': []
    },
    # Level 5: Coral Lagoon (Medium) - Dual coral loop with Locked Gate
    {
        'levelNumber': 5, 'levelName': "Coral Lagoon", 'difficulty': "Medium",
        'activeCreature': 1, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 28, 'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False}],
        'gates': [{'gridPos': (4, 2), 'gateType': 'LockedGate', 'linkedValvePos': (4, 3)}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 6: Reef Crossing (Medium) - Sinuous dual wave with valve bypass
    {
        'levelNumber': 6, 'levelName': "Reef Crossing", 'difficulty': "Medium",
        'activeCreature': 1, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 28, 'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}],
        'gates': [{'gridPos': (4, 2), 'gateType': 'LockedGate', 'linkedValvePos': (4, 1)}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 7: Seahorse Garden (Medium) - Sequential dual-chamber wave with 2 valves
    {
        'levelNumber': 7, 'levelName': "Seahorse Garden", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 32, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [
            {'gridPos': (3, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (5, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'LockedGate', 'linkedValvePos': (3, 3)},
            {'gridPos': (5, 2), 'gateType': 'LockedGate', 'linkedValvePos': (5, 1)}
        ],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 8: Shell Island (Medium) - Triple-branch reef atoll
    {
        'levelNumber': 8, 'levelName': "Shell Island", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 32, 'targetMovesFor3Stars': 22,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [
            {'gridPos': (4, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (4, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'LockedGate', 'linkedValvePos': (4, 3)}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 9: Atoll Labyrinth (Hard) - Winding triple-curve coral current
    {
        'levelNumber': 9, 'levelName': "Atoll Labyrinth", 'difficulty': "Hard",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 36, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2)]
        ],
        'valves': [
            {'gridPos': (2, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (6, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [{'gridPos': (3, 2), 'gateType': 'LockedGate', 'linkedValvePos': (2, 3)}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 10: Whale Pass (Hard) - Interlocking coral channels
    {
        'levelNumber': 10, 'levelName': "Whale Pass", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 38, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [
            {'gridPos': (3, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (5, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'LockedGate', 'linkedValvePos': (3, 3)},
            {'gridPos': (6, 2), 'gateType': 'LockedGate', 'linkedValvePos': (5, 1)}
        ],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 11: Triton's Reef (Hard) - Sinuous double-loop coral labyrinth
    {
        'levelNumber': 11, 'levelName': "Triton's Reef", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 40, 'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(3, 2), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [
            {'gridPos': (2, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (5, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'LockedGate', 'linkedValvePos': (2, 3)},
            {'gridPos': (5, 2), 'gateType': 'LockedGate', 'linkedValvePos': (5, 1)}
        ],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 12: The Great Barrier (Hard) - Grand coral masterwork
    {
        'levelNumber': 12, 'levelName': "The Great Barrier", 'difficulty': "Hard",
        'activeCreature': 0, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 44, 'targetMovesFor3Stars': 32,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [
            {'gridPos': (2, 3), 'valveType': 'OnOff', 'isInitiallyOpen': False},
            {'gridPos': (4, 1), 'valveType': 'OnOff', 'isInitiallyOpen': False}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'LockedGate', 'linkedValvePos': (2, 3)},
            {'gridPos': (6, 2), 'gateType': 'LockedGate', 'linkedValvePos': (4, 1)}
        ],
        'runes': [], 'relics': [], 'seals': []
    }
]

# ========================================================
# WORLD 1: DEEP OCEAN (Industrial Submarine Hydraulic Grids)
# ========================================================
deep_ocean_levels = [
    # Level 1: Abyssal Conduit (Easy) - Direct pipeline with Pressure Valve
    {
        'levelNumber': 1, 'levelName': "Abyssal Conduit", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 24, 'targetMovesFor3Stars': 10,
        'paths': [[(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)]],
        'valves': [{'gridPos': (4, 2), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [], 'runes': [], 'relics': [], 'seals': []
    },
    # Level 2: Hydrostatic Branch (Easy) - Parallel submarine line with Pressure Lock Gate
    {
        'levelNumber': 2, 'levelName': "Hydrostatic Branch", 'difficulty': "Easy",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 26, 'targetMovesFor3Stars': 14,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [{'gridPos': (4, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 45}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 3: Bioluminescent Flow (Easy) - Submarine pipeline with Oxygen Leak
    {
        'levelNumber': 3, 'levelName': "Bioluminescent Flow", 'difficulty': "Easy",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 26, 'targetMovesFor3Stars': 14,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 2), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [], 'oxygenLeaks': [{'gridPos': (4, 3), 'leakRate': 1}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 4: Electric Trench (Easy) - Parallel manifold with Electric Shock Gate
    {
        'levelNumber': 4, 'levelName': "Electric Trench", 'difficulty': "Easy",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 28, 'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [{'gridPos': (3, 2), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [{'gridPos': (5, 2), 'gateType': 'ElectricGate'}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 5: Pressure Manifold (Medium) - 3-line hydraulic manifold
    {
        'levelNumber': 5, 'levelName': "Pressure Manifold", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 30, 'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [{'gridPos': (4, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 45}],
        'oxygenLeaks': [{'gridPos': (4, 3), 'leakRate': 1}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 6: Obsidian Barrier (Medium) - Sequential electric and pressure chambers
    {
        'levelNumber': 6, 'levelName': "Obsidian Barrier", 'difficulty': "Medium",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 32, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [{'gridPos': (5, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'ElectricGate'},
            {'gridPos': (5, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 45}
        ],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 7: Submarine Bypass (Medium) - Dual manifold with booster and electric gate
    {
        'levelNumber': 7, 'levelName': "Submarine Bypass", 'difficulty': "Medium",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 34, 'targetMovesFor3Stars': 22,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [{'gridPos': (4, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [{'gridPos': (5, 2), 'gateType': 'ElectricGate'}],
        'boosters': [{'gridPos': (4, 3), 'boostSpeed': 1.5}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 8: Hydrostatic Rift (Medium) - Dual pressure valves managing rift pressure
    {
        'levelNumber': 8, 'levelName': "Hydrostatic Rift", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 34, 'targetMovesFor3Stars': 22,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)]
        ],
        'valves': [
            {'gridPos': (3, 3), 'valveType': 'PressureRelease', 'isInitiallyOpen': True},
            {'gridPos': (5, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}
        ],
        'gates': [{'gridPos': (4, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 50}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 9: Midnight Trench (Hard) - Darkness & Sonar Beacon
    {
        'levelNumber': 9, 'levelName': "Midnight Trench", 'difficulty': "Hard",
        'activeCreature': 2, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 36, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2)]
        ],
        'valves': [{'gridPos': (6, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [{'gridPos': (5, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 45}],
        'beacons': [{'gridPos': (3, 3), 'lightRadius': 3.0}],
        'abyssalDarkness': 0.85, 'requiresSonar': True,
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 10: Twisting Abyss (Hard) - Darkness + Beacon + Electric + Pressure Gate
    {
        'levelNumber': 10, 'levelName': "Twisting Abyss", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 38, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(2, 2), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(4, 2), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [{'gridPos': (5, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'ElectricGate'},
            {'gridPos': (6, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 45}
        ],
        'beacons': [{'gridPos': (3, 3), 'lightRadius': 3.0}],
        'abyssalDarkness': 0.90, 'requiresSonar': True,
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 11: Seismic Fracture (Hard) - Tremor + 2 Valves + 2 Gates
    {
        'levelNumber': 11, 'levelName': "Seismic Fracture", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 40, 'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(3, 2), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [
            {'gridPos': (2, 3), 'valveType': 'PressureRelease', 'isInitiallyOpen': True},
            {'gridPos': (5, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'ElectricGate'},
            {'gridPos': (5, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 50}
        ],
        'tremors': [{'interval': 12.0, 'scrambleChance': 0.25}],
        'runes': [], 'relics': [], 'seals': []
    },
    # Level 12: The Abyssal Trench (Hard) - Grand Submarine Network Masterwork
    {
        'levelNumber': 12, 'levelName': "The Abyssal Trench", 'difficulty': "Hard",
        'activeCreature': 0, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 44, 'targetMovesFor3Stars': 30,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (4, 2)],
            [(2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2)],
            [(4, 2), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [
            {'gridPos': (2, 3), 'valveType': 'PressureRelease', 'isInitiallyOpen': True},
            {'gridPos': (4, 1), 'valveType': 'PressureRelease', 'isInitiallyOpen': True}
        ],
        'gates': [
            {'gridPos': (3, 2), 'gateType': 'ElectricGate'},
            {'gridPos': (6, 2), 'gateType': 'PressureLockGate', 'requiredPressure': 50}
        ],
        'beacons': [{'gridPos': (5, 3), 'lightRadius': 3.5}],
        'abyssalDarkness': 0.90, 'requiresSonar': True,
        'runes': [], 'relics': [], 'seals': []
    }
]

# =========================================================================
# WORLD 2: LOST CITY (Submerged Greco-Roman / Atlantean Temple Aqueducts)
# Distinctive architectural layouts: multi-tier aqueduct stairways,
# chasm bridges, stepped forums, and interlocking citadel labyrinths!
# =========================================================================
lost_city_levels = [
    # Level 1: Sunken Promenade (Easy) - Cascading stepped aqueduct stairway (descending from row 3 down to row 1)
    {
        'levelNumber': 1, 'levelName': "Sunken Promenade", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 26, 'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (2, 2), (3, 2), (4, 2), (4, 1), (5, 1), (6, 1), (6, 2), (7, 2), (7, 1), (8, 1)]
        ],
        'valves': [], 'gates': [], 'runes': [], 'relics': [], 'seals': []
    },

    # Level 2: Rune of the Tides (Easy) - Ascending stepped forum crossing central Tide Rune at (4, 2)
    {
        'levelNumber': 2, 'levelName': "Rune of the Tides", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 28, 'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (4, 2), (5, 2), (5, 3), (6, 3), (7, 3), (8, 3)]
        ],
        'valves': [], 'gates': [],
        'runes': [{'gridPos': (4, 2), 'runeId': 0, 'runeName': "Rune of the Tides"}],
        'relics': [], 'seals': []
    },

    # Level 3: Sanctuary Portal (Easy) - Dual-tiered aqueduct: water branches through high colonnade (row 4) to energize Portal Rune
    {
        'levelNumber': 3, 'levelName': "Sanctuary Portal", 'difficulty': "Easy",
        'activeCreature': 1, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 30, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4), (7, 3), (7, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 4), 'runeId': 0, 'runeName': "Portal Rune"}],
        'relics': [], 'seals': []
    },

    # Level 4: Pedestal of the Sun (Medium) - Deep serpentine aqueduct descending into Sun Crypt to activate Solar Seal
    {
        'levelNumber': 4, 'levelName': "Pedestal of the Sun", 'difficulty': "Medium",
        'activeCreature': 1, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 30, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 3), (1, 3), (1, 2), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2), (6, 3), (7, 3), (8, 3), (8, 2), (8, 1)]
        ],
        'valves': [], 'gates': [], 'runes': [], 'relics': [],
        'seals': [{'gridPos': (4, 1), 'linkedDoorPos': (-1, -1), 'sealName': "Solar Seal"}]
    },

    # Level 5: Chamber of Relics (Medium) - Grand reservoir: ascending zigzag path to Rune 0, unsealing lower temple door
    {
        'levelNumber': 5, 'levelName': "Chamber of Relics", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 0), 'sinkCell': (8, 4),
        'maxMoves': 34, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2), (7, 3), (7, 4), (8, 4)],
            [(2, 0), (2, 1), (2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Relic Rune"}],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 3, 'relicName': "Sunken Golden Trident"}],
        'seals': []
    },

    # Level 6: The Aqueduct Chasm (Medium) - Deep trench loop down to Abyss Floor (row 0) powers Rune, lowering Stone Bridge at (4, 3)
    {
        'levelNumber': 6, 'levelName': "The Aqueduct Chasm", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 34, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (7, 3), (7, 2), (7, 1), (8, 1)],
            [(2, 3), (2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (6, 1), (6, 2), (6, 3)]
        ],
        'valves': [],
        'gates': [{'gridPos': (4, 3), 'gateType': 'StoneBridge', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 0), 'runeId': 0, 'runeName': "Bridge Key Rune"}],
        'relics': [], 'seals': []
    },

    # Level 7: Twin Rune Corridor (Medium) - Dual-chamber zig-zag with two sequential Temple Doors
    {
        'levelNumber': 7, 'levelName': "Twin Rune Corridor", 'difficulty': "Medium",
        'activeCreature': 3, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 36, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (7, 3), (8, 3)],
            [(1, 1), (1, 2), (1, 3), (2, 3), (3, 3), (3, 2), (3, 1)],
            [(5, 1), (5, 2), (5, 3), (6, 3), (7, 3)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (3, 1), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 1), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 3), 'runeId': 0, 'runeName': "First Seal Rune"},
            {'gridPos': (6, 3), 'runeId': 1, 'runeName': "Second Seal Rune"}
        ],
        'relics': [], 'seals': []
    },

    # Level 8: Sunken Vault (Hard) - Multi-tier vault descending from row 4 to row 0 with Sacred Chalice and Vault Seal
    {
        'levelNumber': 8, 'levelName': "Sunken Vault", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 4), 'sinkCell': (8, 0),
        'maxMoves': 38, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 4), (1, 4), (2, 4), (2, 3), (2, 2), (3, 2), (4, 2), (5, 2), (5, 1), (5, 0), (6, 0), (7, 0), (8, 0)],
            [(2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0)],
            [(2, 4), (3, 4), (4, 4), (5, 4), (5, 3), (5, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (7, 0), 'gateType': 'TempleDoor', 'requiredRuneId': -1}],
        'runes': [],
        'relics': [{'gridPos': (4, 4), 'bonusMoves': 3, 'relicName': "Sacred Chalice"}],
        'seals': [{'gridPos': (3, 0), 'linkedDoorPos': (7, 0), 'sealName': "Vault Seal"}]
    },

    # Level 9: Bridge of the Ancients (Hard) - Interlocking aqueduct chasm with Stone Bridge at (3, 1) & Temple Door at (6, 1)
    {
        'levelNumber': 9, 'levelName': "Bridge of the Ancients", 'difficulty': "Hard",
        'activeCreature': 1, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 40, 'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (7, 3), (8, 3)],
            [(1, 1), (1, 2), (1, 3), (2, 3), (3, 3), (3, 2), (3, 1)],
            [(4, 1), (4, 2), (4, 3), (5, 3), (6, 3), (6, 2), (6, 1)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (3, 1), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (6, 1), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (2, 3), 'runeId': 0, 'runeName': "Aqueduct Rune"}],
        'relics': [],
        'seals': [{'gridPos': (5, 3), 'linkedDoorPos': (6, 1), 'sealName': "Archive Seal"}]
    },

    # Level 10: Temple of the Ancients (Hard) - Labyrinthine colonnade with Guardian Rune, Sanctum Seal & Orb of Tides
    {
        'levelNumber': 10, 'levelName': "Temple of the Ancients", 'difficulty': "Hard",
        'activeCreature': 2, 'sourceCell': (0, 3), 'sinkCell': (8, 0),
        'maxMoves': 42, 'targetMovesFor3Stars': 30,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (7, 3), (7, 2), (7, 1), (7, 0), (8, 0)],
            [(2, 3), (2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2), (6, 3)],
            [(3, 1), (3, 0), (4, 0), (5, 0), (5, 1)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 3), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 1), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (4, 1), 'runeId': 0, 'runeName': "Guardian Rune"}],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 3, 'relicName': "Orb of Tides"}],
        'seals': [{'gridPos': (5, 0), 'linkedDoorPos': (7, 1), 'sealName': "Sanctum Seal"}]
    },

    # Level 11: Celestial Aqueduct (Hard) - High-tier dual spiral aqueduct with Abyss Bridge Rune & Sanctuary Door
    {
        'levelNumber': 11, 'levelName': "Celestial Aqueduct", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 0), 'sinkCell': (8, 4),
        'maxMoves': 44, 'targetMovesFor3Stars': 32,
        'paths': [
            [(0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (7, 1), (7, 2), (7, 3), (7, 4), (8, 4)],
            [(1, 0), (1, 1), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (6, 1), (6, 0)],
            [(2, 2), (2, 3), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 0), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (3, 2), 'runeId': 0, 'runeName': "Abyss Bridge Rune"},
            {'gridPos': (4, 4), 'runeId': 1, 'runeName': "Sanctuary Door Rune"}
        ],
        'relics': [{'gridPos': (5, 2), 'bonusMoves': 3, 'relicName': "Upper Relic"}],
        'seals': []
    },

    # Level 12: The Grand Sanctuary (Hard) - Ultimate 3-tier Atlantean Citadel labyrinth
    {
        'levelNumber': 12, 'levelName': "The Grand Sanctuary", 'difficulty': "Hard",
        'activeCreature': 0, 'sourceCell': (0, 4), 'sinkCell': (8, 0),
        'maxMoves': 48, 'targetMovesFor3Stars': 34,
        'paths': [
            [(0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4), (7, 3), (7, 2), (7, 1), (7, 0), (8, 0)],
            [(1, 4), (1, 3), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2)],
            [(2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (6, 1), (6, 2)],
            [(3, 4), (3, 3), (3, 2)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 4), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 2), 'runeId': 0, 'runeName': "Aqueduct Rune"},
            {'gridPos': (5, 2), 'runeId': 1, 'runeName': "Apex Temple Rune"}
        ],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 4, 'relicName': "Crown of the Sunken Empire"}],
        'seals': [{'gridPos': (3, 0), 'linkedDoorPos': (4, 2), 'sealName': "Citadel Seal"}]
    }
]

def write_unity_level_asset(file_path, data, world_name, pipe_types, initial_rots, sol_rots):
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
  worldName: {world_name}
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

    # Valves
    if data.get('valves'):
        content += "  valves:\n"
        for v in data['valves']:
            content += f"  - gridPos: {{x: {v['gridPos'][0]}, y: {v['gridPos'][1]}}}\n"
            content += f"    valveType: {v['valveType']}\n"
            content += f"    isInitiallyOpen: {'true' if v.get('isInitiallyOpen') else 'false'}\n"
            content += f"    allowedDirection: 1\n"
    else:
        content += "  valves: []\n"

    # Gates
    if data.get('gates'):
        content += "  gates:\n"
        for g in data['gates']:
            content += f"  - gridPos: {{x: {g['gridPos'][0]}, y: {g['gridPos'][1]}}}\n"
            content += f"    gateType: {g['gateType']}\n"
            content += f"    isOpen: false\n"
            lvp = g.get('linkedValvePos', (-1, -1))
            content += f"    linkedValvePos: {{x: {lvp[0]}, y: {lvp[1]}}}\n"
            content += f"    requiredPressure: {g.get('requiredPressure', 0)}\n"
            content += f"    requiredRuneId: {g.get('requiredRuneId', 0)}\n"
    else:
        content += "  gates: []\n"

    content += "  obstacles: []\n"
    content += "  pressureLocks: []\n"

    # Oxygen leaks
    if data.get('oxygenLeaks'):
        content += "  oxygenLeaks:\n"
        for ol in data['oxygenLeaks']:
            content += f"  - gridPos: {{x: {ol['gridPos'][0]}, y: {ol['gridPos'][1]}}}\n"
            content += f"    leakRate: {ol.get('leakRate', 1)}\n"
    else:
        content += "  oxygenLeaks: []\n"

    # Current boosters
    if data.get('boosters'):
        content += "  currentBoosters:\n"
        for cb in data['boosters']:
            content += f"  - gridPos: {{x: {cb['gridPos'][0]}, y: {cb['gridPos'][1]}}}\n"
            content += f"    boostSpeed: {cb.get('boostSpeed', 1.5)}\n"
    else:
        content += "  currentBoosters: []\n"

    content += "  routeDiversions: []\n"

    # Abyssal Tremors
    if data.get('tremors'):
        content += "  abyssalTremors:\n"
        for tr in data['tremors']:
            content += f"  - interval: {tr['interval']}\n"
            content += f"    scrambleChance: {tr['scrambleChance']}\n"
    else:
        content += "  abyssalTremors: []\n"

    content += "  abyssalDarknessZones: []\n"

    # Abyssal Beacons
    if data.get('beacons'):
        content += "  abyssalBeacons:\n"
        for bc in data['beacons']:
            content += f"  - gridPos: {{x: {bc['gridPos'][0]}, y: {bc['gridPos'][1]}}}\n"
            content += f"    lightRadius: {bc['lightRadius']}\n"
    else:
        content += "  abyssalBeacons: []\n"

    # Ancient Runes
    if data.get('runes'):
        content += "  ancientRunes:\n"
        for r in data['runes']:
            content += f"  - gridPos: {{x: {r['gridPos'][0]}, y: {r['gridPos'][1]}}}\n"
            content += f"    runeId: {r['runeId']}\n"
            content += f"    runeName: {r['runeName']}\n"
    else:
        content += "  ancientRunes: []\n"

    # Energy Relics
    if data.get('relics'):
        content += "  energyRelics:\n"
        for el in data['relics']:
            content += f"  - gridPos: {{x: {el['gridPos'][0]}, y: {el['gridPos'][1]}}}\n"
            content += f"    relicName: {el['relicName']}\n"
            content += f"    bonusMoves: {el['bonusMoves']}\n"
    else:
        content += "  energyRelics: []\n"

    # Ancient Seals
    if data.get('seals'):
        content += "  ancientSeals:\n"
        for s in data['seals']:
            content += f"  - gridPos: {{x: {s['gridPos'][0]}, y: {s['gridPos'][1]}}}\n"
            content += f"    linkedDoorPos: {{x: {s['linkedDoorPos'][0]}, y: {s['linkedDoorPos'][1]}}}\n"
            content += f"    sealName: {s['sealName']}\n"
    else:
        content += "  ancientSeals: []\n"

    content += f"  requiresSonar: {'true' if data.get('requiresSonar') else 'false'}\n"
    content += f"  ambientPressure: {data.get('ambientPressure', 0)}\n"

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

def process_world(world_name, levels, dir_paths):
    cols, rows = 9, 5
    print(f"\n{'='*65}")
    print(f"PROCESSING & VERIFYING: {world_name.upper()}")
    print(f"{'='*65}")

    all_passed = True
    for data in levels:
        lvl = data['levelNumber']
        name = data['levelName']
        diff = data['difficulty']
        src = data['sourceCell']
        snk = data['sinkCell']
        paths = data['paths']

        try:
            ptypes, srots, irots, cell_dirs = construct_level_grid(cols, rows, paths, src, snk)
        except Exception as e:
            print(f"FAILED on Level {lvl} ({name}) Grid Construction: {e}")
            all_passed = False
            continue

        is_solved, connected = verify_level_solution(
            cols, rows, ptypes, srots, src, snk,
            data.get('gates'), data.get('valves'), data.get('runes'),
            data.get('relics'), data.get('seals')
        )

        pipe_count = sum(1 for p in ptypes if p >= 0)
        dead_ends = sum(1 for cell, dirs in cell_dirs.items() if cell not in (src, snk) and len(dirs) < 2)

        status_str = "PASSED [SOLVED]" if is_solved else "FAILED [NOT SOLVED]"
        print(f"Level {lvl:02d} [{diff:<6}]: {name:<24} | Pipes: {pipe_count:2d} | DeadEnds: {dead_ends} | {status_str}")

        if not is_solved:
            all_passed = False
            print(f"   Sink {snk} not reached! Connected: {len(connected)} tiles")

        # Write to all requested directories
        for d in dir_paths:
            os.makedirs(d, exist_ok=True)
            asset_path = os.path.join(d, f"Level_{lvl:02d}.asset")
            write_unity_level_asset(asset_path, data, world_name, ptypes, irots, srots)

    return all_passed

def main():
    w0_passed = process_world(
        "Coral Reef", coral_reef_levels,
        ["Assets/Resources/Levels/CoralReef", "Assets/Resources/Levels"]
    )
    w1_passed = process_world(
        "Deep Ocean", deep_ocean_levels,
        ["Assets/Resources/Levels/DeepOcean"]
    )
    w2_passed = process_world(
        "Lost City", lost_city_levels,
        ["Assets/Resources/Levels/LostCity"]
    )

    if w0_passed and w1_passed and w2_passed:
        print("\nSUCCESS: ALL 36 LEVELS ACROSS ALL 3 WORLDS GENERATED, VERIFIED & SAVED!")
    else:
        print("\nERRORS ENCOUNTERED IN LEVEL GENERATION.")

if __name__ == "__main__":
    main()
