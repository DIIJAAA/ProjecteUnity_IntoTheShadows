using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema d'ajuda progressiu per no perdre's al laberint
/// Combina pistes visuals i auditives sense fer-ho massa fàcil
/// </summary>
public class HintSystem : MonoBehaviour
{
    [Header("Configuració d'Ajudes")]
    [SerializeField] private float timeBeforeFirstHint = 45f;
    [SerializeField] private float timeBetweenHints = 30f;
    [SerializeField] private int maxVisualHints = 3;
    
    [Header("Ajuda Visual")]
    [SerializeField] private float arrowDuration = 5f;
    [SerializeField] private float arrowDistance = 3f;
    [SerializeField] private float arrowHeight = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hintSound;
    
    [Header("UI")]
    [SerializeField] private GameObject hintTextObject;
    
    private Text hintText;
    private float timeSinceLastHint = 0f;
    private int hintsGiven = 0;
    private GameObject currentArrow;
    private GameObject player;
    private GameObject exitDoor;
    private AudioSource audioSource;
    private GameManager gameManager;
    private bool keyCollected = false;
    
    void Start()
    {
        if (hintTextObject != null)
            hintText = hintTextObject.GetComponent<Text>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.8f;
        
        // CORREGIT: FindFirstObjectByType
        gameManager = FindFirstObjectByType<GameManager>();
        
        StartCoroutine(FindPlayerAndDoor());
        
        UpdateHintUI();
    }
    
    void Update()
    {
        if (player == null) return;
        
        if (gameManager != null && gameManager.HasKey() && !keyCollected)
        {
            keyCollected = true;
            timeSinceLastHint = 0f;
            hintsGiven = 0;
        }
        
        timeSinceLastHint += Time.deltaTime;
        
        float requiredTime = (hintsGiven == 0) ? timeBeforeFirstHint : timeBetweenHints;
        
        if (timeSinceLastHint >= requiredTime && hintsGiven < maxVisualHints)
        {
            GiveHint();
            timeSinceLastHint = 0f;
        }
        
        if (currentArrow != null)
        {
            UpdateArrowPosition();
        }
    }
    
    /// <summary>
    /// Busca el player i la porta
    /// </summary>
    private IEnumerator FindPlayerAndDoor()
    {
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForSeconds(0.5f);
        }
        
        while (exitDoor == null)
        {
            exitDoor = GameObject.Find("ExitDoor");
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("HintSystem: Player i ExitDoor trobats!");
    }
    
    /// <summary>
    /// Dóna una ajuda visual i auditiva
    /// </summary>
    private void GiveHint()
    {
        hintsGiven++;
        
        Debug.Log($"Ajuda {hintsGiven}/{maxVisualHints}");
        
        GameObject target = keyCollected ? exitDoor : FindClosestKey();
        
        if (target == null)
        {
            Debug.LogWarning("No s'ha trobat objectiu per l'ajuda!");
            return;
        }
        
        string message = keyCollected ? 
            "The exit is calling..." : 
            "You feel a strange energy nearby...";
        
        if (gameManager != null)
        {
            gameManager.ShowMessage(message, 3f);
        }
        
        if (hintSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hintSound);
        }
        
        CreateHintArrow(target);
        
        UpdateHintUI();
    }
    
    /// <summary>
    /// Troba la clau més propera
    /// </summary>
    private GameObject FindClosestKey()
    {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        
        if (keys.Length == 0)
        {
            // CORREGIT: FindObjectsByType
            Key[] keyScripts = FindObjectsByType<Key>(FindObjectsSortMode.None);
            if (keyScripts.Length > 0)
                return keyScripts[0].gameObject;
            return null;
        }
        
        GameObject closest = keys[0];
        float minDistance = Vector3.Distance(player.transform.position, closest.transform.position);
        
        foreach (GameObject key in keys)
        {
            float distance = Vector3.Distance(player.transform.position, key.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = key;
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// Crea una fletxa que apunta cap a l'objectiu
    /// </summary>
    private void CreateHintArrow(GameObject target)
    {
        if (currentArrow != null)
        {
            Destroy(currentArrow);
        }
        
        currentArrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentArrow.name = "HintArrow";
        
        currentArrow.transform.localScale = new Vector3(0.3f, 0.3f, 1.5f);
        
        MeshRenderer renderer = currentArrow.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 1f, 1f, 0.7f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 1f, 1f) * 0.5f);
            
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            renderer.material = mat;
        }
        
        Collider col = currentArrow.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
        
        Light arrowLight = currentArrow.AddComponent<Light>();
        arrowLight.type = LightType.Point;
        arrowLight.color = new Color(0f, 1f, 1f);
        arrowLight.range = 5f;
        arrowLight.intensity = 2f;
        
        UpdateArrowPosition();
        
        Destroy(currentArrow, arrowDuration);
    }
    
    /// <summary>
    /// Actualitza la posició de la fletxa perquè segueixi el jugador
    /// </summary>
    private void UpdateArrowPosition()
    {
        if (currentArrow == null || player == null) return;
        
        GameObject target = keyCollected ? exitDoor : FindClosestKey();
        if (target == null) return;
        
        Vector3 direction = (target.transform.position - player.transform.position).normalized;
        
        Vector3 arrowPosition = player.transform.position + direction * arrowDistance;
        arrowPosition.y = player.transform.position.y + arrowHeight;
        
        currentArrow.transform.position = arrowPosition;
        
        currentArrow.transform.rotation = Quaternion.LookRotation(direction);
        
        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.2f;
        currentArrow.transform.localScale = new Vector3(0.3f * pulse, 0.3f * pulse, 1.5f);
    }
    
    /// <summary>
    /// Actualitza la UI d'ajudes
    /// </summary>
    private void UpdateHintUI()
    {
        if (hintText != null)
        {
            int hintsLeft = maxVisualHints - hintsGiven;
            
            if (hintsLeft > 0)
            {
                hintText.text = $"Hints: {hintsLeft}";
                hintText.color = Color.cyan;
            }
            else
            {
                hintText.text = "No hints left";
                hintText.color = Color.red;
            }
        }
    }
    
    /// <summary>
    /// Reinicia el sistema d'ajudes
    /// </summary>
    public void ResetHints()
    {
        hintsGiven = 0;
        timeSinceLastHint = 0f;
        keyCollected = false;
        
        if (currentArrow != null)
        {
            Destroy(currentArrow);
        }
        
        UpdateHintUI();
        
        StartCoroutine(FindPlayerAndDoor());
    }
}