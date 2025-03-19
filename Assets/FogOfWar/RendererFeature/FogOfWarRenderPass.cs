using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace FreeGo.RenderFeatures.FogOfWar
{
    internal class FogOfWarRenderPass : ScriptableRenderPass
    {
        private class FogOfWarComputePassData
        {
            public BufferHandle OutputBufferHandle;
            public ComputeShader FogComputeShader;
            public TextureHandle Light2DTextureHandle;
        }
        private RTHandle m_OutputCameraRTHandle;
        private const string c_ShaderGloabaPropertyName = "_LightsTexture";
        private static readonly int s_ShaderGlobalPropertyId = Shader.PropertyToID(c_ShaderGloabaPropertyName);

        public FogOfWarRenderPass(RenderPassEvent evt)
        {
            this.renderPassEvent = evt;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData2D = frameData.Get<Universal2DResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.camera.cameraType != CameraType.Game)
            {
                return;
            }

            //设置RenderTexture的属性
            var renderTextureDescriptor = cameraData.cameraTargetDescriptor;
            renderTextureDescriptor.depthBufferBits = 0;
            renderTextureDescriptor.msaaSamples = 1;
            renderTextureDescriptor.width = 16;
            renderTextureDescriptor.height = 16;

            //对m_OutputCameraRTHandle配置renderTextureDescriptor和filterMode, TextureWrapMode
            RenderingUtils.ReAllocateHandleIfNeeded(ref m_OutputCameraRTHandle, renderTextureDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: c_ShaderGloabaPropertyName);
            //设置Shader全局Texture属性
            Shader.SetGlobalTexture(s_ShaderGlobalPropertyId, m_OutputCameraRTHandle);


            if (resourceData2D != null)
            {
                for (var i = 0; i < resourceData2D.lightTextures.Length; i++)
                {
                    for (var j = 0; j < resourceData2D.lightTextures[i].Length; j++)
                    {
                        //获取光照Texture
                        TextureHandle lightTextureHandle = resourceData2D.lightTextures[i][j];
                        //将RenderTexture导入到RenderGraph, 并将其资源标识的赋值给destination
                        var outputCameraTextureHandle = renderGraph.ImportTexture(m_OutputCameraRTHandle);

                        if (!lightTextureHandle.IsValid())
                        {
                            return;
                        }

                        //设置Blit操作的参数
                        var blitMaterialParameters = new UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils.BlitMaterialParameters(lightTextureHandle, outputCameraTextureHandle, Blitter.GetBlitMaterial(TextureDimension.Tex2D), 0);
                        //添加Blit Pass
                        renderGraph.AddBlitPass(blitMaterialParameters, "BlitToRTHandle_CameraOutput");

                        //using (var builder = renderGraph.AddRasterRenderPass("", out FogOfWarComputePassData passData))
                        //{
                        //    passData.FogComputeShader = m_FogComputeShader;
                        //    passData.Light2DTextureHandle = lightTextureHandle;
                        //    builder.SetRenderFunc((FogOfWarComputePassData data, RasterGraphContext context) =>
                        //    {
                        //        ExecutePass(data, context);
                        //    });
                        //}

                        //using (var builder = renderGraph.AddComputePass("FogOfWarRenderPass", out FogOfWarComputePassData passData))
                        //{
                        //    //初始化ComputePassData
                        //    passData.OutputBufferHandle = new BufferHandle();
                        //    passData.FogComputeShader = m_FogComputeShader;
                        //    passData.Light2DTextureHandle = lightTextureHandle;

                        //    builder.SetRenderFunc((FogOfWarComputePassData data, ComputeGraphContext context) =>
                        //    {
                        //        ExecuteComputePass(data, context);
                        //    });

                        //}
                    }
                }
            }
        }


        private static void ExecuteComputePass(FogOfWarComputePassData passData, ComputeGraphContext context)
        {
            context.cmd.SetComputeBufferParam(passData.FogComputeShader, passData.FogComputeShader.FindKernel("CSMain"), "Result", passData.OutputBufferHandle);
            context.cmd.DispatchCompute(passData.FogComputeShader, passData.FogComputeShader.FindKernel("CSMain"), 1, 1, 1);
            //RTHandle rTHandle = passData.Light2DTextureHandle;
        }

        private static void ExecutePass(FogOfWarComputePassData passData, RasterGraphContext context)
        {
            //context.cmd.SetComputeBufferParam(passData.FogComputeShader, passData.FogComputeShader.FindKernel("CSMain"), "Result", passData.OutputBufferHandle);
            //context.cmd.DispatchCompute(passData.FogComputeShader, passData.FogComputeShader.FindKernel("CSMain"), 1, 1, 1);
            //RTHandle rTHandle = passData.Light2DTextureHandle;
            RTHandle rTHandle = passData.Light2DTextureHandle;
        }
    }
}