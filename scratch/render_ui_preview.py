import os, re
from PIL import Image, ImageDraw, ImageFont

def render_level_selection(scene_path, output_png):
    with open(scene_path, 'r', encoding='utf-8') as f:
        text = f.read()

    def get_rect(rtid):
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

    W_canvas, H_canvas = 1920, 1080
    im = Image.new('RGBA', (W_canvas, H_canvas), (10, 30, 60, 255))
    
    # Background
    bg_path = 'Assets/Art/Backgrounds/coral reaf background.png'
    if os.path.exists(bg_path):
        bg = Image.open(bg_path).convert('RGBA').resize((W_canvas, H_canvas), Image.Resampling.LANCZOS)
        im.paste(bg, (0, 0))

    draw = ImageDraw.Draw(im)

    # Header Logo
    logo_path = 'Assets/Art/UI/Icons/Glossy Blue Select Level Logo.png'
    if os.path.exists(logo_path):
        logo = Image.open(logo_path).convert('RGBA')
        logo_w, logo_h = int(logo.width * 0.7), int(logo.height * 0.7)
        logo = logo.resize((logo_w, logo_h), Image.Resampling.LANCZOS)
        logo_x = (W_canvas - logo_w) // 2
        logo_y = 40
        im.paste(logo, (logo_x, logo_y), logo)

    # Back button
    back_path = 'Assets/Art/UI/Buttons/button_back_arrow.png'
    if os.path.exists(back_path):
        back = Image.open(back_path).convert('RGBA').resize((90, 90), Image.Resampling.LANCZOS)
        im.paste(back, (45, 40), back)

    # Compute LevelGrid
    lg_r = get_rect('943318707')
    lg_box = compute_rect(lg_r, float(W_canvas), float(H_canvas))

    # Level Buttons
    star_path = 'Assets/Art/UI/Icons/icon_star_gold.png'
    star_img = Image.open(star_path).convert('RGBA').resize((36, 36), Image.Resampling.LANCZOS) if os.path.exists(star_path) else None

    lock_path = 'Assets/Art/UI/Icons/icon_locked.png'
    lock_img = Image.open(lock_path).convert('RGBA').resize((54, 54), Image.Resampling.LANCZOS) if os.path.exists(lock_path) else None

    level_rtids = {}
    for lvl in range(1, 13):
        m = re.search(rf'm_Name: Level_{lvl}\b', text)
        if m:
            go_start = text.rfind('--- !u!1 &', 0, m.start())
            go_block = text[go_start:m.start()+200]
            rt_m = re.search(r'- component: {fileID: (\d+)}', go_block)
            if rt_m:
                level_rtids[lvl] = rt_m.group(1)

    # Font
    try:
        font_large = ImageFont.truetype("arialbd.ttf", 68)
        font_small = ImageFont.truetype("arialbd.ttf", 24)
    except:
        font_large = font_small = ImageFont.load_default()

    for lvl in sorted(level_rtids.keys()):
        rtid = level_rtids[lvl]
        r = get_rect(rtid)
        box = compute_rect(r, lg_box['w'], lg_box['h'])
        
        # Convert to screen (Y inverted: Unity Y=0 is bottom, PIL Y=0 is top)
        cx = lg_box['left'] + box['cx']
        cy = H_canvas - (lg_box['bottom'] + box['cy'])
        w = max(10, int(box['w']))
        h = max(10, int(box['h']))
        
        x0 = int(cx - w / 2)
        y0 = int(cy - h / 2)
        x1 = x0 + w
        y1 = y0 + h

        # Draw glossy cartoon button card
        # Outer border / shadow
        draw.rounded_rectangle([x0+4, y0+6, x1+4, y1+6], radius=28, fill=(0, 20, 50, 140))
        # Outer bright border
        draw.rounded_rectangle([x0, y0, x1, y1], radius=28, fill=(15, 140, 220, 240), outline=(130, 230, 255, 255), width=4)
        # Inner gloss body
        draw.rounded_rectangle([x0+6, y0+6, x1-6, y1-6], radius=22, fill=(25, 90, 175, 240))
        # Top highlight
        draw.rounded_rectangle([x0+10, y0+8, x1-10, y0+int(h*0.42)], radius=18, fill=(70, 160, 240, 160))

        # Level number text
        num_str = str(lvl)
        # Draw number
        draw.text((cx, cy - 20), num_str, fill=(255, 255, 255, 255), font=font_large, anchor="mm")

        # 3 Stars container underneath
        star_y = y1 - 42
        for s_idx, s_off in enumerate([-42, 0, 42]):
            sx = int(cx + s_off - 18)
            sy = int(star_y - 18)
            if lvl == 1 and star_img:
                # Level 1 has stars
                im.paste(star_img, (sx, sy), star_img)
            else:
                # Unearned star outline
                draw.ellipse([sx+4, sy+4, sx+32, sy+32], fill=(20, 50, 90, 180), outline=(80, 140, 200, 200), width=2)

        # Locked overlay for Level > 1
        if lvl > 1 and lock_img:
            lx = int(cx - 27)
            ly = int(cy - 68)
            im.paste(lock_img, (lx, ly), lock_img)

    os.makedirs(os.path.dirname(output_png), exist_ok=True)
    im.save(output_png)
    print(f"Saved rendering to {output_png} ({os.path.getsize(output_png)} bytes)")

render_level_selection('Assets/Scenes/LevelSelectionScene.unity', 'scratch/level_selection_before.png')
