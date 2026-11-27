import os

# Levels 1-4: 0.0
# Levels 5-8: 0.35
# Levels 9-11: 0.55
# Level 12: 0.65

darkness_map = {
    5: 0.35,
    6: 0.35,
    7: 0.40,
    8: 0.45,
    9: 0.55,
    10: 0.55,
    11: 0.60,
    12: 0.65
}

for lvl, val in darkness_map.items():
    p = f"Assets/Resources/Levels/DeepOcean/Level_{lvl:02d}.asset"
    if os.path.exists(p):
        with open(p, "r", encoding="utf-8") as f:
            c = f.read()
        if "abyssalDarkness:" not in c:
            with open(p, "a", encoding="utf-8") as f:
                f.write(f"  abyssalDarkness: {val}\n")
            print(f"Added abyssalDarkness: {val} to Level_{lvl:02d}")
        else:
            print(f"Level_{lvl:02d} already has abyssalDarkness")
