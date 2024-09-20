using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveDuration = 0.2f; // 1マスの移動にかかる時間
    public LayerMask obstacleMask; // 壁のレイヤー（Inspectorで「Wall」を設定）

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // プレイヤーの初期位置を整数値にスナップ
        Vector2 startPos = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
        rb.position = startPos;
        targetPosition = rb.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // 入力がない場合は何もしない
            if (input != Vector2.zero)
            {
                Debug.Log("Input detected: " + input);
                // 斜め移動を許可する
                input = input.normalized;

                // 新しいターゲット位置を計算
                Vector2 newTarget = rb.position + input;

                // ターゲット位置が移動可能かチェック
                if (IsWalkable(newTarget))
                {
                    Debug.Log("Moving to: " + newTarget);
                    targetPosition = newTarget;
                    StartCoroutine(MoveToTarget());
                }
                else
                {
                    Debug.Log("Target position blocked: " + newTarget);
                }
            }
        }
    }

    bool IsWalkable(Vector2 position)
    {
        // ターゲット位置がFloorタイルであることを確認
        // FloorタイルにはColliderがないため、壁との衝突を確認
        float checkRadius = 0.1f;
        Collider2D hit = Physics2D.OverlapCircle(position, checkRadius, obstacleMask);
        return hit == null;
    }

    IEnumerator MoveToTarget()
    {
        isMoving = true;

        float elapsedTime = 0f;
        Vector2 startingPos = rb.position;
        Vector2 endingPos = targetPosition;

        while (elapsedTime < moveDuration)
        {
            rb.MovePosition(Vector2.Lerp(startingPos, endingPos, (elapsedTime / moveDuration)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(endingPos);

        // 移動終了後に位置を整数にスナップ
        rb.position = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));

        isMoving = false;
    }

    // プレイヤーの位置を設定するメソッド
    public void SetPosition(Vector2 newPosition)
    {
        rb.position = new Vector2(Mathf.Round(newPosition.x), Mathf.Round(newPosition.y));
        targetPosition = rb.position;
    }

    // デバッグ用にターゲット位置を表示
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.1f);
        }
    }
}
