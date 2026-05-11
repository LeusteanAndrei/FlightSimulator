using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GravitySource))]
[RequireComponent(typeof(PlanetScript))]
public class SolarSystemPlanet : MonoBehaviour
{
    private static int moonSpawnContext;

    [SerializeField] private bool generateOnAwake = true;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 0;

    [SerializeField] private Vector2 massRange = new Vector2(0.05f, 500f);
    [SerializeField] private float scaleAtOneEarthMass = 1f;
    [SerializeField] private float massToScaleExponent = 0.3333333f;
    [SerializeField] private float visualScaleMultiplier = 1f;

    [SerializeField] private bool generateMoons = true;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private Vector2Int moonCountRange = new Vector2Int(0, 3);
    [SerializeField] private Vector2 moonMassPercentRange = new Vector2(0.001f, 0.1f);
    [SerializeField] private float moonOrbitDistanceMultiplier = 3.5f;
    [SerializeField] private float moonDistanceMassExponent = 0.12f;
    [SerializeField] private float moonDistanceStepMultiplier = 0.9f;
    [SerializeField] private float moonOrbitTiltDegrees = 10f;

    [SerializeField] private Vector2 noiseStrengthMultiplierRange = new Vector2(0.6f, 2f);
    [SerializeField] private Vector2 noiseRoughnessMultiplierRange = new Vector2(0.75f, 1.5f);
    [SerializeField] private Vector2 biomeNoiseStrengthRange = new Vector2(0.25f, 2f);
    [SerializeField] private Vector2 biomeNoiseOffsetRange = new Vector2(-1f, 1f);

    private GravitySource gravitySource;
    private PlanetScript planetScript;
    private readonly List<GameObject> spawnedMoons = new List<GameObject>();
    private bool useMassOverride;
    private float massOverride;
    private bool isMoon;

    private void Awake()
    {
        if (moonSpawnContext > 0)
        {
            generateOnAwake = false;
            generateMoons = false;
        }

        gravitySource = GetComponent<GravitySource>();
        planetScript = GetComponent<PlanetScript>();

        if (generateOnAwake)
        {
            GenerateRandomPlanet();
        }
    }

    public void GenerateRandomPlanet()
    {
        if (planetScript == null)
        {
            planetScript = GetComponent<PlanetScript>();
        }

        if (gravitySource == null)
        {
            gravitySource = GetComponent<GravitySource>();
        }

        if (planetScript == null || gravitySource == null)
        {
            Debug.LogWarning("SolarSystemPlanet requires both PlanetScript and GravitySource.", this);
            return;
        }

        int actualSeed = useRandomSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random random = new System.Random(actualSeed);

        ShapeSettings shapeSettings = CloneShapeSettings(planetScript.shapeSettings);
        ColourSettings colourSettings = CloneColourSettings(planetScript.colourSettings);

        if (shapeSettings == null || colourSettings == null)
        {
            Debug.LogWarning("SolarSystemPlanet needs shape and colour settings assigned on PlanetScript.", this);
            return;
        }

        float massMultiplier = useMassOverride ? Mathf.Max(0.0001f, massOverride) : RandomRangeLog(random, massRange.x, massRange.y);
        float scaleMultiplier = Mathf.Max(0.1f, scaleAtOneEarthMass * Mathf.Pow(massMultiplier, massToScaleExponent) * visualScaleMultiplier);

        DestroySpawnedMoons();

        shapeSettings.planetRadius = scaleMultiplier;
        RandomizeShapeSettings(shapeSettings, random);
        RandomizeColourSettings(colourSettings, random);

        planetScript.shapeSettings = shapeSettings;
        planetScript.colourSettings = colourSettings;
        planetScript.GeneratePlanet();

        transform.localScale = Vector3.one;

        gravitySource.SetMass(massMultiplier);

        if (!isMoon)
        {
            name = $"Planet_{actualSeed}";
        }

        if (generateMoons)
        {
            SpawnMoons(random, massMultiplier, scaleMultiplier);
        }
    }

    public void ConfigureAsMoon(float absoluteMass)
    {
        isMoon = true;
        ConfigureMassOverride(absoluteMass, false);
    }

    public void ConfigureMassOverride(float absoluteMass, bool shouldGenerateMoons)
    {
        useMassOverride = true;
        massOverride = Mathf.Max(0.0001f, absoluteMass);
        generateMoons = shouldGenerateMoons;
        generateOnAwake = false;
    }

