using UnityEngine.Rendering.Universal;

namespace FreeGo.RenderFeatures.FogOfWar
{
    public class FogOfWarFeature : ScriptableRendererFeature
    {
        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        private FogOfWarRenderPass m_Pass;

        public override void Create()
        {
            m_Pass = new(RenderPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_Pass);
        }
    }
}