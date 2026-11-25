import re

with open("Assets/Scenes/GameplayScene.unity", "r", encoding="utf-8") as f:
    text = f.read()

cam_m = re.search(r'Camera:(.*?)(?=\n---|\Z)', text, re.DOTALL)
if cam_m:
    b = cam_m.group(1)
    print("Camera in GameplayScene:")
    for line in b.splitlines():
        if any(k in line for k in ["orthographic", "field of view", "m_NormalizedViewPortRect"]):
            print(" ", line.strip())
