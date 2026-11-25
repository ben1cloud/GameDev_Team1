using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ConeFlier : MonoBehaviour
{
    private struct HitMemory
    {
        public Vector3 point;
        public Vector3 normal;
        public bool valid;
    }

    [Header("1. Speed & Rotation Knobs")]
    [Tooltip("Constant forward flight speed in units/sec.")]
    [Range(0f, 150f)]
    public float speed = 25f;

    [Tooltip("Max roll rate (radians/sec) when banking into turns.")]
    [Range(0.5f, 15f)]
    public float rollSpeed = 6f;

    [Tooltip("Max pitch rate (radians/sec) when pulling up/down.")]
    [Range(0.5f, 15f)]
    public float pitchSpeed = 3.5f;

    [Tooltip("0 = pitch immediately while rolling, 1 = wait until wings are banked before pitching.")]
    [Range(0f, 1f)]
    public float bankBeforePitch = 0.6f;

    [Header("2. Cone Raycast & Memory Knobs")]
    [Tooltip("How far ahead the cone rays check for obstacles.")]
    [Range(5f, 500f)]
    public float rayDistance = 150f;

    [Tooltip("Half-angle of the random forward ray cone in degrees.")]
    [Range(0f, 60f)]
    public float coneHalfAngle = 20f;

    [Tooltip("Minimum distance between stored hit points so the 5 slots don't clump on one spot.")]
    [Range(0.1f, 20f)]
    public float minPointSpacing = 3f;

    [Tooltip("Layers treated as obstacles.")]
    public LayerMask obstacleMask = ~0;

    [Header("3. Avoidance & Sky Return Knobs")]
    [Tooltip("How strongly stored obstacle points push the flight path away.")]
    [Range(0f, 15f)]
    public float avoidanceWeight = 3.5f;

    [Tooltip("0 = push directly away from hit point, 1 = push along surface normal.")]
    [Range(0f, 1f)]
    public float normalBlend = 0.5f;

    [Tooltip("Downward pull strength triggered when looking above world horizon until a ray hits.")]
    [Range(0f, 5f)]
    public float skyReturnWeight = 0.4f;

    [Header("4. Target Leash Knobs")]
    [Tooltip("Optional Transform to orbit/return to. If null, uses Target Position below.")]
    public Transform targetTransform;

    [Tooltip("World position used if Target Transform is not assigned.")]
    public Vector3 targetPosition = Vector3.zero;

    [Tooltip("Free-roam radius around target before the bird turns back.")]
    [Range(5f, 500f)]
    public float maxTargetDistance = 80f;

    [Tooltip("Distance past Max Target Distance over which pull smoothly ramps to full strength.")]
    [Range(1f, 100f)]
    public float targetRampDistance = 25f;

    [Tooltip("How strongly the bird banks back toward the target when outside the leash.")]
    [Range(0f, 10f)]
    public float targetSeekWeight = 1.5f;

    [Header("5. Collision Limp")]
    [Tooltip("How long the flier stops steering and propelling after a solid collision.")]
    [Min(0f)]
    public float limpDuration = 2f;

    [Tooltip("How long after recovering before another collision can make the flier limp.")]
    [Min(0f)]
    public float collisionCooldown = 2f;

    [Header("6. Debug")]
    public bool drawGizmos = true;

    private Rigidbody rb;
    private readonly HitMemory[] memory = new HitMemory[5];
    private int writeIndex = 0;

    private bool shouldPitchDown = false;
    private bool isSeekingTarget = false;
    private bool isLimping = false;
    private float limpUntilTime;
    private float limpCooldownUntilTime;
    private Vector3 lastRayDir;
    private Vector3 currentDesiredForward;

    private Vector3 ActiveTarget => targetTransform != null ? targetTransform.position : targetPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        currentDesiredForward = transform.forward;
    }

    private void OnEnable()
    {
        if (rb != null) rb.WakeUp();
    }

    private void FixedUpdate()
    {
        if (isLimping)
        {
            if (Time.fixedTime < limpUntilTime) return;

            isLimping = false;
            rb.useGravity = false;
            limpCooldownUntilTime = Time.fixedTime + collisionCooldown;
        }

        if (rb.IsSleeping()) rb.WakeUp();

        if (Vector3.Dot(transform.forward, Vector3.up) > 0f)
        {
            shouldPitchDown = true;
        }

        CastConeRay();
        currentDesiredForward = ComputeDesiredForward();
        ApplyBankAndPitch(currentDesiredForward);

        float cooldownRemaining = limpCooldownUntilTime - Time.fixedTime;
        float propulsionScale = collisionCooldown > 0f
            ? 1f - Mathf.Clamp01(cooldownRemaining / collisionCooldown)
            : 1f;
        rb.linearVelocity = transform.forward * speed * propulsionScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isLimping || Time.fixedTime < limpCooldownUntilTime || limpDuration <= 0f) return;

        isLimping = true;
        limpUntilTime = Time.fixedTime + limpDuration;
        rb.useGravity = true;
    }

    private void CastConeRay()
    {
        float maxRadius = Mathf.Tan(coneHalfAngle * Mathf.Deg2Rad);
        Vector2 disk = Random.insideUnitCircle * maxRadius;
        Vector3 rayDir = (transform.forward + transform.right * disk.x + transform.up * disk.y).normalized;
        lastRayDir = rayDir;

        if (Physics.Raycast(rb.position, rayDir, out RaycastHit hit, rayDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            shouldPitchDown = false;

            float minSqrDist = minPointSpacing * minPointSpacing;
            for (int i = 0; i < memory.Length; i++)
            {
                if (memory[i].valid && (memory[i].point - hit.point).sqrMagnitude < minSqrDist)
                {
                    memory[i].point = hit.point;
                    memory[i].normal = hit.normal;
                    return;
                }
            }

            memory[writeIndex] = new HitMemory
            {
                point = hit.point,
                normal = hit.normal,
                valid = true
            };
            writeIndex = (writeIndex + 1) % memory.Length;
        }
    }

    private Vector3 ComputeDesiredForward()
    {
        Vector3 repulsion = Vector3.zero;

        for (int i = 0; i < memory.Length; i++)
        {
            if (!memory[i].valid) continue;

            Vector3 toHit = memory[i].point - rb.position;

            // Anything behind the forward plane produces zero force
            if (Vector3.Dot(toHit, transform.forward) < 0f) continue;

            float dist = toHit.magnitude;
            if (dist > rayDistance || dist < 0.001f) continue;

            float proximity = 1f - (dist / rayDistance);
            float strength = proximity * proximity;

            Vector3 awayFromPoint = -toHit / dist;
            Vector3 pushDir = Vector3.Slerp(awayFromPoint, memory[i].normal, normalBlend).normalized;

            repulsion += pushDir * strength;
        }

        Vector3 desired = transform.forward + repulsion * avoidanceWeight;

        // Target leash pull when outside maxTargetDistance
        Vector3 toTarget = ActiveTarget - rb.position;
        float targetDist = toTarget.magnitude;
        if (targetDist > maxTargetDistance && targetSeekWeight > 0f)
        {
            isSeekingTarget = true;
            float ramp = Mathf.Clamp01((targetDist - maxTargetDistance) / Mathf.Max(0.01f, targetRampDistance));
            Vector3 seekDir = toTarget / targetDist;

            // Tiny canopy bias breaks symmetry if target is dead-behind (-transform.forward)
            if (Vector3.Dot(seekDir, transform.forward) < -0.98f)
            {
                seekDir = (seekDir + transform.up * 0.1f).normalized;
            }

            desired += seekDir * (targetSeekWeight * ramp);
        }
        else
        {
            isSeekingTarget = false;
        }

        // Gradually push view downward until a ray hits (suppressed while actively climbing to a high target)
        bool targetIsAbove = isSeekingTarget && ActiveTarget.y > rb.position.y;
        if (shouldPitchDown && !targetIsAbove && skyReturnWeight > 0f)
        {
            Vector3 downPull = (Vector3.down + transform.up * 0.05f).normalized;
            desired += downPull * skyReturnWeight;
        }

        return desired.sqrMagnitude > 0.0001f ? desired.normalized : transform.forward;
    }

    private void ApplyBankAndPitch(Vector3 desiredForward)
    {
        Vector3 localTarget = transform.InverseTransformDirection(desiredForward);

        // 1. Roll to align +Y canopy with target direction
        float lateralMag = new Vector2(localTarget.x, localTarget.y).magnitude;
        float rollRate = 0f;

        if (lateralMag > 0.02f)
        {
            float rollError = Mathf.Atan2(-localTarget.x, localTarget.y);
            rollRate = Mathf.Clamp(rollError * 4f, -rollSpeed, rollSpeed);
        }

        // 2. Pitch toward target
        float pitchError = Mathf.Atan2(localTarget.y, localTarget.z);
        float rollAlignment = lateralMag > 0.001f ? Mathf.Clamp01(localTarget.y / lateralMag) : 1f;
        float pitchScale = Mathf.Lerp(1f - bankBeforePitch, 1f, rollAlignment);
        float pitchRate = Mathf.Clamp(-pitchError * 4f, -pitchSpeed, pitchSpeed) * pitchScale;

        // 3. Zero yaw (local Y = 0)
        Vector3 localAngularVel = new Vector3(pitchRate, 0f, rollRate);
        rb.angularVelocity = transform.TransformDirection(localAngularVel);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Target leash sphere
        Gizmos.color = (Application.isPlaying && isSeekingTarget)
            ? new Color(1f, 0.85f, 0.2f, 0.35f)
            : new Color(0.4f, 1f, 0.6f, 0.15f);
        Gizmos.DrawWireSphere(ActiveTarget, maxTargetDistance);

        // Cone ray
        Gizmos.color = (Application.isPlaying && shouldPitchDown)
            ? new Color(1f, 0.55f, 0.1f, 0.45f)
            : new Color(0.2f, 0.8f, 1f, 0.35f);
        Vector3 dir = Application.isPlaying ? lastRayDir : transform.forward;
        Gizmos.DrawRay(transform.position, dir * rayDistance);

        if (!Application.isPlaying) return;

        // Desired steering vector
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, currentDesiredForward * 12f);

        // Stored 5-slot hit memory
        for (int i = 0; i < memory.Length; i++)
        {
            if (!memory[i].valid) continue;

            bool inFront = Vector3.Dot(memory[i].point - transform.position, transform.forward) >= 0f;
            Gizmos.color = inFront ? Color.red : new Color(0.5f, 0.5f, 0.5f, 0.4f);
            Gizmos.DrawSphere(memory[i].point, 0.75f);
            Gizmos.DrawRay(memory[i].point, memory[i].normal * 3f);
        }
    }
}