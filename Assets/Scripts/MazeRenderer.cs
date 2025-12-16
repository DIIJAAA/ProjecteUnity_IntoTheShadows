using UnityEngine;

/// <summary>
/// Renderitza el laberint complet: parets, terra, decoracions, clau i porta
/// </summary>
public class MazeRenderer : MonoBehaviour
{
    [Header("Generador")]
    [SerializeField] private MazeGenerator mazeGenerator;

    [Header("Estructura Bàsica")]
    [SerializeField] private GameObject mazeCellPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Objectes Principals")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject exitDoorPrefab;

    [Header("Exit Door Placement")]
    [SerializeField] private bool exitOnTopWall = true;        // true -> TopWall, false -> RightWall
    [SerializeField] private float doorInset = 0.08f;          // un poc cap dins perquè no quede fora
    [SerializeField] private float doorYawCorrection = 0f;     // si el model mira mal: prova 90 o -90
    [SerializeField] private Vector3 doorScaleFallback = new Vector3(0.4f, 0.4f, 0.8f);

    [Header("Exit Door Auto-Fit (mida com la paret)")]
    [SerializeField] private bool autoFitDoorToWall = true;
    [SerializeField, Range(0.7f, 1.0f)] private float doorWidthFill = 0.95f;
    [SerializeField, Range(0.7f, 1.0f)] private float doorHeightFill = 0.98f;

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

    [Header("DEBUG (temporal)")]
    [SerializeField] private bool debugShowExitBeacon = true;
    [SerializeField] private float debugBeaconHeight = 12f;

    private Vector2Int exitPosition;

    private void Start()
    {
        // Evita doble generació si el GameManager ja ho gestiona.
        if (FindFirstObjectByType<GameManager>() == null)
            GenerateNewMaze();
    }

    public void GenerateNewMaze()
    {
        // Neteja tot el laberint anterior
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        MazeCell[,] maze = mazeGenerator.GetMaze();

        exitPosition = new Vector2Int(
            mazeGenerator.mazeWidth - 1,
            mazeGenerator.mazeHeight - 1
        );

        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0f, y * cellSize);

                GameObject cell = Instantiate(mazeCellPrefab, cellPosition, Quaternion.identity, transform);
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

