using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("Generador")]
    [SerializeField] private MazeGenerator mazeGenerator;

    [Header("Estructura Bàsica")]
    [SerializeField] private GameObject mazeCellPrefab;
    [SerializeField] private GameObject ceilingPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Objectes Principals")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject exitDoorPrefab;

    [Header("Enemies")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(0)] private int enemyCount = 1;
    [SerializeField, Min(0)] private int minEnemyDistanceFromStart = 4;

    [Header("Decoracions de Terra")]
    [SerializeField] private GameObject[] cratePrefabs;
    [SerializeField] private GameObject bucketPrefab;
    [SerializeField] private GameObject[] stoolPrefabs;
    [SerializeField] private GameObject candlesPrefab;

    [Header("Decoracions de Paret/Sostre")]
    [SerializeField] private GameObject[] chainPrefabs;
    [SerializeField] private GameObject hangingCagePrefab;

    [Header("Configuració")]
    public float cellSize = 4f;
    public float wallHeight = 5f;
    public float ceilingHeight = 4.5f;

    [Header("Probabilitats de Decoracions (0-1)")]
    [Range(0f, 1f)] public float crateChance = 0.08f;
    [Range(0f, 1f)] public float bucketChance = 0.05f;
    [Range(0f, 1f)] public float stoolChance = 0.06f;
    [Range(0f, 1f)] public float candlesChance = 0.10f;
    [Range(0f, 1f)] public float chainChance = 0.12f;
    [Range(0f, 1f)] public float cageChance = 0.01f;

    private Vector2Int exitPosition;
    private Vector2Int keyCell;

    void Start()
    {
        GenerateNewMaze();
    }

    public void GenerateNewMaze()
    {
        Debug.Log("=== Generant nou laberint ===");

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        MazeCell[,] maze = mazeGenerator.GetMaze();

        exitPosition = new Vector2Int(
            mazeGenerator.mazeWidth - 1,
            mazeGenerator.mazeHeight - 1
        );

        Debug.Log($"Mida laberint: {mazeGenerator.mazeWidth}x{mazeGenerator.mazeHeight}");
        Debug.Log($"Posició sortida: ({exitPosition.x}, {exitPosition.y})");

        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0f, y * cellSize);

                GameObject cell = Instantiate(
                    mazeCellPrefab,
                    cellPosition,
                    Quaternion.identity,
                    transform
                );
                cell.name = $"Cell_{x}_{y}";

                MazeCellObject cellObj = cell.GetComponent<MazeCellObject>();
                if (cellObj != null)
                {
                    bool top = maze[x, y].topWall;
                    bool left = maze[x, y].leftWall;
                    bool bottom = (y == 0);
                    bool right = (x == mazeGenerator.mazeWidth - 1);

                    cellObj.Init(top, bottom, right, left);
                    PlaceDecorations(cell.transform, cellPosition, top, bottom, left, right);
                }

                if (x == exitPosition.x && y == exitPosition.y)
                    PlaceExitDoor(cellPosition, maze[x, y]);
            }
        }

        PlaceKey();
        SpawnPlayer();
        SpawnEnemies();

        Debug.Log("=== Laberint generat amb èxit ===");
    }

    private void PlaceDecorations(Transform parent, Vector3 cellPos, bool hasTop, bool hasBottom, bool hasLeft, bool hasRight)
    {
        float roll = Random.value;

        Vector3 randomPos = Vector3.zero;
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: randomPos = new Vector3(Random.Range(0.5f, 1.5f), 0f, Random.Range(0.5f, 1.5f)); break;
            case 1: randomPos = new Vector3(Random.Range(-1.5f, -0.5f), 0f, Random.Range(0.5f, 1.5f)); break;
            case 2: randomPos = new Vector3(Random.Range(0.5f, 1.5f), 0f, Random.Range(-1.5f, -0.5f)); break;
            case 3: randomPos = new Vector3(Random.Range(-1.5f, -0.5f), 0f, Random.Range(-1.5f, -0.5f)); break;
        }

        if (roll < candlesChance && candlesPrefab != null)
            Instantiate(candlesPrefab, cellPos + randomPos + Vector3.up * 0.1f, Quaternion.identity, parent);
        else if (roll < candlesChance + bucketChance && bucketPrefab != null)
            Instantiate(bucketPrefab, cellPos + randomPos, Quaternion.identity, parent);
        else if (roll < candlesChance + bucketChance + stoolChance && stoolPrefabs != null && stoolPrefabs.Length > 0)
            Instantiate(stoolPrefabs[Random.Range(0, stoolPrefabs.Length)], cellPos + randomPos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
        else if (roll < candlesChance + bucketChance + stoolChance + crateChance && cratePrefabs != null && cratePrefabs.Length > 0)
            Instantiate(cratePrefabs[Random.Range(0, cratePrefabs.Length)], cellPos + randomPos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);

        if (hasTop && Random.value < chainChance * 0.5f && chainPrefabs != null && chainPrefabs.Length > 0)
            Instantiate(chainPrefabs[Random.Range(0, chainPrefabs.Length)], cellPos + new Vector3(Random.Range(-1f, 1f), 2.5f, 1.9f), Quaternion.identity, parent);

        if (hasLeft && Random.value < chainChance * 0.5f && chainPrefabs != null && chainPrefabs.Length > 0)
            Instantiate(chainPrefabs[Random.Range(0, chainPrefabs.Length)], cellPos + new Vector3(-1.9f, 2.5f, Random.Range(-1f, 1f)), Quaternion.Euler(0, 90, 0), parent);

        if (hasRight && Random.value < chainChance * 0.5f && chainPrefabs != null && chainPrefabs.Length > 0)
            Instantiate(chainPrefabs[Random.Range(0, chainPrefabs.Length)], cellPos + new Vector3(1.9f, 2.5f, Random.Range(-1f, 1f)), Quaternion.Euler(0, -90, 0), parent);

        if (hasBottom && Random.value < chainChance * 0.5f && chainPrefabs != null && chainPrefabs.Length > 0)
            Instantiate(chainPrefabs[Random.Range(0, chainPrefabs.Length)], cellPos + new Vector3(Random.Range(-1f, 1f), 2.5f, -1.9f), Quaternion.Euler(0, 180, 0), parent);

        if (Random.value < cageChance && hangingCagePrefab != null)
            Instantiate(hangingCagePrefab, cellPos + Vector3.up * (ceilingHeight - 0.5f), Quaternion.identity, parent);
    }

    private void PlaceExitDoor(Vector3 cellPos, MazeCell cell)
    {
        if (exitDoorPrefab == null)
        {
            Debug.LogError("ExitDoor Prefab no assignat!");
            return;
        }

        Quaternion rotation = Quaternion.identity;
        Vector3 offset = Vector3.zero;

        // Determina on col·locar la porta segons quina paret està oberta
        bool hasRightWall = (exitPosition.x == mazeGenerator.mazeWidth - 1);
        bool hasTopWall = cell.topWall;

        if (hasRightWall && !hasTopWall)
        {
            // Si té paret dreta però NO té paret superior → sortida per DALT
            rotation = Quaternion.Euler(0, 180, 0);
            offset = new Vector3(0, 0, cellSize / 2f - 0.15f);
            Debug.Log("[MazeRenderer] Porta col·locada a paret SUPERIOR");
        }
        else if (hasRightWall)
        {
            // Té paret dreta → sortida per la DRETA
            rotation = Quaternion.Euler(0, 90, 0);
            offset = new Vector3(cellSize / 2f - 0.15f, 0, 0);
            Debug.Log("[MazeRenderer] Porta col·locada a paret DRETA");
        }
        else if (!hasTopWall)
        {
            // NO té paret dreta però NO té paret superior → sortida per DALT
            rotation = Quaternion.Euler(0, 180, 0);
            offset = new Vector3(0, 0, cellSize / 2f - 0.15f);
            Debug.Log("[MazeRenderer] Porta col·locada a paret SUPERIOR (fallback)");
        }
        else
        {
            // Fallback: paret dreta
            rotation = Quaternion.Euler(0, 90, 0);
            offset = new Vector3(cellSize / 2f - 0.15f, 0, 0);
            Debug.Log("[MazeRenderer] Porta col·locada a paret DRETA (fallback)");
        }

        Vector3 doorPosition = cellPos + offset;
        GameObject door = Instantiate(exitDoorPrefab, doorPosition, rotation, transform);
        door.name = "ExitDoor";

        Debug.Log($"[MazeRenderer] Porta spawn a posició world: {doorPosition}, rotació: {rotation.eulerAngles}");
    }

    private void PlaceKey()
    {
        if (keyPrefab == null)
        {
            Debug.LogError("Key Prefab no assignat!");
            return;
        }

        Vector2Int keyPos;
        int attempts = 0;

        do
        {
            keyPos = new Vector2Int(
                Random.Range(1, mazeGenerator.mazeWidth - 1),
                Random.Range(1, mazeGenerator.mazeHeight - 1)
            );
            attempts++;
        }
        while (
            ((keyPos.x == mazeGenerator.startX && keyPos.y == mazeGenerator.startY) ||
             (keyPos.x == exitPosition.x && keyPos.y == exitPosition.y))
            && attempts < 100
        );

        keyCell = keyPos;

        Vector3 keyPosition = new Vector3(keyPos.x * cellSize, 1.2f, keyPos.y * cellSize);
        GameObject key = Instantiate(keyPrefab, keyPosition, Quaternion.identity, transform);
        key.name = "Key";
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player Prefab no assignat!");
            return;
        }

        Vector3 startPos = new Vector3(
            mazeGenerator.startX * cellSize,
            1f,
            mazeGenerator.startY * cellSize
        );

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Instantiate(playerPrefab, startPos, Quaternion.identity);
        }
        else
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = startPos;
                player.transform.rotation = Quaternion.identity;
                cc.enabled = true;
            }
            else
            {
                player.transform.position = startPos;
                player.transform.rotation = Quaternion.identity;
            }
        }
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null || enemyCount <= 0) return;

        int spawned = 0;
        int attempts = 0;

        while (spawned < enemyCount && attempts < 300)
        {
            attempts++;

            Vector2Int cell = new Vector2Int(
                Random.Range(0, mazeGenerator.mazeWidth),
                Random.Range(0, mazeGenerator.mazeHeight)
            );

            if (cell == new Vector2Int(mazeGenerator.startX, mazeGenerator.startY)) continue;
            if (cell == keyCell) continue;
            if (cell == exitPosition) continue;

            int manhattan = Mathf.Abs(cell.x - mazeGenerator.startX) + Mathf.Abs(cell.y - mazeGenerator.startY);
            if (manhattan < minEnemyDistanceFromStart) continue;

            Vector3 worldPos = new Vector3(cell.x * cellSize, 3f, cell.y * cellSize);

            if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 20f))
                worldPos = hit.point + Vector3.up * 0.02f;
            else
                worldPos.y = 0.02f;

            Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
            spawned++;
        }

        if (spawned < enemyCount)
            Debug.LogWarning($"[Enemies] He pogut spawn {spawned}/{enemyCount} (massa restriccions o intents).");
    }
}