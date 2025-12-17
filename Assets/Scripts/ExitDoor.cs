using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class ExitDoor : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform doorVisual;

    [Header("Obertura")]
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private float openDistance = 4f;

    [Header("Audio")]
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip openSound;

    private bool isOpening = false;
    private bool isOpen = false;
    private Vector3 closedPosition;
    private AudioSource audioSource;
    private GameManager gameManager;
    private BoxCollider triggerCollider;

    void Start()
    {
        InitializeDoorVisual();
        InitializeAudio();
        InitializeTrigger();
        gameManager = GameManager.Instance;
        
        Debug.Log($"[ExitDoor] Inicialitzada a {transform.position}");
    }

    private void InitializeDoorVisual()
    {
        // Busca automàticament el visual si no està assignat
        if (doorVisual == null)
        {
            doorVisual = transform.Find("DoorVisual");
        }

        if (doorVisual != null)
        {
            // Assegura que la porta visual estigui ben posicionada
            doorVisual.localPosition = new Vector3(0f, 2f, 0f);
            doorVisual.localRotation = Quaternion.identity;
            doorVisual.localScale = new Vector3(2.5f, 3.5f, 0.2f);
            
            closedPosition = doorVisual.localPosition;
            
            Debug.Log($"[ExitDoor] DoorVisual configurat a posició local: {doorVisual.localPosition}");
        }
        else
        {
            Debug.LogError("[ExitDoor] No s'ha trobat DoorVisual!");
        }
    }

    private void InitializeAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 20f;
        audioSource.volume = 0.8f;
        audioSource.playOnAwake = false;
    }

    private void InitializeTrigger()
    {
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(4f, 5f, 4f); // Més gran per assegurar detecció
        triggerCollider.center = new Vector3(0f, 2.5f, 0f);
        
        Debug.Log($"[ExitDoor] Trigger configurat: size={triggerCollider.size}, center={triggerCollider.center}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ExitDoor] Trigger detectat amb: {other.name}, Tag: {other.tag}");
        
        if (!other.CompareTag("Player"))
        {
            Debug.Log("[ExitDoor] No és el Player, ignorant");
            return;
        }

        if (isOpen || isOpening)
        {
            Debug.Log("[ExitDoor] Porta ja oberta o obrint-se");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("[ExitDoor] GameManager és null!");
            return;
        }

        bool hasKey = gameManager.HasKey();
        Debug.Log($"[ExitDoor] Player té clau? {hasKey}");

        if (hasKey)
        {
            Debug.Log("[ExitDoor] Obrint porta!");
            StartCoroutine(OpenDoor());
        }
        else
        {
            Debug.Log("[ExitDoor] Porta tancada - clau necessària");
            
            if (lockedSound != null)
                audioSource.PlayOneShot(lockedSound);

            gameManager.ShowMessage("Troba la clau per sortir!", 2f);
        }
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        if (doorVisual == null)
        {
            Debug.LogError("[ExitDoor] DoorVisual és null a OpenDoor!");
            yield break;
        }

        // La porta s'obre cap amunt (Y+)
        Vector3 targetPosition = closedPosition + Vector3.up * openDistance;

        Debug.Log($"[ExitDoor] Obrint de {doorVisual.localPosition} a {targetPosition}");

        // Desactiva col·lisions de la porta
        Collider doorCollider = doorVisual.GetComponent<Collider>();
        if (doorCollider != null)
            doorCollider.enabled = false;

        // Animació d'obertura
        float elapsed = 0f;
        while (Vector3.Distance(doorVisual.localPosition, targetPosition) > 0.01f && elapsed < 5f)
        {
            doorVisual.localPosition = Vector3.MoveTowards(
                doorVisual.localPosition,
                targetPosition,
                openSpeed * Time.deltaTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        doorVisual.localPosition = targetPosition;
        isOpen = true;

        Debug.Log("[ExitDoor] Porta completament oberta!");

        yield return new WaitForSeconds(1.5f);

        if (gameManager != null)
        {
            Debug.Log("[ExitDoor] Completant joc!");
            gameManager.CompleteGame();
        }
    }

    // Debug visual al Scene View
    private void OnDrawGizmos()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null && bc.isTrigger)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(bc.center, bc.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}