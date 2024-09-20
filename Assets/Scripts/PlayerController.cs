using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveDuration = 0.2f; // 1�}�X�̈ړ��ɂ����鎞��
    public LayerMask obstacleMask; // �ǂ̃��C���[�iInspector�ŁuWall�v��ݒ�j

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
        // �v���C���[�̏����ʒu�𐮐��l�ɃX�i�b�v
        Vector2 startPos = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
        rb.position = startPos;
        targetPosition = rb.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // ���͂��Ȃ��ꍇ�͉������Ȃ�
            if (input != Vector2.zero)
            {
                Debug.Log("Input detected: " + input);
                // �΂߈ړ���������
                input = input.normalized;

                // �V�����^�[�Q�b�g�ʒu���v�Z
                Vector2 newTarget = rb.position + input;

                // �^�[�Q�b�g�ʒu���ړ��\���`�F�b�N
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
        // �^�[�Q�b�g�ʒu��Floor�^�C���ł��邱�Ƃ��m�F
        // Floor�^�C���ɂ�Collider���Ȃ����߁A�ǂƂ̏Փ˂��m�F
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

        // �ړ��I����Ɉʒu�𐮐��ɃX�i�b�v
        rb.position = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));

        isMoving = false;
    }

    // �v���C���[�̈ʒu��ݒ肷�郁�\�b�h
    public void SetPosition(Vector2 newPosition)
    {
        rb.position = new Vector2(Mathf.Round(newPosition.x), Mathf.Round(newPosition.y));
        targetPosition = rb.position;
    }

    // �f�o�b�O�p�Ƀ^�[�Q�b�g�ʒu��\��
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.1f);
        }
    }
}
