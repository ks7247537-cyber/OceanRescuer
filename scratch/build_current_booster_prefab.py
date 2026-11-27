import os

def generate_current_booster_prefab():
    booster_script_guid = 'd56e14064c464fd18c038978480e4bad'
    base_sprite_guid    = '55d3222c578b6fd4ea91367c711b307b'
    base_sprite_id      = '-5353219563556483298'
    arrow_sprite_guid   = '8672d7e301c67f84c9b9b84c640c06d8'
    arrow_sprite_id     = '-2671245110766956269'
    flow_sprite_guid    = '43349bb6fcd954a2676f97f282638e5a'
    flow_sprite_id      = '5549887748833783432'
    font_guid           = '8f586378b4e144a9851e7b34d9b748ee'

    yaml = []
    yaml.append('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:')

    # 1. Root GameObject
    yaml.append('''--- !u!1 &5000000001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 5000000002}
  - component: {fileID: 5000000003}
  - component: {fileID: 5000000004}
  m_Layer: 0
  m_Name: CurrentBooster
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &5000000002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 5000000001}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 0.85, y: 0.85, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 5000000012}
  - {fileID: 5000000022}
  - {fileID: 5000000032}
  - {fileID: 5000000042}
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!61 &5000000003
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 5000000001}
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
  m_IsTrigger: 1
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

    # CurrentBooster MonoBehaviour
    yaml.append(f'''--- !u!114 &5000000004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000001}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {booster_script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  gridPos: {{x: 0, y: 0}}
  boostDirection: 1
  pressureRelief: 15
  basePlateColor: {{r: 0.12, g: 0.4, b: 0.65, a: 0.9}}
  indicatorIdleColor: {{r: 0.2, g: 0.8, b: 1, a: 0.7}}
  indicatorActiveColor: {{r: 0, g: 1, b: 1, a: 1}}
  flowEffectColor: {{r: 0.15, g: 1, b: 0.95, a: 0.85}}
  boosterBase: {{fileID: 5000000013}}
  directionIndicator: {{fileID: 5000000023}}
  flowEffect: {{fileID: 5000000033}}
  feedbackText: {{fileID: 5000000043}}
  _isBoosting: 0''')

    # 2. BoosterBase
    yaml.append(f'''--- !u!1 &5000000011
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 5000000012}}
  - component: {{fileID: 5000000013}}
  m_Layer: 0
  m_Name: BoosterBase
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &5000000012
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000011}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 5000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &5000000013
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000011}}
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
  m_SortingOrder: 4
  m_Sprite: {{fileID: {base_sprite_id}, guid: {base_sprite_guid}, type: 3}}
  m_Color: {{r: 0.12, g: 0.4, b: 0.65, a: 0.9}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 3. CurrentDirectionIndicator
    yaml.append(f'''--- !u!1 &5000000021
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 5000000022}}
  - component: {{fileID: 5000000023}}
  m_Layer: 0
  m_Name: CurrentDirectionIndicator
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &5000000022
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000021}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -0.05}}
  m_LocalScale: {{x: 0.75, y: 0.75, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 5000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &5000000023
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000021}}
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
  m_Sprite: {{fileID: {arrow_sprite_id}, guid: {arrow_sprite_guid}, type: 3}}
  m_Color: {{r: 0.2, g: 0.8, b: 1, a: 0.7}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 4. FlowEffect
    yaml.append(f'''--- !u!1 &5000000031
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 5000000032}}
  - component: {{fileID: 5000000033}}
  m_Layer: 0
  m_Name: FlowEffect
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!4 &5000000032
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000031}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -0.1}}
  m_LocalScale: {{x: 0.8, y: 0.8, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 5000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &5000000033
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000031}}
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
  m_Sprite: {{fileID: {flow_sprite_id}, guid: {flow_sprite_guid}, type: 3}}
  m_Color: {{r: 0.15, g: 1, b: 0.95, a: 0.85}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0''')

    # 5. InteractionFeedback
    yaml.append(f'''--- !u!1 &5000000041
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 5000000042}}
  - component: {{fileID: 5000000043}}
  - component: {{fileID: 5000000044}}
  m_Layer: 0
  m_Name: InteractionFeedback
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!4 &5000000042
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000041}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0.95, z: -0.2}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 5000000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &5000000043
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000041}}
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
--- !u!23 &5000000044
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 5000000041}}
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
  m_SortingOrder: 20''')

    return '\n'.join(yaml) + '\n'

if __name__ == '__main__':
    yaml_content = generate_current_booster_prefab()
    os.makedirs('Assets/Prefab/Interactive', exist_ok=True)
    os.makedirs('Assets/Resources/Prefabs', exist_ok=True)

    with open('Assets/Prefab/Interactive/CurrentBooster.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    with open('Assets/Resources/Prefabs/CurrentBooster.prefab', 'w', encoding='utf-8') as f:
        f.write(yaml_content)

    print('CurrentBooster prefabs generated successfully in Assets/Prefab/Interactive and Assets/Resources/Prefabs!')
