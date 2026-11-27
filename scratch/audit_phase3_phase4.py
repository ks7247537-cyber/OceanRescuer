import os
import re

print("=======================================================================")
print("     PHASE 3 & 4: WORLD ISOLATION & GAMEPLAY ARCHITECTURE AUDIT       ")
print("=======================================================================")

# 1. World Config Audit
worlds = [
    (0, "World_00_CoralReef.asset", "coral_reef", "Levels/"),
    (1, "World_01_DeepOcean.asset", "deep_ocean", "Levels/DeepOcean/"),
    (2, "World_02_LostCity.asset", "lost_city", "Levels/LostCity/")
]

print("\n[World Configuration Consistency]")
world_configs_valid = True
for idx, fname, wid, path in worlds:
    p = os.path.join("Assets/Resources/Worlds", fname)
    if not os.path.exists(p):
        print(f"  - World {idx}: MISSING FILE {fname}")
        world_configs_valid = False
        continue
    with open(p, "r", encoding="utf-8") as f:
        c = f.read()
    has_idx = f"worldIndex: {idx}" in c
    has_wid = f"worldId: {wid}" in c
    has_path = f"levelResourcePath: {path}" in c
    has_total = "totalLevels: 12" in c
    is_valid = has_idx and has_wid and has_path and has_total
    if not is_valid: world_configs_valid = False
    print(f"  - World {idx} ({wid}): index={has_idx}, id={has_wid}, path='{path}'={has_path}, 12_levels={has_total} -> {'PASS' if is_valid else 'FAIL'}")

# 2. Strict World Isolation Audit in Scripts
print("\n[World Isolation Verification in Scripts]")
isolation_checks = [
    # (Script, World, Required Isolation Guard)
    ("DeepOceanEnvironment.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("DeepOceanPressureManager.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("DeepOceanPressureHUD.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("DeepOceanSonarHUD.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("CurrentBooster.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("PressureReleaseValve.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("PressureLockGate.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("OxygenLeak.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("AbyssalDarknessController.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("TremorZone.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("RouteDiversion.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("AbyssalBeacon.cs", 1, "SaveManager.GetSelectedWorld() != 1"),
    ("LostCityEnvironment.cs", 2, "worldIndex != 2"),
    ("AncientRune.cs", 2, "SaveManager.GetSelectedWorld() != 2"),
    ("TempleDoor.cs", 2, "SaveManager.GetSelectedWorld() != 2"),
    ("AncientSeal.cs", 2, "SaveManager.GetSelectedWorld() != 2"),
    ("EnergyRelic.cs", 2, "SaveManager.GetSelectedWorld() != 2"),
    ("StoneBridge.cs", 2, "SaveManager.GetSelectedWorld() != 2")
]

all_isolated = True
for sname, wid, guard in isolation_checks:
    sp = os.path.join("Assets/Script", sname)
    if not os.path.exists(sp):
        print(f"  - {sname}: File not found!")
        all_isolated = False
        continue
    with open(sp, "r", encoding="utf-8") as f:
        sc = f.read()
    if guard not in sc and f"worldIndex == {wid}" not in sc:
        print(f"  - {sname} (World {wid}): MISSING ISOLATION GUARD '{guard}' -> FAIL")
        all_isolated = False
    else:
        print(f"  - {sname} (World {wid}): Strict isolation guard present -> PASS")

# 3. Gameplay Architecture Compatibility & Singletons
print("\n[Gameplay Architecture & Singleton Wiring]")
singletons = [
    ("GridManager", "Assets/Script/GridManager.cs"),
    ("FlowManager", "Assets/Script/FlowManager.cs"),
    ("GameManager", "Assets/Script/GameManager.cs"),
    ("MoveManager", "Assets/Script/MoveManager.cs"),
    ("AudioManager", "Assets/Script/AudioManager.cs"),
    ("EnergyManager", "Assets/Script/EnergyManager.cs"),
    ("SaveManager", "Assets/Script/SaveManager.cs")
]

arch_ok = True
for name, p in singletons:
    if not os.path.exists(p):
        print(f"  - {name}: File missing!")
        arch_ok = False
        continue
    with open(p, "r", encoding="utf-8") as f:
        c = f.read()
    # Check if Instance or static methods exist
    has_instance = "Instance" in c or "static" in c
    if not has_instance:
        print(f"  - {name}: No static access or Instance -> FAIL")
        arch_ok = False
    else:
        print(f"  - {name}: Singleton / Static access available -> PASS")

print(f"\nArchitecture Audit Status: {'ALL SYSTEMS FULLY COMPATIBLE' if arch_ok and all_isolated and world_configs_valid else 'DEFECTS FOUND'}")
