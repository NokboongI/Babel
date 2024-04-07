using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    public int moveDuration;
    public int nextDirection;
    public int moveSpeed;
    public int sameDirectionCount = 0;
    public int beforeDirection;
    Vector2 front = Vector2.left;
    Vector2 boxSize = new Vector2(12f, 4f);
    bool noPlatForm;
    bool followPlayer;
    bool isFoundPlayer = false;
    RaycastHit2D frontRayHit;
    RaycastHit2D frontRayHit2;
    
    RaycastHit2D checkWall;
    RaycastHit2D rayHit;
    GameObject player; // 플레이어 객체
    public PlayerMove playerMove;
    BoxCollider2D boxCollider2D;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player"); // 플레이어 태그로 찾기
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        chooseNextDirection();
    }

    void Update(){
        if(rigid.velocity.x == 0){
            animator.SetBool("isMoving", false);

        }
            else if(nextDirection != 0){
                spriteRenderer.flipX = nextDirection == 1;
                animator.SetBool("isMoving", true);
            }else{
                animator.SetBool("isMoving", false);
            }
        
    }

    void FixedUpdate()
    {
        
        Move();
        CheckForGround();
        DetectPlayer();
        
    }

    void Move(){
        if (!followPlayer) // 플레이어를 추적 중이지 않은 경우에만 이동
        {
            rigid.velocity = new Vector2(nextDirection * moveSpeed, rigid.velocity.y);
        }else{
            CancelInvoke("chooseNextDirection"); // 자동 방향 전환 중지
            

            Invoke("MoveTowardsPlayer", 1.5f);
        }

    }
    //전방에 PlatForm, 즉 바닥이 존재하는지 판단하는 함수
    void CheckForGround()
    {
        if(followPlayer){
            Vector2 frontVec = new Vector2(rigid.position.x + nextDirection * 0.4f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1f, LayerMask.GetMask("PlatForm"));
        }else{
            Vector2 frontVec = new Vector2(rigid.position.x + nextDirection * 0.4f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1f, LayerMask.GetMask("PlatForm"));
        }
        noPlatForm = rayHit.collider == null;
        
        if (noPlatForm)
        {
            int next = Random.Range(-1, 1);

            nextDirection *= next;// 바닥이 없으면 방향 전환
        }
    }

    //플레이어를 인식하는 함수
    void DetectPlayer()
    {
        
        if(!isFoundPlayer){
            if(nextDirection!=0){
                if(nextDirection==1){
                    front = Vector2.right;
                }else{
                    front = Vector2.left;
                }
            }else{
                front = new Vector2();
            }
            Debug.DrawRay(rigid.position, front, new Color(0, 0, 1));
            
            frontRayHit = Physics2D.Raycast(rigid.position, front, 10.0f, LayerMask.GetMask("Player"));
        }else{
            Debug.DrawRay(rigid.position, Vector2.left, new Color(0, 0, 1));
            Debug.DrawRay(rigid.position, Vector2.right, new Color(0, 0, 1));
            frontRayHit = Physics2D.Raycast(rigid.position, Vector2.left, 10.0f, LayerMask.GetMask("Player"));
            frontRayHit2 = Physics2D.Raycast(rigid.position, Vector2.right, 10.0f, LayerMask.GetMask("Player"));
        }       
        checkWall = Physics2D.Raycast(rigid.position, front, boxSize.x/2, LayerMask.GetMask("PlatForm"));
        float wallPlace = 0;
        if(checkWall.collider != null){
            wallPlace = checkWall.point.x;
        }
        if ((frontRayHit.collider != null && !noPlatForm) || (frontRayHit2.collider != null && !noPlatForm))
        {
            if (!followPlayer)
            {
                followPlayer = true; // 플레이어 추적 시작
                isFoundPlayer = true;
                CancelInvoke("chooseNextDirection"); // 자동 방향 전환 중지
                rigid.velocity = Vector2.zero; // 움직임을 즉시 중지
                Debug.Log("Player detected - following.");
            }
        }
        if(playerMove.getIsOnPlatForm()){
            float playerY = player.transform.position.y;
            float platformY = rigid.position.y;
            if (Mathf.Abs(playerY - platformY) > 1f)
            {
                // 플레이어와 플랫폼의 y 축 위치가 차이가 크면 추적 중지
                Debug.Log("Player is on another place");
                followPlayer = false;
                return;
            }else{
                Debug.Log("Player is on almost same place");
            }
        }
        }

        void MoveTowardsPlayer()
        {
            if (player != null && followPlayer && !noPlatForm)
            {
                Vector2 directionVec = ((Vector2)player.transform.position - rigid.position).normalized;
                // Y 축 이동을 없애고 싶다면, directionVec의 y를 0으로 설정
                directionVec.y = 0;
                rigid.velocity = new Vector2(directionVec.x * moveSpeed, rigid.velocity.y);
                nextDirection = Mathf.RoundToInt(directionVec.x);
            }
            else
            {
                if (noPlatForm)
                {
                    Debug.Log("no platform there");
                    isFoundPlayer = false;
                    followPlayer = false;
                    frontRayHit = new RaycastHit2D();
                    frontRayHit2 = new RaycastHit2D();
                }
                nextDirection = 0;
                CancelInvoke();

                Invoke("chooseNextDirection", 1.5f);
            }
        }


        void chooseNextDirection()
        {
                if(nextDirection==0){
                    do {
                        nextDirection = Random.Range(-1, 2);
                    } while (nextDirection == 0);
                    beforeDirection = nextDirection;
                    sameDirectionCount = 0;
                }else{
                    nextDirection = Random.Range(-1, 2);
                    if(beforeDirection == nextDirection && sameDirectionCount >= 1){
                        do {
                        nextDirection = Random.Range(-1, 2);
                        } while (nextDirection == beforeDirection);
                        beforeDirection = nextDirection;
                        sameDirectionCount = 0;
                    }else if(beforeDirection == nextDirection){
                        beforeDirection = nextDirection;
                        sameDirectionCount++;
                    }else{
                        beforeDirection = nextDirection;
                        sameDirectionCount = 0;
                    }
                }
                
                if(nextDirection==0){
                    moveDuration = Random.Range(1, 3);

                }else{
                    moveDuration = Random.Range(2, 5);
                }
                Invoke("chooseNextDirection", moveDuration);
            
            
        }
        

        
}