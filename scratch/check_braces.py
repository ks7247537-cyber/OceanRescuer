import glob

def check_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()
    # Simple brace balance check ignoring strings and comments
    depth = 0
    i = 0
    n = len(text)
    in_line_comment = False
    in_block_comment = False
    in_string = False
    in_char = False
    
    while i < n:
        c = text[i]
        c2 = text[i:i+2]
        
        if in_line_comment:
            if c == '\n':
                in_line_comment = False
        elif in_block_comment:
            if c2 == '*/':
                in_block_comment = False
                i += 1
        elif in_string:
            if c2 == '\\"':
                i += 1
            elif c == '"':
                in_string = False
        elif in_char:
            if c2 == "\\'":
                i += 1
            elif c == "'":
                in_char = False
        else:
            if c2 == '//':
                in_line_comment = True
                i += 1
            elif c2 == '/*':
                in_block_comment = True
                i += 1
            elif c == '"':
                in_string = True
            elif c == "'":
                in_char = True
            elif c == '{':
                depth += 1
            elif c == '}':
                depth -= 1
                if depth < 0:
                    return f"{path} - negative depth (extra '}}')"
        i += 1
        
    if depth != 0:
        return f"{path} - unbalanced depth: {depth}"
    return None

files = glob.glob('Assets/Script/**/*.cs', recursive=True)
errs = []
for p in files:
    res = check_file(p)
    if res:
        errs.append(res)

if errs:
    print("Brace balance ERRORS:")
    for e in errs:
        print(" ", e)
else:
    print(f"All {len(files)} C# files have perfectly balanced braces!")
