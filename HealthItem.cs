using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("Configuración de Curación")]
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            PlayerController_Guerrero player = other.GetComponent<PlayerController_Guerrero>();

            if (player != null)
            {
                player.Heal(healAmount);

                Destroy(gameObject);
            }
        }
    }
}
