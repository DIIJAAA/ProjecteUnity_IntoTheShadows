using UnityEngine;
using System.Collections;

/// <summary>
/// Porta de sortida que s'obre quan el jugador té la clau
/// IMPORTANT: Necessita un BoxCollider amb "Is Trigger" activat
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ExitDoor : MonoBehaviour
{
    [Header("Referències")]
    [SerializeField] private Transform doorTransform;

    [Header("Configuració d'Obertura")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 4, 0);

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
        InitializeDoorTransform();
        InitializeAudio();
        InitializeTrigger();
        CacheGameManager();
    }

    private void InitializeDoorTransform()
    {
        if (doorTransform == null)
        {
            // Busca automàticament la porta si no està assignada
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("Door") && child != transform && !child.name.Contains("Doorway"))
                {
                    doorTransform = child;
                    break;
                }
            }
        }

        if (doorTransform != null)
        {
            closedPosition = doorTransform.position;
        }
        else
        {
            Debug.LogError($"[ExitDoor] No s'ha trobat la porta! Assigna 'Door Transform' manualment a {gameObject.name}");
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
        // Assegura que hi ha un trigger configurat correctament
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        triggerCollider.isTrigger = true;
        
        // Ajusta la mida del trigger si és molt petita
        if (triggerCollider.size.magnitude < 1f)
        {
            triggerCollider.size = new Vector3(2f, 3f, 2f);
            Debug.Log($"[ExitDoor] Trigger ajustat automàticament a {gameObject.name}");
        }
    }

    private void CacheGameManager()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[ExitDoor] No s'ha trobat el GameManager!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug per verificar col·lisions
        Debug.Log($"[ExitDoor] Trigger activat per: {other.gameObject.name} (Tag: {other.tag})");

        if (!other.CompareTag("Player") || isOpen || isOpening)
        {
            return;
        }

        if (gameManager != null && gameManager.HasKey())
        {
            StartCoroutine(OpenDoor());
        }
        else
        {
            PlayLockedSound();
            ShowLockedMessage();
        }
    }

    private void PlayLockedSound()
    {
        if (lockedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lockedSound);
        }
    }

    private void ShowLockedMessage()
    {
        if (gameManager != null)
        {
            gameManager.ShowMessage("Troba la clau per sortir!", 2f);
        }
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        if (doorTransform == null)
        {
            Debug.LogError("[ExitDoor] doorTransform és null!");
            yield break;
        }

        Vector3 targetPosition = closedPosition + openOffset;

        // Desactiva el collider de la porta
        Collider doorCollider = doorTransform.GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Animació d'obertura
        while (Vector3.Distance(doorTransform.position, targetPosition) > 0.01f)
        {
            doorTransform.position = Vector3.MoveTowards(
                doorTransform.position,
                targetPosition,
                openSpeed * Time.deltaTime
            );
            yield return null;
        }

        doorTransform.position = targetPosition;
        isOpen = true;

        yield return new WaitForSeconds(1.5f);

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        if (gameManager != null)
        {
            gameManager.CompleteGame();
        }
        else
        {
            Debug.LogError("[ExitDoor] GameManager no disponible per completar el joc!");
        }
    }

    // Debug visual
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (triggerCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
        }
    }
}