using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip footstepsLoop;
    [SerializeField] private float baseVolume = 0.5f;
    [SerializeField] private float fadeDuration = 0.2f;
    
    private AudioSource audioSource;
    private CharacterController characterController;
    private bool isWalking = false;
    private float targetVolume = 0f;
    private float currentVolumeVelocity = 0f;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepsLoop;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
        
        if (footstepsLoop != null)
            audioSource.Play();
    }
    
    void Update()
    {
        CheckMovement();
        UpdateVolume();
    }
    
    private void CheckMovement()
    {
        isWalking = characterController.isGrounded && 
                    characterController.velocity.magnitude > 0.5f;
        
        targetVolume = isWalking ? baseVolume : 0f;
    }
    
    private void UpdateVolume()
    {
        if (audioSource == null) return;
        
        audioSource.volume = Mathf.SmoothDamp(
            audioSource.volume,
            targetVolume,
            ref currentVolumeVelocity,
            fadeDuration
        );
    }
}