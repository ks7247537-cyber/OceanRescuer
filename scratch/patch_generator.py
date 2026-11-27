with open('scratch/generate_lost_city_levels.py', 'r', encoding='utf-8') as f:
    text = f.read()

target1 = '''def construct_level_grid(cols, rows, paths, deco_pipes=None):
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

    if deco_pipes:
        for cell, (ptype, rot) in deco_pipes.items():
            c, r = cell
            idx = r * cols + c
            if pipe_types[idx] == -1:
                pipe_types[idx] = ptype
                solution_rotations[idx] = rot

    initial_rotations = [0] * (cols * rows)
    for idx in range(cols * rows):
        ptype = pipe_types[idx]
        if ptype >= 0:
            s_rot = solution_rotations[idx]
            scramble_offset = random.choice([1, 2, 3])
            initial_rotations[idx] = (s_rot + scramble_offset) % 4
        else:
            initial_rotations[idx] = 0

    return pipe_types, solution_rotations, initial_rotations'''

replacement1 = '''def construct_level_grid(cols, rows, paths, source_cell, sink_cell, deco_pipes=None):
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

    return pipe_types, solution_rotations, initial_rotations'''

target2 = "pipe_types, solution_rot, initial_rot = construct_level_grid(cols, rows, lcfg['paths'], lcfg.get('deco'))"
replacement2 = "pipe_types, solution_rot, initial_rot = construct_level_grid(cols, rows, lcfg['paths'], lcfg['sourceCell'], lcfg['sinkCell'], None)"

assert target1 in text, "target1 not found"
text = text.replace(target1, replacement1, 1)

assert target2 in text, "target2 not found"
text = text.replace(target2, replacement2, 1)

with open('scratch/generate_lost_city_levels.py', 'w', encoding='utf-8') as f:
    f.write(text)

print('Updated scratch/generate_lost_city_levels.py successfully!')
