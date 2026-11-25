import re

scenes = [
    'SplashScene',
    'MainMenuScene',
    'WorldSelectionScene',
    'LevelSelectionScene',
    'GameplayScene'
]

scaler_guid = '0cd44c1031e13a943bb63640046fad76'

for s in scenes:
    path = f'Assets/Scenes/{s}.unity'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    print(f'=== Scene: {s} ===')
    
    # Canvases
    canvas_blocks = re.findall(r'--- !u!223 &(\d+)\nCanvas:(.*?)(?=\n---|\Z)', content, re.DOTALL)
    for c_id, cb in canvas_blocks:
        rm = re.search(r'm_RenderMode: (\d+)', cb)
        rm_val = rm.group(1) if rm else '?'
        rm_name = {'0': 'ScreenSpaceOverlay', '1': 'ScreenSpaceCamera', '2': 'WorldSpace'}.get(rm_val, 'Unknown')
        print(f'  Canvas &{c_id}: RenderMode = {rm_name} ({rm_val})')

    # CanvasScalers
    scalers = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:(.*?guid: ' + scaler_guid + r'.*?)(?=\n---|\Z)', content, re.DOTALL)
    print(f'  CanvasScaler count: {len(scalers)}')
    for sc_id, sb in scalers:
        ui_mode = re.search(r'm_UiScaleMode: (\d+)', sb)
        mode_val = ui_mode.group(1) if ui_mode else '?'
        mode_str = {'0': 'ConstantPixelSize', '1': 'ScaleWithScreenSize', '2': 'ConstantPhysicalSize'}.get(mode_val, 'Unknown')
        
        ref_res = re.search(r'm_ReferenceResolution: {x: ([\d.]+), y: ([\d.]+)}', sb)
        match_mode = re.search(r'm_ScreenMatchMode: (\d+)', sb)
        mm_val = match_mode.group(1) if match_mode else '?'
        mm_str = {'0': 'MatchWidthOrHeight', '1': 'Expand', '2': 'Shrink'}.get(mm_val, 'Unknown')
        match_val = re.search(r'm_MatchWidthOrHeight: ([\d.]+)', sb)
        
        print(f'  CanvasScaler &{sc_id}:')
        print(f'    UI Scale Mode: {mode_str} ({mode_val})')
        if ref_res:
            rx, ry = float(ref_res.group(1)), float(ref_res.group(2))
            is_landscape = rx > ry
            print(f'    Reference Resolution: {rx} x {ry} ({"Landscape" if is_landscape else "PORTRAIT!"})')
        else:
            print(f'    Reference Resolution: None')
        print(f'    Screen Match Mode: {mm_str} ({mm_val})')
        print(f'    Match Width Or Height: {match_val.group(1) if match_val else "None"}')
    print()
