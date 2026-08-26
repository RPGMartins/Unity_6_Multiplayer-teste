using UnityEngine;

public class LocalTarget : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private int life;

    public void Initialize(int life)
    {
        this.life = life;
    }

    public void Hit(int damage)
    {
        life -= damage;

        if (life <= 0)
        {
            Destroy(targetTransform.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        life -= damage;

        //Efeitos de hit
    }
}
