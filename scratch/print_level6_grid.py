import re

with open('Assets/Resources/Levels/DeepOcean/Level_06.asset', 'r', encoding='utf-8') as f:
    text = f.read()

# Extract pipeTypes, solutionRotations, initialRotations
pipe_types = [int(x.strip()) for x in re.findall(r'- (-?\d+)', text.split('pipeTypes:')[1].split('initialRotations:')[0])]
init_rots = [int(x.strip()) for x in re.findall(r'- (-?\d+)', text.split('initialRotations:')[1].split('solutionRotations:')[0])]
sol_rots = [int(x.strip()) for x in re.findall(r'- (-?\d+)', text.split('solutionRotations:')[1].split('valves:')[0])]

cols = 9
rows = 5

names = {-1: "EMPTY", 0: "STRAIGHT", 1: "ELBOW", 2: "TEE", 3: "CROSS"}

print(f"Total pipes: {len(pipe_types)}")

# Print grid row by row (y from 4 down to 0)
for y in range(rows - 1, -1, -1):
    row_str = f"Y={y}: "
    for x in range(cols):
        idx = y * cols + x
        pt = pipe_types[idx]
        sr = sol_rots[idx]
        ir = init_rots[idx]
        if pt == -1:
            row_str += "  .   "
        else:
            row_str += f"{names[pt][:2]}(s{sr}) "
    print(row_str)

print("\nValves:")
m = re.search(r'valves:(.*?)gates:', text, re.DOTALL)
if m: print(m.group(1))

print("Gates:")
m = re.search(r'gates:(.*?)abyssalDarkness:', text, re.DOTALL)
if m: print(m.group(1))

print("Beacons:")
m = re.search(r'beacons:(.*)', text, re.DOTALL)
if m: print(m.group(1))
