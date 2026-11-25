import re

scenes = [
    'SplashScene',
    'MainMenuScene',
    'WorldSelectionScene',
    'LevelSelectionScene',
    'GameplayScene'
]

for s in scenes:
    path = f'Assets/Scenes/{s}.unity'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    print(f'=== Scene: {s} ===')
    
    # Canvases
    canvas_blocks = re.findall(r'Canvas:\n(.*?)(?=\n---|\Z)', content, re.DOTALL)
    print(f'Canvas count: {len(canvas_blocks)}')
    for i, cb in enumerate(canvas_blocks):
        rm = re.search(r'm_RenderMode: (\d+)', cb)
        camera = re.search(r'm_Camera: {fileID: (\d+)}', cb)
        sort_order = re.search(r'm_SortingOrder: (\d+)', cb)
        rm_val = rm.group(1) if rm else '?'
        rm_name = {'0': 'ScreenSpaceOverlay', '1': 'ScreenSpaceCamera', '2': 'WorldSpace'}.get(rm_val, 'Unknown')
        cam_val = camera.group(1) if camera else '0'
        so_val = sort_order.group(1) if sort_order else '0'
        print(f'  Canvas #{i+1}: RenderMode={rm_name} ({rm_val}), CameraID={cam_val}, SortingOrder={so_val}')
        
    # CanvasScalers (guid dc42784cf147c0c48a680349fa168899)
    scaler_blocks = re.findall(r'm_Script: {fileID: 11500000, guid: dc42784cf147c0c48a680349fa168899, type: 3}\n(.*?)(?=\n---|\Z)', content, re.DOTALL)
    print(f'CanvasScaler count: {len(scaler_blocks)}')
    for i, sb in enumerate(scaler_blocks):
        scale_mode = re.search(r'm_UiScaleMode: (\d+)', sb)
        ref_x = re.search(r'm_ReferenceResolution: {x: ([\d.]+), y: ([\d.]+)}', sb)
        match_mode = re.search(r'm_ScreenMatchMode: (\d+)', sb)
        match_val = re.search(r'm_MatchWidthOrHeight: ([\d.]+)', sb)
        
        sm_val = scale_mode.group(1) if scale_mode else '?'
        mode_str = {'0': 'ConstantPixelSize', '1': 'ScaleWithScreenSize', '2': 'ConstantPhysicalSize'}.get(sm_val, 'Unknown')
        mm_val = match_mode.group(1) if match_mode else '?'
        mm_str = {'0': 'MatchWidthOrHeight', '1': 'Expand', '2': 'Shrink'}.get(mm_val, 'Unknown')
        
        print(f'  CanvasScaler #{i+1}:')
        print(f'    ScaleMode: {mode_str} ({sm_val})')
        if ref_x:
            print(f'    ReferenceResolution: {ref_x.group(1)} x {ref_x.group(2)}')
        else:
            print('    ReferenceResolution: Not set')
        print(f'    ScreenMatchMode: {mm_str} ({mm_val})')
        print(f'    MatchWidthOrHeight: {match_val.group(1) if match_val else "Not set"}')
    print()
