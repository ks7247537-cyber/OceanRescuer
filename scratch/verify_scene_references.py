import re
import sys

def verify_scene(scene_path):
    with open(scene_path, 'r', encoding='utf-8') as f:
        content = f.read()

    defined_ids = set(re.findall(r'--- !u!\d+ &(\d+)', content))
    print(f"Scene: {scene_path}")
    print(f"Total defined fileIDs: {len(defined_ids)}")

    # Check local references {fileID: 12345} where guid is omitted
    # Note: unity format is {fileID: 12345} or {fileID: 12345, guid: ...}
    # Matches {fileID: 12345} without a guid on the same line/struct
    pattern = r'\{fileID:\s*(\d+)\s*\}'
    local_refs = re.findall(pattern, content)
    
    missing = set()
    for ref in local_refs:
        if ref != "0" and ref not in defined_ids:
            missing.add(ref)

    # Check GameObject references in components: m_GameObject: {fileID: 12345}
    # And check Component references in GameObjects: - component: {fileID: 12345}
    go_in_comp = re.findall(r'm_GameObject:\s*\{fileID:\s*(\d+)\s*\}', content)
    dangling_comps = []
    for go_id in go_in_comp:
        if go_id != "0" and go_id not in defined_ids:
            dangling_comps.append(go_id)

    print(f"Missing local fileIDs: {len(missing)}")
    if missing:
        print("Missing fileIDs:", list(missing)[:20])
    if dangling_comps:
        print("Dangling components pointing to nonexistent GameObjects:", dangling_comps[:20])

    if not missing and not dangling_comps:
        print("SUCCESS: Zero broken local references, zero dangling components!")

if __name__ == '__main__':
    verify_scene("Assets/Scenes/GameplayScene.unity")
