import os
import sys
import random
sys.path.insert(0, os.path.abspath('scratch'))
import generate_all_3_worlds as gen

new_lost_city_levels = [
    # Level 1: Sunken Promenade (Easy) - Cascading stepped aqueduct stairway (descending from row 3 down to row 1)
    {
        'levelNumber': 1, 'levelName': "Sunken Promenade", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 26, 'targetMovesFor3Stars': 16,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (2, 2), (3, 2), (4, 2), (4, 1), (5, 1), (6, 1), (6, 2), (7, 2), (7, 1), (8, 1)]
        ],
        'valves': [], 'gates': [], 'runes': [], 'relics': [], 'seals': []
    },

    # Level 2: Rune of the Tides (Easy) - Ascending stepped forum crossing central Tide Rune at (4, 2)
    {
        'levelNumber': 2, 'levelName': "Rune of the Tides", 'difficulty': "Easy",
        'activeCreature': 0, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 28, 'targetMovesFor3Stars': 18,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (2, 2), (3, 2), (4, 2), (5, 2), (5, 3), (6, 3), (7, 3), (8, 3)]
        ],
        'valves': [], 'gates': [],
        'runes': [{'gridPos': (4, 2), 'runeId': 0, 'runeName': "Rune of the Tides"}],
        'relics': [], 'seals': []
    },

    # Level 3: Sanctuary Portal (Easy) - Dual-tiered aqueduct: water branches through high colonnade (row 4) to energize Portal Rune
    {
        'levelNumber': 3, 'levelName': "Sanctuary Portal", 'difficulty': "Easy",
        'activeCreature': 1, 'sourceCell': (0, 2), 'sinkCell': (8, 2),
        'maxMoves': 30, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2), (8, 2)],
            [(1, 2), (1, 3), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4), (7, 3), (7, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 4), 'runeId': 0, 'runeName': "Portal Rune"}],
        'relics': [], 'seals': []
    },

    # Level 4: Pedestal of the Sun (Medium) - Deep serpentine aqueduct descending into Sun Crypt to activate Solar Seal
    {
        'levelNumber': 4, 'levelName': "Pedestal of the Sun", 'difficulty': "Medium",
        'activeCreature': 1, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 30, 'targetMovesFor3Stars': 20,
        'paths': [
            [(0, 3), (1, 3), (1, 2), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2), (6, 3), (7, 3), (8, 3), (8, 2), (8, 1)]
        ],
        'valves': [], 'gates': [], 'runes': [], 'relics': [],
        'seals': [{'gridPos': (4, 1), 'linkedDoorPos': (-1, -1), 'sealName': "Solar Seal"}]
    },

    # Level 5: Chamber of Relics (Medium) - Grand reservoir: ascending zigzag path to Rune 0, unsealing lower temple door
    {
        'levelNumber': 5, 'levelName': "Chamber of Relics", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 0), 'sinkCell': (8, 4),
        'maxMoves': 34, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2), (7, 3), (7, 4), (8, 4)],
            [(2, 0), (2, 1), (2, 2), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (6, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 3), 'runeId': 0, 'runeName': "Relic Rune"}],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 3, 'relicName': "Sunken Golden Trident"}],
        'seals': []
    },

    # Level 6: The Aqueduct Chasm (Medium) - Deep trench loop down to Abyss Floor (row 0) powers Rune, lowering Stone Bridge at (4, 3)
    {
        'levelNumber': 6, 'levelName': "The Aqueduct Chasm", 'difficulty': "Medium",
        'activeCreature': 2, 'sourceCell': (0, 3), 'sinkCell': (8, 1),
        'maxMoves': 34, 'targetMovesFor3Stars': 24,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (7, 3), (7, 2), (7, 1), (8, 1)],
            [(2, 3), (2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (6, 1), (6, 2), (6, 3)]
        ],
        'valves': [],
        'gates': [{'gridPos': (4, 3), 'gateType': 'StoneBridge', 'requiredRuneId': 0}],
        'runes': [{'gridPos': (4, 0), 'runeId': 0, 'runeName': "Bridge Key Rune"}],
        'relics': [], 'seals': []
    },

    # Level 7: Twin Rune Corridor (Medium) - Dual-chamber zig-zag with two sequential Temple Doors
    {
        'levelNumber': 7, 'levelName': "Twin Rune Corridor", 'difficulty': "Medium",
        'activeCreature': 3, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 36, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (7, 3), (8, 3)],
            [(1, 1), (1, 2), (1, 3), (2, 3), (3, 3), (3, 2), (3, 1)],
            [(5, 1), (5, 2), (5, 3), (6, 3), (7, 3)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (3, 1), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 1), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 3), 'runeId': 0, 'runeName': "First Seal Rune"},
            {'gridPos': (6, 3), 'runeId': 1, 'runeName': "Second Seal Rune"}
        ],
        'relics': [], 'seals': []
    },

    # Level 8: Sunken Vault (Hard) - Multi-tier vault descending from row 4 to row 0 with Sacred Chalice and Vault Seal
    {
        'levelNumber': 8, 'levelName': "Sunken Vault", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 4), 'sinkCell': (8, 0),
        'maxMoves': 38, 'targetMovesFor3Stars': 26,
        'paths': [
            [(0, 4), (1, 4), (2, 4), (2, 3), (2, 2), (3, 2), (4, 2), (5, 2), (5, 1), (5, 0), (6, 0), (7, 0), (8, 0)],
            [(2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0)],
            [(2, 4), (3, 4), (4, 4), (5, 4), (5, 3), (5, 2)]
        ],
        'valves': [],
        'gates': [{'gridPos': (7, 0), 'gateType': 'TempleDoor', 'requiredRuneId': -1}],
        'runes': [],
        'relics': [{'gridPos': (4, 4), 'bonusMoves': 3, 'relicName': "Sacred Chalice"}],
        'seals': [{'gridPos': (3, 0), 'linkedDoorPos': (7, 0), 'sealName': "Vault Seal"}]
    },

    # Level 9: Bridge of the Ancients (Hard) - Interlocking aqueduct chasm with Stone Bridge at (3, 1) & Temple Door at (6, 1)
    {
        'levelNumber': 9, 'levelName': "Bridge of the Ancients", 'difficulty': "Hard",
        'activeCreature': 1, 'sourceCell': (0, 1), 'sinkCell': (8, 3),
        'maxMoves': 40, 'targetMovesFor3Stars': 28,
        'paths': [
            [(0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (7, 2), (7, 3), (8, 3)],
            [(1, 1), (1, 2), (1, 3), (2, 3), (3, 3), (3, 2), (3, 1)],
            [(4, 1), (4, 2), (4, 3), (5, 3), (6, 3), (6, 2), (6, 1)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (3, 1), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (6, 1), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (2, 3), 'runeId': 0, 'runeName': "Aqueduct Rune"}],
        'relics': [],
        'seals': [{'gridPos': (5, 3), 'linkedDoorPos': (6, 1), 'sealName': "Archive Seal"}]
    },

    # Level 10: Temple of the Ancients (Hard) - Labyrinthine colonnade with Guardian Rune, Sanctum Seal & Orb of Tides
    {
        'levelNumber': 10, 'levelName': "Temple of the Ancients", 'difficulty': "Hard",
        'activeCreature': 2, 'sourceCell': (0, 3), 'sinkCell': (8, 0),
        'maxMoves': 42, 'targetMovesFor3Stars': 30,
        'paths': [
            [(0, 3), (1, 3), (2, 3), (3, 3), (4, 3), (5, 3), (6, 3), (7, 3), (7, 2), (7, 1), (7, 0), (8, 0)],
            [(2, 3), (2, 2), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (6, 2), (6, 3)],
            [(3, 1), (3, 0), (4, 0), (5, 0), (5, 1)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 3), 'gateType': 'TempleDoor', 'requiredRuneId': 0},
            {'gridPos': (7, 1), 'gateType': 'TempleDoor', 'requiredRuneId': -1}
        ],
        'runes': [{'gridPos': (4, 1), 'runeId': 0, 'runeName': "Guardian Rune"}],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 3, 'relicName': "Orb of Tides"}],
        'seals': [{'gridPos': (5, 0), 'linkedDoorPos': (7, 1), 'sealName': "Sanctum Seal"}]
    },

    # Level 11: Celestial Aqueduct (Hard) - High-tier dual spiral aqueduct with Abyss Bridge Rune & Sanctuary Door
    {
        'levelNumber': 11, 'levelName': "Celestial Aqueduct", 'difficulty': "Hard",
        'activeCreature': 3, 'sourceCell': (0, 0), 'sinkCell': (8, 4),
        'maxMoves': 44, 'targetMovesFor3Stars': 32,
        'paths': [
            [(0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (7, 1), (7, 2), (7, 3), (7, 4), (8, 4)],
            [(1, 0), (1, 1), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (6, 1), (6, 0)],
            [(2, 2), (2, 3), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 0), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (3, 2), 'runeId': 0, 'runeName': "Abyss Bridge Rune"},
            {'gridPos': (4, 4), 'runeId': 1, 'runeName': "Sanctuary Door Rune"}
        ],
        'relics': [{'gridPos': (5, 2), 'bonusMoves': 3, 'relicName': "Upper Relic"}],
        'seals': []
    },

    # Level 12: The Grand Sanctuary (Hard) - Ultimate 3-tier Atlantean Citadel labyrinth
    {
        'levelNumber': 12, 'levelName': "The Grand Sanctuary", 'difficulty': "Hard",
        'activeCreature': 0, 'sourceCell': (0, 4), 'sinkCell': (8, 0),
        'maxMoves': 48, 'targetMovesFor3Stars': 34,
        'paths': [
            [(0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4), (7, 4), (7, 3), (7, 2), (7, 1), (7, 0), (8, 0)],
            [(1, 4), (1, 3), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (6, 2), (7, 2)],
            [(2, 2), (2, 1), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (6, 1), (6, 2)],
            [(3, 4), (3, 3), (3, 2)]
        ],
        'valves': [],
        'gates': [
            {'gridPos': (4, 4), 'gateType': 'StoneBridge', 'requiredRuneId': 0},
            {'gridPos': (4, 2), 'gateType': 'TempleDoor', 'requiredRuneId': -1},
            {'gridPos': (7, 2), 'gateType': 'TempleDoor', 'requiredRuneId': 1}
        ],
        'runes': [
            {'gridPos': (2, 2), 'runeId': 0, 'runeName': "Aqueduct Rune"},
            {'gridPos': (5, 2), 'runeId': 1, 'runeName': "Apex Temple Rune"}
        ],
        'relics': [{'gridPos': (4, 0), 'bonusMoves': 4, 'relicName': "Crown of the Sunken Empire"}],
        'seals': [{'gridPos': (3, 0), 'linkedDoorPos': (4, 2), 'sealName': "Citadel Seal"}]
    }
]

print("Validating all 12 redesigned Lost City levels...")
for l in new_lost_city_levels:
    num = l['levelNumber']
    name = l['levelName']
    src = l['sourceCell']
    snk = l['sinkCell']
    paths = l['paths']
    ptypes, srot, irot, cell_dirs = gen.construct_level_grid(9, 5, paths, src, snk)
    success, conn = gen.verify_level_solution(
        9, 5, ptypes, srot, src, snk,
        gates=l.get('gates'), valves=l.get('valves'),
        runes=l.get('runes'), relics=l.get('relics'), seals=l.get('seals')
    )
    if not success:
        print(f"FAILED: Level {num} ({name}) could not be solved! Connected: {len(conn)}")
    else:
        print(f"PASSED: Level {num:2d} ({name:26s}) src={src} snk={snk} tiles={len(conn)}")
