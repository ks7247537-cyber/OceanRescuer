import re

BASE_OPENINGS = [
    [False, True, False, True],   # 0: Straight (Right & Left)
    [False, False, True, True],   # 1: Elbow (Down & Left)
    [True, True, False, True],    # 2: TJunction (Up, Right, Left)
    [True, True, True, True],     # 3: Cross (all 4)
]

DX = [0, 1, 0, -1] # Up, Right, Down, Left
DY = [1, 0, -1, 0]
OPP = [2, 3, 0, 1]

def has_opening(pipe_type, rot, d):
    if pipe_type < 0: return False
    base = BASE_OPENINGS[pipe_type]
    adj = (d - rot + 4) % 4
    return base[adj]

def test_level(path, booster_pos, booster_dir):
    with open(path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    def parse_arr(name):
        arr = []
        cap = False
        for l in lines:
            if name + ':' in l:
                cap = True
                continue
            if cap:
                if l.strip().startswith('- '):
                    arr.append(int(l.strip()[2:]))
                else:
                    break
        return arr

    types = parse_arr('pipeTypes')
    sols = parse_arr('solutionRotations')
    
    # Grid 9x5
    cols, rows = 9, 5
    grid = [[types[r * cols + c] for r in range(rows)] for c in range(cols)]
    rot_grid = [[sols[r * cols + c] for r in range(rows)] for c in range(cols)]

    # parse source and sink
    src_match = re.search(r'sourceCell:\s*\{x:\s*(\d+),\s*y:\s*(\d+)\}', ''.join(lines))
    snk_match = re.search(r'sinkCell:\s*\{x:\s*(\d+),\s*y:\s*(\d+)\}', ''.join(lines))
    src = (int(src_match.group(1)), int(src_match.group(2)))
    snk = (int(snk_match.group(1)), int(snk_match.group(2)))

    # BFS from src to snk with solved rotations
    queue = [src]
    visited = {src}
    parent = {}

    while queue:
        curr = queue.pop(0)
        cx, cy = curr
        pt = grid[cx][cy]
        rot = rot_grid[cx][cy]
        if pt < 0: continue

        for d in range(4):
            if not has_opening(pt, rot, d): continue
            nx, ny = cx + DX[d], cy + DY[d]
            if 0 <= nx < cols and 0 <= ny < rows:
                if (nx, ny) not in visited:
                    npt = grid[nx][ny]
                    nrot = rot_grid[nx][ny]
                    if npt >= 0 and has_opening(npt, nrot, OPP[d]):
                        visited.add((nx, ny))
                        parent[(nx, ny)] = curr
                        queue.append((nx, ny))

    # check if snk reached
    reached = snk in visited
    path = []
    if reached:
        curr = snk
        while curr != src:
            path.append(curr)
            curr = parent[curr]
        path.append(src)
        path.reverse()

    booster_in_path = booster_pos in path
    # Check if booster aligns with flow
    booster_aligned = False
    if booster_in_path:
        bx, by = booster_pos
        bpt = grid[bx][by]
        brot = rot_grid[bx][by]
        booster_aligned = has_opening(bpt, brot, booster_dir)

    return {
        'reached': reached,
        'path_len': len(path),
        'booster_in_path': booster_in_path,
        'booster_aligned': booster_aligned
    }

res3 = test_level('Assets/Resources/Levels/DeepOcean/Level_03.asset', (3, 2), 1)
print(f"Level 03: Reached={res3['reached']}, PathLen={res3['path_len']}, BoosterInPath={res3['booster_in_path']}, Aligned={res3['booster_aligned']}")

res7 = test_level('Assets/Resources/Levels/DeepOcean/Level_07.asset', (5, 3), 1)
print(f"Level 07: Reached={res7['reached']}, PathLen={res7['path_len']}, BoosterInPath={res7['booster_in_path']}, Aligned={res7['booster_aligned']}")

res10 = test_level('Assets/Resources/Levels/DeepOcean/Level_10.asset', (4, 3), 1)
print(f"Level 10: Reached={res10['reached']}, PathLen={res10['path_len']}, BoosterInPath={res10['booster_in_path']}, Aligned={res10['booster_aligned']}")
