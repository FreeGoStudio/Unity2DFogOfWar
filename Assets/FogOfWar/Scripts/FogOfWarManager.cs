using UnityEngine;
using UnityEngine.UI;

public class FogOfWarManager : MonoBehaviour
{
    [SerializeField] private RenderTexture m_PlayerViewCameraOutputRenderTexture;
    [SerializeField] private RawImage m_FogOfWarMask;
    [SerializeField] private Material m_BlitMaterial;
    [SerializeField] private Vector2Int m_RenderTextureSize = new Vector2Int(1024, 1024);

    private RenderTexture m_FogOfWarMaskRenderTexture;

    void Start()
    {
        m_FogOfWarMaskRenderTexture = new RenderTexture(m_RenderTextureSize.x, m_RenderTextureSize.y, 0);
        m_FogOfWarMaskRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SNorm;
        m_FogOfWarMaskRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        m_FogOfWarMaskRenderTexture.filterMode = FilterMode.Bilinear;
        m_FogOfWarMaskRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FogOfWarMaskRenderTexture.enableRandomWrite = true;

        m_FogOfWarMaskRenderTexture.Create();

        m_FogOfWarMask.gameObject.SetActive(true);
        m_FogOfWarMask.texture = m_FogOfWarMaskRenderTexture;
    }

    void Update()
    {
        Graphics.Blit(m_PlayerViewCameraOutputRenderTexture, m_FogOfWarMaskRenderTexture, m_BlitMaterial);
    }
}
