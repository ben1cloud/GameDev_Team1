using UnityEngine;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void takeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth-=damage;
        if(currentHealth<=0){
            die();
        }
    }

    public void die()
    {
        Debug.Log(gameObject.name + " has died!");
        Destroy(gameObject);
    }


}
