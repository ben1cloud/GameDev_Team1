using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AttackVisualizer))]
public class Player : MonoBehaviour
{   

    public int BasicAttackDamage = 10;
    public int HeavyAttackDamage = 20;

    public float BasicAttackWidth = 1f;
    public float HeavyAttackWidth = 3f;

    public float BasicAttackRange = 2f;
    public float HeavyAttackRange = 5f;
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;

    private AttackVisualizer attackVisualizer;


    //for ranged attacks
    public GameObject projectilePrefab;
    public Transform attackPoint;
    public float projectileSpeed = 15f;

    // for spherecast in camera's direction
    public Transform playerCamera;


    void Awake()
    {
        attackVisualizer = GetComponent<AttackVisualizer>();
        if (attackVisualizer == null)
        {
            attackVisualizer = gameObject.AddComponent<AttackVisualizer>();
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.enabled = false;
        }
    }

    void Start()
    {   
        // retrieves the Rigidbody attached to the same GameObject as your script
        rb = GetComponent<Rigidbody>();

        // Prevent the player from tipping over
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    void Update(){
        if (Keyboard.current == null) return;

        if (Keyboard.current.jKey.wasPressedThisFrame) BasicAttack();
        if (Keyboard.current.kKey.wasPressedThisFrame) HeavyAttack();
        if (Mouse.current.rightButton.wasPressedThisFrame) RayAttack();
        if (Mouse.current.leftButton.wasPressedThisFrame) ProjectileAttack();

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;

        // movement in players direction
        // normalize to prevent diagnol being faster
        movement = (transform.forward * vertical + transform.right * horizontal).normalized;

    }

    public void BasicAttack(){

        Debug.Log("Basic attack button pressed!");
        Vector3 start = playerCamera.position;
        attackVisualizer.ShowAttack(start, playerCamera.forward, BasicAttackRange, BasicAttackWidth, Color.red, false);
        
        RaycastHit hit;

        // Cast a sphere from the camera in the direction it faces
        // arguments are in order: starting position, sphere radius, casting direction/camera direction, casting distance
        if (Physics.SphereCast(playerCamera.position, BasicAttackWidth, playerCamera.forward, out hit, BasicAttackRange))
        {
            Health enemyHealth = hit.collider.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.takeDamage(BasicAttackDamage);
            }
        }
    }

    public void HeavyAttack()
    {   

        Debug.Log("Heavy attack button pressed!");
        Vector3 start = playerCamera.position;
        attackVisualizer.ShowAttack(start, playerCamera.forward, HeavyAttackRange, HeavyAttackWidth, Color.blue, true);

        RaycastHit hit;

        if (Physics.SphereCast(playerCamera.position, HeavyAttackWidth, playerCamera.forward, out hit, HeavyAttackRange))
        {
            Health enemyHealth = hit.collider.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.takeDamage(HeavyAttackDamage);
            }
        }
    }

    public void ProjectileAttack()
    {
        // Find the point the camera is aiming at
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        RaycastHit hit;
        Vector3 targetPoint;

        // if the ray hits something, use that point; otherwise, use a point far away in the direction of the ray
        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }

        // Calculate direction from weapon to target
        Vector3 direction = (targetPoint - attackPoint.position).normalized;
        // Spawn the projectile facing its travel direction
        // note playerCamera should not reference the player root but the main camera, otherwise it doesn't recieve the vertical pitch
        GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.LookRotation(direction));
        Collider[] playerColliders = GetComponentsInChildren<Collider>();
        Collider[] projectileColliders = projectile.GetComponentsInChildren<Collider>();
        foreach (Collider playerCollider in playerColliders)
        {
            foreach (Collider projectileCollider in projectileColliders)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider);
            }
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * projectileSpeed;
    }

    //for ray attack
    public LineRenderer laserLine;
    public float laserRange = 100f;
    public float laserDuration = 0.2f;
    public int LaserDamage = 5;
    public void RayAttack()
    {
        // Find where the camera is aiming
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, laserRange))
        {
            targetPoint = hit.point;

            Debug.Log("Hit: " + hit.collider.name);

            Health enemyHealth = hit.collider.GetComponentInParent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.takeDamage(LaserDamage);
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * laserRange;
        }

        // Show laser from weapon to target
        laserLine.SetPosition(0, attackPoint.position);
        laserLine.SetPosition(1, targetPoint);

        laserLine.enabled = true;

        StartCoroutine(HideLaser());
    }

    IEnumerator HideLaser()
    {
        yield return new WaitForSeconds(laserDuration);

        laserLine.enabled = false;
    }


    
   void FixedUpdate()
    {
        // move
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }
    
}
