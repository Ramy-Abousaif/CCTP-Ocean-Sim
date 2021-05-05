using UnityEngine;

public class FastFourierTransform : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    ComputeBuffer buffer_dbg;
    int kernel_FFT2Dfunc256inv, kernel_DFT2Dfunc256inv;

    void Awake()
    {
        kernel_FFT2Dfunc256inv = shader.FindKernel("FFT2Dfunc256inv");
        kernel_DFT2Dfunc256inv = shader.FindKernel("DFT2Dfunc256inv");
    }

    // buffer is input, FFT result is output to buffer
    public void FFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer_dmy);
        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);

        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer", buffer_dmy);
        shader.SetBuffer(kernel_FFT2Dfunc256inv, "buffer_dmy", buffer);

        shader.Dispatch(kernel_FFT2Dfunc256inv, 256, 1, 1);
    }


    // buffer is input, FFT result is output to buffer_dmy
    public void DFT2D_256_Dispatch(ComputeBuffer buffer, ComputeBuffer buffer_dmy)
    {
        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer", buffer);
        shader.SetBuffer(kernel_DFT2Dfunc256inv, "buffer_dmy", buffer_dmy);

        shader.Dispatch(kernel_DFT2Dfunc256inv, 256, 1, 1);
    }
}
