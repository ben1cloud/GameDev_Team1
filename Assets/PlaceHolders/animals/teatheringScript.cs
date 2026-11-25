using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class TetherMob : MonoBehaviour
{
    private Rigidbody rb;
    private LineRenderer lineRenderer;

    private bool isTethered = false;
    private Vector3 tetherPoint;
    private float previousVerticalVelocity = 0f;
    private float cooldownTimer = 0f;
    private float tetheredDuration = 0f;
    private const float tetherStrength = 1.5f; // Adjust this value to control how strongly the object is pulled towards the tether point

    private const float RayDistance = 500f;
    private const float CooldownDuration = 0.3f;
    private const float UntetherGraceDuration = 0.2f;
    private const float MinimumVerticalNormalAlignment = 0.6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // 1. Handle the 2-second untethered cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 2. When not tethered (and off cooldown), fire a raycast every frame
        if (!isTethered && cooldownTimer <= 0f)
        {
            TryFireTether();
        }

        // 3. Update the visual line so it connects the sphere's center to the tether point
        if (isTethered)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, tetherPoint);
        }
    }

    void FixedUpdate()
    {
        if (!isTethered) return;

        // 1. Cancel any velocity/movement going AWAY from the tether point
        Vector3 toTether = (tetherPoint - transform.position).normalized;
        
        // Note: In Unity 6+, use rb.linearVelocity. In older Unity versions, use rb.velocity.
        Vector3 currentVelocity = rb.linearVelocity; 

        // Dot product checks how much our velocity aligns with the direction to the tether.
        // If it's negative (< 0), the ball has velocity moving away from the point.
        float velAlongTether = Vector3.Dot(currentVelocity, toTether);

        if (velAlongTether < 0f)
        {
            // Subtract the outward velocity component, leaving only lateral or inward movement
            currentVelocity -= toTether * velAlongTether;
            rb.linearVelocity = currentVelocity;
        }

        rb.AddForce(toTether * (Physics.gravity.magnitude * tetherStrength), ForceMode.Acceleration);

        // 2. Check if vertical movement changed from world up to world down
        float currentVerticalVelocity = rb.linearVelocity.y;
        tetheredDuration += Time.fixedDeltaTime;

        if (tetheredDuration <= UntetherGraceDuration)
        {
            previousVerticalVelocity = currentVerticalVelocity;
            return;
        }

        if (previousVerticalVelocity > 0f && currentVerticalVelocity < 0f)
        {
            Untether();
        }

        previousVerticalVelocity = currentVerticalVelocity;
    }

    private void TryFireTether()
    {
        Vector3 dir = GetRandomUpperHemisphereDirection();

        // Fire raycast of length 500
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, RayDistance))
        {
            if (!IsValidTetherHit(hit)) return;

            isTethered = true;
            tetherPoint = hit.point;
            tetheredDuration = 0f;
            
            previousVerticalVelocity = rb.linearVelocity.y;
            
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, tetherPoint);
        }
    }

    private bool IsValidTetherHit(RaycastHit hit)
    {
        float verticalNormalAlignment = Mathf.Abs(Vector3.Dot(hit.normal.normalized, Vector3.up));
        return verticalNormalAlignment >= MinimumVerticalNormalAlignment;
    }

    private void Untether()
    {
        isTethered = false;
        lineRenderer.enabled = false;
        cooldownTimer = CooldownDuration;
    }

    private Vector3 GetRandomUpperHemisphereDirection()
    {
        // Random.onUnitSphere picks a random normalized 3D vector.
        Vector3 randomDir = Random.onUnitSphere;

        // If the dot product with Vector3.up is negative, invert the Y axis (or the whole vector)
        // so that Vector3.Dot(randomDir, Vector3.up) is guaranteed to be >= 0.
        if (Vector3.Dot(randomDir, Vector3.up) < 0f)
        {
            randomDir = -randomDir;
        }

        return randomDir;
    }
}