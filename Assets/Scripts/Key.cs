using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Key : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1.5f;
    [SerializeField] private float bobHeight = 1.5f;

    [Header("Llum")]
    [SerializeField] private Color keyColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private float lightIntensity = 4f;
    [SerializeField] private float lightRange = 12f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private Light keyLight;

    void Start()
    {
        InitializePosition();
        InitializeVisuals();
        InitializeLight();
        InitializeTrigger();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        if (keyLight != null)
            keyLight.intensity = lightIntensity + Mathf.Sin(Time.time * 3f) * 1f;
    }

    private void InitializePosition()
    {
        transform.position = new Vector3(transform.position.x, bobHeight, transform.position.z);
        startPosition = transform.position;
    }

    private void InitializeVisuals()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.material == null) continue;

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

    private void InitializeLight()
    {
        keyLight = gameObject.AddComponent<Light>();
        keyLight.type = LightType.Point;
        keyLight.color = keyColor;
        keyLight.range = lightRange;
        keyLight.intensity = lightIntensity;
        keyLight.shadows = LightShadows.None;
    }

    private void InitializeTrigger()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger == null)
            trigger = gameObject.AddComponent<SphereCollider>();
        
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
            gameManager.CollectKey();

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);

        Destroy(gameObject);
    }
}