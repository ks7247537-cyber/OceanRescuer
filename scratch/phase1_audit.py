import os, re, hashlib

print("=== PHASE 1: LEVEL DATA ASSETS AUDIT ===")
hashes = set()
for lvl in range(1, 13):
    path = f'Assets/Resources/Levels/Level_{lvl:02d}.asset'
    if not os.path.exists(path):
        print(f"Level {lvl:02d}: MISSING!")
        continue

    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    name = re.search(r'levelName: (.*)', content).group(1)
    diff = re.search(r'difficulty: (.*)', content).group(1)
    cols = int(re.search(r'cols: (\d+)', content).group(1))
    rows = int(re.search(r'rows: (\d+)', content).group(1))
    moves = int(re.search(r'maxMoves: (\d+)', content).group(1))
    target3 = int(re.search(r'targetMovesFor3Stars: (\d+)', content).group(1))
    src = re.search(r'sourceCell: {x: (\d+), y: (\d+)}', content)
    sink = re.search(r'sinkCell: {x: (\d+), y: (\d+)}', content)

    pipe_block = re.search(r'pipeTypes:\n(.*?)(?=\n  [a-zA-Z]|\n---|\Z)', content, re.DOTALL)
    pipes = [int(x.strip().replace('- ', '')) for x in pipe_block.group(1).strip().splitlines() if x.strip()]
    
    pipe_hash = hashlib.md5(str(pipes).encode('utf-8')).hexdigest()[:8]
    is_unique = pipe_hash not in hashes
    hashes.add(pipe_hash)

    print(f"Level {lvl:02d}: Name='{name}', Grid={cols}x{rows}, Source=({src.group(1)},{src.group(2)}), Sink=({sink.group(1)},{sink.group(2)}), Pipes={len(pipes)}, Hash={pipe_hash}, Unique={is_unique}, MaxMoves={moves}, 3StarTarget={target3}, Diff={diff}")

print(f"\nTotal unique arrangements verified: {len(hashes)} / 12")
