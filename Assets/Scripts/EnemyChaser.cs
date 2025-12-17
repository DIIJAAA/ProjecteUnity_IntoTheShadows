using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyChaser : MonoBehaviour
{
    [Header("Persecució")]
    [SerializeField] private float speed = 1.7f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float stopDistance = 1.1f;
    [SerializeField] private float turnSpeedDeg = 540f;

    [Header("Atac")]
    [SerializeField] private float hitCooldownSeconds = 0.75f;
    [SerializeField] private float despawnDelayAfterHit = 0.1f;

    [Header("Audio 3D")]
    [SerializeField] private AudioClip footstepsClip;
    [SerializeField] private float footstepsInterval = 0.45f;
    [SerializeField] private AudioClip enemyVoiceClip;
    [SerializeField] private float voiceInterval = 5f;
    [SerializeField] private float audioMaxDistance = 18f;

    private CharacterController cc;
    private Transform player;
    private AudioSource audioSource;
    private GameObject visualCylinder;
    private float nextFootstepTime;
    private float nextVoiceTime;
    private float nextHitTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        InitializeAudio();
        CreateSimpleVisual();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        bool chasing = dist <= detectionRange;

        if (!chasing)
        {
            TryPlayVoice(false);
            return;
        }

        RotateTowardsPlayer(toPlayer);
        MoveTowardsPlayer(toPlayer, dist);
        TryPlayVoice(true);
    }

    private void CreateSimpleVisual()
    {
        // Crea un cilindre roig com a visual
        visualCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualCylinder.name = "EnemyVisual";
        visualCylinder.transform.SetParent(transform);
        visualCylinder.transform.localPosition = new Vector3(0f, 1f, 0f);
        visualCylinder.transform.localRotation = Quaternion.identity;
        visualCylinder.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

        // Elimina el collider del cilindre (ja tenim CharacterController)
        Collider cylinderCollider = visualCylinder.GetComponent<Collider>();
        if (cylinderCollider != null)
            Destroy(cylinderCollider);

        // Material roig
        Renderer renderer = visualCylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material redMat = new Material(Shader.Find("Standard"));
            redMat.color = Color.red;
            redMat.SetFloat("_Metallic", 0.5f);
            redMat.SetFloat("_Glossiness", 0.5f);
            renderer.material = redMat;
        }
    }

    private void InitializeAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = audioMaxDistance;
        audioSource.dopplerLevel = 0f;
    }

    private void RotateTowardsPlayer(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeedDeg * Time.deltaTime);
        }
    }

    private void MoveTowardsPlayer(Vector3 direction, float distance)
    {
        if (distance > stopDistance)
        {
            Vector3 move = direction.normalized * speed;
            cc.SimpleMove(move);

            if (footstepsClip != null && Time.time >= nextFootstepTime)
            {
                PlayOneShot(footstepsClip, 1f, 1f);
                nextFootstepTime = Time.time + footstepsInterval;
            }
        }
        else
        {
            cc.SimpleMove(Vector3.zero);
        }
    }

    private void TryPlayVoice(bool nearPlayer)
    {
        if (enemyVoiceClip == null) return;

        float interval = nearPlayer ? voiceInterval : voiceInterval * 1.5f;
        
        if (Time.time >= nextVoiceTime)
        {
            PlayOneShot(enemyVoiceClip, 0.8f, Random.Range(0.95f, 1.05f));
            nextVoiceTime = Time.time + interval;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag("Player")) return;
        if (Time.time < nextHitTime) return;

        nextHitTime = Time.time + hitCooldownSeconds;

        GameManager gm = GameManager.Instance;
        if (gm != null) 
            gm.OnPlayerHit();

        StartCoroutine(DespawnAfter(despawnDelayAfterHit));
    }

    private IEnumerator DespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}