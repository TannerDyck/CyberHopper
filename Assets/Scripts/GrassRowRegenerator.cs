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
    public GameObject[] turtlePrefabs;

    public int initialBottomRow = -6;
    public int totalRows = 16;
    public float tileHeight = 1f;
    public int triggerRowIndex = 2; 
    private int consecutiveWaterRows = 0;


    private List<GameObject> activeRows = new List<GameObject>();
    private int topRowIndex;
    private int lastLogPrefabIndex = -1;
    private float lastLogSpeed = 0f;

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

        int type;
        if (forceType.HasValue)
        {
            type = forceType.Value;
        }
        else
        {
            // Logic to limit water rows to max 2 in a row
            List<int> validTypes = new List<int> { 0, 1, 2 }; // grass, road, water
            if (consecutiveWaterRows >= 2)
            {
                validTypes.Remove(2); // Remove water
            }

            type = validTypes[Random.Range(0, validTypes.Count)];
        }

        // Instantiate row
        GameObject prefabToSpawn = type == 0 ? grassPrefab : type == 1 ? roadPrefab : waterPrefab;
        GameObject row = Instantiate(prefabToSpawn, position, Quaternion.identity, transform);
        activeRows.Add(row);

        // Handle row content
        if (prefabToSpawn == roadPrefab)
        {
            SpawnCarsOnRoad(row.transform);
            consecutiveWaterRows = 0;
        }
        else if (prefabToSpawn == waterPrefab)
        {
            SpawnLogsOnWater(row.transform);
            consecutiveWaterRows++;
        }
        else
        {
            consecutiveWaterRows = 0;
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
        // 🐢 Decide whether to spawn logs or turtles
        bool spawnTurtles = Random.value < 0.5f;

        GameObject[] prefabPool = spawnTurtles ? turtlePrefabs : logPrefabs;

        // 🔁 Choose a different prefab than the last one
        int newPrefabIndex;
        if (prefabPool.Length == 1)
        {
            newPrefabIndex = 0;
        }
        else
        {
            do
            {
                newPrefabIndex = Random.Range(0, prefabPool.Length);
            } while (newPrefabIndex == lastLogPrefabIndex);
        }

        lastLogPrefabIndex = newPrefabIndex;
        GameObject selectedPrefab = prefabPool[newPrefabIndex];

        // Direction: logs go left ➡️, turtles go right ⬅️
        float direction = spawnTurtles ? -1f : 1f;

        // Pick speed, avoiding being too similar to last
        float speed;
        float baseSpeed;
        int maxAttempts = 10;
        int attempts = 0;
        do
        {
            baseSpeed = Random.Range(1f, 2f); // Decent consistent speed range
            speed = baseSpeed * direction;
            attempts++;
        } 
        while ((Mathf.Abs(speed - lastLogSpeed) < 1.0f || Mathf.Sign(speed) == Mathf.Sign(lastLogSpeed)) && attempts < maxAttempts);



        lastLogSpeed = speed;

        // Set spawn start based on direction
        float startX = direction > 0 ? -12f : 12f;
        float y = waterParent.position.y;

        int objectCount = 4;
        float spacing = 5f;

        for (int i = 0; i < objectCount; i++)
        {
            float offset = i * spacing * -direction;
            Vector3 spawnPosition = new Vector3(startX + offset, y, 0f);

            GameObject obj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            obj.transform.SetParent(waterParent);

            LogMover mover = obj.GetComponent<LogMover>();
            if (mover == null)
            {
                mover = obj.AddComponent<LogMover>();
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
