using UnityEngine;

/// <summary>
/// Clau dorada molt visible amb efectes de llum i partícules
/// </summary>
public class Key : MonoBehaviour
{
    [Header("Configuració Visual")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1.5f;
    [SerializeField] private float bobHeight = 1.5f;

    [Header("Audio 3D (Hint - opcional)")]
    [SerializeField] private bool enableAudioHint = false;
    [SerializeField] private AudioClip keyAmbientSound;
    [SerializeField] private float soundInterval = 2.5f;
    [SerializeField] private float maxHearDistance = 50f;

    [Header("Audio Recollida")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Efectes de Llum")]
    [SerializeField] private Color keyColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private float lightIntensity = 4f;
    [SerializeField] private float lightRange = 12f;

    private Vector3 startPosition;
    private AudioSource ambientAudioSource;
    private Light keyLight;
    private float timeSinceLastSound = 0f;
    private MeshRenderer[] renderers;

    void Start()
    {
        transform.position = new Vector3(transform.position.x, bobHeight, transform.position.z);
        startPosition = transform.position;

        renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                renderer.material.color = keyColor;

                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", keyColor * 0.5f);
                }

                if (renderer.material.HasProperty("_Metallic"))
                    renderer.material.SetFloat("_Metallic", 0.8f);

                if (renderer.material.HasProperty("_Glossiness"))
                    renderer.material.SetFloat("_Glossiness", 0.9f);
            }
        }

        keyLight = gameObject.AddComponent<Light>();
        keyLight.type = LightType.Point;
        keyLight.color = keyColor;
        keyLight.range = lightRange;
        keyLight.intensity = lightIntensity;
        keyLight.shadows = LightShadows.None;

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.clip = keyAmbientSound;
        ambientAudioSource.loop = false;
        ambientAudioSource.spatialBlend = 1f;
        ambientAudioSource.volume = 0.7f;
        ambientAudioSource.minDistance = 5f;
        ambientAudioSource.maxDistance = maxHearDistance;
        ambientAudioSource.rolloffMode = AudioRolloffMode.Linear;
        ambientAudioSource.dopplerLevel = 0f;
        ambientAudioSource.playOnAwake = false;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        if (keyLight != null)
            keyLight.intensity = lightIntensity + Mathf.Sin(Time.time * 3f) * 1f;

        // Hint sonor (DESACTIVAT per defecte)
        if (enableAudioHint && keyAmbientSound != null)
        {
            timeSinceLastSound += Time.deltaTime;

            if (timeSinceLastSound >= soundInterval)
            {
                ambientAudioSource.PlayOneShot(keyAmbientSound);
                timeSinceLastSound = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
            gameManager.CollectKey();

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