                // Eixida
                if (x == exitPosition.x && y == exitPosition.y)
                {
                    RemoveExitWallFromCell(cell.transform);
                    PlaceExitDoor(cell.transform);
                }
            }
        }

        PlaceKey();
        SpawnPlayer();
    }

    private void RemoveExitWallFromCell(Transform cell)
    {
        string wallName = exitOnTopWall ? "TopWall" : "RightWall";
        Transform wall = cell.Find(wallName);

        if (wall != null)
        {
            wall.gameObject.SetActive(false);
            Debug.Log($"[EXIT] Apagant {wallName} de la cel·la d'eixida");
        }
        else
        {
            // fallback per si els noms no són exactes
            Transform w2 = FindChildContains(cell, wallName);
            if (w2 != null)
            {
                w2.gameObject.SetActive(false);
                Debug.Log($"[EXIT] Apagant {w2.name} (contains {wallName})");
                return;
            }

            Debug.LogError($"[EXIT] No he trobat {wallName} dins del MazeCellPrefab!");
        }
    }

    private void PlaceExitDoor(Transform cellParent)
    {
        if (exitDoorPrefab == null)
        {
            Debug.LogError("ExitDoor Prefab no assignat!");
            return;
        }

        GameObject door = Instantiate(exitDoorPrefab, cellParent);
        door.name = "ExitDoor";

        // Fallback (per si autoFit està OFF o el model no té renderers)
        door.transform.localScale = doorScaleFallback;

        // Col·locació base a la paret (local space de la cel·la)
        float half = cellSize * 0.5f;
        if (exitOnTopWall)
        {
            door.transform.localPosition = new Vector3(0f, 0f, half - doorInset);
            door.transform.localRotation = Quaternion.Euler(0f, doorYawCorrection, 0f);
        }
        else
        {
            door.transform.localPosition = new Vector3(half - doorInset, 0f, 0f);
            door.transform.localRotation = Quaternion.Euler(0f, -90f + doorYawCorrection, 0f);
        }

        // Auto-fit mida segons la paret real (TopWall / RightWall)
        if (autoFitDoorToWall)
        {
            FitDoorToCellWall(door.transform, cellParent);
        }

        // Snap final
        SnapDoorToFloor(door.transform);
        SnapDoorToRightEdgeIfTopWall(door.transform);

        if (debugShowExitBeacon)
            CreateExitBeacon(door.transform.position);

        Debug.Log($"[EXIT] Porta localPos={door.transform.localPosition} exitOnTopWall={exitOnTopWall}");
    }

    // ===========================
    // AUTO-FIT DOOR TO WALL SIZE
    // ===========================

    private void FitDoorToCellWall(Transform doorRoot, Transform cellRoot)
    {
        Transform wall = FindWallForExit(cellRoot);
        if (wall == null) return;

        if (!TryGetBoundsInLocalSpace(doorRoot, cellRoot, out Bounds doorB)) return;
        if (!TryGetBoundsInLocalSpace(wall, cellRoot, out Bounds wallB)) return;

        // Evita divisions rares
        const float eps = 0.0001f;

        if (exitOnTopWall)
        {
            // TOP: ample = X, alçada = Y
            float targetW = wallB.size.x * doorWidthFill;
            float targetH = wallB.size.y * doorHeightFill;

            float doorW = Mathf.Max(eps, doorB.size.x);
            float doorH = Mathf.Max(eps, doorB.size.y);

            float sx = targetW / doorW;
            float sy = targetH / doorH;

            Vector3 s = doorRoot.localScale;
            doorRoot.localScale = new Vector3(s.x * sx, s.y * sy, s.z);
        }
        else
        {
            // RIGHT: ample = Z, alçada = Y
            float targetW = wallB.size.z * doorWidthFill;
            float targetH = wallB.size.y * doorHeightFill;

            float doorW = Mathf.Max(eps, doorB.size.z);
            float doorH = Mathf.Max(eps, doorB.size.y);

            float sz = targetW / doorW;
            float sy = targetH / doorH;

            Vector3 s = doorRoot.localScale;
            doorRoot.localScale = new Vector3(s.x, s.y * sy, s.z * sz);
        }
    }

    private Transform FindWallForExit(Transform cellRoot)
    {
        string wanted = exitOnTopWall ? "TopWall" : "RightWall";

        // primer intent: Find directe
        Transform direct = cellRoot.Find(wanted);
        if (direct != null) return direct;

        // fallback: conté el nom
        return FindChildContains(cellRoot, wanted);
    }

    // ===========================
    // SNAP / BOUNDS HELPERS
    // ===========================

    private void SnapDoorToRightEdgeIfTopWall(Transform doorRoot)
    {
        if (!exitOnTopWall) return;

        if (!TryGetBoundsInLocalSpace(doorRoot, doorRoot.parent, out Bounds localBounds))
            return;

        float desiredMaxX = (cellSize * 0.5f) - doorInset; // que arribe fins la paret dreta
        float currentMaxX = localBounds.max.x;

        float deltaX = desiredMaxX - currentMaxX;
        doorRoot.localPosition += new Vector3(deltaX, 0f, 0f);
    }

    private static void SnapDoorToFloor(Transform doorRoot)
    {
        Renderer[] rends = doorRoot.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);

        float minY = b.min.y;
        doorRoot.position += new Vector3(0f, -minY, 0f);
    }

    private bool TryGetBoundsInLocalSpace(Transform targetRoot, Transform referenceSpace, out Bounds bounds)
    {
        Renderer[] rends = targetRoot.GetComponentsInChildren<Renderer>(true);
        bounds = default;
        if (rends == null || rends.Length == 0) return false;

        Bounds b = new Bounds(referenceSpace.InverseTransformPoint(rends[0].bounds.center), Vector3.zero);
        EncapsulateRendererBoundsLocal(rends[0], referenceSpace, ref b);

        for (int i = 1; i < rends.Length; i++)
            EncapsulateRendererBoundsLocal(rends[i], referenceSpace, ref b);

        bounds = b;
        return true;
    }

    private void EncapsulateRendererBoundsLocal(Renderer r, Transform referenceSpace, ref Bounds b)
    {
        Bounds wb = r.bounds;
        Vector3 c = wb.center;
        Vector3 e = wb.extents;

        Vector3[] corners =
        {
            c + new Vector3( e.x,  e.y,  e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3( e.x, -e.y,  e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3(-e.x,  e.y,  e.z),
            c + new Vector3(-e.x,  e.y, -e.z),
            c + new Vector3(-e.x, -e.y,  e.z),
            c + new Vector3(-e.x, -e.y, -e.z),
        };

        foreach (var pWorld in corners)
        {
            Vector3 pLocal = referenceSpace.InverseTransformPoint(pWorld);
            b.Encapsulate(pLocal);
        }
    }

    private static Transform FindChildContains(Transform root, string contains)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains(contains))
                return t;
        }
        return null;
    }

    // ===========================
    // KEY / PLAYER / DECORATIONS
    // ===========================

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

    private void PlaceDecorations(Transform parent, Vector3 cellPos, bool hasTop, bool hasBottom, bool hasLeft, bool hasRight)
    {
        float roll = Random.value;

        Vector3 randomPos;
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: randomPos = new Vector3(Random.Range(0.5f, 1.5f), 0f, Random.Range(0.5f, 1.5f)); break;
            case 1: randomPos = new Vector3(Random.Range(-1.5f, -0.5f), 0f, Random.Range(0.5f, 1.5f)); break;
            case 2: randomPos = new Vector3(Random.Range(0.5f, 1.5f), 0f, Random.Range(-1.5f, -0.5f)); break;
            default: randomPos = new Vector3(Random.Range(-1.5f, -0.5f), 0f, Random.Range(-1.5f, -0.5f)); break;
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

    private void CreateExitBeacon(Vector3 doorPos)
    {
        GameObject lightGO = new GameObject("DEBUG_ExitLight");
        lightGO.transform.SetParent(transform);
        lightGO.transform.position = new Vector3(doorPos.x, 0f, doorPos.z) + Vector3.up * debugBeaconHeight;

        Light l = lightGO.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 60f;
        l.intensity = 8f;

        Debug.DrawRay(new Vector3(doorPos.x, 0.1f, doorPos.z), Vector3.up * debugBeaconHeight, Color.green, 999f);
    }
}
