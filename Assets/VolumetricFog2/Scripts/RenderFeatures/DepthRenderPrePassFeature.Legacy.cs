using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if !UNITY_6000_4_OR_NEWER
namespace VolumetricFogAndMist2 {

    public partial class DepthRenderPrePassFeature {

        public partial class DepthRenderPass {

            public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                if (transparentLayerMask != filterSettings.layerMask || alphaCutoutLayerMask != currentCutoutLayerMask) {
                    filterSettings = new FilteringSettings(RenderQueueRange.transparent, transparentLayerMask);
                    currentCutoutLayerMask = alphaCutoutLayerMask;
                    SetupKeywords();
                }
                RenderTextureDescriptor depthDesc = cameraTextureDescriptor;
                VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();
                if (manager != null) {
                    depthDesc.width = VolumetricFogRenderFeature.GetScaledSize(depthDesc.width, manager.downscaling);
                    depthDesc.height = VolumetricFogRenderFeature.GetScaledSize(depthDesc.height, manager.downscaling);
                }
                depthDesc.colorFormat = RenderTextureFormat.Depth;
                depthDesc.depthBufferBits = 24;
                depthDesc.msaaSamples = 1;

                cmd.GetTemporaryRT(ShaderParams.CustomDepthTexture, depthDesc, FilterMode.Point);
                cmd.SetGlobalTexture(ShaderParams.CustomDepthTexture, m_Depth);
                ConfigureTarget(m_Depth);
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
                if (transparentLayerMask == 0 && alphaCutoutLayerMask == 0) return;
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();

                if (alphaCutoutLayerMask != 0) {
                    if (manager != null) {
                        if (depthOnlyMaterialCutOff == null) {
                            Shader depthOnlyCutOff = Shader.Find(m_DepthOnlyShader);
                            depthOnlyMaterialCutOff = new Material(depthOnlyCutOff);
                        }
                        int renderersCount = cutOutRenderers.Count;
                        if (depthOverrideMaterials == null || depthOverrideMaterials.Length < renderersCount) {
                            depthOverrideMaterials = new Material[renderersCount];
                        }
                        bool listNeedsPacking = false;
                        for (int k = 0; k < renderersCount; k++) {
                            Renderer renderer = cutOutRenderers[k];
                            if (renderer == null) {
                                listNeedsPacking = true;
                            } else if (renderer.isVisible) {
                                Material mat = renderer.sharedMaterial;
                                if (mat != null && mat.shader != fogShader) {
                                    if (depthOverrideMaterials[k] == null) {
                                        depthOverrideMaterials[k] = Instantiate(depthOnlyMaterialCutOff);
                                        depthOverrideMaterials[k].EnableKeyword(ShaderParams.SKW_CUSTOM_DEPTH_ALPHA_TEST);
                                    }
                                    Material overrideMaterial = depthOverrideMaterials[k];
                                    overrideMaterial.SetFloat(ShaderParams.CustomDepthAlphaCutoff, manager.alphaCutOff);
                                    if (mat.HasProperty(ShaderParams.CustomDepthBaseMap)) {
                                        overrideMaterial.SetTexture(ShaderParams.CustomDepthBaseMap, mat.GetTexture(ShaderParams.CustomDepthBaseMap));
                                    } else if (mat.HasProperty(ShaderParams.MainTex)) {
                                        overrideMaterial.SetTexture(ShaderParams.CustomDepthBaseMap, mat.GetTexture(ShaderParams.MainTex));
                                    }
                                    if (mat.HasProperty(ShaderParams.CullMode)) {
                                        overrideMaterial.SetInt(ShaderParams.CullMode, mat.GetInt(ShaderParams.CullMode));
                                    } else {
                                        overrideMaterial.SetInt(ShaderParams.CullMode, (int)manager.semiTransparentCullMode);
                                    }
                                    cmd.DrawRenderer(renderer, overrideMaterial);
                                }
                            }
                        }
                        if (listNeedsPacking) {
                            cutOutRenderers.RemoveAll(item => item == null);
                        }
                    }
                }

                if (transparentLayerMask != 0) {

                    foreach (VolumetricFog vg in VolumetricFog.volumetricFogs) {
                        if (vg != null && vg.meshRenderer != null) {
                            vg.renderingLayerMaskCopy = vg.meshRenderer.renderingLayerMask;
                            vg.meshRenderer.renderingLayerMask = VolumetricFogManager.FOG_VOLUMES_RENDERING_LAYER;
                            if (options.useOptimizedDepthOnlyShader) {
                                vg.meshRenderer.GetPropertyBlock(fogSkipPropertyBlock);
                                fogSkipPropertyBlock.SetFloat(ShaderParams.SkipRendering, 1f);
                                vg.meshRenderer.SetPropertyBlock(fogSkipPropertyBlock);
                            }
                        }
                    }

                    // Exclude fog volumes from rendering
                    filterSettings.renderingLayerMask = ~VolumetricFogManager.FOG_VOLUMES_RENDERING_LAYER;

                    SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
                    var drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
                    drawSettings.perObjectData = PerObjectData.None;
                    if (options.useOptimizedDepthOnlyShader) {
                        if (depthOnlyMaterial == null) {
                            Shader depthOnly = Shader.Find(m_DepthOnlyShader);
                            depthOnlyMaterial = new Material(depthOnly);
                        }
                        if (manager != null) {
                            depthOnlyMaterial.SetInt(ShaderParams.CullMode, (int)manager.transparentCullMode);
                        }
                        drawSettings.overrideMaterial = depthOnlyMaterial;
                    }
                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
                }

                context.ExecuteCommandBuffer(cmd);

                // Restore rendering layer mask
                if (transparentLayerMask != 0) {
                    foreach (VolumetricFog vg in VolumetricFog.volumetricFogs) {
                        if (vg != null) {
                            vg.meshRenderer.renderingLayerMask = vg.renderingLayerMaskCopy;
                        }
                    }
                }

                CommandBufferPool.Release(cmd);
            }
        }
    }
}
#endif
