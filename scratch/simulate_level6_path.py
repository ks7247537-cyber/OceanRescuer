# Exact FlowManager BFS simulation on Level 6

import re

with open('Assets/Resources/Levels/DeepOcean/Level_06.asset', 'r', encoding='utf-8') as f:
    text = f.read()

pipe_types = [int(x.strip()) for x in re.findall(r'- (-?\d+)', text.split('pipeTypes:')[1].split('initialRotations:')[0])]
sol_rots = [int(x.strip()) for x in re.findall(r'- (-?\d+)', text.split('solutionRotations:')[1].split('valves:')[0])]

cols = 9
rows = 5

DX = [0, 1, 0, -1]   # 0=Up, 1=Right, 2=Down, 3=Left
DY = [1, 0, -1, 0]
Opposite = [2, 3, 0, 1]

def get_openings(pipe_type, rotation):
    # Straight: 0 -> Up/Down [True, False, True, False]
    #           1 -> Right/Left [False, True, False, True]
    # Elbow:    0 -> Up/Right [True, True, False, False]
    #           1 -> Right/Down [False, True, True, False]
    #           2 -> Down/Left [False, False, True, True]
    #           3 -> Left/Up [True, False, False, True]
    # Tee:      0 -> Up/Right/Down
    # Cross:    all
    if pipe_type == 0: # Straight
        r = rotation % 2
        return [True, False, True, False] if r == 0 else [False, True, False, True]
    elif pipe_type == 1: # Elbow
        r = rotation % 4
        if r == 0: return [True, True, False, False]
        elif r == 1: return [False, True, True, False]
        elif r == 2: return [False, False, True, True]
        elif r == 3: return [True, False, False, True]
    elif pipe_type == 2: # Tee
        r = rotation % 4
        base = [True, True, True, False]
        # rotate right by r
        return base[-r:] + base[:-r] if r > 0 else base
    elif pipe_type == 3: # Cross
        return [True, True, True, True]
    return [False, False, False, False]

# Let's check what rotations are in Image 3!
# In Image 3:
# (0, 1): Straight. It points Up. Rotation 0 -> openings [Up, Down]
# (0, 2): Elbow. Connects Down and Right. Rotation 1 -> [False, True, True, False] (Down and Right)
# (1, 2): Straight. Connects Left and Right. Rotation 1 -> [False, True, False, True]
# (2, 2): Elbow. Connects Left and Up. Rotation 3 -> [True, False, False, True] (Up and Left)
# (2, 3): Elbow. Connects Down and Right. Rotation 1 -> [False, True, True, False] (Down and Right)
# (3, 3): Straight. Connects Left and Right. Rotation 1. Valve is here.
# (4, 3): Straight. Connects Left and Right. Rotation 1. Beacon is here.
# (5, 3): Elbow. Connects Left and Down. Rotation 2 -> [False, False, True, True] (Down and Left)
# (5, 2): Gate. Elbow. Connects Up and Right. Rotation 0 -> [True, True, False, False] (Up and Right)
# (6, 2): Straight. Connects Left and Right. Rotation 1.
# (7, 2): Elbow. Connects Left and Down. Rotation 2 -> [Down and Left]
# (7, 1): Elbow. Connects Up and Right. Rotation 0 -> [Up and Right]
# (8, 1): Sink. Straight. Connects Left. Rotation 1 -> [Left and Right]

print("Checking connectivity tile by tile along this path:")
path = [
    (0, 1, 0, 0), # Straight, rot 0
    (0, 2, 1, 1), # Elbow, rot 1 (Down, Right)
    (1, 2, 0, 1), # Straight, rot 1 (Left, Right)
    (2, 2, 1, 3), # Elbow, rot 3 (Up, Left)
    (2, 3, 1, 1), # Elbow, rot 1 (Down, Right)
    (3, 3, 0, 1), # Straight, rot 1 (Left, Right)
    (4, 3, 0, 1), # Straight, rot 1 (Left, Right)
    (5, 3, 1, 2), # Elbow, rot 2 (Down, Left)
    (5, 2, 1, 0), # Elbow, rot 0 (Up, Right)
    (6, 2, 0, 1), # Straight, rot 1 (Left, Right)
    (7, 2, 1, 2), # Elbow, rot 2 (Down, Left)
    (7, 1, 1, 0), # Elbow, rot 0 (Up, Right)
    (8, 1, 0, 1)  # Straight, rot 1 (Left, Right)
]

for i in range(len(path) - 1):
    x1, y1, pt1, r1 = path[i]
    x2, y2, pt2, r2 = path[i+1]
    dx = x2 - x1
    dy = y2 - y1
    # find dir
    d = -1
    for k in range(4):
        if DX[k] == dx and DY[k] == dy:
            d = k
            break
    
    op1 = get_openings(pt1, r1)
    op2 = get_openings(pt2, r2)
    can_exit = op1[d]
    can_enter = op2[Opposite[d]]
    dir_name = ["UP", "RIGHT", "DOWN", "LEFT"][d]
    print(f"({x1},{y1}) -> ({x2},{y2}) [{dir_name}]: exit={can_exit}, enter={can_enter} -> {'OK' if can_exit and can_enter else 'BLOCKED!'}")
