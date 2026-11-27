import os

def generate_pressure_lock_gate_prefab():
    gate_script_guid   = '019c623b11934d4f90094e8169ca76f2'
    base_sprite_guid   = 'd55069bb692dec8825dc48f05204e689'
    base_sprite_id     = '6892959146073744609'
    bars_sprite_guid   = '8991d06e963cf1d68abc3d808b3a1a1f'
    bars_sprite_id     = '-6148210024504244235'
    bubbles_sprite_guid= '9c065ebcc4144aa599b25bc5db80eac5'
    bubbles_sprite_id  = '1649192393020575799'
    glow_sprite_guid   = '43349bb6fcd954a2676f97f282638e5a'
    glow_sprite_id     = '5549887748833783432'
    font_guid          = '8f586378b4e144a9851e7b34d9b748ee'

    yaml = []
    yaml.append('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:')

    # 1. Root GameObject
    yaml.append('''--- !u!1 &7000000001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 7000000002}
  - component: {fileID: 7000000003}
  - component: {fileID: 7000000004}
  m_Layer: 0
  m_Name: PressureLockGate
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7000000002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 7000000001}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 0.42, y: 0.42, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 7000000012}
  - {fileID: 7000000022}
  - {fileID: 7000000032}
  - {fileID: 7000000042}
  - {fileID: 7000000052}
  - {fileID: 7000000062}
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &7000000003
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 7000000001}
  m_Enabled: 1
  m_Density: 1
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_UsedByComposite: 0
  m_Offset: {x: 0, y: 0}
  m_SpriteTilingProperty:
    border: {x: 0, y: 0, z: 0, w: 0}
    pivot: {x: 0, y: 0}
    oldSize: {x: 0, y: 0}
    newSize: {x: 0, y: 0}
    adaptiveTilingThreshold: 0
    maxElementAdaptiveCutOff: 0
    scale: {x: 0, y: 0}
  m_AutoTiling: 0
  m_Size: {x: 0.9, y: 0.9}
  m_EdgeRadius: 0''')

    # PressureLockGate MonoBehaviour
    yaml.append(f'''--- !u!114 &7000000004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000001}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {gate_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  gateType: 5
  gridPos: {{x: 0, y: 0}}
  isOpen: 0
  requiredPressure: 5
  linkedValvePos: {{x: -1, y: -1}}
  barrierRenderer: {{fileID: 7000000023}}
  badgeRenderer: {{fileID: 7000000013}}
  lockedSprite: {{fileID: 0}}
  unlockedSprite: {{fileID: 0}}
  minActivationPressure: 40
  maxActivationPressure: 78
  requiredStableDuration: 1
  lockedBaseColor: {{r: 0.25, g: 0.38, b: 0.48, a: 1}}
  lockedBarsColor: {{r: 0.95, g: 0.75, b: 0.25, a: 1}}
  activatingGlowColor: {{r: 0, g: 1, b: 1, a: 0.95}}
  overpressureColor: {{r: 1, g: 0.25, b: 0.2, a: 1}}
  unlockedColor: {{r: 0.2, g: 1, b: 0.5, a: 0.25}}
  gateBase: {{fileID: 7000000013}}
  gateBars: {{fileID: 7000000023}}
  pressureGauge: {{fileID: 7000000033}}
  hydraulicGlow: {{fileID: 7000000043}}
  statusBadgeText: {{fileID: 7000000053}}
  feedbackText: {{fileID: 7000000063}}''')

    # 2. GateBase
    yaml.append(f'''--- !u!1 &7000000011
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000012}}
  - component: {{fileID: 7000000013}}
  m_Layer: 0
  m_Name: GateBase
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7000000012
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000011}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &7000000013
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000011}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 6
  m_Sprite: {{fileID: {base_sprite_id}, guid: {base_sprite_guid}, type: 3}}
  m_Color: {{r: 0.25, g: 0.38, b: 0.48, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 3. GateBars
    yaml.append(f'''--- !u!1 &7000000021
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000022}}
  - component: {{fileID: 7000000023}}
  m_Layer: 0
  m_Name: GateBars
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7000000022
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000021}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -0.05}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &7000000023
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000021}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 7
  m_Sprite: {{fileID: {bars_sprite_id}, guid: {bars_sprite_guid}, type: 3}}
  m_Color: {{r: 0.95, g: 0.75, b: 0.25, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 4. PressureGauge
    yaml.append(f'''--- !u!1 &7000000031
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000032}}
  - component: {{fileID: 7000000033}}
  m_Layer: 0
  m_Name: PressureGauge
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7000000032
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000031}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: -0.35, z: -0.1}}
  m_LocalScale: {{x: 0.6, y: 0.6, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &7000000033
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000031}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 8
  m_Sprite: {{fileID: {bubbles_sprite_id}, guid: {bubbles_sprite_guid}, type: 3}}
  m_Color: {{r: 0, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 5. HydraulicGlow
    yaml.append(f'''--- !u!1 &7000000041
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000042}}
  - component: {{fileID: 7000000043}}
  m_Layer: 0
  m_Name: HydraulicGlow
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!4 &7000000042
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000041}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -0.12}}
  m_LocalScale: {{x: 0.9, y: 0.9, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &7000000043
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000041}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 9
  m_Sprite: {{fileID: {glow_sprite_id}, guid: {glow_sprite_guid}, type: 3}}
  m_Color: {{r: 0, g: 1, b: 1, a: 0.95}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 6. StatusBadgeText
    yaml.append(f'''--- !u!1 &7000000051
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000052}}
  - component: {{fileID: 7000000053}}
  - component: {{fileID: 7000000054}}
  m_Layer: 0
  m_Name: StatusBadgeText
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7000000052
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000051}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: -0.65, z: -0.15}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &7000000053
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000051}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 9541d86e2fd84c1d9990ee0852d2ae50, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: 40-78 PSI
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {font_guid}, type: 2}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 2
  m_fontSizeBase: 2
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 1.5
  m_fontSizeMax: 3
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 0
--- !u!23 &7000000054
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000051}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: {font_guid}, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_SortingOrder: 10''')

    # 7. InteractionFeedback
    yaml.append(f'''--- !u!1 &7000000061
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 7000000062}}
  - component: {{fileID: 7000000063}}
  - component: {{fileID: 7000000064}}
  m_Layer: 0
  m_Name: InteractionFeedback
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!4 &7000000062
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000061}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0.95, z: -0.2}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 7000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &7000000063
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000061}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 9541d86e2fd84c1d9990ee0852d2ae50, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: ''
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {font_guid}, type: 2}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: 2.4
  m_fontSizeBase: 2.4
  m_fontWeight: 700
  m_enableAutoSizing: 0
  m_fontSizeMin: 1.5
  m_fontSizeMax: 4
  m_fontStyle: 1
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 0
--- !u!23 &7000000064
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7000000061}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: {font_guid}, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_SortingOrder: 22''')

    return '\n'.join(yaml) + '\n'

if __name__ == '__main__':
    yaml_content = generate_pressure_lock_gate_prefab()
    os.makedirs('Assets/Prefab/Interactive', exist_ok=True)
    os.makedirs('Assets/Resources/Prefabs', exist_ok=True)

    with open('Assets/Prefab/Interactive/PressureLockGate.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    with open('Assets/Resources/Prefabs/PressureLockGate.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    print('PressureLockGate prefabs generated successfully!')
