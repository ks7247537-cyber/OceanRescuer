import re

with open('Assets/Scenes/LevelSelectionScene.unity', 'r', encoding='utf-8') as f:
    text_now = f.read()

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

# LevelGrid RT ID is 943318707
lg_r = get_rect(text_now, '943318707')
canvas_w, canvas_h = 1920.0, 1080.0
lg_box = compute_rect(lg_r, canvas_w, canvas_h)
print(f"LevelGrid on Canvas 1920x1080: W={lg_box['w']:.1f}, H={lg_box['h']:.1f}, Left={lg_box['left']:.1f}, Bottom={lg_box['bottom']:.1f}")

level_rtids = {}
for lvl in range(1, 13):
    m = re.search(rf'm_Name: Level_{lvl}\b', text_now)
    if m:
        go_start = text_now.rfind('--- !u!1 &', 0, m.start())
        go_block = text_now[go_start:m.start()+200]
        rt_m = re.search(r'- component: {fileID: (\d+)}', go_block)
        if rt_m:
            level_rtids[lvl] = rt_m.group(1)

print("\n--- Level Button Positions inside LevelGrid (1766.4 x 756.0) ---")
for lvl in sorted(level_rtids.keys()):
    rtid = level_rtids[lvl]
    r = get_rect(text_now, rtid)
    box = compute_rect(r, lg_box['w'], lg_box['h'])
    canvas_cx = lg_box['left'] + box['cx']
    canvas_cy = lg_box['bottom'] + box['cy']
    print(f"Level {lvl:02d} (RT &{rtid}): GridCenter=({box['cx']:.1f}, {box['cy']:.1f}) Size=({box['w']:.1f}, {box['h']:.1f}) | CanvasCenter=({canvas_cx:.1f}, {canvas_cy:.1f})")
