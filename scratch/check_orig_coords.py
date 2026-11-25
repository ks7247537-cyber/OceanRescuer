import re

with open('scratch/backup_landscape_step1/LevelSelectionScene.unity', 'r', encoding='utf-8') as f:
    text_orig = f.read()

def get_rect(text, rtid):
    m = re.search(rf'--- !u!224 &{rtid}\nRectTransform:(.*?)(?=\n---|\Z)', text, re.DOTALL)
    if not m: return None
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
    
    left = cx - r['pivot'][0] * w
    bottom = cy - r['pivot'][1] * h
    return {
        'cx': cx, 'cy': cy, 'w': w, 'h': h,
        'left': left, 'bottom': bottom,
        'right': left + w, 'top': bottom + h
    }

# Under 1920x1080 screen with 1080x1920 reference and Match 0.5:
# As calculated earlier, scale factor = 1.0!
# So screen pixels = canvas pixels = 1920 x 1080!
lg_r = get_rect(text_orig, '943318707')
lg_box = compute_rect(lg_r, 1920.0, 1080.0)

level_rtids = {}
for lvl in range(1, 13):
    m = re.search(rf'm_Name: Level_{lvl}\b', text_orig)
    if m:
        go_start = text_orig.rfind('--- !u!1 &', 0, m.start())
        go_block = text_orig[go_start:m.start()+200]
        rt_m = re.search(r'- component: {fileID: (\d+)}', go_block)
        if rt_m:
            level_rtids[lvl] = rt_m.group(1)

print("Original Level Positions at 1920x1080:")
for lvl in sorted(level_rtids.keys()):
    rtid = level_rtids[lvl]
    r = get_rect(text_orig, rtid)
    box = compute_rect(r, lg_box['w'], lg_box['h'])
    print(f"Level {lvl:02d}: Center=({box['cx']:.1f}, {box['cy']:.1f}) Size=({box['w']:.1f}, {box['h']:.1f})")
