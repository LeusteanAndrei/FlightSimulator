using UnityEngine;
[System.Serializable]
public class NoiseSettings
{
    public float strength = 1;
    public float roughness = 2;
    public Vector3 centre;
    [Range(1, 8)]
    public int numLayers = 1;
    public float persistance = .5f;
    public float baseRoughness = 1;
    public float minValue;
}
