using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FrogMob : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Drag the Player GameObject or Transform here.")]
    
    public float detectionRadius = 10f;

    [Header("Jump Impulse Settings")]
    public float horizontalForce = 5f;
    public float upwardForce = 5f;
    public float minJumpInterval = 2.0f;
    public float maxJumpInterval = 4.2f;

    private Rigidbody rb;
    private float jumpTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ResetJumpTimer();
    }

    void FixedUpdate()
    {
        if (StaticObjects.Player == null) return;

        // Tick down the timer continuously
        jumpTimer -= Time.fixedDeltaTime;

        if (jumpTimer <= 0f)
        {
            Vector3 offset = transform.position - StaticObjects.Player.position;

            // Use sqrMagnitude to avoid a square root calculation every check
            if (offset.sqrMagnitude <= detectionRadius * detectionRadius)
            {
                HopAway(offset);
                ResetJumpTimer();
            }
            else
            {
                // Keep timer at 0 so it hops immediately the moment the player enters range
                jumpTimer = 0f;
            }
        }
    }

    private void HopAway(Vector3 offsetFromPlayer)
    {
        // Flatten the Y axis so the "away" direction is strictly along the horizontal plane
        Vector3 flatAwayDir = new Vector3(offsetFromPlayer.x, 0f, offsetFromPlayer.z).normalized;

        // Edge case: if the player is at the exact same X/Z coordinate, pick a random horizontal direction
        if (flatAwayDir.sqrMagnitude < 0.001f)
        {
            Vector2 rand = Random.insideUnitCircle.normalized;
            flatAwayDir = new Vector3(rand.x, 0f, rand.y);
        }

        // Combine outward push with an upward hop so it arcs like a frog instead of just rolling
        Vector3 impulse = (flatAwayDir * horizontalForce) + (Vector3.up * upwardForce);
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private void ResetJumpTimer()
    {
        jumpTimer = Random.Range(minJumpInterval, maxJumpInterval);
    }

    // Visualizes the detection radius in the Scene view when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}