import re

resolutions = [
    ("16:9 Standard", 1920, 1080),
    ("18:9 Modern", 2160, 1080),
    ("19.5:9 Ultrawide", 2340, 1080),
    ("20:9 Extra Long", 2400, 1080),
    ("16:10 Tablet", 1920, 1200),
]

def canvas_scale(screen_w, screen_h, ref_w=1920, ref_h=1080, match=0.5):
    import math
    log_w = math.log2(screen_w / ref_w)
    log_h = math.log2(screen_h / ref_h)
    log_avg = log_w * (1 - match) + log_h * match
    scale = 2 ** log_avg
    return screen_w / scale, screen_h / scale

# Check Level 4 and Level 9 in new scene file
with open('Assets/Scenes/LevelSelectionScene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

def get_rect(rtid):
    m = re.search(rf'--- !u!224 &{rtid}\nRectTransform:(.*?)(?=\n---|\Z)', text, re.DOTALL)
    b = m.group(1)
    amin = re.search(r'm_AnchorMin: {x: ([\d.-]+), y: ([\d.-]+)}', b)
    amax = re.search(r'm_AnchorMax: {x: ([\d.-]+), y: ([\d.-]+)}', b)
    pos = re.search(r'm_AnchoredPosition: {x: ([\d.-]+), y: ([\d.-]+)}', b)
    size = re.search(r'm_SizeDelta: {x: ([\d.-]+), y: ([\d.-]+)}', b)
    pivot = re.search(r'm_Pivot: {x: ([\d.-]+), y: ([\d.-]+)}', b)
    return {
        'amin': (float(amin.group(1)), float(amin.group(2))),
        'amax': (float(amax.group(1)), float(amax.group(2))),
        'pos': (float(pos.group(1)), float(pos.group(2))),
        'size': (float(size.group(1)), float(size.group(2))),
        'pivot': (float(pivot.group(1)), float(pivot.group(2)))
    }

def compute_rect(r, W_p, H_p):
    x_a1 = r['amin'][0] * W_p
    x_a2 = r['amax'][0] * W_p
    y_a1 = r['amin'][1] * H_p
    y_a2 = r['amax'][1] * H_p
    w = (x_a2 - x_a1) + r['size'][0]
    h = (y_a2 - y_a1) + r['size'][1]
    cx = (x_a1 + (x_a2 - x_a1) * r['pivot'][0]) + r['pos'][0]
    cy = (y_a1 + (y_a2 - y_a1) * r['pivot'][1]) + r['pos'][1]
    return cx, cy, w, h

print("=== NEW RESPONSIVE BEHAVIOR ACROSS MOBILE RESOLUTIONS ===")
for name, sw, sh in resolutions:
    cw, ch = canvas_scale(sw, sh)
    lg_r = get_rect('943318707') # LevelGrid
    lg_cx, lg_cy, lg_w, lg_h = compute_rect(lg_r, cw, ch)
    
    # Check Level 1, 4, 9, 12
    l1_r = get_rect('1087879200')
    l4_r = get_rect('1403391770')
    l9_r = get_rect('909502708')
    l12_r = get_rect('1143820066')
    
    _, _, l1_w, l1_h = compute_rect(l1_r, lg_w, lg_h)
    _, _, l4_w, l4_h = compute_rect(l4_r, lg_w, lg_h)
    
    # Grid margins on screen
    left_margin = (cw - lg_w) / 2
    print(f"\n{name} ({sw}x{sh}): Canvas = {cw:.1f} x {ch:.1f}")
    print(f"  LevelGrid: Size = {lg_w:.1f} x {lg_h:.1f} | Side Margins = {left_margin:.1f}px")
    print(f"  Level 1 Button Size: {l1_w:.1f} x {l1_h:.1f} (Fixed crisp aspect ratio, NO distortion)")
    print(f"  Level 4 Button Size: {l4_w:.1f} x {l4_h:.1f} (Fixed crisp aspect ratio, NO distortion)")