    public void RefreshMoonOrbits()
    {
        if (gravitySource == null)
        {
            gravitySource = GetComponent<GravitySource>();
        }

        if (gravitySource == null)
        {
            return;
        }

        System.Random random = new System.Random(unchecked(GetInstanceID() * 397) ^ Time.frameCount);
        for (int i = 0; i < spawnedMoons.Count; i++)
        {
            GameObject moonObj = spawnedMoons[i];
            if (moonObj == null)
            {
                continue;
            }

            OrbitScript orbitScript = moonObj.GetComponent<OrbitScript>();
            if (orbitScript == null)
            {
                orbitScript = moonObj.AddComponent<OrbitScript>();
            }

            Vector3 orbitNormal = GetMostlyHorizontalOrbitNormal(random, moonOrbitTiltDegrees);
            orbitScript.ConfigurePassiveOrbit(gravitySource, orbitNormal.normalized, random.NextDouble() > 0.5);
        }
    }

    private void DestroySpawnedMoons()
    {
        for (int i = 0; i < spawnedMoons.Count; i++)
        {
            if (spawnedMoons[i] != null)
            {
                Destroy(spawnedMoons[i]);
            }
        }

        spawnedMoons.Clear();
    }

    private void SpawnMoons(System.Random random, float planetMass, float planetRadius)
    {
        if (moonPrefab == null)
        {
            return;
        }

        int minMoons = Mathf.Max(0, moonCountRange.x);
        int maxMoons = Mathf.Max(minMoons, moonCountRange.y);
        int moonCount = random.Next(minMoons, maxMoons + 1);

        float minPercent = Mathf.Max(0.00001f, Mathf.Min(moonMassPercentRange.x, moonMassPercentRange.y));
        float maxPercent = Mathf.Max(minPercent, Mathf.Max(moonMassPercentRange.x, moonMassPercentRange.y));

        for (int i = 0; i < moonCount; i++)
        {
            float moonPercent = RandomRangeLog(random, minPercent, maxPercent);
            float moonMass = Mathf.Max(0.0001f, planetMass * moonPercent);
            float moonRadiusEstimate = Mathf.Max(0.05f, scaleAtOneEarthMass * Mathf.Pow(moonMass, massToScaleExponent) * visualScaleMultiplier);

            float massDistanceScale = Mathf.Pow(planetMass, moonDistanceMassExponent);
            float minDistance = planetRadius + moonRadiusEstimate;
            float spacingFactor = moonOrbitDistanceMultiplier + (i * moonDistanceStepMultiplier);
            float orbitDistance = minDistance + (planetRadius * spacingFactor * massDistanceScale);

            float hillLimit = float.MaxValue;
            if (GravityManager.Instance != null)
            {
                GravitySource dominant = null;
                foreach (var src in GravityManager.Instance.GetSources())
                {
                    if (src == gravitySource) continue;
                    if (dominant == null || src.Mass > dominant.Mass)
                        dominant = src;
                }

                if (dominant != null)
                {
                    float a = Vector3.Distance(transform.position, dominant.transform.position);
                    float M = Mathf.Max(1e-9f, dominant.Mass);
                    float m = Mathf.Max(1e-9f, planetMass);
                    float hillRadius = a * Mathf.Pow(m / (3f * M), 1f / 3f);
                    hillLimit = hillRadius * 0.4f;
                }
            }

            if (orbitDistance > hillLimit)
            {
                orbitDistance = Mathf.Max(minDistance, hillLimit * 0.9f);
            }

            Vector3 direction = RandomInsideUnitSphere(random);
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.right;
            }
            direction.Normalize();

            Vector3 moonPosition = transform.position + direction * orbitDistance;
            moonSpawnContext++;
            GameObject moonObj = Instantiate(moonPrefab, moonPosition, Quaternion.identity);
            moonSpawnContext--;
            moonObj.name = $"Moon_{i + 1}";

            SolarSystemPlanet moonGenerator = moonObj.GetComponent<SolarSystemPlanet>();
            if (moonGenerator != null)
            {
                moonGenerator.ConfigureAsMoon(moonMass);
                moonGenerator.GenerateRandomPlanet();
            }

            OrbitScript orbitScript = moonObj.GetComponent<OrbitScript>();
            if (orbitScript == null)
            {
                orbitScript = moonObj.AddComponent<OrbitScript>();
            }

            Vector3 orbitNormal = GetMostlyHorizontalOrbitNormal(random, moonOrbitTiltDegrees);
            orbitScript.ConfigurePassiveOrbit(gravitySource, orbitNormal.normalized, random.NextDouble() > 0.5);
            spawnedMoons.Add(moonObj);
        }
    }

    public Vector3 GetMostlyHorizontalOrbitNormal(System.Random random, float maxTiltDegrees)
    {
        float tiltRadians = Mathf.Deg2Rad * Mathf.Abs(maxTiltDegrees);
        float tilt = Mathf.Lerp(-tiltRadians, tiltRadians, NextFloat(random));
        float azimuth = Mathf.Lerp(0f, Mathf.PI * 2f, NextFloat(random));

        Vector3 tiltedNormal = new Vector3(
            Mathf.Sin(azimuth) * Mathf.Sin(tilt),
            Mathf.Cos(tilt),
            Mathf.Cos(azimuth) * Mathf.Sin(tilt));

        if (tiltedNormal.sqrMagnitude < 0.0001f)
        {
            return Vector3.up;
        }

        return tiltedNormal.normalized;
    }

    private ShapeSettings CloneShapeSettings(ShapeSettings source)
    {
        if (source == null)
        {
            return null;
        }

        return Instantiate(source);
    }

    private ColourSettings CloneColourSettings(ColourSettings source)
    {
        if (source == null)
        {
            return null;
        }

        ColourSettings clone = Instantiate(source);
        if (source.planetMaterial != null)
        {
            clone.planetMaterial = new Material(source.planetMaterial);
        }

        return clone;
    }

    private void RandomizeShapeSettings(ShapeSettings shapeSettings, System.Random random)
    {
        if (shapeSettings.noiseLayers == null)
        {
            return;
        }

        for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
        {
            ShapeSettings.NoiseLayer layer = shapeSettings.noiseLayers[i];
            if (layer == null || layer.noiseSettings == null)
            {
                continue;
            }

            layer.enabled = i == 0 || NextFloat(random) > 0.2f;
            if (i > 0)
            {
                layer.useFirstlayerAsMask = NextFloat(random) > 0.5f;
            }

            RandomizeNoiseSettings(layer.noiseSettings, random);
        }
    }

    private void RandomizeNoiseSettings(NoiseSettings noiseSettings, System.Random random)
    {
        if (noiseSettings == null)
        {
            return;
        }

        if (noiseSettings.filterType == NoiseSettings.FilterType.Simple && noiseSettings.simpleNoiseSettings != null)
        {
            RandomizeSimpleNoise(noiseSettings.simpleNoiseSettings, random);
        }
        else if (noiseSettings.filterType == NoiseSettings.FilterType.Rigid && noiseSettings.rigidNoiseSettings != null)
        {
            RandomizeSimpleNoise(noiseSettings.rigidNoiseSettings, random);
            noiseSettings.rigidNoiseSettings.weightMultiplier = Mathf.Clamp01(noiseSettings.rigidNoiseSettings.weightMultiplier * Mathf.Lerp(0.7f, 1.4f, NextFloat(random)));
        }
    }

    private void RandomizeSimpleNoise(NoiseSettings.SimpleNoiseSettings settings, System.Random random)
    {
        settings.strength *= Mathf.Lerp(noiseStrengthMultiplierRange.x, noiseStrengthMultiplierRange.y, NextFloat(random));
        settings.roughness *= Mathf.Lerp(noiseRoughnessMultiplierRange.x, noiseRoughnessMultiplierRange.y, NextFloat(random));
        settings.baseRoughness *= Mathf.Lerp(0.75f, 1.5f, NextFloat(random));
        settings.persistance = Mathf.Clamp01(settings.persistance * Mathf.Lerp(0.75f, 1.25f, NextFloat(random)));
        settings.minValue *= Mathf.Lerp(0.5f, 2f, NextFloat(random));
        settings.numLayers = Mathf.Clamp(settings.numLayers + (int)Mathf.Round(RandomRange(random, -1f, 1f)), 1, 8);
        settings.centre = RandomInsideUnitSphere(random) * Mathf.Lerp(0.25f, 2f, NextFloat(random));
    }

    private void RandomizeColourSettings(ColourSettings colourSettings, System.Random random)
    {
        RandomizeOceanColour(colourSettings, random);

        if (colourSettings.biomeColourSettings != null)
        {
            colourSettings.biomeColourSettings.noiseStrength = Mathf.Lerp(biomeNoiseStrengthRange.x, biomeNoiseStrengthRange.y, NextFloat(random));
            colourSettings.biomeColourSettings.noiseOffset = Mathf.Lerp(biomeNoiseOffsetRange.x, biomeNoiseOffsetRange.y, NextFloat(random));

            ColourSettings.BiomeColourSettings.Biome[] biomes = colourSettings.biomeColourSettings.biomes;
            if (biomes != null && biomes.Length > 0)
            {
                float step = biomes.Length > 1 ? 1f / (biomes.Length - 1) : 1f;
                for (int i = 0; i < biomes.Length; i++)
                {
                    ColourSettings.BiomeColourSettings.Biome biome = biomes[i];
                    if (biome == null)
                    {
                        continue;
                    }

                    float hue = NextFloat(random);
                    float saturation = Mathf.Lerp(0.45f, 0.95f, NextFloat(random));
                    float value = Mathf.Lerp(0.5f, 1f, NextFloat(random));
                    biome.tint = Color.HSVToRGB(hue, saturation, value);
                    biome.tintPercent = Mathf.Clamp01(biome.tintPercent * Mathf.Lerp(0.6f, 1.4f, NextFloat(random)));

                    float offset = Mathf.Lerp(-0.15f, 0.15f, NextFloat(random));
                    biome.startHeight = Mathf.Clamp01((i * step) + offset);
                }

                Array.Sort(biomes, delegate (ColourSettings.BiomeColourSettings.Biome left, ColourSettings.BiomeColourSettings.Biome right)
                {
                    if (left == null && right == null)
                    {
                        return 0;
                    }

                    if (left == null)
                    {
                        return 1;
                    }

                    if (right == null)
                    {
                        return -1;
                    }

                    return left.startHeight.CompareTo(right.startHeight);
                });
            }
        }

        if (colourSettings.planetMaterial != null)
        {
            colourSettings.planetMaterial.SetColor("_BaseColor", Color.HSVToRGB(NextFloat(random), Mathf.Lerp(0.4f, 0.9f, NextFloat(random)), Mathf.Lerp(0.6f, 1f, NextFloat(random))));
        }
    }

    private void RandomizeOceanColour(ColourSettings colourSettings, System.Random random)
    {
        Gradient gradient = new Gradient();

        float shallowHue = NextFloat(random);
        float deepHue = Mathf.Repeat(shallowHue + Mathf.Lerp(-0.08f, 0.08f, NextFloat(random)), 1f);

        Color shallowColor = Color.HSVToRGB(
            shallowHue,
            Mathf.Lerp(0.45f, 0.8f, NextFloat(random)),
            Mathf.Lerp(0.65f, 1f, NextFloat(random)));

        Color deepColor = Color.HSVToRGB(
            deepHue,
            Mathf.Lerp(0.55f, 0.95f, NextFloat(random)),
            Mathf.Lerp(0.25f, 0.65f, NextFloat(random)));

        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(shallowColor, 0f),
            new GradientColorKey(deepColor, 1f)
        };

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        gradient.SetKeys(colorKeys, alphaKeys);
        colourSettings.oceanColour = gradient;
    }

    private float RandomRangeLog(System.Random random, float min, float max)
    {
        min = Mathf.Max(0.0001f, min);
        max = Mathf.Max(min, max);

        float logMin = Mathf.Log(min);
        float logMax = Mathf.Log(max);
        return Mathf.Exp(Mathf.Lerp(logMin, logMax, NextFloat(random)));
    }

    private float RandomRange(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }

    private float NextFloat(System.Random random)
    {
        return (float)random.NextDouble();
    }

    private Vector3 RandomInsideUnitSphere(System.Random random)
    {
        Vector3 value = new Vector3(
            RandomRange(random, -1f, 1f),
            RandomRange(random, -1f, 1f),
            RandomRange(random, -1f, 1f));

        if (value.sqrMagnitude > 1f)
        {
            value.Normalize();
        }

        return value;
    }
}
