using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class FFTOcean : MonoBehaviour
{
    [SerializeField] ComputeShader shaderInitialSpectrum;
    [SerializeField] ComputeShader shaderCreateNormal;
    [SerializeField] Shader renderingShader;
    private Material renderingShader_Material;
    [SerializeField] FastFourierTransform fft;
    [SerializeField] PhillipsSpectrum phillips;
    int kernel_InitialSpectrumKernel, kernel_CreateNormal;
    Vector2[] h_h0;
    public ComputeBuffer d_h0, d_ht, d_ht_dx, d_ht_dz, d_ht_dmy, d_displaceX, d_displaceZ;
    RenderTexture normal_Tex;
    int cnt;
    [SerializeField] float lambda = -1.0f;
    public Color waterColour = new Vector4(0.11f, 0.64f, 0.79f, 1f);
    public Color highlightColour = new Vector4(0, 0.31f, 0.40f, 1f);

    private void Awake()
    {
        kernel_InitialSpectrumKernel = shaderInitialSpectrum.FindKernel("InitialSpectrumKernel");
        kernel_CreateNormal = shaderCreateNormal.FindKernel("CreateNormal");
        renderingShader_Material = new Material(renderingShader);
        cnt = 0;
    }

    void Start()
    {
        normal_Tex = CreateRenderTeture();
        h_h0 = phillips.GenerateH0();
        d_h0 = new ComputeBuffer(h_h0.Length, sizeof(float) * 2);
        d_ht = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_ht_dx = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_ht_dz = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_displaceX = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_displaceZ = new ComputeBuffer((int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize), sizeof(float) * 2);
        d_ht_dmy = new ComputeBuffer(d_ht.count, sizeof(float) * 2);
        d_h0.SetData(h_h0);
        SetArgs();
    }

    private RenderTexture tex;
    private Texture2D tex2D;

    RenderTexture CreateRenderTeture()
    {
        tex = new RenderTexture((int)PhillipsSpectrum.meshSize, (int)PhillipsSpectrum.meshSize, 0, RenderTextureFormat.ARGBFloat);
        tex.enableRandomWrite = true;
        tex.Create();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex2D = RenderTextureTo2DTexture(tex);
        return tex;
    }

    void SetArgs()
    {
        // Set ComputeShader arguments
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "h0", d_h0);
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "ht", d_ht);
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "ht_dx", d_ht_dx);
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "ht_dz", d_ht_dz);
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "displaceX", d_displaceX);
        shaderInitialSpectrum.SetBuffer(kernel_InitialSpectrumKernel, "displaceZ", d_displaceZ);
        shaderInitialSpectrum.SetInt("N", (int)PhillipsSpectrum.meshSize);
        shaderInitialSpectrum.SetInt("Lx", (int)PhillipsSpectrum.Lx);
        shaderInitialSpectrum.SetInt("Lz", (int)PhillipsSpectrum.Lz);
        shaderCreateNormal.SetBuffer(kernel_CreateNormal, "ht_dx", d_ht_dx);
        shaderCreateNormal.SetBuffer(kernel_CreateNormal, "ht_dz", d_ht_dz);
        shaderCreateNormal.SetBuffer(kernel_InitialSpectrumKernel, "displaceX", d_displaceX);
        shaderCreateNormal.SetBuffer(kernel_InitialSpectrumKernel, "displaceZ", d_displaceZ);
        shaderCreateNormal.SetTexture(kernel_CreateNormal, "tex", normal_Tex);
        shaderCreateNormal.SetFloat("dx", 1.0f * PhillipsSpectrum.Lx / PhillipsSpectrum.meshSize);
        shaderCreateNormal.SetFloat("dz", 1.0f * PhillipsSpectrum.Lz / PhillipsSpectrum.meshSize);
        shaderCreateNormal.SetFloat("lambda", lambda);
        shaderCreateNormal.SetInt("N", (int)PhillipsSpectrum.meshSize);
        // Set GPU buffer as material
        renderingShader_Material.SetBuffer("d_ht", d_ht);
        renderingShader_Material.SetTexture("_MainTex", normal_Tex);
        renderingShader_Material.SetBuffer("d_displaceX", d_displaceX);
        renderingShader_Material.SetBuffer("d_displaceZ", d_displaceZ);
        // Shader Constants
        renderingShader_Material.SetInt("N", (int)PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("halfN", 0.5f * PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("dx", 1.0f * PhillipsSpectrum.Lx / PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("dz", 1.0f * PhillipsSpectrum.Lz / PhillipsSpectrum.meshSize);
        renderingShader_Material.SetFloat("lambda", lambda);
    }


    void Update()
    {
        CalculateSpectrum();
        CalcFFT_ht();
        CalcFFT_ht_dxz();
        CalcFFT_displaceXZ();
        CreateNormal();
        tex2D = RenderTextureTo2DTexture(tex);
        renderingShader_Material.SetColor("_WaterColour", waterColour);
        renderingShader_Material.SetColor("_HighlightColour", highlightColour);
        cnt++;
    }

    public float GetWaterHeight(Vector3 position)
    {
        Vector3 displacement = GetWaterDisplacement(position);
        displacement = GetWaterDisplacement(position - displacement);

        return 1 - GetWaterDisplacement(position - displacement).y;
    }

    public Vector3 GetWaterDisplacement(Vector3 position)
    {
        Color c = tex2D.GetPixelBilinear(position.x / 250, position.z / 250);
        return new Vector3(c.r, c.g, c.b);
    }

    private Texture2D RenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture currentActiveRT = RenderTexture.active;

        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        RenderTexture.active = currentActiveRT;
        return tex;
    }

    void CalculateSpectrum()
    {
        shaderInitialSpectrum.SetFloat("t", 0.04f * cnt + 100.0f);
        shaderInitialSpectrum.Dispatch(kernel_InitialSpectrumKernel, 1, 256, 1);// Calculate d_ht from d_h0, partial derivative component
    }

    void CalcFFT_ht()
    {
        fft.FFT2D_256_Dispatch(d_ht, d_ht_dmy);// Calculate height data from d_ht
    }

    void CalcFFT_ht_dxz()
    {
        fft.FFT2D_256_Dispatch(d_ht_dx, d_ht_dmy);// Calculate the height data from d_ht_dx and put the result in d_ht_dx
        fft.FFT2D_256_Dispatch(d_ht_dz, d_ht_dmy);// Calculate the height data from d_ht_dz and put the result in d_ht_dz
    }
    void CalcFFT_displaceXZ()
    {
        fft.FFT2D_256_Dispatch(d_displaceX, d_ht_dmy);
        fft.FFT2D_256_Dispatch(d_displaceZ, d_ht_dmy);
    }

    void CreateNormal()
    {
        shaderCreateNormal.Dispatch(kernel_CreateNormal, 1, 256, 1);// Extract the value from the partial differential buffer, calculate the normal vector and save it in the texture
    }

    void OnRenderObject()
    {
        //Render whatever is in the compute buffer in the form of a multi-quad plane
        renderingShader_Material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, (int)(PhillipsSpectrum.meshSize * PhillipsSpectrum.meshSize) * 4);
    }

    void ReleaseBuffers()
    {
        d_h0.Release();
        d_ht.Release();
        d_ht_dx.Release();
        d_ht_dz.Release();
        d_ht_dmy.Release();
        d_displaceX.Release();
        d_displaceZ.Release();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

}