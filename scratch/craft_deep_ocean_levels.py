import os
import re

# Base openings for each pipe type at rotation 0: (Up, Right, Down, Left)
# 0: Straight (Left & Right)
# 1: Elbow (Down & Left)
# 2: TJunction (Up, Right, Left)
# 3: Cross (all 4)
BASE_OPENINGS = [
    [False, True, False, True],   # 0: Straight
    [False, False, True, True],   # 1: Elbow
    [True, True, False, True],    # 2: TJunction
    [True, True, True, True]      # 3: Cross
]

# Direction vectors: 0=Up, 1=Right, 2=Down, 3=Left
DIRS = [(0, 1), (1, 0), (0, -1), (-1, 0)]
OPP = [2, 3, 0, 1]

def has_opening(pipe_type, rot, direction):
    base = BASE_OPENINGS[pipe_type]
    adjusted = (direction - rot + 4) % 4
    return base[adjusted]

def verify_path(grid_pipes, grid_rot, src, snk):
    q = [src]
    visited = set([src])
    while q:
        cx, cy = q.pop(0)
        c_pt = grid_pipes[cy][cx]
        c_rot = grid_rot[cy][cx]
        for d, (dx, dy) in enumerate(DIRS):
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < 9 and 0 <= ny < 5:
                n_pt = grid_pipes[ny][nx]
                if n_pt >= 0:
                    n_rot = grid_rot[ny][nx]
                    if has_opening(c_pt, c_rot, d) and has_opening(n_pt, n_rot, OPP[d]):
                        if (nx, ny) not in visited:
                            visited.add((nx, ny))
                            q.append((nx, ny))
    return snk in visited, len(visited)

def find_pipe_and_rot(required_openings):
    for pt in [0, 1, 2, 3]:
        for rot in range(4):
            openings = set(d for d in range(4) if has_opening(pt, rot, d))
            if required_openings == openings:
                return pt, rot
    for pt in [2, 3]:
        for rot in range(4):
            openings = set(d for d in range(4) if has_opening(pt, rot, d))
            if required_openings.issubset(openings):
                return pt, rot
    return 0, 0

def build_level_data(level_num, name, creature, moves, target_moves, path_coords, valves, gates, forks=None):
    pipes = [[-1]*9 for _ in range(5)]
    sol_rot = [[0]*9 for _ in range(5)]
    init_rot = [[0]*9 for _ in range(5)]

    tile_dirs = {}
    for i in range(len(path_coords)):
        cx, cy = path_coords[i]
        tile_dirs.setdefault((cx, cy), set())
        if i > 0:
            px, py = path_coords[i-1]
            for d, (dx, dy) in enumerate(DIRS):
                if cx + dx == px and cy + dy == py:
                    tile_dirs[(cx, cy)].add(d)
        if i < len(path_coords) - 1:
            nx, ny = path_coords[i+1]
            for d, (dx, dy) in enumerate(DIRS):
                if cx + dx == nx and cy + dy == ny:
                    tile_dirs[(cx, cy)].add(d)

    if forks:
        for f_coords in forks:
            for i in range(len(f_coords)):
                cx, cy = f_coords[i]
                tile_dirs.setdefault((cx, cy), set())
                if i > 0:
                    px, py = f_coords[i-1]
                    for d, (dx, dy) in enumerate(DIRS):
                        if cx + dx == px and cy + dy == py:
                            tile_dirs[(cx, cy)].add(d)
                if i < len(f_coords) - 1:
                    nx, ny = f_coords[i+1]
                    for d, (dx, dy) in enumerate(DIRS):
                        if cx + dx == nx and cy + dy == ny:
                            tile_dirs[(cx, cy)].add(d)

    src = path_coords[0]
    snk = path_coords[-1]

    if len(tile_dirs[src]) == 1:
        d = list(tile_dirs[src])[0]
        tile_dirs[src].add(OPP[d])
    if len(tile_dirs[snk]) == 1:
        d = list(tile_dirs[snk])[0]
        tile_dirs[snk].add(OPP[d])

    for (cx, cy), req in tile_dirs.items():
        pt, srot = find_pipe_and_rot(req)
        pipes[cy][cx] = pt
        sol_rot[cy][cx] = srot
        if (cx, cy) == src or (cx, cy) == snk:
            init_rot[cy][cx] = srot
        else:
            shift = ((cx * 3 + cy * 7 + level_num) % 3) + 1
            init_rot[cy][cx] = (srot + shift) % 4

    solv, count = verify_path(pipes, sol_rot, src, snk)
    assert solv, f"Level {level_num} is not solvable!"
    print(f"Level {level_num:02d} ({name}): Solvable={solv}, PathCount={len(path_coords)}, TotalPipes={count}, Creature={creature}")

    pipe_types_1d = []
    init_rot_1d = []
    sol_rot_1d = []
    for r in range(5):
        for c in range(9):
            pipe_types_1d.append(pipes[r][c])
            init_rot_1d.append(init_rot[r][c])
            sol_rot_1d.append(sol_rot[r][c])

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
        f"  m_Name: Level_{level_num:02d}",
        "  m_EditorClassIdentifier: ",
        f"  levelNumber: {level_num}",
        f"  levelName: {name}",
        "  worldName: Deep Ocean",
        f"  difficulty: {'Medium' if level_num <= 6 else 'Hard'}",
        f"  activeCreature: {creature}",
        f"  abilityUses: 2",
        "  cols: 9",
        "  rows: 5",
        f"  sourceCell: {{x: {src[0]}, y: {src[1]}}}",
        f"  sinkCell: {{x: {snk[0]}, y: {snk[1]}}}",
        f"  maxMoves: {moves}",
        f"  targetMovesFor3Stars: {target_moves}",
        "  pipeTypes:"
    ]
    for pt in pipe_types_1d:
        lines.append(f"  - {pt}")
    lines.append("  initialRotations:")
    for ir in init_rot_1d:
        lines.append(f"  - {ir}")
    lines.append("  solutionRotations:")
    for sr in sol_rot_1d:
        lines.append(f"  - {sr}")

    lines.append("  valves:")
    for v in valves:
        lines.append(f"  - gridPos: {{x: {v['x']}, y: {v['y']}}}")
        lines.append(f"    valveType: {v.get('type', 0)}")
        lines.append(f"    isInitiallyOpen: {v.get('open', 0)}")
        lines.append(f"    allowedDirection: {v.get('dir', 1)}")

    lines.append("  gates:")
    for g in gates:
        lines.append(f"  - gridPos: {{x: {g['x']}, y: {g['y']}}}")
        lines.append(f"    gateType: {g.get('type', 2)}")
        lines.append(f"    isInitiallyOpen: {g.get('open', 0)}")
        lines.append(f"    requiredPressure: {g.get('pressure', 0)}")
        lines.append(f"    linkedValvePos: {{x: -1, y: -1}}")

    lines.append("")
    return "\n".join(lines)

