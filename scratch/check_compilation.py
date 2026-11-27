import os
import re
import subprocess

# 1. Parse csproj to find all CS files and references
with open('Assembly-CSharp.csproj', 'r', encoding='utf-8') as f:
    text = f.read()

compiles = re.findall(r'<Compile Include="([^"]+)"', text)
refs = re.findall(r'<HintPath>([^<]+)</HintPath>', text)

# Also collect all .cs files in Assets
cs_files = []
for root, dirs, files in os.walk('Assets'):
    for file in files:
        if file.endswith('.cs'):
            cs_files.append(os.path.join(root, file))

print(f"Total .cs files in Assets: {len(cs_files)}")
print(f"Total HintPath references: {len(refs)}")

# Let's write an rsp file to test compiling with Unity's Roslyn csc.dll
rsp_lines = [
    "-target:library",
    "-out:Temp/Assembly-CSharp-Test.dll",
    "-langversion:9.0",
    "-nullable:disable",
    "-nowarn:0169,USG0001",
]

# Standard netstandard reference from Unity Editor
netstandard_dll = r"C:\Program Files\Unity\Hub\Editor\6000.1.17f1\Editor\Data\NetStandard\compat\2.1.0\shims\netstandard\netstandard.dll"
if os.path.exists(netstandard_dll):
    rsp_lines.append(f'-r:"{netstandard_dll}"')

for r in refs:
    if os.path.exists(r):
        rsp_lines.append(f'-r:"{r}"')

for cs in cs_files:
    rsp_lines.append(f'"{cs}"')

with open("Temp/test_compile.rsp", "w", encoding="utf-8") as f:
    f.write("\n".join(rsp_lines))

dotnet = r"C:\Program Files\Unity\Hub\Editor\6000.1.17f1\Editor\Data\NetCoreRuntime\dotnet.exe"
csc = r"C:\Program Files\Unity\Hub\Editor\6000.1.17f1\Editor\Data\DotNetSdkRoslyn\csc.dll"

res = subprocess.run([dotnet, csc, "@Temp/test_compile.rsp"], capture_output=True, text=True)
print("Return code:", res.returncode)
if res.stdout:
    print("STDOUT:\n", res.stdout[:2000])
if res.stderr:
    print("STDERR:\n", res.stderr[:2000])
