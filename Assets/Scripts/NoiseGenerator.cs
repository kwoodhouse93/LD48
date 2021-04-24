using UnityEngine;

public class NoiseGenerator
{
    public static float[] Generate(int height, float scale, Wave[] waves, float offset)
    {
        float[] noiseMap = new float[height];

        for (int i = 0; i < height; i++)
        {
            float samplePos = (float)i * scale + offset;

            float normalization = 0.0f;

            foreach (Wave wave in waves)
            {
                noiseMap[i] += wave.amplitude * Mathf.PerlinNoise(samplePos * wave.frequency + wave.seed, 0f);
                normalization += wave.amplitude;
            }

            noiseMap[i] /= normalization;
        }

        return noiseMap;
    }
}

[System.Serializable]
public struct Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}
