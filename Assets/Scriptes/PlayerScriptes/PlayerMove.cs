using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    public float jumpCount;
    public float currentDirection = 100;
    public bool canWalk = true;
    public bool isOnPlatForm;
    public Vector2 boxSize = new Vector2(0.5f, 0.1f);

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    BoxCollider2D boxCollider2D;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Update(){
        // 점프 관련 기능
        if(Input.GetButtonDown("Jump") && jumpCount > 0){
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", true);
            setIsOnPlatFrom(false);
            canWalk = false;
            jumpCount--;
        }
        if(Input.GetButtonUp("Jump")){
            animator.SetBool("isJumping", false);
        }
        if(rigid.velocity.y<0.0f){
            animator.SetBool("isFalling", true);
        }
        // 이동 방향 변경
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if(horizontalInput != 0){
            spriteRenderer.flipX = horizontalInput == -1;
        }
        if(math.abs(rigid.velocity.x)<0.2||!canWalk){

            animator.SetBool("isMoving", false);
        }
        else if(canWalk){
            animator.SetBool("isMoving", true);
        }
       
        if (rigid.velocity.y < 0)
        {
            // 바닥 접촉 판단
            RaycastHit2D rayHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, 1f, LayerMask.GetMask("PlatForm"));

            // 바닥에 닿았을 경우 점프 최대 횟수 초기화
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                {
                    jumpCount = 2;
                    animator.SetBool("isJumping", false);
                    canWalk = true;
                    animator.SetBool("isFalling", false);
                    setIsOnPlatFrom(true);
                }
            }
        }

    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if(currentDirection ==100){
            currentDirection = h;
        }
        // 즉시 멈추는 로직
        if (h != currentDirection){
            rigid.velocity = new Vector2(0, rigid.velocity.y);
            currentDirection = h;

        } else {
            // 속도 조정을 통한 이동
            float targetVelocityX = h * maxSpeed;
            rigid.velocity = Vector2.right * targetVelocityX + Vector2.up * rigid.velocity.y;
}

        // 최대 속도 제한
        if (Mathf.Abs(rigid.velocity.x) > maxSpeed){
            rigid.velocity = new Vector2(Mathf.Sign(rigid.velocity.x) * maxSpeed, rigid.velocity.y);
        }
    }

    void setIsOnPlatFrom(bool state){
        this.isOnPlatForm = state;
    }
    public bool getIsOnPlatForm(){
        return this.isOnPlatForm;
    }
}