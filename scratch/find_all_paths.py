import re

dx = [0, 1, 0, -1]
dy = [1, 0, -1, 0]
opposite = [2, 3, 0, 1]
base_openings = [
    [False, True, False, True],  # Straight
    [False, False, True, True],  # Elbow
    [True, True, False, True],   # TJunction
    [True, True, True, True]     # Cross
]

def has_opening(pipe_type, rot_step, direction):
    if pipe_type < 0 or pipe_type > 3: return False
    adj = (direction - rot_step + 4) % 4
    return base_openings[pipe_type][adj]

for lvl in [2, 6]:
    path = f'Assets/Resources/Levels/Level_{lvl:02d}.asset'
    with open(path, 'r', encoding='utf-8') as f: content = f.read()
    moves_max = int(re.search(r'maxMoves: (\d+)', content).group(1))
    src_x = int(re.search(r'sourceCell: {x: (\d+), y: (\d+)}', content).group(1))
    src_y = int(re.search(r'sourceCell: {x: (\d+), y: (\d+)}', content).group(2))
    sink_x = int(re.search(r'sinkCell: {x: (\d+), y: (\d+)}', content).group(1))
    sink_y = int(re.search(r'sinkCell: {x: (\d+), y: (\d+)}', content).group(2))

    pipe_block = re.search(r'pipeTypes:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    pipes = [int(x.strip().replace('- ', '')) for x in pipe_block.group(1).strip().splitlines() if x.strip()]
    init_block = re.search(r'initialRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    inits = [int(x.strip().replace('- ', '')) for x in init_block.group(1).strip().splitlines() if x.strip()]
    sol_block = re.search(r'solutionRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    sols = [int(x.strip().replace('- ', '')) for x in sol_block.group(1).strip().splitlines() if x.strip()]

    # Find ALL simple paths from source to sink in the grid using solution rotations
    cols, rows = 9, 5
    all_paths = []
    def dfs(cx, cy, current_path, visited):
        if cx == sink_x and cy == sink_y:
            all_paths.append(list(current_path))
            return
        c_idx = cy * cols + cx
        c_type = pipes[c_idx]
        c_rot = sols[c_idx]
        for d in range(4):
            nx, ny = cx + dx[d], cy + dy[d]
            if 0 <= nx < cols and 0 <= ny < rows and (nx, ny) not in visited:
                n_idx = ny * cols + nx
                n_type = pipes[n_idx]
                n_rot = sols[n_idx]
                if has_opening(c_type, c_rot, d) and has_opening(n_type, n_rot, opposite[d]):
                    visited.add((nx, ny))
                    current_path.append((nx, ny))
                    dfs(nx, ny, current_path, visited)
                    current_path.pop()
                    visited.remove((nx, ny))

    dfs(src_x, src_y, [(src_x, src_y)], set([(src_x, src_y)]))
    print(f"Level {lvl}: Found {len(all_paths)} paths from source ({src_x},{src_y}) to sink ({sink_x},{sink_y})")
    for p_idx, p in enumerate(all_paths):
        # compute minimal moves for path p
        p_moves = 0
        for cx, cy in p:
            idx = cy * cols + cx
            p_type = pipes[idx]
            init_r = inits[idx]
            sol_r = sols[idx]
            best_diff = 999
            for test_r in range(4):
                same = all(has_opening(p_type, test_r, d) == has_opening(p_type, sol_r, d) for d in range(4))
                if same:
                    diff = (test_r - init_r + 4) % 4
                    if diff < best_diff: best_diff = diff
            p_moves += best_diff
        print(f"  Path {p_idx+1}: Length={len(p)}, MinMoves={p_moves} (MaxMoves={moves_max})")
