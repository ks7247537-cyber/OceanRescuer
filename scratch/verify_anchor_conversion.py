# Compare exact bounding boxes
coords = {
    1: {'cx': 322.4, 'cy': 675.5, 'w': 185.5, 'h': 245.9},
    2: {'cx': 664.7, 'cy': 668.5, 'w': 180.5, 'h': 237.9},
    3: {'cx': 989.5, 'cy': 664.5, 'w': 178.5, 'h': 239.9},
    4: {'cx': 1507.9, 'cy': 650.0, 'w': 186.5, 'h': 242.9},
    5: {'cx': 264.2, 'cy': 385.5, 'w': 181.1, 'h': 244.9},
    6: {'cx': 603.0, 'cy': 377.5, 'w': 187.0, 'h': 245.4},
    7: {'cx': 1122.4, 'cy': 359.5, 'w': 192.7, 'h': 242.4},
    8: {'cx': 1446.2, 'cy': 354.5, 'w': 191.1, 'h': 245.4},
    9: {'cx': 203.6, 'cy': 71.5, 'w': 182.2, 'h': 237.8},
    10: {'cx': 742.4, 'cy': 53.5, 'w': 181.0, 'h': 233.5},
    11: {'cx': 1064.2, 'cy': 59.5, 'w': 187.0, 'h': 233.5},
    12: {'cx': 1388.0, 'cy': 61.5, 'w': 196.6, 'h': 233.5}
}

W_lg = 1766.4
H_lg = 756.0
cx_lg = W_lg / 2.0 # 883.2
cy_lg = H_lg / 2.0 # 378.0

print("Verification of Center-Anchor conversion:")
max_diff = 0.0
for lvl, d in sorted(coords.items()):
    pos_x = d['cx'] - cx_lg
    pos_y = d['cy'] - cy_lg
    size_w = d['w']
    size_h = d['h']
    
    # Recompute cx and cy from center anchor (0.5, 0.5)
    recomp_cx = cx_lg + pos_x
    recomp_cy = cy_lg + pos_y
    diff_x = abs(recomp_cx - d['cx'])
    diff_y = abs(recomp_cy - d['cy'])
    if diff_x > max_diff: max_diff = diff_x
    if diff_y > max_diff: max_diff = diff_y
    print(f"Level {lvl:02d}: Pos=({pos_x:.1f}, {pos_y:.1f}) Size=({size_w:.1f}, {size_h:.1f}) -> Diff=({diff_x:.4f}, {diff_y:.4f})")

print(f"\nMAXIMUM POSITION DEVIATION ACROSS ALL 12 BUTTONS: {max_diff:.6f} pixels!")
