using UnityEngine;
using UnityEngine.UI;

public class FOWManger : MonoBehaviour
{
    [SerializeField] private GameObject m_Player;

    //FOV
    [SerializeField] private Camera m_FOVCamera;
    [SerializeField] private float m_FOVCameraSize = 8f;
    [SerializeField] private int m_FOVCameraRenderTextureSize = 256;
    private RenderTexture m_FOVCameraRenderTexture;

    //FOW
    [SerializeField] private RawImage m_FOWMask;
    [SerializeField] private float m_FOWMaskSize = 128;
    [SerializeField] private int m_FOWMaskRenderTextureSize = 2048;
    private RenderTexture m_FOWMaskRenderTexture;

    [SerializeField] private ComputeShader m_AddFOVToFOWComputeShader;
    private int m_FOVAddFOWCSMainKernelID;


    private void Awake()
    {
        m_FOVCameraRenderTexture = new RenderTexture(m_FOVCameraRenderTextureSize, m_FOVCameraRenderTextureSize, 0);
        m_FOVCameraRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SNorm;
        m_FOVCameraRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
        m_FOVCameraRenderTexture.filterMode = FilterMode.Point;
        m_FOVCameraRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FOVCameraRenderTexture.enableRandomWrite = true;
        m_FOVCameraRenderTexture.Create();

        m_FOWMaskRenderTexture = new RenderTexture(m_FOWMaskRenderTextureSize, m_FOWMaskRenderTextureSize, 0);
        m_FOWMaskRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SNorm;
        m_FOWMaskRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        m_FOWMaskRenderTexture.filterMode = FilterMode.Bilinear;
        m_FOWMaskRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FOWMaskRenderTexture.enableRandomWrite = true;

        m_FOWMaskRenderTexture.Create();
    }

    void Start()
    {
        m_FOVCamera.aspect = 1.0f;
        m_FOVCamera.orthographic = true;
        m_FOVCamera.orthographicSize = m_FOVCameraSize;
        m_FOVCamera.targetTexture = m_FOVCameraRenderTexture;

        m_FOWMask.gameObject.SetActive(true);
        m_FOWMask.texture = m_FOWMaskRenderTexture;

        m_FOVAddFOWCSMainKernelID = m_AddFOVToFOWComputeShader.FindKernel("CSMain");

        m_AddFOVToFOWComputeShader.SetTexture(m_FOVAddFOWCSMainKernelID, "Input", m_FOVCameraRenderTexture);
        m_AddFOVToFOWComputeShader.SetTexture(m_FOVAddFOWCSMainKernelID, "Result", m_FOWMaskRenderTexture);
    }

    void Update()
    {
        // 获取角色的世界坐标
        Vector3 playerWorldPos = m_Player.transform.position;

        var FOVPixelsPerWorldUnit = m_FOVCameraRenderTextureSize / (m_FOVCameraSize * 2f);

        // 世界坐标转换成RenderTexture坐标, 1世界坐标=16像素
        Vector2Int renderTexturePos = new Vector2Int(
            (int)(playerWorldPos.x * FOVPixelsPerWorldUnit),
            (int)(playerWorldPos.y * FOVPixelsPerWorldUnit)
        );

        //坐标对齐到遮罩的中心
        int maskOffset = (int)((m_FOWMaskSize / 2 - m_FOVCameraSize) * m_FOWMaskRenderTextureSize / m_FOWMaskSize);

        // 将纹理坐标传递给计算着色器
        m_AddFOVToFOWComputeShader.SetInts("Offset", renderTexturePos.x + maskOffset, renderTexturePos.y + maskOffset);
        m_AddFOVToFOWComputeShader.Dispatch(m_FOVAddFOWCSMainKernelID, m_FOVCameraRenderTexture.width / 8, m_FOVCameraRenderTexture.height / 8, 1);
    }
}
