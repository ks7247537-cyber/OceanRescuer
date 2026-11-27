import os

base = {
    0: [False, True, False, True],
    1: [False, False, True, True],
    2: [True, True, False, True],
    3: [True, True, True, True]
}
def has_op(t, r, d):
    return base[t][(d - r) % 4]

delta = {0: (0, 1), 1: (1, 0), 2: (0, -1), 3: (-1, 0)}
opp = {0: 2, 1: 3, 2: 0, 3: 1}

print("=======================================================================")
print("          DEEP OCEAN (WORLD 1) COMPREHENSIVE LEVEL AUDIT              ")
print("=======================================================================")

solvability_results = []

for lvl in range(1, 13):
    p = f"Assets/Resources/Levels/DeepOcean/Level_{lvl:02d}.asset"
    if not os.path.exists(p):
        print(f"Level {lvl:02d}: MISSING FILE!")
        solvability_results.append((lvl, False, 0, "Missing"))
        continue

    with open(p, "r", encoding="utf-8") as f:
        c = f.read()

    lines = c.splitlines()

    def get_val(key):
        for l in lines:
            if l.strip().startswith(key + ':'):
                return l.split(':', 1)[1].strip()
        return None

    def get_list(name):
        res = []
        act = False
        for l in lines:
            if l.strip().startswith(name + ':'):
                act = True; continue
            if act and not l.strip().startswith('-'):
                break
            if act and l.strip().startswith('-'):
                res.append(int(l.strip().replace('-', '')))
        return res

    name = get_val('levelName') or f"Level {lvl}"
    diff = get_val('difficulty') or "Easy"
    max_moves = int(get_val('maxMoves') or 20)
    target_moves = int(get_val('targetMovesFor3Stars') or 10)
    creature = int(get_val('activeCreature') or 1)

    src_str = get_val('sourceCell') or "{x: 0, y: 0}"
    snk_str = get_val('sinkCell') or "{x: 8, y: 0}"

    import re
    src_m = re.findall(r'-?\d+', src_str)
    snk_m = re.findall(r'-?\d+', snk_str)
    src = (int(src_m[0]), int(src_m[1]))
    snk = (int(snk_m[0]), int(snk_m[1]))

    pt = get_list('pipeTypes')
    sol = get_list('solutionRotations')
    init_rot = get_list('initialRotations')

    # Detect mechanics
    has_valve = 'valves:' in c
    has_gate = 'gates:' in c
    has_booster = 'currentBoosters:' in c
    has_leak = 'oxygenLeaks:' in c
    has_darkness = 'abyssalDarkness:' in c
    has_tremor = 'tremors:' in c
    has_diversion = 'diversions:' in c
    has_beacon = 'beacons:' in c

    darkness_val = 0.0
    for l in lines:
        if l.strip().startswith('abyssalDarkness:'):
            try:
                darkness_val = float(l.split(':', 1)[1].strip())
            except:
                pass

    mechanics = []
    if has_booster: mechanics.append("Booster")
    if has_leak: mechanics.append("OxygenLeak")
    if darkness_val > 0: mechanics.append(f"Darkness({darkness_val})")
    if has_tremor: mechanics.append("Tremor")
    if has_diversion: mechanics.append("Diversion")
    if has_beacon: mechanics.append("Beacon")
    if has_gate: mechanics.append("Gate")
    if has_valve: mechanics.append("Valve")

    # Solve
    visited = set([src])
    q = [src]
    while q:
        cx, cy = q.pop(0)
        c_idx = cy * 9 + cx
        for d in range(4):
            if not has_op(pt[c_idx], sol[c_idx], d): continue
            nx, ny = cx + delta[d][0], cy + delta[d][1]
            if 0 <= nx < 9 and 0 <= ny < 5 and (nx, ny) not in visited:
                n_idx = ny * 9 + nx
                if has_op(pt[n_idx], sol[n_idx], opp[d]):
                    visited.add((nx, ny))
                    q.append((nx, ny))

    solvable = snk in visited
    solvability_results.append((lvl, solvable, len(visited), ", ".join(mechanics)))

    print(f"Lvl {lvl:02d} [{diff:6s}] \"{name:22s}\" | Src:{src} Snk:{snk} Moves:{max_moves:2d} | Solvable: {str(solvable):5s} (Pipes:{len(visited):2d}) | {', '.join(mechanics)}")

all_solvable = all(r[1] for r in solvability_results)
print("-----------------------------------------------------------------------")
print(f"ALL 12 DEEP OCEAN LEVELS SOLVABILITY VERIFIED: {all_solvable}")
print("=======================================================================")
