using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   

    public int damage = 10;
    public float attackRange = 5f;
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 movement;

    // for spherecast in camera's direction
    public Transform playerCamera;


    void Start()
    {   
        // retrieves the Rigidbody attached to the same GameObject as your script
        rb = GetComponent<Rigidbody>();

        // Prevent the player from tipping over
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    void Update(){
        if (Keyboard.current == null) return;

        if (Keyboard.current.jKey.wasPressedThisFrame) attack();
        
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

    public void attack(){

        Debug.Log("Attack button pressed!");
        Debug.DrawRay(playerCamera.position, playerCamera.forward * attackRange, Color.red, 1f);
        
        RaycastHit hit;

        // Cast a sphere from the camera in the direction it faces
        // arguments are in order: starting position, sphere radius, casting direction/camera direction, casting distance
        if (Physics.SphereCast(playerCamera.position, 0.3f, playerCamera.forward, out hit, attackRange))
        {
            Health enemyHealth = hit.collider.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.takeDamage(damage);
            }
        }
    }

    
   void FixedUpdate()
    {
        // move
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }
    
}
