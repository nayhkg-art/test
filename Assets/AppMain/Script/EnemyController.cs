using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("アニメーター（手動設定）")]
    public Animator enemyAnimator;

    [Header("移動速度")]
    [SerializeField] private float normalSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3.5f;

    // --- 削除 ---
    // wanderRadius と wanderTimer は使用しないため削除しました。

    [Header("攻撃設定")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackInterval = 2f;
    private float attackCooldownTimer = 0f;

    [Header("徘徊エリア設定")]
    [Tooltip("敵が徘徊する長方形のエリアをワールド座標で指定します。\nX=minX, Y=minZ, Width=幅, Height=高さ")]
    public Rect patrolArea = new Rect(-15f, -15f, 30f, 30f);

    [Header("境界での回転設定")]
    [SerializeField, Tooltip("境界に接触した際の回転角度")]
    private float rotationAngle = 70f;
    [SerializeField, Tooltip("一度回転した後の再回転までの待ち時間（秒）")]
    private float rotationCooldown = 5f;
    private float rotationCooldownTimer = 0f;

    private Transform target;
    // --- 削除 ---
    // timer は待機時間に使用していたため削除しました。

    private Vector3 wanderDestination;
    private bool isWanderingToDestination = false;

    void Start()
    {
        if (enemyAnimator == null)
        {
            Debug.LogError("InspectorでEnemy Animatorが設定されていません！", this.gameObject);
        }

        enemyAnimator.SetBool("IsRunning", false);
        transform.position = ClampPositionToArea(transform.position);
    }

    void Update()
    {
        if (enemyAnimator == null) return;

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (rotationCooldownTimer > 0f)
        {
            rotationCooldownTimer -= Time.deltaTime;
        }

        if (target != null)
        {
            Chase();
        }
        else
        {
            Wander();
        }
    }

    private void Chase()
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        enemyAnimator.SetBool("IsRunning", true);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            if (attackCooldownTimer <= 0f)
            {
                AttackPlayer();
            }
        }
        else
        {
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, target.position, chaseSpeed * Time.deltaTime);
            transform.position = ClampPositionToArea(nextPosition);
        }
    }

    private void AttackPlayer()
    {
        enemyAnimator.SetTrigger("Attack");
        attackCooldownTimer = attackInterval;

        StatusManagerPlayer playerStatus = target.GetComponent<StatusManagerPlayer>();
        if (playerStatus != null)
        {
            playerStatus.TouchDamage();
        }
    }

    private void Wander()
    {
        enemyAnimator.SetBool("IsRunning", false);

        // エリアの境界線に近づき、かつ回転のクールダウンが終わっていたら回転
        if (IsNearBoundary(0.5f) && rotationCooldownTimer <= 0f)
        {
            transform.Rotate(0, rotationAngle, 0);
            rotationCooldownTimer = rotationCooldown;
            // 回転後、すぐに新しい目的地を設定
            SetNewWanderDestination();
            return;
        }

        // --- ▼▼▼ ここからが重要な変更点 ▼▼▼ ---

        // 目的地がなければ、すぐに新しい目的地を設定
        if (!isWanderingToDestination)
        {
            SetNewWanderDestination();
        }

        // 目的地へ移動
        transform.position = Vector3.MoveTowards(transform.position, wanderDestination, normalSpeed * Time.deltaTime);

        // 目的地に到着したら、待機せず、すぐに次の目的地を設定
        if (Vector3.Distance(transform.position, wanderDestination) < 0.5f)
        {
            SetNewWanderDestination();
        }
        // --- ▲▲▲ ここまでが重要な変更点 ▲▲▲ ---
    }

    /// <summary>
    /// 徘徊用の新しい目的地を設定する
    /// </summary>
    private void SetNewWanderDestination()
    {
        float randomX = Random.Range(patrolArea.xMin, patrolArea.xMax);
        float randomZ = Random.Range(patrolArea.yMin, patrolArea.yMax);
        wanderDestination = new Vector3(randomX, transform.position.y, randomZ);

        transform.LookAt(new Vector3(wanderDestination.x, transform.position.y, wanderDestination.z));
        isWanderingToDestination = true;
    }

    private Vector3 ClampPositionToArea(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, patrolArea.xMin, patrolArea.xMax);
        position.z = Mathf.Clamp(position.z, patrolArea.yMin, patrolArea.yMax);
        return position;
    }

    private bool IsNearBoundary(float margin)
    {
        return transform.position.x <= patrolArea.xMin + margin ||
               transform.position.x >= patrolArea.xMax - margin ||
               transform.position.z <= patrolArea.yMin + margin ||
               transform.position.z >= patrolArea.yMax - margin;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void LoseTarget()
    {
        target = null;
    }
}