using UnityEngine;
using System.Collections.Generic;

public class GrassRowRegenerator : MonoBehaviour
{
    public GameObject grassPrefab;
    public GameObject roadPrefab;
    public Transform playerTransform;
    public GameObject[] carPrefabs;
    public GameObject waterPrefab;
    public GameObject[] logPrefabs;

    public int initialBottomRow = -6;
    public int totalRows = 16;
    public float tileHeight = 1f;
    public int triggerRowIndex = 2; 

    private List<GameObject> activeRows = new List<GameObject>();
    private int topRowIndex;
    private int lastLogPrefabIndex = -1;

    void Start()
    {
        topRowIndex = initialBottomRow + totalRows - 1;

        for (int i = initialBottomRow; i <= topRowIndex; i++)
        {
            if (i <= 1) 
                SpawnRow(i, forceType: 0); 
            else
                SpawnRow(i);
        }

    }

    void Update()
    {
        int currentRow = Mathf.FloorToInt(playerTransform.position.y / tileHeight);
        Debug.Log("Player is in row: " + currentRow);
        if (currentRow >= triggerRowIndex)
        {
            ShiftEnvironmentDown();
        }
    }

    void SpawnRow(int rowIndex, int? forceType = null)
    {
        Vector3 position = new Vector3(0, rowIndex * tileHeight, 0);
        int type = forceType.HasValue ? forceType.Value : Random.Range(0, 3);
        GameObject prefabToSpawn = type == 0 ? grassPrefab : type == 1 ? roadPrefab : waterPrefab;
        GameObject row = Instantiate(prefabToSpawn, position, Quaternion.identity, transform);
        activeRows.Add(row);

        if (prefabToSpawn == roadPrefab)
        {
            SpawnCarsOnRoad(row.transform);
        }
        else if (prefabToSpawn == waterPrefab)
        {
            SpawnLogsOnWater(row.transform);
        }
    }



    void SpawnCarsOnRoad(Transform roadParent)
    {
        GameObject selectedCar = carPrefabs[Random.Range(0, carPrefabs.Length)];
        float[] laneOffsets = { -6f, -3f, 0f, 3f, 6f };
        float laneX = laneOffsets[Random.Range(0, laneOffsets.Length)];

        Vector3 carPosition = new Vector3(laneX, roadParent.position.y, 0f);
        GameObject car = Instantiate(selectedCar, carPosition, Quaternion.identity);
        car.transform.SetParent(roadParent); 
    }


    void SpawnLogsOnWater(Transform waterParent)
    {

        int newLogIndex;
        if (logPrefabs.Length == 1)
        {
            newLogIndex = 0;
        }
        else
        {
            do
            {
                newLogIndex = Random.Range(0, logPrefabs.Length);
            } while (newLogIndex == lastLogPrefabIndex);
        }

        lastLogPrefabIndex = newLogIndex;
        GameObject selectedLogPrefab = logPrefabs[newLogIndex];

        float[] laneOffsets = { -6f, -3f, 0f, 3f, 6f };
        int numLogs = Random.Range(2, Mathf.Min(4, laneOffsets.Length + 1));

        float speed = Random.Range(0.8f, 1.2f) * (Random.value < 0.5f ? 1 : -1);

        List<float> availableOffsets = new List<float>(laneOffsets);

        for (int i = 0; i < numLogs; i++)
        {
            int laneIndex = Random.Range(0, availableOffsets.Count);
            float laneX = availableOffsets[laneIndex];
            availableOffsets.RemoveAt(laneIndex);

            Vector3 logPosition = new Vector3(laneX, waterParent.position.y, 0f);
            GameObject log = Instantiate(selectedLogPrefab, logPosition, Quaternion.identity);
            log.transform.SetParent(waterParent);

            LogMover mover = log.GetComponent<LogMover>();
            if (mover == null)
            {
                mover = log.AddComponent<LogMover>();
            }

            mover.speed = speed;
        }
    }


    void ShiftEnvironmentDown()
    {

        GameObject bottomRow = activeRows[0];
        activeRows.RemoveAt(0);
        Destroy(bottomRow);

        topRowIndex++;
        SpawnRow(topRowIndex);

        Vector3 shift = new Vector3(0, -tileHeight, 0);
        foreach (GameObject row in activeRows)
        {
            row.transform.position += shift;
        }

        if (playerTransform.parent == null || playerTransform.parent.gameObject.layer != LayerMask.NameToLayer("Platform"))
        {
            playerTransform.position += shift;
        }

        topRowIndex--;
    }
}
