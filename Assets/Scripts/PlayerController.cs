﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigid2D;

    [Header("Ground Check")]
    public float     isOnGroundRememberTime = 0.2f;
    public Transform groundCheck;
    public LayerMask ground;
    private bool     isOnGround = false;
    private float    groundCheckRadius = 0.2f;
    private float    isOnGroundRemember;

    [Space(10)]

    [Header("Wall Check")]
    public Transform wallCheck;
    public float     wallCheckRadius = 0.2f;
    public LayerMask wall;
    private bool     isOnWall = false;

    [Space(10)]

    [Header("Movement")]
    public float  walkSpeed = 15f;
    private float moveInput = 0;
    private int   direction = 1;
    private float verticalVelocity = 0;

    [Space(10)]

    [Header("Dash")]
    public float  dashVelocity = 30f;
    public float  dashTime = 1f;
    private float dashTimer = 0;
    private bool  isDashing = false;

    [Space(10)]

    [Header("Air Dash")]
    public float  airDashVelocity = 30f;
    public float  airDashTime = 1f;
    private float airDashTimer = 0;
    private bool  isAirDashing = false;

    [Space(10)]

    [Header("Wall Slide / Jump")]
    public float wallSlideMultiplier = 0.5f;
    public float wallJumpHorizontalVelocity = 10f;
    public float wallJumpVerticalVelocity = 25f;
    private bool isWallSliding = false;
    private bool isWallJumping = false;

    [Space(10)]

    [Header("Jump")]
    public float      jumpVelocity = 10f;
    public float      cutJumpHeight = 0.5f; 
    private float     jumpPressedRemember;
    public float      jumpPressedRememberTime = 0.2f;
    private bool      isJumping = false;
    public GameObject playerLanding;
    private bool      jumpSpawnParticle = false;

    [Space(10)]

    [Header("Air Stomp")]
    public float airStompVelocity = 30f;
    private bool isAirStomping = false;
    private bool isAirStompLanding = false;


    // Start
	void Start () {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    // Fixed Update
    void FixedUpdate() {
        isOnGround = IsOnGround();
        isOnWall = IsOnWall();

        Move();
    }

    // Update
    void Update () {
        Dash();
        Jump();
        AirDash();
        AirStomp();
        WallSlide();
        WallJump();
        
        Test();
    }

    // Test
    private void Test() {
       
    }

    // Debug Output
    void DebugOutput() {
        GUI.Label(new Rect(50, 10, 300, 20), "Horizontal velocity: " + rigid2D.velocity.x);
        GUI.Label(new Rect(50, 25, 300, 20), "Vertical velocity: " + rigid2D.velocity.y);
        GUI.Label(new Rect(50, 40, 300, 20), "direction: " + direction);

        GUI.Label(new Rect(50, 60, 300, 20), "On ground: " + isOnGround);
        GUI.Label(new Rect(50, 75, 300, 20), "On wall: " + isOnWall);

        GUI.Label(new Rect(50, 95, 300, 20), "Dash: " + isDashing);
        GUI.Label(new Rect(50, 110, 300, 20), "Air Dash: " + isAirDashing);
        GUI.Label(new Rect(50, 125, 300, 20), "Jump: " + isJumping);
        GUI.Label(new Rect(50, 140, 300, 20), "Wall jump: " + isWallJumping);
        GUI.Label(new Rect(50, 155, 300, 20), "Wall slide: " + isWallSliding);
        GUI.Label(new Rect(50, 170, 300, 20), "Air stomp: " + isAirStomping);
    }

    // GUI
    void OnGUI() {
        DebugOutput();
    }

    // Checks if the player is colliding with ground
    private bool IsOnGround() {

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);
    }

    // Checks if the player is colliding with a wall
    private bool IsOnWall() {

        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wall);
    }

    // Flips the sprite according to the direction the player is facing
    private void Flip() {
        direction = -1 * (int)Mathf.Sign(direction);
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Move() {
        
        if (!isWallSliding && !isDashing && !isAirDashing) {
            moveInput = Input.GetAxisRaw("Horizontal");
            rigid2D.velocity = new Vector2(walkSpeed * moveInput, rigid2D.velocity.y);
        }

        // Flip sprite according to the direction the player is facing
        if (moveInput > 0 && direction == -1) {
            Flip();
        }
        else if (moveInput < 0 && direction == 1) {
            Flip();
        }
    }
    
    // Dash controlls
    private void Dash() {

        if (!isOnGround) {
            isDashing = false;
        }
        else {

            if (isDashing) {
                dashTimer = dashTimer + Time.deltaTime;
            }
            else {
                dashTimer = 0;
            }

            if (dashTimer < dashTime) {

                if (Input.GetButtonDown("Dash")) {
                    isDashing        = true;
                    rigid2D.velocity = new Vector2(rigid2D.velocity.x + (direction * dashVelocity), rigid2D.velocity.y);
                }
                else if (Input.GetButtonUp("Dash")) {
                    isDashing = false;
                }
            }
            else {
                isDashing = false;
            }
        }
    }

    // Air dash controlls
    private void AirDash() {

        if (isOnGround) {
            isAirDashing = false;
        }
        else {

            if (isAirDashing) {
                airDashTimer = airDashTimer + Time.deltaTime;
            }
            else {
                airDashTimer = 0;
            }

            if (airDashTimer < airDashTime) {

                if (Input.GetButtonDown("Dash")) {
                    isAirDashing         = true;
                    rigid2D.gravityScale = 0;
                    rigid2D.velocity     = new Vector2(rigid2D.velocity.x + (direction * airDashVelocity), 0);
                }
            }
            else {
                isAirDashing         = false;
                rigid2D.gravityScale = 1;
            }
        }
    }

    // Jump controlls
    private void Jump() {
        // The player is able to jump nethertheless he is falling from a ledge,
        // if he is in the specified time period, defined in isOnGroundRememberTime
        isOnGroundRemember -= Time.deltaTime;

        if (isOnGround) {
            isJumping          = false;
            isOnGroundRemember = isOnGroundRememberTime;

            if (jumpSpawnParticle) {
                Instantiate(playerLanding, groundCheck.position, Quaternion.identity);
                jumpSpawnParticle = false;
            }
        } else {
            jumpSpawnParticle = true;
        }

        // The player is able to jump nethertheless he is shortly above the ground,
        // if he is in the specified time period, defined in jumpPressedRememberTime
        jumpPressedRemember -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) {
            jumpPressedRemember = jumpPressedRememberTime;
        }

        // Determines how high the player will jump depending on how long
        // the jump button is held down
        if (Input.GetButtonUp("Jump")) {
            
            if (rigid2D.velocity.y > 0) {
                rigid2D.velocity = new Vector2(rigid2D.velocity.x, rigid2D.velocity.y * cutJumpHeight);
            }
        }

        if (jumpPressedRemember > 0 && isOnGroundRemember > 0) {
            isJumping = true;
            isOnGroundRemember = 0;
            jumpPressedRemember = 0;
            rigid2D.velocity    = new Vector2(rigid2D.velocity.x, jumpVelocity);
        }
    }

    // Air stomp controlls
    private void AirStomp() {

        if (isOnGround) {
            if (isAirStompLanding) {
                CameraShaker.Instance.ShakeOnce(4f, 4f, .1f, 1f);
            }

            isAirStompLanding = false;
            isAirStomping = false;
        }
        else {

            if (Input.GetAxisRaw("Vertical") == -1) {
                isAirStomping     = true;
                isAirStompLanding = true;
                rigid2D.velocity  = new Vector2(0, rigid2D.velocity.y + (-1f * airStompVelocity));
            }

        }
    }

    // Wall slide controlls
    private void WallSlide() {

        if (isOnWall && !isOnGround) {
            isWallSliding = true;
            isWallJumping = false;

            verticalVelocity = rigid2D.velocity.y * wallSlideMultiplier;
            rigid2D.velocity = new Vector2(rigid2D.velocity.x, verticalVelocity);
        }
        else {
            isWallSliding = false;
        }        
    }

    // Wall jump controlls
    private void WallJump() {

        if (isWallSliding) {

            if (direction == 1 && Input.GetAxis("Horizontal") == -1 && Input.GetButton("Jump")) {
                rigid2D.velocity = new Vector2(-(wallJumpHorizontalVelocity), wallJumpVerticalVelocity);
                isWallSliding    = false;
                isWallJumping    = true;
            } 
            else if (direction == -1 && Input.GetAxis("Horizontal") == 1 && Input.GetButton("Jump")) {
                rigid2D.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
                isWallSliding    = false;
                isWallJumping    = true;
            }               
        }
    }

}