using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public float maxHealth = 50f;
    public float moveSpeed = 3f;
    public float detectionRange = 8f;
    public int attackDamage = 10;

    [Header("Efecto de Empuje (Knockback)")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float knockbackPower = 1f;

    [Header("Efectos de 'Juice'")]
    public Vector2 squashAndStretch = new Vector2(1.25f, 0.75f);
    public float deformDuration = 0.2f;

    [Header("Referencia al Jugador")]
    public Transform playerTransform;

    private float currentHealth;
    private bool isBeingKnockedBack = false;
    private Vector3 originalScale;
    private bool isDeforming = false;

    void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
        if (playerTransform == null)
        {
            Debug.LogError("¡No se ha asignado la referencia del jugador (Player Transform) en el Inspector del Enemigo!");
        }
    }

    void Update()
    {
        if (isBeingKnockedBack || playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        if (!isBeingKnockedBack) StartCoroutine(KnockbackCoroutine(knockbackDirection));
        if (!isDeforming) StartCoroutine(DeformCoroutine());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator DeformCoroutine()
    {
        isDeforming = true;

        transform.localScale = new Vector3(originalScale.x * squashAndStretch.x, originalScale.y * squashAndStretch.y, originalScale.z);
        yield return new WaitForSeconds(deformDuration / 2);


        transform.localScale = originalScale;

        isDeforming = false;
    }

    private IEnumerator KnockbackCoroutine(Vector2 knockbackDirection)
    {
        isBeingKnockedBack = true;
        float elapsedTime = 0f;
        while (elapsedTime < knockbackDuration)
        {
            transform.Translate(knockbackDirection * knockbackForce * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isBeingKnockedBack = false;
    }

    void Die() { Destroy(gameObject); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController_Guerrero player = other.GetComponent<PlayerController_Guerrero>();
            if (player != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                player.TakeDamage(attackDamage, knockbackDirection, knockbackPower);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

