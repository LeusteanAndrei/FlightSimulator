using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject {
    public float planetRadius = 1;
    //public NoiseSettings noiseSettings;
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstlayerAsMask;
        public NoiseSettings noiseSettings;
    }

}
