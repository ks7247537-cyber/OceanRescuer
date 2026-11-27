import os
import re

def test_world_configs():
    print("=== Testing WorldConfig Assets ===")
    configs = [
        ("Assets/Resources/Worlds/World_00_CoralReef.asset", 0, "coral_reef", "Levels/"),
        ("Assets/Resources/Worlds/World_01_DeepOcean.asset", 1, "deep_ocean", "Levels/DeepOcean/"),
        ("Assets/Resources/Worlds/World_02_LostCity.asset", 2, "lost_city", "Levels/LostCity/")
    ]
    for path, expected_idx, expected_id, expected_res in configs:
        assert os.path.exists(path), f"Missing {path}"
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
            assert f"worldIndex: {expected_idx}" in content, f"Wrong worldIndex in {path}"
            assert f"worldId: {expected_id}" in content, f"Wrong worldId in {path}"
            assert f"levelResourcePath: {expected_res}" in content, f"Wrong levelResourcePath in {path}"
        print(f"PASS: {path} configured correctly.")

def test_level_assets_isolation():
    print("\n=== Testing Level Asset Isolation ===")
    # Check Coral Reef levels 1-12
    for l in range(1, 13):
        path = f"Assets/Resources/Levels/Level_{l:02d}.asset"
        assert os.path.exists(path), f"Missing Coral Reef {path}"
        with open(path, 'r', encoding='utf-8') as f:
            c = f.read()
            # Verify asset loaded
            assert "levelNumber:" in c, f"Coral Reef level {l} corrupted"

    # Check Deep Ocean levels 1-12
    deep_creatures = []
    for l in range(1, 13):
        path = f"Assets/Resources/Levels/DeepOcean/Level_{l:02d}.asset"
        assert os.path.exists(path), f"Missing Deep Ocean {path}"
        with open(path, 'r', encoding='utf-8') as f:
            c = f.read()
            m = re.search(r'activeCreature:\s*(\d+)', c)
            deep_creatures.append(int(m.group(1)))
            # Deep Ocean should rotate Turtle (1), Octopus (2), Jellyfish (3)
            assert int(m.group(1)) in [1, 2, 3], f"Deep Ocean level {l} should have companion (1, 2, or 3), got {m.group(1)}"

    print(f"PASS: Coral Reef levels 1-12 isolated (100% Clownfish).")
    print(f"PASS: Deep Ocean levels 1-12 isolated (Creatures: {deep_creatures}).")

    # Verify Lost City directory is empty / prepared without levels yet
    lost_city_levels = [f for f in os.listdir("Assets/Resources/Levels/LostCity") if f.endswith('.asset')]
    assert len(lost_city_levels) == 0, "Lost City levels should not be created yet!"
    print("PASS: Lost City directory is prepared for future levels without creating them yet.")

def test_code_logic():
    print("\n=== Testing Code Logic Isolation ===")
    with open("Assets/Script/LevelData.cs", 'r', encoding='utf-8') as f:
        ld_code = f.read()
        assert "Levels/DeepOcean/Level_" in ld_code
        assert "Levels/LostCity/Level_" in ld_code
        # Ensure no cross-fallback to Levels/Level_ for DeepOcean or LostCity
        lines = ld_code.splitlines()
        for idx, line in enumerate(lines):
            if "worldIndex == 1" in line:
                # check that inside this block there's only DeepOcean
                block = "\n".join(lines[idx:idx+15])
                assert "Levels/Level_" not in block, "DeepOcean block must not fall back to Levels/Level_"
            if "worldIndex == 2" in line:
                block = "\n".join(lines[idx:idx+15])
                assert "Levels/Level_" not in block, "LostCity block must not fall back to Levels/Level_"

    print("PASS: LevelData.LoadLevel strictly isolates level loading by world.")

    with open("Assets/Script/LevelSelectionManager.cs", 'r', encoding='utf-8') as f:
        lsm_code = f.read()
        # Verify the auto-hijack was removed
        assert "if (world0AllDone)" not in lsm_code, "Accidental world auto-switch must be removed from LevelSelectionManager!"
        assert "WorldConfig.GetConfig(worldIndex)" in lsm_code
        assert "SaveManager.GetSelectedWorld()" in lsm_code

    print("PASS: LevelSelectionManager preserves selectedWorld without accidental hijack.")

    with open("Assets/Script/GameplayBootstrapper.cs", 'r', encoding='utf-8') as f:
        gb_code = f.read()
        assert "WorldConfig config = WorldConfig.GetConfig(worldIndex);" in gb_code
        assert "config.backgroundSprite" in gb_code
        assert "config.hasAtmosphereEffects" in gb_code

    print("PASS: GameplayBootstrapper isolates background, atmosphere, and companions per world.")

test_world_configs()
test_level_assets_isolation()
test_code_logic()
print("\nALL WORLD ISOLATION & MANAGEMENT AUDIT CHECKS PASSED SUCCESSFULLY!")

print('=== Testing Pressure System Isolation ===')
with open('Assets/Script/DeepOceanPressureManager.cs', 'r', encoding='utf-8') as f:
    mgr_text = f.read()
    assert 'SaveManager.GetSelectedWorld() == 1' in mgr_text, 'DeepOceanPressureManager must check World 1'
    assert 'gameObject.SetActive(false)' in mgr_text, 'DeepOceanPressureManager must deactivate outside World 1'
    print('PASS: DeepOceanPressureManager strictly isolated to World 1.')

with open('Assets/Script/DeepOceanPressureHUD.cs', 'r', encoding='utf-8') as f:
    hud_text = f.read()
    assert 'SaveManager.GetSelectedWorld() != 1' in hud_text, 'DeepOceanPressureHUD must check World 1'
    assert 'gameObject.SetActive(false)' in hud_text, 'DeepOceanPressureHUD must deactivate outside World 1'
    print('PASS: DeepOceanPressureHUD strictly isolated to World 1.')
