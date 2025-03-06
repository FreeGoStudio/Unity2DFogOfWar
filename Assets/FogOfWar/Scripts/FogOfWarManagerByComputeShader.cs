using UnityEngine;
using UnityEngine.UI;

public class FogOfWarManagerByComputeShader : MonoBehaviour
{
    [SerializeField] private RenderTexture m_PlayerViewCameraOutputRenderTexture;
    [SerializeField] private RawImage m_FogOfWarMask;
    [SerializeField] private ComputeShader m_AddComputeShader;

    private RenderTexture m_FogOfWarMaskRenderTexture;
    private int m_CSMainKernelID;

    void Start()
    {
        m_FogOfWarMaskRenderTexture = new RenderTexture(1024, 1024, 0);
        m_FogOfWarMaskRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SNorm;
        m_FogOfWarMaskRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        m_FogOfWarMaskRenderTexture.filterMode = FilterMode.Bilinear;
        m_FogOfWarMaskRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FogOfWarMaskRenderTexture.enableRandomWrite = true;

        m_FogOfWarMaskRenderTexture.Create();

        m_FogOfWarMask.gameObject.SetActive(true);
        m_FogOfWarMask.texture = m_FogOfWarMaskRenderTexture;

        m_CSMainKernelID = m_AddComputeShader.FindKernel("CSMain");

        m_AddComputeShader.SetTexture(m_CSMainKernelID, "Input", m_PlayerViewCameraOutputRenderTexture);
        m_AddComputeShader.SetTexture(m_CSMainKernelID, "Result", m_FogOfWarMaskRenderTexture);
    }

    void Update()
    {

        m_AddComputeShader.Dispatch(m_CSMainKernelID, m_PlayerViewCameraOutputRenderTexture.width / 8, m_PlayerViewCameraOutputRenderTexture.height / 8, 1);
    }
}
