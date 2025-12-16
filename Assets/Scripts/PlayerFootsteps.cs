using UnityEngine;

/// <summary>
/// Gestiona els sons de passes del jugador amb un àudio continu en loop.
/// El volum augmenta quan el jugador es mou i disminueix quan s'atura.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip footstepsLoop; // Àudio llarg de passes en loop
    [SerializeField] private float baseVolume = 0.5f; // Volum quan es camina
    [SerializeField] private float fadeDuration = 0.2f; // Temps de fade in/out
    
    private AudioSource audioSource;
    private CharacterController characterController;
    private bool isWalking = false;
    private float targetVolume = 0f;
    private float currentVolumeVelocity = 0f;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Crea l'AudioSource per les passes
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepsLoop;
        audioSource.loop = true; // LOOP activat
        audioSource.spatialBlend = 0f; // So 2D (sempre igual de fort)
        audioSource.playOnAwake = false;
        audioSource.volume = 0f; // Comença en silenci
        
        // Inicia la reproducció (en silenci)
        if (footstepsLoop != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Footsteps Loop no assignat al PlayerFootsteps!");
        }
    }
    
    void Update()
    {
        CheckMovement();
        UpdateVolume();
    }
    
    /// <summary>
    /// Comprova si el jugador està caminant
    /// </summary>
    private void CheckMovement()
    {
        // Detecta si el jugador es mou i està a terra
        isWalking = characterController.isGrounded && 
                    characterController.velocity.magnitude > 0.5f;
        
        // Estableix el volum objectiu
        targetVolume = isWalking ? baseVolume : 0f;
    }
    
    /// <summary>
    /// Actualitza el volum amb transició suau (fade)
    /// </summary>
    private void UpdateVolume()
    {
        if (audioSource == null) return;
        
        // Fade suau per evitar talls bruscos
        audioSource.volume = Mathf.SmoothDamp(
            audioSource.volume,
            targetVolume,
            ref currentVolumeVelocity,
            fadeDuration
        );
    }
}