LEVEL_SPECS = [
    # Level 1: Turtle introduction to Deep Ocean! Breakable Rock at (5, 2)
    {
        "num": 1, "name": "Abyssal Gate", "creature": 1, "moves": 28, "target": 18,
        "path": [(0, 1), (1, 1), (1, 2), (2, 2), (2, 3), (3, 3), (4, 3), (4, 2), (5, 2), (5, 1), (6, 1), (6, 2), (7, 2), (7, 1), (8, 1)],
        "valves": [],
        "gates": [{"x": 5, "y": 2, "type": 3}], # Type 3: BreakableRock for Turtle!
        "forks": []
    },
    # Level 2: Octopus Trench! Deep sea ink hazard at (4, 3)
    {
        "num": 2, "name": "Octopus Trench", "creature": 2, "moves": 30, "target": 20,
        "path": [(0, 2), (1, 2), (1, 3), (2, 3), (3, 3), (4, 3), (5, 3), (5, 2), (4, 2), (3, 2), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (8, 2)],
        "valves": [],
        "gates": [{"x": 4, "y": 3, "type": 4}], # Type 4: InkedHazard for Octopus!
        "forks": []
    },
    # Level 3: Bioluminescent Shallows! Jellyfish electric gate at (4, 2)
    {
        "num": 3, "name": "Bioluminescent Flow", "creature": 3, "moves": 32, "target": 22,
        "path": [(0, 3), (1, 3), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (5, 3), (6, 3), (7, 3), (7, 2), (6, 2), (6, 1), (7, 1), (8, 1)],
        "valves": [],
        "gates": [{"x": 4, "y": 2, "type": 2}], # Type 2: ElectricGate for Jellyfish!
        "forks": []
    },
    # Level 4: Obsidian Barrier! Turtle + Water Valve at (3, 2)
    {
        "num": 4, "name": "Obsidian Barrier", "creature": 1, "moves": 32, "target": 22,
        "path": [(0, 0), (1, 0), (1, 1), (2, 1), (3, 1), (3, 2), (4, 2), (4, 3), (5, 3), (6, 3), (6, 2), (7, 2), (7, 1), (8, 1)],
        "valves": [{"x": 3, "y": 2, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 6, "y": 2, "type": 3}],
        "forks": []
    },
    # Level 5: Sunken Vault! Octopus + Water Valve at (2, 2)
    {
        "num": 5, "name": "Sunken Vault", "creature": 2, "moves": 34, "target": 24,
        "path": [(0, 3), (1, 3), (2, 3), (2, 2), (3, 2), (4, 2), (4, 1), (5, 1), (5, 2), (6, 2), (6, 3), (7, 3), (8, 3)],
        "valves": [{"x": 2, "y": 2, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 5, "y": 2, "type": 4}],
        "forks": []
    },
    # Level 6: Electric Caverns! Jellyfish + Water Valve at (3, 3)
    {
        "num": 6, "name": "Electric Caverns", "creature": 3, "moves": 34, "target": 24,
        "path": [(0, 1), (0, 2), (1, 2), (2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (5, 2), (6, 2), (7, 2), (7, 1), (8, 1)],
        "valves": [{"x": 3, "y": 3, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 5, "y": 2, "type": 2}],
        "forks": []
    },
    # Level 7: Coral Labyrinth! Turtle + 2 Breakable Rocks
    {
        "num": 7, "name": "Coral Labyrinth", "creature": 1, "moves": 36, "target": 25,
        "path": [(0, 2), (1, 2), (1, 1), (2, 1), (3, 1), (3, 2), (4, 2), (4, 3), (5, 3), (6, 3), (6, 2), (5, 2), (5, 1), (6, 1), (7, 1), (8, 1)],
        "valves": [],
        "gates": [{"x": 3, "y": 2, "type": 3}, {"x": 6, "y": 2, "type": 3}],
        "forks": []
    },
    # Level 8: Hydrostatic Rift! Octopus + Water Valve + Inked Hazard
    {
        "num": 8, "name": "Hydrostatic Rift", "creature": 2, "moves": 36, "target": 26,
        "path": [(0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (4, 1), (4, 2), (5, 2), (5, 3), (6, 3), (7, 3), (7, 2), (8, 2)],
        "valves": [{"x": 4, "y": 1, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 5, "y": 3, "type": 4}],
        "forks": []
    },
    # Level 9: Twisting Abyss! Jellyfish + Electric Gate + Valve
    {
        "num": 9, "name": "Twisting Abyss", "creature": 3, "moves": 36, "target": 26,
        "path": [(0, 3), (1, 3), (1, 2), (2, 2), (3, 2), (3, 1), (4, 1), (5, 1), (5, 2), (6, 2), (6, 3), (7, 3), (8, 3)],
        "valves": [{"x": 3, "y": 2, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 6, "y": 2, "type": 2}],
        "forks": []
    },
    # Level 10: Deep Vault! Turtle + Valve + Rock
    {
        "num": 10, "name": "Deep Vault", "creature": 1, "moves": 38, "target": 28,
        "path": [(0, 1), (1, 1), (1, 2), (2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (5, 2), (4, 2), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (8, 2)],
        "valves": [{"x": 3, "y": 3, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 5, "y": 2, "type": 3}],
        "forks": []
    },
    # Level 11: Midnight Trench! Octopus + Valve + Inked Hazard
    {
        "num": 11, "name": "Midnight Trench", "creature": 2, "moves": 38, "target": 28,
        "path": [(0, 2), (1, 2), (2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (5, 2), (6, 2), (6, 3), (7, 3), (8, 3), (8, 2)],
        "valves": [{"x": 4, "y": 1, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 6, "y": 2, "type": 4}],
        "forks": []
    },
    # Level 12: Abyssal Sanctuary (Grand Finale)! Jellyfish + Valve + Electric Gate
    {
        "num": 12, "name": "Abyssal Sanctuary", "creature": 3, "moves": 40, "target": 30,
        "path": [(0, 0), (0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (4, 2), (4, 3), (5, 3), (6, 3), (6, 2), (7, 2), (7, 1), (8, 1), (8, 0)],
        "valves": [{"x": 3, "y": 2, "type": 0, "open": 0, "dir": 1}],
        "gates": [{"x": 6, "y": 3, "type": 2}],
        "forks": []
    }
]

out_dir = "Assets/Resources/Levels/DeepOcean"
for spec in LEVEL_SPECS:
    content = build_level_data(
        level_num=spec["num"],
        name=spec["name"],
        creature=spec["creature"],
        moves=spec["moves"],
        target_moves=spec["target"],
        path_coords=spec["path"],
        valves=spec["valves"],
        gates=spec["gates"],
        forks=spec["forks"]
    )
    p = os.path.join(out_dir, f"Level_{spec['num']:02d}.asset")
    with open(p, "w", encoding="utf-8") as f:
        f.write(content)

print("\nSUCCESS: All 12 Deep Ocean levels successfully crafted and saved!")
