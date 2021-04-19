using UnityEngine;

public class FFTOcean : MonoBehaviour
{
    [SerializeField] ComputeShader shaderGenerateSpectrum;
    [SerializeField] ComputeShader shaderSetNormal;
    [SerializeField] Shader renderingShader;
    public Material renderingShader_Material;

    [SerializeField] FastFourierTransform fFT;
    [SerializeField] PhillipsSpectrum phillips;
    int kernel_GenerateSpectrumKernel, kernel_Center_difference;
    Vector2[] h_h0;
    public ComputeBuffer d_h0, d_ht, d_ht_dmy;
    RenderTexture normal_Tex;
    int cnt;

    private void Awake()
    {
        kernel_GenerateSpectrumKernel = shaderGenerateSpectrum.FindKernel("GenerateSpectrumKernel");
        kernel_Center_difference = shaderSetNormal.FindKernel("Center_difference");
        renderingShader_Material = new Material(renderingShader);
        cnt = 0;
    }

    void Start()
    {
        normal_Tex = new RenderTexture((int)PhillipsSpectrum.meshSize, (int)PhillipsSpectrum.meshSize, 0, RenderTextureFormat.ARGBFloat);
        normal_Tex.enableRandomWrite = true;
        normal_Tex.Create();
        normal_Tex.filterMode = FilterMode.Bilinear;
        normal_Tex.wrapMode = TextureWrapMode.Repeat;

        h_h0 = phillips.Generate_h0();
        d_h0 = new ComputeBuffer(h_h0.Length, sizeof(float) * 2);
        d_ht = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_ht_dmy = new ComputeBuffer(d_ht.count, sizeof(float) * 2);
        d_h0.SetData(h_h0);
        SetArgs();
    }

    void SetArgs()
    {

        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "h0", d_h0);
        shaderGenerateSpectrum.SetBuffer(kernel_GenerateSpectrumKernel, "ht", d_ht);
        shaderGenerateSpectrum.SetInt("N", (int)PhillipsSpectrum.meshSize);
        shaderGenerateSpectrum.SetInt("seasizeLx", (int)PhillipsSpectrum.seaSizeLX);
        shaderGenerateSpectrum.SetInt("seasizeLz", (int)PhillipsSpectrum.seaSizeLX);
        shaderSetNormal.SetBuffer(kernel_Center_difference, "ht", d_ht);
        shaderSetNormal.SetTexture(kernel_Center_difference, "tex", normal_Tex);
        shaderSetNormal.SetInt("N", (int)PhillipsSpectrum.meshSize);
        shaderSetNormal.SetFloat("rdx", 1.0f * PhillipsSpectrum.meshSize / PhillipsSpectrum.seaSizeLX);
        shaderSetNormal.SetFloat("rdz", 1.0f * PhillipsSpectrum.meshSize / PhillipsSpectrum.seaSizeLZ);

        renderingShader_Material.SetBuffer("d_ht", d_ht);
        renderingShader_Material.SetTexture("_MainTex", normal_Tex);

        renderingShader_Material.SetInt("N", (int)PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("halfN", 0.5f * PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("dx", 1.0f * PhillipsSpectrum.seaSizeLX / PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("dz", 1.0f * PhillipsSpectrum.seaSizeLZ / PhillipsSpectrum.meshSize);
    }


    void Update()
    {

        shaderGenerateSpectrum.SetFloat("t", 0.03f * cnt);
        shaderGenerateSpectrum.Dispatch(kernel_GenerateSpectrumKernel, 1, 256, 1);
        fFT.FFT2D_256_Dispatch(d_ht, d_ht_dmy);


        shaderSetNormal.Dispatch(kernel_Center_difference, 1, 256, 1);
        cnt++;

    }

    //void OnRenderObject()
    //{
    //
    //    renderingShader_Material.SetPass(0);
    //    Graphics.DrawProceduralNow(MeshTopology.Triangles, 256 * 256 * 4);
    //}

    private void OnDestroy()
    {

        d_h0.Release();
        d_ht.Release();
        d_ht_dmy.Release();
    }

}