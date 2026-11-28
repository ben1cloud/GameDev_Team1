using UnityEngine;

// Single-point worm. Ignores world collisions and moves itself.
// "In ground" = ground geometry exists between the worm and a point directly above it.
public class WormScript : MonoBehaviour
{
    [Header("Ground check")]
    [Tooltip("Layers that count as ground. Colliders with a Rigidbody and triggers are always ignored.")]
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    [Tooltip("Height of the check point above the worm. Must reach above the highest ground.")]
    public float skyHeight = 500f;

    [Header("Target")]
    [Tooltip("If empty, uses StaticObjects.Player.")]
    public Transform target;

    [Header("Out of ground")]
    public float gravity = 9.81f;

    [Header("In ground")]
    [Tooltip("Reversed gravity.")]
    public float upwardAccel = 9.81f;
    [Tooltip("How fast velocity can change toward the desired velocity. Above upwardAccel, it can stay underground at a deep target.")]
    public float steerAccel = 15f;
    public float steerSpeed = 8f;
    public float drag = 0.5f;

    [Header("Limits")]
    public float maxSpeed = 25f;

    [Header("Debug")]
    public Color inGroundColor = Color.red;
    public Color outOfGroundColor = Color.green;
    [Tooltip("Seconds the path stays drawn (Scene view, or Game view with Gizmos on). 0 = off.")]
    public float pathDuration = 5f;
    public Vector3 initialVelocity;
    [SerializeField] private bool inGround;
    [SerializeField] private Vector3 velocity;

    private readonly RaycastHit[] hits = new RaycastHit[32];
    private Material material;

    void Awake()
    {
        // The default sphere primitive comes with a collider; the worm must not have one.
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) material = rend.material;

        velocity = initialVelocity;
    }

    void Start()
    {
        inGround = IsInGround(transform.position);
        UpdateColor();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 position = transform.position;

        bool nowInGround = IsInGround(position);
        if (nowInGround != inGround)
        {
            inGround = nowInGround;
            UpdateColor();
        }

        if (inGround)
        {
            Transform t = target != null ? target : StaticObjects.Player;
            if (t != null)
            {
                Vector3 desired = (t.position - position).normalized * steerSpeed;
                velocity = Vector3.MoveTowards(velocity, desired, steerAccel * dt);
            }
            velocity += Vector3.up * (upwardAccel * dt);
            velocity *= Mathf.Exp(-drag * dt);
        }
        else
        {
            velocity += Vector3.down * (gravity * dt);
        }

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Vector3 next = position + velocity * dt;

        if (pathDuration > 0f)
            Debug.DrawLine(position, next, inGround ? inGroundColor : outOfGroundColor, pathDuration);

        transform.position = next;
    }

    public bool IsInGround(Vector3 point)
    {
        Vector3 sky = point + Vector3.up * skyHeight;

        // Raycasts skip backfaces, so cast both ways.
        return HitsGround(sky, Vector3.down) || HitsGround(point, Vector3.up);
    }

    private bool HitsGround(Vector3 origin, Vector3 direction)
    {
        // All hits, so an ignored object can't hide the ground behind it.
        int count = Physics.RaycastNonAlloc(origin, direction, hits, skyHeight, groundMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            // Skips the player and other animals.
            if (hits[i].collider.attachedRigidbody == null) return true;
        }
        return false;
    }

    private void UpdateColor()
    {
        if (material != null) material.color = inGround ? inGroundColor : outOfGroundColor;
    }

    void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}
