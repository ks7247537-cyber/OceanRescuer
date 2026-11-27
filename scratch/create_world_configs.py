import uuid

world_guid = '4e4b81de554a481fb986452b8aef195f'

worlds = [
    {
        'file': 'World_00_CoralReef.asset',
        'worldIndex': 0,
        'worldId': 'coral_reef',
        'displayName': 'Coral Reef',
        'description': 'Vibrant sunlit shallow waters with gentle currents and colorful coral reefs.',
        'levelResourcePath': 'Levels/',
        'totalLevels': 12,
        'bg_guid': '7d4130da2e054ef5a6a5b5a98003cd8e',
        'banner_guid': '5f1f30bfe5b1ac3409dd1251923b09d5',
        'banner_id': '-1975562576801453055',
        'card_guid': 'c5864ecc2a5f6b744b8efee6849207e6',
        'card_id': '-4758216830704204586',
        'themeColor': '{r: 0.20, g: 0.75, b: 0.95, a: 1.0}',
        'defaultCreature': 0, # Clownfish
        'allowsCreatureAbilities': 0,
        'hasAtmosphereEffects': 0,
        'unlockRequiredWorld': -1,
        'unlockRequiredStars': 0,
        'worldClearCoins': 1000,
        'worldClearGems': 50
    },
    {
        'file': 'World_01_DeepOcean.asset',
        'worldIndex': 1,
        'worldId': 'deep_ocean',
        'displayName': 'Deep Ocean',
        'description': 'Mysterious oceanic abyss filled with glowing caverns, breakable rocks, and electric gates.',
        'levelResourcePath': 'Levels/DeepOcean/',
        'totalLevels': 12,
        'bg_guid': '8d6acd0bce5c4de6899b1ae8b19f79aa',
        'banner_guid': '0794fff946061f74b854fd5be5d232d5',
        'banner_id': '2332284168378836857',
        'card_guid': 'e468473f06b61e54682eeb232b9ab3ff',
        'card_id': '5647984744162562416',
        'themeColor': '{r: 0.08, g: 0.45, b: 0.85, a: 1.0}',
        'defaultCreature': 1, # Turtle
        'allowsCreatureAbilities': 1,
        'hasAtmosphereEffects': 1,
        'unlockRequiredWorld': 0,
        'unlockRequiredStars': 0,
        'worldClearCoins': 1500,
        'worldClearGems': 75
    },
    {
        'file': 'World_02_LostCity.asset',
        'worldIndex': 2,
        'worldId': 'lost_city',
        'displayName': 'Lost City',
        'description': 'Submerged ancient civilization with forgotten ruins and legendary machinery.',
        'levelResourcePath': 'Levels/LostCity/',
        'totalLevels': 12,
        'bg_guid': 'f24933afabaa4b7a8811c45a7c949671',
        'banner_guid': '301ba50c13bb28e42a7e120d981befc0',
        'banner_id': '-3922298887951087738',
        'card_guid': 'cfe7b4947d3c5d940ac475e5dacb4197',
        'card_id': '35362211394748318',
        'themeColor': '{r: 0.85, g: 0.65, b: 0.15, a: 1.0}',
        'defaultCreature': 0, # Clownfish
        'allowsCreatureAbilities': 1,
        'hasAtmosphereEffects': 0,
        'unlockRequiredWorld': 1,
        'unlockRequiredStars': 0,
        'worldClearCoins': 2000,
        'worldClearGems': 100
    }
]

for w in worlds:
    yaml = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {world_guid}, type: 3}}
  m_Name: {w['file'].replace('.asset', '')}
  m_EditorClassIdentifier: 
  worldIndex: {w['worldIndex']}
  worldId: {w['worldId']}
  displayName: {w['displayName']}
  description: {w['description']}
  levelResourcePath: {w['levelResourcePath']}
  totalLevels: {w['totalLevels']}
  backgroundSprite: {{fileID: 21300000, guid: {w['bg_guid']}, type: 3}}
  bannerSprite: {{fileID: {w['banner_id']}, guid: {w['banner_guid']}, type: 3}}
  worldCardSprite: {{fileID: {w['card_id']}, guid: {w['card_guid']}, type: 3}}
  themeColor: {w['themeColor']}
  defaultCreature: {w['defaultCreature']}
  allowsCreatureAbilities: {w['allowsCreatureAbilities']}
  hasAtmosphereEffects: {w['hasAtmosphereEffects']}
  unlockRequiredWorld: {w['unlockRequiredWorld']}
  unlockRequiredStars: {w['unlockRequiredStars']}
  worldClearCoins: {w['worldClearCoins']}
  worldClearGems: {w['worldClearGems']}
"""
    asset_path = f"Assets/Resources/Worlds/{w['file']}"
    with open(asset_path, 'w', encoding='utf-8') as f:
        f.write(yaml)

    meta_path = asset_path + '.meta'
    meta_guid = uuid.uuid4().hex
    meta_yaml = f"""fileFormatVersion: 2
guid: {meta_guid}
NativeFormatImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_yaml)
    print(f"Created {asset_path} with guid {meta_guid}")
