import json

data = json.load(open('scratch/phase2_results.json'))
for d in data:
    print(f"Level {d['level']:02d}: Path={d['pathLength']:02d} tiles | MinMoves={d['minMovesToSolve']:02d} | MaxMoves={d['maxMoves']:02d} | 3StarTarget={d['target3Stars']:02d} | Solvable={d['solvable']} | MovesOk={d['movesWithinLimit']} | Status={d['status']}")
