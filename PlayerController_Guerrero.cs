using UnityEngine;
using System.Collections;

public class PlayerController_Guerrero : MonoBehaviour
{
    [Header("Estadísticas de Movimiento")]
    public float moveSpeed = 5f;
    public float dodgeSpeed = 10f;
    public float dodgeDuration = 0.3f;

    [Header("Estadísticas de Combate")]
    public float attackRange = 1.5f;
    public float attackAngle = 180f;
    public float attackCooldown = 0.5f;
    public int maxSharpness = 10;
    public float sharpenTime = 3f;
    public LayerMask enemyLayer;

    [Header("Salud del Jugador")]
    public int maxRecoverableHealth = 4;
    private int currentHealth;

    [Header("Efecto de Empuje Recibido")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.2f;

    [Header("Efectos de 'Juice'")]
    public float hitStopDuration = 0.05f;
    public float cameraShakeMagnitude = 0.1f;
    public float cameraShakeDuration = 0.15f;
    public Vector2 squashAndStretch = new Vector2(0.8f, 1.2f); 
    public float deformDuration = 0.2f; 

    [Header("Referencias Visuales")]
    public UIHealthManager healthUI;
    public UISharpnessManager sharpnessUI;
    public GameObject attackVisualizer;
    public float attackVisualRotationOffset = 0f;
    public float attackVisualDistance = 0.5f;
    public CameraFollow mainCameraFollow;


    private Vector2 moveInput;
    private bool isDodging = false;
    private float dodgeTimer;
    private float lastAttackTime;
    private float currentSharpness; 
    private bool isSharpening = false;
    private float sharpenTimer;
    private Camera mainCamera;
    private bool isBeingKnockedBack = false;
    private Vector3 originalScale;
    private bool isDeforming = false;

    void Start()
    {
        currentHealth = 5;
        currentSharpness = maxSharpness;
        mainCamera = Camera.main;
        originalScale = transform.localScale;

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth, maxRecoverableHealth);
        }
        if (sharpnessUI != null)
        {
            sharpnessUI.UpdateSharpnessUI(currentSharpness, maxSharpness);
        }
        if (attackVisualizer != null)
        {
            attackVisualizer.SetActive(false);
        }
        if (mainCameraFollow == null)
        {
            mainCameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
    }

    void Update()
    {
        if (isBeingKnockedBack || isDeforming) return;
        if (isSharpening) { ProcessSharpen(); return; }
        if (isDodging) { ProcessDodge(); return; }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (Input.GetKeyDown(KeyCode.Space) && moveInput != Vector2.zero) StartDodge();
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown) Attack();
        if (Input.GetKeyDown(KeyCode.R)) StartSharpen();
    }

    void FixedUpdate()
    {
        if (!isDodging && !isSharpening && !isBeingKnockedBack && !isDeforming)
        {
            transform.Translate(moveInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        float damage = (currentSharpness > 0) ? GetDamage() : 1.0f;
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (mousePos - (Vector2)transform.position).normalized;

        StartCoroutine(ShowAttackVisual(attackDirection));

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        bool hasHitEnemy = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            if (Vector2.Angle(attackDirection, directionToEnemy) < attackAngle / 2)
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(damage, attackDirection);
                    hasHitEnemy = true;
                    if (mainCameraFollow != null)
                    {
                        mainCameraFollow.TriggerShake(attackDirection, cameraShakeMagnitude, cameraShakeDuration);
                    }
                }
            }
        }
        if (hasHitEnemy && currentSharpness > 0)
        {
            currentSharpness--;
            if (sharpnessUI != null)
            {
                sharpnessUI.UpdateSharpnessUI(currentSharpness, maxSharpness);
            }
        }
    }

    IEnumerator ShowAttackVisual(Vector2 attackDirection)
    {
        if (attackVisualizer == null) yield break;
        Vector2 visualizerPosition = (Vector2)transform.position + (attackDirection * attackVisualDistance);
        attackVisualizer.transform.position = visualizerPosition;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        attackVisualizer.transform.rotation = Quaternion.Euler(0, 0, angle + attackVisualRotationOffset);
        attackVisualizer.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        attackVisualizer.SetActive(false);
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxRecoverableHealth)
        {
            Debug.Log("Vida al máximo, no se puede curar.");
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxRecoverableHealth);

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth, maxRecoverableHealth);
        }
        Debug.Log("¡Jugador curado! Vida actual: " + currentHealth);
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float enemyKnockbackPower)
    {
        if (isDodging) return;

        currentHealth -= damage;
        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth, maxRecoverableHealth);
        }

        StopAllCoroutines();
        StartCoroutine(HitStopCoroutine());
        StartCoroutine(KnockbackCoroutine(knockbackDirection, enemyKnockbackPower));
        StartCoroutine(DeformCoroutine());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator KnockbackCoroutine(Vector2 knockbackDirection, float enemyKnockbackPower)
    {
        isBeingKnockedBack = true;
        float elapsedTime = 0f;
        float totalKnockbackForce = knockbackForce * enemyKnockbackPower;
        while (elapsedTime < knockbackDuration)
        {
            transform.Translate(knockbackDirection * totalKnockbackForce * Time.unscaledDeltaTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        isBeingKnockedBack = false;
    }

    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    private IEnumerator DeformCoroutine()
    {
        isDeforming = true;
        transform.localScale = new Vector3(originalScale.x * squashAndStretch.x, originalScale.y * squashAndStretch.y, originalScale.z);
        yield return new WaitForSeconds(deformDuration / 2);
        transform.localScale = originalScale;
        isDeforming = false;
    }

    void StartDodge() { isDodging = true; dodgeTimer = dodgeDuration; }
    void ProcessDodge() { transform.Translate(moveInput * dodgeSpeed * Time.deltaTime); dodgeTimer -= Time.deltaTime; if (dodgeTimer <= 0f) isDodging = false; }
    float GetDamage() { return 1.0f * currentSharpness; }

    void StartSharpen()
    {
        if (currentSharpness >= maxSharpness || isSharpening) return;

        if (mainCameraFollow != null) mainCameraFollow.SetZoom(true);

        isSharpening = true;
        sharpenTimer = 0f;
    }

    void ProcessSharpen()
    {
        sharpenTimer += Time.deltaTime;
        float recoveredSharpness = (sharpenTimer / sharpenTime) * maxSharpness;
        currentSharpness = Mathf.Min(recoveredSharpness, maxSharpness);

        if (sharpnessUI != null)
        {
            sharpnessUI.UpdateSharpnessUI(currentSharpness, maxSharpness);
        }

        if (sharpenTimer >= sharpenTime)
        {
            isSharpening = false;
            currentSharpness = maxSharpness;

            if (mainCameraFollow != null) mainCameraFollow.SetZoom(false);
        }
    }

    void Die() { this.enabled = false; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        UnityEditor.Handles.color = new Color(1, 0, 0, 0.2f);
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.forward, direction, attackAngle / 2, attackRange);
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.forward, direction, -attackAngle / 2, attackRange);
    }
#endif
}
