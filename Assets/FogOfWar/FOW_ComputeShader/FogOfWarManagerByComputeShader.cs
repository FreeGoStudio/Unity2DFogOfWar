using UnityEngine;
using UnityEngine.UI;

public class FogOfWarManagerByComputeShader : MonoBehaviour
{

    [SerializeField] private Transform m_TargetTransform;

    [SerializeField] private int m_PixelsPerWorldUnit = 16;
    [SerializeField] private int m_FOVCameraSize = 4;
    [SerializeField] private int m_FOWMaskSize = 128;

    //Prefabs
    [SerializeField] private GameObject m_PlayerViewPrefab;
    [SerializeField] private GameObject m_PlayerViewCameraPrefab;
    [SerializeField] private GameObject m_MaskCanvasPrefab;
    [SerializeField] private ComputeShader m_FogOfWarAddComputeShader;
    [SerializeField] private Material m_RedToAlphaMaterial;

    //FOV
    private GameObject m_PlayerView;
    private Camera m_FOVCamera;
    private RenderTexture m_FOVCameraRenderTexture;

    //FOW
    private RawImage m_FOWMask;
    private RenderTexture m_FOWMaskRenderTexture;

    private int m_FOVAddFOWCSMainKernelID;

    private void Awake()
    {
        m_FOWMask = CreateMask();

        m_FOWMask.material = m_RedToAlphaMaterial;

        int FOVCameraRenderTextureSize = m_PixelsPerWorldUnit * m_FOVCameraSize * 2;
        m_FOVCameraRenderTexture = new RenderTexture(FOVCameraRenderTextureSize, FOVCameraRenderTextureSize, 0);
        m_FOVCameraRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        m_FOVCameraRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
        m_FOVCameraRenderTexture.filterMode = FilterMode.Point;
        m_FOVCameraRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FOVCameraRenderTexture.enableRandomWrite = true;
        m_FOVCameraRenderTexture.Create();

        int m_FOWMaskRenderTextureSize = m_PixelsPerWorldUnit * m_FOWMaskSize;
        m_FOWMaskRenderTexture = new RenderTexture(m_FOWMaskRenderTextureSize, m_FOWMaskRenderTextureSize, 0);
        m_FOWMaskRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SNorm;
        m_FOWMaskRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        m_FOWMaskRenderTexture.filterMode = FilterMode.Bilinear;
        m_FOWMaskRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        m_FOWMaskRenderTexture.enableRandomWrite = true;

        m_FOWMaskRenderTexture.Create();

        m_FOVAddFOWCSMainKernelID = m_FogOfWarAddComputeShader.FindKernel("CSMain");

        m_FogOfWarAddComputeShader.SetTexture(m_FOVAddFOWCSMainKernelID, "Input", m_FOVCameraRenderTexture);
        m_FogOfWarAddComputeShader.SetTexture(m_FOVAddFOWCSMainKernelID, "Result", m_FOWMaskRenderTexture);

        if (m_TargetTransform != null)
        {
            SetTarget(m_TargetTransform);
        }
    }

    void Start()
    {
        m_FOWMask.texture = m_FOWMaskRenderTexture;
    }

    void Update()
    {
        if (m_TargetTransform == null)
        {
            return;
        }

        // 获取角色的世界坐标
        var worldPosition = m_TargetTransform.transform.position;

        // 世界坐标转换成RenderTexture坐标, 1世界坐标=16像素
        Vector2Int renderTexturePos = new Vector2Int(
            (int)(worldPosition.x * m_PixelsPerWorldUnit),
            (int)(worldPosition.y * m_PixelsPerWorldUnit)
        );

        //坐标对齐到遮罩的中心
        int maskOffset = (int)((m_FOWMaskSize / 2 - m_FOVCameraSize) * m_PixelsPerWorldUnit);


        // 将纹理坐标传递给计算着色器
        m_FogOfWarAddComputeShader.SetInts("Offset", renderTexturePos.x + maskOffset, renderTexturePos.y + maskOffset);
        m_FogOfWarAddComputeShader.Dispatch(m_FOVAddFOWCSMainKernelID, m_FOVCameraRenderTexture.width / 8, m_FOVCameraRenderTexture.height / 8, 1);
    }

    public void SetTarget(Transform targetTransform)
    {
        if (m_FOVCamera != null)
        {
            Destroy(m_FOVCamera.gameObject);
        }

        if (m_PlayerView != null)
        {
            Destroy(m_PlayerView);
        }

        m_TargetTransform = targetTransform;

        m_PlayerView = CreateView();
        m_FOVCamera = CreateCamera();
    }

    private GameObject CreateView()
    {
        var gameObject = Instantiate(m_PlayerViewPrefab);
        gameObject.transform.SetParent(m_TargetTransform);
        gameObject.transform.localPosition = Vector3.zero;

        return gameObject;
    }

    private Camera CreateCamera()
    {
        var gameObject = Instantiate(m_PlayerViewCameraPrefab);
        gameObject.transform.SetParent(m_TargetTransform);
        gameObject.transform.localPosition = new Vector3(0, 0, -10);
        var component = gameObject.GetComponent<Camera>();
        component.aspect = 1.0f;
        component.orthographic = true;
        component.orthographicSize = m_FOVCameraSize;
        component.targetTexture = m_FOVCameraRenderTexture;
        return component;
    }

    private RawImage CreateMask()
    {
        var gameObject = Instantiate(m_MaskCanvasPrefab);
        //获取GameObject的子物体中的RawImage组件
        var component = gameObject.GetComponentInChildren<RawImage>();
        return component;
    }
}
