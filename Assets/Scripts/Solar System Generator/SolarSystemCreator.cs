using UnityEngine;
using System.Collections.Generic;

public class SolarSystemCreator : MonoBehaviour
{
    public List<SolarSystemPosition> solarSystems = new List<SolarSystemPosition>();
    public Transform playerShuttle;
    public GameObject solarSystemPrefab;

    [SerializeField] private float maxDistanceFromClosestSystem = 500f; // If player gets this far from closest system, spawn a new one
    [SerializeField] private float spawnBuffer = 50f; // Extra distance to add between systems to ensure no overlap
    [SerializeField] private float systemHeightOffset = 0f; // Y offset to keep all systems at same height
    [SerializeField] private float spawnCooldown = 5f; // Seconds to wait after spawning before another spawn
    
    private Vector3 playerLastPosition;
    private float spawnCheckTimer = 0f;
    [SerializeField] private float spawnCheckInterval = 1f;

    private float lastSpawnTime = -9999f;

    void Start()
    {
        if (playerShuttle == null)
        {
            playerShuttle = transform;
        }
        playerLastPosition = playerShuttle.position;
    }

    void Update()
    {
        spawnCheckTimer -= Time.deltaTime;
        if (spawnCheckTimer <= 0)
        {
            spawnCheckTimer = spawnCheckInterval;
            CheckAndSpawnNewSystem();
        }
    }

    private void CheckAndSpawnNewSystem()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        if (solarSystems.Count == 0 || playerShuttle == null)
            return;

        float closestDistance = float.MaxValue;
        SolarSystemPosition closestSystem = null;
        foreach (var system in solarSystems)
        {
            float distance = Vector3.Distance(playerShuttle.position, system.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSystem = system;
            }
        }

        if (closestSystem == null)
            return;

        if (closestDistance > maxDistanceFromClosestSystem)
        {
            Vector3 spawnDirection = playerShuttle.position - closestSystem.transform.position;
            spawnDirection.y = 0f;
            spawnDirection = spawnDirection.normalized;

            float spawnDistance = maxDistanceFromClosestSystem + spawnBuffer;
            Vector3 spawnPosition = closestSystem.transform.position + spawnDirection * spawnDistance;
            spawnPosition.y = closestSystem.transform.position.y + systemHeightOffset;

        
            if (IsSpawnPositionClear(spawnPosition))
            {
                if (SpawnNewSolarSystem(spawnPosition))
                {
                    lastSpawnTime = Time.time;
                }
            }
        }

        playerLastPosition = playerShuttle.position;
    }

    private bool SpawnNewSolarSystem(Vector3 position)
    {
        if (solarSystemPrefab == null)
        {
            Debug.LogError("SolarSystemCreator: solarSystemPrefab not assigned!");
            return false;
        }

        GameObject newSunObject = Instantiate(solarSystemPrefab, position, Quaternion.identity);
        newSunObject.name = "SolarSystem_" + solarSystems.Count;

        SolarSystemPosition newSystemPos = newSunObject.GetComponent<SolarSystemPosition>();
        if (newSystemPos == null)
        {
            newSystemPos = newSunObject.AddComponent<SolarSystemPosition>();
        }
        solarSystems.Add(newSystemPos);

        SolarSystemGeneratorSun generator = newSunObject.GetComponent<SolarSystemGeneratorSun>();
        if (generator != null)
        {
            generator.GenerateSystem();
            Debug.Log($"Spawned new solar system at {position}. Total systems: {solarSystems.Count}");
            return true;
        }
        else
        {
            Debug.LogWarning("SolarSystemCreator: New sun doesn't have SolarSystemGeneratorSun component!");
            return true;
        }

        return true;
    }

    private bool IsSpawnPositionClear(Vector3 position)
    {
        for (int i = 0; i < solarSystems.Count; i++)
        {
            SolarSystemPosition system = solarSystems[i];
            if (system == null)
                continue;

            float requiredDistance = maxDistanceFromClosestSystem + spawnBuffer;
            if (Vector3.Distance(position, system.transform.position) < requiredDistance)
            {
                return false;
            }
        }

        return true;
    }
}
