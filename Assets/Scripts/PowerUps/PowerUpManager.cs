using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
  [Header("Power-up Settings")]
  [SerializeField] private GameObject powerUpPrefab;
  public float respawnDelay = 5f;
  public float powerUpDuration = 5f;

  [Header("Spawn Area")]
  public float minX = -8f;
  public float maxX = 8f;
  public float minY = -4f;
  public float maxY = 4f;

  // For debugging
  [Header("Debug Info")]
  [SerializeField] private bool prefabValid = false;
  [SerializeField] private string prefabName = "None";

  private GameObject cachedPrefab;
  private Coroutine spawnCoroutine;

  private void OnValidate()
  {
    // Update debug info in editor
    if (powerUpPrefab != null)
    {
      prefabValid = true;
      prefabName = powerUpPrefab.name;
    }
    else
    {
      prefabValid = false;
      prefabName = "None";
    }
  }

  private void Awake()
  {
    Debug.Log("PowerUpManager Awake: Checking prefab reference...");

    // Log initial state
    if (powerUpPrefab != null)
    {
      Debug.Log($"Initial prefab reference is valid: {powerUpPrefab.name}");
      cachedPrefab = powerUpPrefab;
      prefabValid = true;
      prefabName = powerUpPrefab.name;
    }
    else
    {
      Debug.LogError("No power-up prefab assigned to PowerUpManager!");
      prefabValid = false;
      prefabName = "None";
    }
  }

  private void Start()
  {
    Debug.Log("PowerUpManager Start: Checking cached prefab reference...");

    // Double-check our cached reference
    if (cachedPrefab != null)
    {
      Debug.Log($"Cached prefab is valid: {cachedPrefab.name}");
      ScheduleNextSpawn(2f);
    }
    else
    {
      Debug.LogError("Cached prefab is null! Attempting recovery...");

      // Try to recover by using the original reference again
      if (powerUpPrefab != null)
      {
        Debug.Log("Recovered using original prefab reference");
        cachedPrefab = powerUpPrefab;
        ScheduleNextSpawn(2f);
      }
      else
      {
        Debug.LogError("Fatal error: Cannot spawn power-ups without a valid prefab!");
      }
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

    // Check prefab reference again
    if (cachedPrefab == null && powerUpPrefab != null)
    {
      Debug.Log("Cached prefab was lost but original reference exists. Recovering...");
      cachedPrefab = powerUpPrefab;
    }

    if (cachedPrefab != null)
    {
      prefabValid = true;
      prefabName = cachedPrefab.name;
      SpawnPowerUp();
    }
    else
    {
      prefabValid = false;
      prefabName = "None";
      Debug.LogError("Power-up prefab reference is null or destroyed!");
    }

    spawnCoroutine = null;
  }

  private void SpawnPowerUp()
  {
    // One final safety check before instantiating
    if (cachedPrefab == null)
    {
      Debug.LogError("Cannot spawn power-up: prefab reference is null!");
      return;
    }

    Vector3 position = GetRandomPosition();
    Debug.Log($"Spawning power-up at position: {position}");

    try
    {
      GameObject newPowerUp = Instantiate(cachedPrefab, position, Quaternion.identity);
      Debug.Log($"Successfully spawned power-up: {newPowerUp.name}");

      // Set up the power-up with a reference to this manager
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

    // Avoid spawning at (0,0) to ensure variety
    if (x == 0 && y == 0)
    {
      x = Random.Range(0, 2) == 0 ? -1 : 1;
    }

    return new Vector3(x, y, 0);
  }

  // Visualize the spawn area in the editor
  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.yellow;
    Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
    Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
    Gizmos.DrawWireCube(center, size);
  }
}