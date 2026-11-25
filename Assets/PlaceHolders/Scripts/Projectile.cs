using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 5;
    public float lifetime = 5f;

    void Start(){
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {   
        if (collision.gameObject.CompareTag("Player")){ 
            return; 
        }

        Health targetHealth = collision.collider.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            targetHealth.takeDamage(damage);
        }

        Destroy(gameObject);
    }
}
