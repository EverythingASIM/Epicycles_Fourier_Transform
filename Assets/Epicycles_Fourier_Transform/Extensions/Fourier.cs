using System.Collections.Generic;
using UnityEngine;

public static class Fourier
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Discrete_Fourier_transform
    /// Compute the inverse discreate fourier transform of a sequence (path). 
    /// In this interpretation, the result is a list of complex number that 
    /// encodes both amplitude and phase of a complex sinusoidal component 
    /// returns a list of sine waves, from the freq, amp and phase
    /// </summary>
    public static List<(float, float, float, float, float)> IDFT(List<float> x)
    {
        int N = x.Count;
        var X = new List<(float, float, float, float, float)>();

        for (int k = 0; k < N; k++)
        {
            float Re = 0;
            float Im = 0;
            for (int n = 0; n < N; n++)
            {
                var phi = (2 * Mathf.PI * k * n) / N;
                Re += x[n] * Mathf.Cos(phi);
                Im -= x[n] * Mathf.Sin(phi);
            }
            Re /= N; // Real Result of IDFT
            Im /= N; // Imag Result of IDFT

            float Freq = k;
            float Amp = Mathf.Sqrt(Re * Re + Im * Im);
            float Phase = Mathf.Atan2(Im, Re);

            X.Add((Re, Im, Freq, Amp, Phase));
        }
        return X;
    }
}
