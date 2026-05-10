using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SolarSystemGeneratorSun : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private bool generateOnAwake = true;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 0;

    [Header("Planet Count")]
    [SerializeField] private Vector2Int planetCountRange = new Vector2Int(2, 7);

    [Header("Planet Mass")]
    [SerializeField] private Vector2 planetMassPercentRange = new Vector2(0.001f, 0.1f);
    [SerializeField] private float planetRadiusExponent = 0.3333333f;
    [SerializeField] private float planetScaleAtOneEarthMass = 1f;

    [Header("Orbit Layout")]
    [SerializeField] private float orbitDistanceMultiplier = 8f;
    [SerializeField] private float orbitDistanceMassExponent = 0.12f;
    [SerializeField] private float orbitDistanceStepMultiplier = 1.8f;
    [SerializeField] private float orbitTiltDegrees = 8f;

    [Header("Spawn")]
    [SerializeField] private Transform systemRoot;

    private readonly List<GameObject> spawnedPlanets = new List<GameObject>();

    private void Awake()
    {
        if (generateOnAwake)
        {
            GenerateSystem();
        }
    }

    public void GenerateSystem()
    {
        if (planetPrefab == null)
        {
            Debug.LogWarning("SolarSystemGeneratorSun needs a planetPrefab assigned.", this);
            return;
        }

        DestroySpawnedPlanets();

        int actualSeed = useRandomSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random random = new System.Random(actualSeed);

        int minPlanets = Mathf.Max(2, planetCountRange.x);
        int maxPlanets = Mathf.Max(minPlanets, planetCountRange.y);
        int planetCount = random.Next(minPlanets, maxPlanets + 1);

        Transform parentTransform = systemRoot != null ? systemRoot : null;
        float previousOrbitDistance = 0f;

        for (int i = 0; i < planetCount; i++)
        {
            GameObject planetObj = parentTransform != null ? Instantiate(planetPrefab, parentTransform) : Instantiate(planetPrefab);
            planetObj.name = $"Planet_{i + 1}";

            float planetMassPercent = RandomRangeLog(random, planetMassPercentRange.x, planetMassPercentRange.y);
            float planetMass = Mathf.Max(0.0001f, GetSunMass() * planetMassPercent);
            float planetRadius = Mathf.Max(0.1f, planetScaleAtOneEarthMass * Mathf.Pow(planetMass, planetRadiusExponent));

            float massDistanceScale = Mathf.Pow(GetSunMass(), orbitDistanceMassExponent);
            float minDistance = planetRadius + previousOrbitDistance;
            float orbitDistance = minDistance + (planetRadius * orbitDistanceMultiplier * massDistanceScale) + (i * orbitDistanceStepMultiplier);

            Vector3 orbitNormal = GetOrbitNormal(random, orbitTiltDegrees);
            Vector3 orbitDirection = RandomDirectionInPlane(random, orbitNormal);
            planetObj.transform.position = transform.position + orbitDirection * orbitDistance;

            SolarSystemPlanet generator = planetObj.GetComponent<SolarSystemPlanet>();
            if (generator != null)
            {
                generator.ConfigureMassOverride(planetMass, true);
                generator.GenerateRandomPlanet();
            }

            OrbitScript orbitScript = planetObj.GetComponent<OrbitScript>();
            if (orbitScript == null)
            {
                orbitScript = planetObj.AddComponent<OrbitScript>();
            }

            orbitScript.ConfigurePassiveOrbit(GetComponent<GravitySource>(), orbitNormal, random.NextDouble() > 0.5);
            if (generator != null)
            {
                generator.RefreshMoonOrbits();
            }

            spawnedPlanets.Add(planetObj);
            previousOrbitDistance = orbitDistance;
        }
    }

    private float GetSunMass()
    {
        GravitySource sunGravity = GetComponent<GravitySource>();
        return sunGravity != null ? Mathf.Max(0.0001f, sunGravity.Mass) : 1f;
    }

    private void DestroySpawnedPlanets()
    {
        for (int i = 0; i < spawnedPlanets.Count; i++)
        {
            if (spawnedPlanets[i] != null)
            {
                Destroy(spawnedPlanets[i]);
            }
        }

        spawnedPlanets.Clear();
    }

    private Vector3 GetOrbitNormal(System.Random random, float maxTiltDegrees)
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

    private Vector3 RandomDirectionInPlane(System.Random random, Vector3 normal)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
        {
            tangent = Vector3.Cross(normal, Vector3.right);
        }

        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
        float angle = Mathf.Lerp(0f, Mathf.PI * 2f, NextFloat(random));
        return (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent).normalized;
    }

    private float RandomRangeLog(System.Random random, float min, float max)
    {
        min = Mathf.Max(0.0001f, min);
        max = Mathf.Max(min, max);
        return Mathf.Exp(Mathf.Lerp(Mathf.Log(min), Mathf.Log(max), NextFloat(random)));
    }

    private float NextFloat(System.Random random)
    {
        return (float)random.NextDouble();
    }
}
