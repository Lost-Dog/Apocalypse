using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if !UNITY_6000_4_OR_NEWER
namespace VolumetricFogAndMist2 {

    public partial class VolumetricFogRenderFeature {

        partial class VolumetricFogRenderPass {

            public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                RenderTextureDescriptor lightBufferDesc = cameraTextureDescriptor;
                VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();
                if (manager != null) {
                    if (manager.downscaling > 1f) {
                        int size = GetScaledSize(cameraTextureDescriptor.width, manager.downscaling);
                        lightBufferDesc.width = size;
                        lightBufferDesc.height = size;
                    }
                    lightBufferDesc.colorFormat = manager.blurHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
                    cmd.SetGlobalVector(ShaderParams.LightBufferSize, new Vector4(lightBufferDesc.width, lightBufferDesc.height, manager.downscaling > 1f ? 1f : 0, 0));
                }
                lightBufferDesc.depthBufferBits = 0;
                lightBufferDesc.msaaSamples = 1;
                lightBufferDesc.useMipMap = false;

                cmd.GetTemporaryRT(ShaderParams.LightBuffer, lightBufferDesc, FilterMode.Bilinear);
                ConfigureTarget(m_LightBuffer);
                ConfigureClear(ClearFlag.Color, new Color(0, 0, 0, 0));
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {

                VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();

                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                cmd.SetGlobalInt(ShaderParams.ForcedInvisible, 0);
                context.ExecuteCommandBuffer(cmd);

                if (manager == null || (manager.downscaling <= 1f && manager.blurPasses < 1 && manager.scattering <= 0 && !isUsingDepthPeeling)) {
                    CommandBufferPool.Release(cmd);
                    return;
                }

                cmd.Clear();

                bool isFrontPass = renderPassEvent >= RenderPassEvent.AfterRenderingTransparents;

                foreach (VolumetricFog vg in VolumetricFog.volumetricFogs) {
                    if (vg != null) {
                        vg.meshRenderer.renderingLayerMask |= VolumetricFogManager.FOG_VOLUMES_RENDERING_LAYER;
                        if (isUsingDepthPeeling && vg.DistantFogUsesTransparencySupport) {
                            if (!isFrontPass) {
                                // First pass: render distant fog only behind transparent objects
                                cmd.EnableShaderKeyword(ShaderParams.SKW_DEPTH_PEELING);
                                cmd.DisableShaderKeyword(ShaderParams.SKW_DEPTH_PREPASS);
                                vg.RenderDistantFog(cmd);
                            } else {
                                // Second pass: render distant fog using custom depth for correct distance to transparents
                                cmd.DisableShaderKeyword(ShaderParams.SKW_DEPTH_PEELING);
                                cmd.EnableShaderKeyword(ShaderParams.SKW_DEPTH_PREPASS);
                                vg.RenderDistantFog(cmd);
                            }
                        } else if (!isFrontPass) {
                            vg.RenderDistantFog(cmd);
                        }
                    }
                }

                if (isUsingDepthPeeling) {
                    if (!isFrontPass) {
                        cmd.EnableShaderKeyword(ShaderParams.SKW_DEPTH_PEELING);
                    } else {
                        cmd.DisableShaderKeyword(ShaderParams.SKW_DEPTH_PEELING);
                        cmd.EnableShaderKeyword(ShaderParams.SKW_DEPTH_PREPASS);
                    }
                    context.ExecuteCommandBuffer(cmd);
                }
                var sortFlags = SortingCriteria.CommonTransparent;
                var drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortFlags);
                var filterSettings = filteringSettings;
                filterSettings.layerMask = settings.fogLayerMask;
                filterSettings.renderingLayerMask = VolumetricFogManager.FOG_VOLUMES_RENDERING_LAYER;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

                CommandBufferPool.Release(cmd);

            }
        }

        partial class BlurRenderPass {

            public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                sourceDesc = cameraTextureDescriptor;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {

#if UNITY_2022_1_OR_NEWER
                passData.source = renderer.cameraColorTargetHandle;
#else
                passData.source = renderer.cameraColorTarget;
#endif
                passData.renderPassEvent = renderPassEvent;
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                ExecutePass(passData, cmd);
                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);

            }
        }
    }
}
#endif
