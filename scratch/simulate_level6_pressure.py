# Simulate DeepOcean Level 6 Pressure Calculation

# Level 6 config:
# levelIndex = 5 (Level 6 is 0-indexed 5)
levelIndex = 5
basePressure = 20.0 + min(levelIndex * 1.5, 20.0) # 20 + 7.5 = 27.5
pressurePerPipe = 1.5
pressurePerBend = 2.0
pressurePerObstruction = 8.0

# In Level 6, the path from source (0,1) to gate (5,2):
# (0,1) -> (1,1) -> (1,2) -> (1,3) -> (2,3) -> (3,3) -> (4,3) -> (5,3) -> (5,2)
# That is 8 connected pipes:
connectedCount = 8
# Bends on this path: (1,1), (1,3), (5,3) -> 3 bends
bendCount = 3
# Has obstruction: gate at (5,2) is closed and neighbors (5,3) -> True
hasObstruction = True

rawPressure = basePressure + (connectedCount * pressurePerPipe) + (bendCount * pressurePerBend) + (pressurePerObstruction if hasObstruction else 0)
print(f"Base pressure: {basePressure}")
print(f"Connected pipes: {connectedCount} * 1.5 = {connectedCount * 1.5}")
print(f"Bends: {bendCount} * 2.0 = {bendCount * 2.0}")
print(f"Obstruction: 8.0")
print(f"Total raw pressure: {rawPressure} PSI")

minActivationPressure = 40.0
maxActivationPressure = 78.0
print(f"Required window: {minActivationPressure} - {maxActivationPressure} PSI")
print(f"Is in range? {minActivationPressure <= rawPressure <= maxActivationPressure}")
