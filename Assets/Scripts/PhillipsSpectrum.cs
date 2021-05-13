using UnityEngine;

public class PhillipsSpectrum : MonoBehaviour
{
    public const uint meshSize = 256;
    public const uint Lx = meshSize * 5 / 2;
    public const uint Lz = meshSize * 5 / 2;

    public float windSpeed = 30.0f;
    //Angle of wind in radians
    public float windDir = Mathf.PI * 1.234f;
    //Gravitational constant
    public float gravity = 9.81f;
    //Wave scale factor / Constant
    private float A = 0.00000121f;

    //Generates Phillips spectrum
    //(Kx, Ky) - normalized wave vector
    float GeneratePhillips(float Kx, float Ky)
    {
        float k2Mag = Kx * Kx + Ky * Ky;

        float k4Mag = k2Mag * k2Mag;

        //Largest possible wave from constant wind speed
        float L = windSpeed * windSpeed / gravity;

        float k_x = Kx / Mathf.Sqrt(k2Mag);
        float k_y = Ky / Mathf.Sqrt(k2Mag);
        //Makes wave direction stand out more
        float w_dot_k = k_x * Mathf.Cos(windDir) + k_y * Mathf.Sin(windDir);

        //Multiply waves with a tiny constant as Tessendorf stated that the model has poor convergence properties at high values
        float l2 = (L / 1000) * (L / 1000);

        float phillips = A * Mathf.Exp(-1.0f / (k2Mag * L * L)) / k4Mag * w_dot_k * w_dot_k * Mathf.Exp(-k2Mag * l2);
        return phillips;
    }

    //Generate heightfield from input frequency
    public Vector2[] GenerateH0()
    {
        Vector2[] h0 = new Vector2[meshSize * meshSize];
        for (uint y = 0; y < meshSize; y++)
        {
            for (uint x = 0; x < meshSize; x++)
            {
                float kx = (-(int)meshSize / 2.0f + x) * (2.0f * Mathf.PI / Lx);
                float ky = (-(int)meshSize / 2.0f + y) * (2.0f * Mathf.PI / Lz);
                float P = GeneratePhillips(kx, ky);
                if (kx == 0.0f && ky == 0.0f)
                {
                    P = 0.0f;
                }

                uint i = y * meshSize + x;
                h0[i] = RandGaussian() * Mathf.Sqrt(P * 0.5f);
            }
        }
        return h0;
    }

    //Generates Gaussian random number for generating heightmap
    Vector2 RandGaussian()
    {
        var u1 = Random.value;
        var u2 = Random.value;
        var logU1 = -2f * Mathf.Log(u1);
        var sqrt = (logU1 <= 0f) ? 0f : Mathf.Sqrt(logU1);
        var theta = Mathf.PI * 2.0f * u2;
        var z0 = sqrt * Mathf.Cos(theta);
        var z1 = sqrt * Mathf.Sin(theta);
        return new Vector2(z0, z1);
    }
}
