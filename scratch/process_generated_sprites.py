import os
import shutil
from PIL import Image, ImageOps

source_dir = r"C:\Users\khush\.gemini\antigravity\brain\6bc7d02b-2bac-425d-858a-b96f7499eae9"
target_dir = r"d:\UnityFolder\OceanRescuer\Assets\Art\UI\Generated"
os.makedirs(target_dir, exist_ok=True)

files = {
    "sonar_scan_icon.png": "sonar_scan_icon_1795716458337.jpg",
    "pressure_gauge_bg.png": "pressure_gauge_bg_1795716494426.jpg",
    "ability_badge_frame.png": "ability_badge_frame_1795716552228.jpg"
}

def remove_dark_background(img_path, out_path, threshold=25, softness=40):
    img = Image.open(img_path).convert("RGBA")
    datas = img.getdata()
    
    new_data = []
    for item in datas:
        # brightness
        r, g, b, a = item
        brightness = (r + g + b) / 3.0
        if brightness < threshold:
            new_data.append((r, g, b, 0))
        elif brightness < threshold + softness:
            alpha = int(255 * (brightness - threshold) / softness)
            new_data.append((r, g, b, alpha))
        else:
            new_data.append((r, g, b, 255))
            
    img.putdata(new_data)
    img.save(out_path, "PNG")
    print(f"Processed and saved: {out_path}")

for out_name, src_name in files.items():
    src_path = os.path.join(source_dir, src_name)
    out_path = os.path.join(target_dir, out_name)
    if os.path.exists(src_path):
        remove_dark_background(src_path, out_path)
    else:
        print(f"Warning: source not found {src_path}")
