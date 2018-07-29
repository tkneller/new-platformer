using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 10f;
    private float moveInput = 0f;
    private bool facingRight = true;

    private bool onGround = false;
    public Transform groundCheck;
    private float groundCheckRadius = 0.2f;
    public LayerMask ground;

    public float jumpForce = 10f;

    private Rigidbody2D rigid2;

	void Start () {
        rigid2 = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        onGround = isOnGround();

        move();
    }

    void Update () {
        jump();
    }

    private bool isOnGround() {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);
    }

    private void move() {
        moveInput = Input.GetAxis("Horizontal");
        rigid2.velocity = new Vector2(moveInput * maxSpeed, rigid2.velocity.y);

        if (moveInput > 0 && !facingRight) {
            Flip();
        } else if (moveInput < 0 && facingRight) {
            Flip();
        }
    }

    private void jump() {
        if (onGround && Input.GetButtonDown("Jump")) {
            rigid2.velocity = Vector2.up * jumpForce;
        }
    }

    private void Flip() {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
