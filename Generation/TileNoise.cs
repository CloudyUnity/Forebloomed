using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNoise : MonoBehaviour
{
    public static float[,] Generate(int size, float scale, TileWave[] waves, Vector2 offset)
    {
        float[,] noiseMap = new float[size, size];
        float[] seeds = RandomSeed(waves.Length);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float samplePosX = x * scale + offset.x;
                float samplePosY = y * scale + offset.y;
                float normalization = 0.0f;

                for (int i = 0; i < waves.Length; i++)
                {
                    TileWave wave = waves[i];
                    noiseMap[x, y] += wave.Amplitude * Mathf.PerlinNoise(samplePosX * wave.Frequency + seeds[i], samplePosY * wave.Frequency + seeds[i]);
                    normalization += wave.Amplitude;
                }
                noiseMap[x, y] /= normalization;
            }
        }

        return noiseMap;
    }

    public static float[] RandomSeed(int length)
    {
        float[] array = new float[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = A_Extensions.RandomBetween(0, 300f, A_LevelManager.QuickSeed);
        }
        return array;
    }
}
