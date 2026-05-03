using UnityEngine;

public class SimpleNoiseFilter : INoiseFilter
{
    Noise noise = new Noise();
    NoiseSettings.SimpleNoiseSettings settings;
    public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
    {
        this.settings = settings;
    }
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        for(int i =0; i< settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += (v+1)*.5f *amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistance;
        }
        //     (noise.Evaluate(point * settings.roughness +settings.centre) + 1) * .5f;
        //noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        noiseValue = noiseValue - settings.minValue;

        return noiseValue * settings.strength;
    }
}
