using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power-up Settings")]
    [SerializeField] private GameObject[] powerUpPrefabs;  // Array of power-ups
    public float respawnDelay = 5f;
    public float powerUpDuration = 5f;

    [Header("Spawn Area")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Debug Info")]
    [SerializeField] private bool prefabValid = false;
    [SerializeField] private string prefabName = "None";

    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (powerUpPrefabs.Length > 0)
        {
            prefabValid = true;
            ScheduleNextSpawn(2f);
        }
        else
        {
            Debug.LogError("No power-up prefabs assigned!");
            prefabValid = false;
        }
    }

    public void OnPowerUpCollected()
    {
        Debug.Log("Power-up collected! Scheduling next spawn.");
        ScheduleNextSpawn(powerUpDuration + respawnDelay);
    }

    private void ScheduleNextSpawn(float delay)
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnAfterDelay(delay));
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        Debug.Log($"Waiting {delay} seconds before spawning next power-up.");
        yield return new WaitForSeconds(delay);

        if (powerUpPrefabs.Length > 0)
        {
            prefabValid = true;
            SpawnPowerUp();
        }
        else
        {
            prefabValid = false;
            Debug.LogError("No power-up prefabs available!");
        }

        spawnCoroutine = null;
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0)
        {
            Debug.LogError("Cannot spawn power-up: No prefabs assigned!");
            return;
        }

        // Select a random power-up prefab
        GameObject selectedPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        Vector3 position = GetRandomPosition();
        Debug.Log($"Spawning power-up {selectedPrefab.name} at position: {position}");

        try
        {
            GameObject newPowerUp = Instantiate(selectedPrefab, position, Quaternion.identity);
            PowerUp powerUpScript = newPowerUp.GetComponent<PowerUp>();
            if (powerUpScript != null)
            {
                powerUpScript.SetManager(this);
            }
            else
            {
                Debug.LogError("Spawned power-up doesn't have PowerUp component!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning power-up: {e.Message}");
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Mathf.Round(Random.Range(minX, maxX));
        float y = Mathf.Round(Random.Range(minY, maxY));

        if (x == 0 && y == 0)
        {
            x = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        return new Vector3(x, y, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}
