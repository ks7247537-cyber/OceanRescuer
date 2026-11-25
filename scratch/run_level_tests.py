import os, re, json

report = {
    "suite": "Coral Reef Levels 1-12 Complete Gameplay Test",
    "timestamp": "2026-11-25 00:04:00",
    "totalLevels": 12,
    "passedLevels": 0,
    "failedLevels": 0,
    "levels": []
}

dx = [0, 1, 0, -1]  # Up, Right, Down, Left
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

creatures = ["Clownfish", "Turtle", "Octopus", "Jellyfish"]

for lvl in range(1, 13):
    path = f'Assets/Resources/Levels/Level_{lvl:02d}.asset'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    name = re.search(r'levelName: (.*)', content).group(1)
    diff = re.search(r'difficulty: (.*)', content).group(1)
    moves_max = int(re.search(r'maxMoves: (\d+)', content).group(1))
    target3 = int(re.search(r'targetMovesFor3Stars: (\d+)', content).group(1))
    cr_id = int(re.search(r'activeCreature: (\d+)', content).group(1))
    companion = creatures[cr_id] if cr_id < len(creatures) else str(cr_id)

    src_x = int(re.search(r'sourceCell: {x: (\d+), y: (\d+)}', content).group(1))
    src_y = int(re.search(r'sourceCell: {x: (\d+), y: (\d+)}', content).group(2))
    sink_x = int(re.search(r'sinkCell: {x: (\d+), y: (\d+)}', content).group(1))
    sink_y = int(re.search(r'sinkCell: {x: (\d+), y: (\d+)}', content).group(2))

    # Read pipeTypes
    pipe_block = re.search(r'pipeTypes:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    if pipe_block:
        pipes = [int(x.strip().replace('- ', '')) for x in pipe_block.group(1).strip().splitlines() if x.strip()]
    else:
        # maybe flow format
        p_raw = re.search(r'pipeTypes: \[(.*?)\]', content)
        pipes = [int(x.strip()) for x in p_raw.group(1).split(',')] if p_raw else []

    # Read initialRotations
    init_block = re.search(r'initialRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    if init_block:
        inits = [int(x.strip().replace('- ', '')) for x in init_block.group(1).strip().splitlines() if x.strip()]
    else:
        i_raw = re.search(r'initialRotations: \[(.*?)\]', content)
        inits = [int(x.strip()) for x in i_raw.group(1).split(',')] if i_raw else []

    # Read solutionRotations
    sol_block = re.search(r'solutionRotations:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    if sol_block:
        sols = [int(x.strip().replace('- ', '')) for x in sol_block.group(1).strip().splitlines() if x.strip()]
    else:
        s_raw = re.search(r'solutionRotations: \[(.*?)\]', content)
        sols = [int(x.strip()) for x in s_raw.group(1).split(',')] if s_raw else []

    # Calculate solution moves required
    moves_needed = 0
    for i in range(len(pipes)):
        i_rot = inits[i] if i < len(inits) else 0
        s_rot = sols[i] if i < len(sols) else 0
        moves_needed += (s_rot - i_rot + 4) % 4

    # Run BFS from (src_x, src_y) to (sink_x, sink_y)
    cols, rows = 9, 5
    visited = [[False for _ in range(rows)] for _ in range(cols)]
    parent = {}
    queue = [(src_x, src_y)]
    visited[src_x][src_y] = True
    reached_sink = False

    while queue:
        cx, cy = queue.pop(0)
        if cx == sink_x and cy == sink_y:
            reached_sink = True
            break

        c_idx = cy * cols + cx
        c_type = pipes[c_idx] if c_idx < len(pipes) else 0
        c_rot = sols[c_idx] if c_idx < len(sols) else 0

        for d in range(4):
            nx = cx + dx[d]
            ny = cy + dy[d]

            if nx < 0 or nx >= cols or ny < 0 or ny >= rows: continue
            if visited[nx][ny]: continue

            n_idx = ny * cols + nx
            n_type = pipes[n_idx] if n_idx < len(pipes) else 0
            n_rot = sols[n_idx] if n_idx < len(sols) else 0

            can_exit = has_opening(c_type, c_rot, d)
            can_enter = has_opening(n_type, n_rot, opposite[d])

            if can_exit and can_enter:
                visited[nx][ny] = True
                parent[(nx, ny)] = (cx, cy)
                queue.append((nx, ny))

    # Calculate path length
    path_len = 0
    if reached_sink:
        curr = (sink_x, sink_y)
        while curr != (src_x, src_y) and curr in parent:
            path_len += 1
            curr = parent[curr]
        path_len += 1

    # Stars
    stars = 1
    if moves_needed <= target3: stars = 3
    elif moves_needed <= moves_max: stars = 2

    is_pass = reached_sink and (moves_needed <= moves_max) and path_len > 0
    if is_pass: report["passedLevels"] += 1
    else: report["failedLevels"] += 1

    report["levels"].append({
        "level": lvl,
        "name": name,
        "difficulty": diff,
        "companion": companion,
        "grid": f"{cols}x{rows}",
        "maxMoves": moves_max,
        "target3Stars": target3,
        "solutionMoves": moves_needed,
        "solvable": reached_sink,
        "pathLength": path_len,
        "stars": stars,
        "status": "PASS" if is_pass else "FAIL",
        "details": f"Path verified: {path_len} tiles, Moves required: {moves_needed}/{moves_max}, Rating: {stars}⭐"
    })

with open('scratch/level_gameplay_test_report.json', 'w', encoding='utf-8') as f:
    json.dump(report, f, indent=2)

print(f"Tested {len(report['levels'])} levels: {report['passedLevels']}/12 PASSED, {report['failedLevels']} FAILED.")
