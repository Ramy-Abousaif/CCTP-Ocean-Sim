using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhillipsSpectrum : MonoBehaviour
{
    public const uint meshSize = 256;
    public const uint seaSizeLX = meshSize * 5 / 2;
    public const uint seaSizeLZ = meshSize * 5 / 2;
    //Gravitational constant
    public float G = 9.81F;
    //Wave scale factor
    public float A = 0.000001f;

    public float windSpeed = 30.0f;
    public float windDir = Mathf.PI * 1.234f;

    //Random number sampling method (Box-Muller Transform)
    Vector2  RandGauss()
    {
        var u1 = Random.value;
        var u2 = Random.value;
        var logU1 = -2F * Mathf.Log(u1);
        var sqrt = (logU1 <= 0f) ? 0f : Mathf.Sqrt(logU1);
        var theta = Mathf.PI * 2.0f * u2;
        var z0 = sqrt * Mathf.Cos(theta);
        var z1 = sqrt * Mathf.Sin(theta);
        return new Vector2(z0, z1);
    }

    public float GeneratePhillips(float kX, float kY)
    {
        float k2Mag = kX * kX + kY * kY;

        if (k2Mag == 0.0f)
        {
            return 0.0f;
        }

        float k4Mag = k2Mag * k2Mag;

        //Largest possible wave from constant wind of velocity v 
        float L = windSpeed * windSpeed / G;

        float k_x = kX / Mathf.Sqrt(k2Mag);
        float k_y = kY / Mathf.Sqrt(k2Mag);
        float w_dot_k = k_x * Mathf.Cos(windDir) + k_y * Mathf.Sin(windDir);
        float phillips = A * Mathf.Exp(-1.0f / (k2Mag * L * L)) / k4Mag * w_dot_k * w_dot_k;

        //Damp out waves with very small length w << l 
        float l2 = (L / 1000) * (L / 1000);
        phillips *= Mathf.Exp(-k2Mag * l2);
        return phillips;
    }

    //Generate base heightfield in frequency space 
    public Vector2[] Generate_h0()
    {
        Vector2[] h0 = new Vector2[meshSize * meshSize];
        for (uint y = 0; y < meshSize; y++)
        {
            for (uint x = 0; x < meshSize; x++)
            {
                float kx = (-(int)meshSize / 2.0f + x) * (2.0f * Mathf.PI / seaSizeLX);
                float ky = (-(int)meshSize / 2.0f + y) * (2.0f * Mathf.PI / seaSizeLZ);
                float P = GeneratePhillips(kx, ky);
                if (kx == 0.0f && ky == 0.0f)
                {
                    P = 0.0f;
                }

                uint i = y * meshSize + x;
                h0[i] = RandGauss() * Mathf.Sqrt(P * 0.5f);
            }
        }
        return h0;
    }
}
