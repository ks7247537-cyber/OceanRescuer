# Simulate CanvasScaler behavior for different resolutions
resolutions = [
    ("16:9 Standard", 1920, 1080),
    ("18:9 Modern", 2160, 1080),
    ("19.5:9 Ultrawide / Notch", 2340, 1080),
    ("20:9 Extra Long", 2400, 1080),
    ("16:10 Android Tablet", 1920, 1200),
    ("4:3 Tablet Landscape", 2048, 1536)
]

def canvas_scale(screen_w, screen_h, ref_w, ref_h, match):
    # Unity formula:
    # logWidth = log2(screen_w / ref_w)
    # logHeight = log2(screen_h / ref_h)
    # logWeightedAverage = lerp(logWidth, logHeight, match)
    # scaleFactor = 2 ^ logWeightedAverage
    import math
    log_w = math.log2(screen_w / ref_w)
    log_h = math.log2(screen_h / ref_h)
    log_avg = log_w * (1 - match) + log_h * match
    scale = 2 ** log_avg
    canvas_w = screen_w / scale
    canvas_h = screen_h / scale
    return scale, canvas_w, canvas_h

print("=== SCENARIO 1: GameplayScene (Ref: 1920 x 1080, Match: 1.0 - Height) ===")
for name, sw, sh in resolutions:
    scale, cw, ch = canvas_scale(sw, sh, 1920, 1080, 1.0)
    print(f"{name} ({sw}x{sh}): Scale={scale:.3f} -> Canvas={cw:.1f} x {ch:.1f} (Aspect={cw/ch:.2f})")

print("\n=== SCENARIO 2: MainMenuScene (Ref: 1920 x 1080, Match: 0.5 - Balanced) ===")
for name, sw, sh in resolutions:
    scale, cw, ch = canvas_scale(sw, sh, 1920, 1080, 0.5)
    print(f"{name} ({sw}x{sh}): Scale={scale:.3f} -> Canvas={cw:.1f} x {ch:.1f} (Aspect={cw/ch:.2f})")

print("\n=== SCENARIO 3: Splash, WorldSelection, LevelSelection (INVERTED Ref: 1080 x 1920, Match: 0.5) ===")
for name, sw, sh in resolutions:
    scale, cw, ch = canvas_scale(sw, sh, 1080, 1920, 0.5)
    print(f"{name} ({sw}x{sh}): Scale={scale:.3f} -> Canvas={cw:.1f} x {ch:.1f} (Aspect={cw/ch:.2f})")
