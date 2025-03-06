using UnityEngine;

public class TestComputeManager : MonoBehaviour
{
    public ComputeShader shader;

    private int size = 128;
    private int _kernel;
    public Material _mat;

    void Start()
    {
        //_kernel = shader.FindKernel("CSMain");
        _kernel = shader.FindKernel("A1");

        RenderTexture tex = new RenderTexture(size, size, 0);
        tex.enableRandomWrite = true;
        tex.Create();

        _mat.SetTexture("_MainTex", tex);
        shader.SetTexture(_kernel, "Result", tex);
    }

    void Update()
    {
        shader.Dispatch(_kernel, Mathf.CeilToInt(size / 8f), Mathf.CeilToInt(size / 8f), 1);
    }
}
