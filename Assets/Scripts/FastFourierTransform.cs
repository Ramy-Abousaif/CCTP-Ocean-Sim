using UnityEngine;

public class FastFourierTransform : MonoBehaviour
{
    const bool DEBUG = false;
    [SerializeField] ComputeShader shader;
    ComputeBuffer buffer_dbg;
    int kernel_FFT2Dfunc256inv, kernel_DFT2Dfunc256inv;

    void Awake()
    {
        kernel_FFT2Dfunc256inv = shader.FindKernel("FFT2Dfunc256inv");
        kernel_DFT2Dfunc256inv = shader.FindKernel("DFT2Dfunc256inv");
    }

    void Start()
    {
        if (DEBUG) buffer_dbg = new ComputeBuffer(1, sizeof(float) * 2);
    }

    public void FFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        if (DEBUG) DEBUG_func1(buffer);

        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer_dmy);

        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);


        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer_dmy);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer);

        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);

        if (DEBUG) DEBUG_func2(buffer);
    }


    public void DFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {

        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer_dmy", buffer_dmy);

        shader.Dispatch(kernel_DFT2Dfunc256inv, 256, 1, 1);
    }




    void DEBUG_func1(ComputeBuffer buffer)
    {
        if (buffer_dbg.count != buffer.count)
            buffer_dbg = new ComputeBuffer(buffer.count, sizeof(float) * 2);
        DFT2D_256_Dispatch(buffer, buffer_dbg);
    }


    void DEBUG_func2(ComputeBuffer buffer)
    {
        Vector2[] bfr = new Vector2[buffer.count];
        Vector2[] bfr_dbg = new Vector2[buffer.count];
        buffer.GetData(bfr);
        buffer_dbg.GetData(bfr_dbg);
        float rss = 0.0f;
        for (int i = 0; i < buffer.count; i++)
        {
            Vector2 v2 = bfr[i] - bfr_dbg[i];
            rss += v2.sqrMagnitude;
        }
        Debug.Log(rss);
    }

}
