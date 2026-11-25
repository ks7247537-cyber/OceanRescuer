import re, heapq

dx = [0, 1, 0, -1]  # Up, Right, Down, Left
dy = [1, 0, -1, 0]
opposite = [2, 3, 0, 1]

base_openings = [
    [False, True, False, True],  # 0: Straight
    [False, False, True, True],  # 1: Elbow
    [True, True, False, True],   # 2: TJunction
    [True, True, True, True]     # 3: Cross
]

def has_opening(pipe_type, rot_step, direction):
    if pipe_type < 0 or pipe_type > 3: return False
    adj = (direction - rot_step + 4) % 4
    return base_openings[pipe_type][adj]

def min_rotations_to_match(pipe_type, init_rot, target_rot):
    if pipe_type < 0: return 0
    best = 999
    for r in range(4):
        if all(has_opening(pipe_type, r, d) == has_opening(pipe_type, target_rot, d) for d in range(4)):
            cost = (r - init_rot + 4) % 4
            if cost < best:
                best = cost
    return best

def min_rotations_for_openings(pipe_type, init_rot, req_openings):
    # req_openings is a dict {dir: bool}
    best = 999
    best_r = None
    for r in range(4):
        match = True
        for d, req in req_openings.items():
            if has_opening(pipe_type, r, d) != req:
                match = False
                break
        if match:
            cost = (r - init_rot + 4) % 4
            if cost < best:
                best = cost
                best_r = r
    return best, best_r

for lvl in [2, 6]:
    path = f'Assets/Resources/Levels/Level_{lvl:02d}.asset'
    with open(path, 'r', encoding='utf-8') as f: content = f.read()

    moves_max = int(re.search(r'maxMoves: (\d+)', content).group(1))
    pipe_block = re.search(r'pipeTypes:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    pipes = [int(x.strip().replace('- ', '')) for x in pipe_block.group(1).strip().splitlines() if x.strip()]
    init_block = re.search(r'initialRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    inits = [int(x.strip().replace('- ', '')) for x in init_block.group(1).strip().splitlines() if x.strip()]
    sol_block = re.search(r'solutionRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    sols = [int(x.strip().replace('- ', '')) for x in sol_block.group(1).strip().splitlines() if x.strip()]

    cols, rows = 9, 5

    # Check the designed solution moves
    designed_moves = 0
    for i in range(45):
        if pipes[i] >= 0:
            designed_moves += min_rotations_to_match(pipes[i], inits[i], sols[i])

    print(f"=== Level {lvl:02d} ===")
    print(f"Current maxMoves: {moves_max}")
    print(f"All 45 tiles rotation moves to designed solution: {designed_moves}")

    # Check path-only tiles for designed solution
    # BFS from (0,0) to (8,0) using sols
    parent = {}
    queue = [(0, 0)]
    visited = set([(0, 0)])
    while queue:
        cx, cy = queue.pop(0)
        if cx == 8 and cy == 0: break
        c_idx = cy * cols + cx
        for d in range(4):
            nx, ny = cx + dx[d], cy + dy[d]
            if 0 <= nx < cols and 0 <= ny < rows and (nx, ny) not in visited:
                n_idx = ny * cols + nx
                if pipes[n_idx] >= 0:
                    if has_opening(pipes[c_idx], sols[c_idx], d) and has_opening(pipes[n_idx], sols[n_idx], opposite[d]):
                        visited.add((nx, ny))
                        parent[(nx, ny)] = (cx, cy)
                        queue.append((nx, ny))
    
    path_tiles = []
    curr = (8, 0)
    while curr in parent:
        path_tiles.append(curr)
        curr = parent[curr]
    path_tiles.append((0, 0))
    path_tiles.reverse()

    path_moves = 0
    print(f"Designed path length: {len(path_tiles)} tiles")
    for cx, cy in path_tiles:
        idx = cy * cols + cx
        cost = min_rotations_to_match(pipes[idx], inits[idx], sols[idx])
        path_moves += cost
        print(f"  Tile ({cx},{cy}) Pipe={pipes[idx]} Init={inits[idx]} Sol={sols[idx]} -> MinCost={cost}")
    print(f"Total min moves for designed path tiles: {path_moves}")

    # Now let's check if ANY simple path from (0,0) to (8,0) can be formed with fewer moves
    # We can do a path search (DFS or Dijkstra on grid paths)
    def find_all_paths(cx, cy, current_path, current_dirs, current_cost, visited_cells):
        if cx == 8 and cy == 0:
            yield current_path, current_dirs, current_cost
            return

        for d in range(4):
            nx, ny = cx + dx[d], cy + dy[d]
            if 0 <= nx < cols and 0 <= ny < rows and (nx, ny) not in visited_cells:
                n_idx = ny * cols + nx
                if pipes[n_idx] >= 0:
                    visited_cells.add((nx, ny))
                    yield from find_all_paths(nx, ny, current_path + [(nx, ny)], current_dirs + [d], current_cost, visited_cells)
                    visited_cells.remove((nx, ny))

    # Evaluate paths
    min_any_path_cost = 999
    best_any_path = None

    for p, dirs, _ in find_all_paths(0, 0, [(0,0)], [], 0, set([(0,0)])):
        # To make this path valid, for each tile in path:
        # tile 0 must connect to dirs[0]
        # tile i must connect to opposite[dirs[i-1]] and dirs[i]
        # last tile must connect to opposite[dirs[-1]]
        total_p_cost = 0
        possible = True
        for i in range(len(p)):
            idx = p[i][1] * cols + p[i][0]
            ptype = pipes[idx]
            req = {}
            if i > 0:
                req[opposite[dirs[i-1]]] = True
            if i < len(dirs):
                req[dirs[i]] = True
            cost, _ = min_rotations_for_openings(ptype, inits[idx], req)
            if cost == 999:
                possible = False
                break
            total_p_cost += cost
        if possible and total_p_cost < min_any_path_cost:
            min_any_path_cost = total_p_cost
            best_any_path = p

    print(f"Exhaustive search across ALL possible grid paths: min moves = {min_any_path_cost}")
