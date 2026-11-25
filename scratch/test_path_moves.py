import os, re

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

for lvl in range(1, 13):
    path = f'Assets/Resources/Levels/Level_{lvl:02d}.asset'
    with open(path, 'r', encoding='utf-8') as f: content = f.read()
    moves_max = int(re.search(r'maxMoves: (\d+)', content).group(1))
    target3 = int(re.search(r'targetMovesFor3Stars: (\d+)', content).group(1))
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

    cols, rows = 9, 5
    visited = [[False for _ in range(rows)] for _ in range(cols)]
    parent = {}
    queue = [(src_x, src_y)]
    visited[src_x][src_y] = True

    while queue:
        cx, cy = queue.pop(0)
        if cx == sink_x and cy == sink_y: break
        c_idx = cy * cols + cx
        c_type = pipes[c_idx]
        c_rot = sols[c_idx]

        for d in range(4):
            nx, ny = cx + dx[d], cy + dy[d]
            if nx < 0 or nx >= cols or ny < 0 or ny >= rows or visited[nx][ny]: continue
            n_idx = ny * cols + nx
            n_type = pipes[n_idx]
            n_rot = sols[n_idx]
            if has_opening(c_type, c_rot, d) and has_opening(n_type, n_rot, opposite[d]):
                visited[nx][ny] = True
                parent[(nx, ny)] = (cx, cy)
                queue.append((nx, ny))

    # Collect path tiles
    path_tiles = []
    curr = (sink_x, sink_y)
    while curr != (src_x, src_y):
        path_tiles.append(curr)
        curr = parent[curr]
    path_tiles.append((src_x, src_y))

    # Moves only on path tiles
    path_moves = 0
    for cx, cy in path_tiles:
        idx = cy * cols + cx
        path_moves += (sols[idx] - inits[idx] + 4) % 4

    stars = 3 if path_moves <= target3 else (2 if path_moves <= moves_max else 1)
    status = "PASS" if path_moves <= moves_max else "FAIL"
    print(f"Level {lvl:02d}: PathTiles={len(path_tiles)}, PathMovesRequired={path_moves}/{moves_max} (3★ target={target3}) -> Stars={stars}★, Status={status}")
