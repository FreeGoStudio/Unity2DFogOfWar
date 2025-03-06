using UnityEngine;

public class FogOfWarManger : MonoBehaviour
{
    [SerializeField] private ComputeShader m_FogOfWarMaskComputeShader;
    [SerializeField] private float m_BrushSize = 5f;
    private RenderTexture m_FogOfWarMaskRenderTexture;

    void Start()
    {
        m_FogOfWarMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        m_FogOfWarMaskRenderTexture.filterMode = FilterMode.Point;
        m_FogOfWarMaskRenderTexture.enableRandomWrite = true;
        m_FogOfWarMaskRenderTexture.Create();

        var initBackgroundKernel = m_FogOfWarMaskComputeShader.FindKernel("InitBackground");
        m_FogOfWarMaskComputeShader.SetTexture(initBackgroundKernel, "_Canvas", m_FogOfWarMaskRenderTexture);
        m_FogOfWarMaskComputeShader.Dispatch(initBackgroundKernel, m_FogOfWarMaskRenderTexture.width / 8, m_FogOfWarMaskRenderTexture.height / 8, 1);
    }

    void Update()
    {
        var mousePosition = Input.mousePosition;

        var updateFogOfWarKernel = m_FogOfWarMaskComputeShader.FindKernel("Update");
        m_FogOfWarMaskComputeShader.SetTexture(updateFogOfWarKernel, "_Canvas", m_FogOfWarMaskRenderTexture);
        m_FogOfWarMaskComputeShader.SetFloats("_MousePosition", mousePosition.x, mousePosition.y);
        m_FogOfWarMaskComputeShader.SetBool("_isMouseDown", true);
        m_FogOfWarMaskComputeShader.SetFloat("_BrushSize", m_BrushSize);
        m_FogOfWarMaskComputeShader.Dispatch(updateFogOfWarKernel, m_FogOfWarMaskRenderTexture.width / 8, m_FogOfWarMaskRenderTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(m_FogOfWarMaskRenderTexture, destination);
    }
}
