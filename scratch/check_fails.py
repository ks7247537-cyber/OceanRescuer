import json
with open('scratch/level_gameplay_test_report.json') as f:
    d = json.load(f)
for l in d['levels']:
    print(f"Level {l['level']:02d}: status={l['status']}, solvable={l['solvable']}, moves={l['solutionMoves']}/{l['maxMoves']}, path={l['pathLength']}")
