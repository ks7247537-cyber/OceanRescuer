import os, re

print('=== 3. LEVEL DATA ASSETS AUDIT ===')
for i in range(1, 13):
    path = f'Assets/Resources/Levels/Level_{i:02d}.asset'
    exists = os.path.exists(path)
    if exists:
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
        cols = re.search(r'cols: (\d+)', content)
        rows = re.search(r'rows: (\d+)', content)
        moves = re.search(r'maxMoves: (\d+)', content)
        diff = re.search(r'difficulty: (.*)', content)
        target3 = re.search(r'targetMovesFor3Stars: (\d+)', content)
        creature = re.search(r'activeCreature: (\d+)', content)
        c_str = cols.group(1) if cols else '?'
        r_str = rows.group(1) if rows else '?'
        m_str = moves.group(1) if moves else '?'
        d_str = diff.group(1) if diff else '?'
        t3_str = target3.group(1) if target3 else '?'
        creatures = ['Clownfish', 'Turtle', 'Octopus', 'Jellyfish']
        cr_idx = int(creature.group(1)) if creature else 0
        cr_name = creatures[cr_idx] if cr_idx < len(creatures) else str(cr_idx)

        print(f'  Level {i:02d}: Grid={c_str}x{r_str}, MaxMoves={m_str}, 3-Star Target={t3_str}, Difficulty={d_str}, Companion={cr_name}')
    else:
        print(f'  Level {i:02d}: MISSING!')
