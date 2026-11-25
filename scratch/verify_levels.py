import os, re

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

def test_level(path):
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()

    ln = int(re.search(r'levelNumber:\s*(\d+)', text).group(1))
    wn = re.search(r'worldName:\s*([^\n]+)', text).group(1).strip()
    diff = re.search(r'difficulty:\s*([^\n]+)', text).group(1).strip()
    mm = int(re.search(r'maxMoves:\s*(\d+)', text).group(1))
    t3 = int(re.search(r'targetMovesFor3Stars:\s*(\d+)', text).group(1))
    cr = int(re.search(r'activeCreature:\s*(\d+)', text).group(1))

    src_x = int(re.search(r'sourceCell:\s*\{x:\s*(\d+)', text).group(1))
    src_y = int(re.search(r'sourceCell:\s*\{x:\s*\d+,\s*y:\s*(\d+)', text).group(1))
    snk_x = int(re.search(r'sinkCell:\s*\{x:\s*(\d+)', text).group(1))
    snk_y = int(re.search(r'sinkCell:\s*\{x:\s*\d+,\s*y:\s*(\d+)', text).group(1))

    # Read pipeTypes (45 integers)
    pt_block = re.search(r'pipeTypes:\s*\n((?:\s*-\s*-?\d+\n?)+)', text).group(1)
    pipe_types = [int(x) for x in re.findall(r'-\s*(-?\d+)', pt_block)]

    # Read initialRotations
    ir_block = re.search(r'initialRotations:\s*\n((?:\s*-\s*-?\d+\n?)+)', text).group(1)
    initial_rots = [int(x) for x in re.findall(r'-\s*(-?\d+)', ir_block)]

    # Read solutionRotations
    sr_block = re.search(r'solutionRotations:\s*\n((?:\s*-\s*-?\d+\n?)+)', text).group(1)
    sol_rots = [int(x) for x in re.findall(r'-\s*(-?\d+)', sr_block)]

    valves = len(re.findall(r'valveType:', text))
    gates = len(re.findall(r'gateType:', text))

    # Trace solution path from source to sink
    source = (src_x, src_y)
    sink = (snk_x, snk_y)

    queue = [source]
    visited = {source}
    parent = {}
    found_sink = False

    while queue:
        curr = queue.pop(0)
        cx, cy = curr
        if curr == sink:
            found_sink = True
            break
        c_idx = cy * 9 + cx
        c_type = pipe_types[c_idx]
        c_rot = sol_rots[c_idx]
        c_open = get_openings(c_type, c_rot)

        for d in c_open:
            dx, dy = DIR_OFFSETS[d]
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < 9 and 0 <= ny < 5 and (nx, ny) not in visited:
                n_idx = ny * 9 + nx
                n_type = pipe_types[n_idx]
                n_rot = sol_rots[n_idx]
                n_open = get_openings(n_type, n_rot)
                if OPPOSITE[d] in n_open:
                    visited.add((nx, ny))
                    parent[(nx, ny)] = curr
                    queue.append((nx, ny))

    # Reconstruct path
    path = []
    if found_sink:
        node = sink
        while node in parent:
            path.append(node)
            node = parent[node]
        path.append(source)
        path.reverse()

    # Calculate min rotation moves required
    moves_needed = 0
    for cx, cy in path:
        idx = cy * 9 + cx
        p_type = pipe_types[idx]
        cur_r = initial_rots[idx] % 4
        sol_r = sol_rots[idx] % 4
        if p_type == 0:
            diff = (sol_r - cur_r) % 2
            moves_needed += diff
        elif p_type == 3:
            pass
        else:
            diff = (sol_r - cur_r) % 4
            moves_needed += diff

    return {
        'level': ln,
        'world': wn,
        'diff': diff,
        'maxMoves': mm,
        'target3': t3,
        'creature': cr,
        'valves': valves,
        'gates': gates,
        'path_len': len(path),
        'solvable': found_sink,
        'min_moves': moves_needed,
        'buffer': mm - moves_needed
    }

print('=== AUDITING 12 DEEP OCEAN LEVELDATA ASSETS ===\n')
all_ok = True
for i in range(1, 13):
    p = f'Assets/Resources/Levels/DeepOcean/Level_{i:02d}.asset'
    res = test_level(p)
    status = 'PASS' if res['solvable'] and res['buffer'] >= 5 else 'FAIL'
    if status == 'FAIL':
        all_ok = False
    print(f"Level {res['level']:02d} ({res['diff']}): Solvable={res['solvable']} | PathLen={res['path_len']:2d} | MinMoves={res['min_moves']:2d} | MaxMoves={res['maxMoves']:2d} (Buffer +{res['buffer']:2d}) | Valves={res['valves']} | Gates={res['gates']} -> [{status}]")

print('\n' + ('SUCCESS: ALL 12 LEVELS ARE 100% VERIFIED SOLVABLE WITH SAFE MOVE BUFFERS!' if all_ok else 'FAILURE!'))
