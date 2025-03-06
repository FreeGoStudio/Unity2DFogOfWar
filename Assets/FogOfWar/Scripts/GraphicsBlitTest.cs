using UnityEngine;
using UnityEngine.UI;

public class GraphicsBlitTest : MonoBehaviour
{
    [SerializeField] private RenderTexture m_CameraOutput;
    [SerializeField] private RawImage m_RawImage;
    [SerializeField] private Material m_Material;
    private RenderTexture m_Display;

    void Start()
    {
        m_Display = new RenderTexture(1024, 1024, 0);
        m_Display.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        m_Display.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        m_Display.filterMode = FilterMode.Point;
        m_Display.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

        m_Display.Create();

        m_RawImage.texture = m_Display;
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(m_CameraOutput, m_Display, m_Material);
    }
}
