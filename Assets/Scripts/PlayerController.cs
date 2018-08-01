using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float moveInput = 0;
    private bool facingRight = true;
    public float horizontalVelocityMax = 10f;
    private float horizontalVelocity;
    private float verticalVelocity = 0;
    public float horizontalAcceleration = 0.5f;
    private float horizontalAccelerationTime = 0;
    public float horizontalFriction = 0.2f;
    private float horizontalFrictionTime = 0;
    public float horizontalFrictionStopping = 0.2f;
    public float horizontalFrictionTurning = 0.2f;
    
    private bool onGround = false;
    public Transform groundCheck;
    private float groundCheckRadius = 0.2f;
    public LayerMask ground;
    private float onGroundRemember;
    public float onGroundRememberTime = 0.2f;

    private bool onWall = false;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wall;
    public float wallSlideMultiplier = 0.5f;
    public float wallJumpHorizontalVelocity = 10f;
    public float wallJumpVerticalVelocity = 25f;
    public bool isWallSliding = false;
    public bool isWallJumping = false;

    public float jumpVelocity = 10f;
    public float cutJumpHeight = 0.5f; 
    private float jumpPressedRemember;
    public float jumpPressedRememberTime = 0.2f;

    private Rigidbody2D rigid2D;

	void Start () {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        onGround = IsOnGround();
        onWall = IsOnWall();

        Move();
    }

    void Update () {
        Jump();
        WallSlide();
        WallJump();
    }

    void OnGUI() {
        GUI.Label(new Rect(50, 10, 300, 20), "Horizontal velocity: " + rigid2D.velocity.x);
        GUI.Label(new Rect(50, 25, 300, 20), "Vertical velocity: " + rigid2D.velocity.y);
        
        GUI.Label(new Rect(50, 45, 300, 20), "On ground: " + onGround);
        GUI.Label(new Rect(50, 60, 300, 20), "On wall: " + onWall);
        GUI.Label(new Rect(50, 75, 300, 20), "Wall Jump: " + isWallJumping);
        GUI.Label(new Rect(50, 90, 300, 20), "Wall Slide: " + isWallSliding);
    }

    // Checks if the player is on ground
    private bool IsOnGround() {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);
    }

    // Checks if the player is on a wall
    private bool IsOnWall() {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wall);
    }

    // Movement controlls
    private float Move() {
        moveInput = Input.GetAxisRaw("Horizontal");
      
        if (isWallSliding) {
            return moveInput;  
        }

        // Acceleration
        if (moveInput != 0) {
            horizontalAccelerationTime = horizontalAccelerationTime + Time.deltaTime;
            horizontalVelocity = (horizontalAcceleration - horizontalFriction) * (horizontalAccelerationTime * moveInput) + rigid2D.velocity.x;
  
        } else {
            horizontalAccelerationTime = 0;
        }

        // Speedcap
        if (moveInput * horizontalVelocity > horizontalVelocityMax) {
            horizontalVelocity = moveInput * horizontalVelocityMax;
        }

        // Stopping
        if (Mathf.Abs(moveInput) < 0.01f) {

            horizontalFrictionTime = horizontalFrictionTime + Time.deltaTime;

            if (facingRight) {
                if (horizontalVelocity <= 0) {
                    horizontalVelocity = 0;
                    horizontalFrictionTime = 0;
                } else {
                    horizontalVelocity = -(horizontalFrictionStopping * horizontalFrictionTime) + rigid2D.velocity.x;
                }
            } else {
                if (horizontalVelocity >= 0) {
                    horizontalVelocity = 0;
                    horizontalFrictionTime = 0;
                } else {
                    horizontalVelocity = (horizontalFrictionStopping * horizontalFrictionTime) + rigid2D.velocity.x;
                }
            }
        }   

        rigid2D.velocity = new Vector2(horizontalVelocity, rigid2D.velocity.y);

        // Flip sprite according to the direction the player is facing
        if (moveInput > 0 && !facingRight) {
            Flip();
        } else if (moveInput < 0 && facingRight) {
            Flip();
        }

        return moveInput;
    }

    // Jump controlls
    private void Jump() {
        // The player is able to jump nethertheless he is falling from a ledge,
        // if he is in the specified time period, defined in onGroundRememberTime
        onGroundRemember -= Time.deltaTime;

        if (onGround) {
            onGroundRemember = onGroundRememberTime;
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

        if (jumpPressedRemember > 0 && onGroundRemember > 0) {
            onGroundRemember = 0;
            jumpPressedRemember = 0;
            rigid2D.velocity = new Vector2(rigid2D.velocity.x, jumpVelocity);
        }
    }

    private void WallSlide() {
        if (onWall && !onGround) {
            isWallSliding = true;
            isWallJumping = false;

            verticalVelocity = rigid2D.velocity.y * wallSlideMultiplier;
            rigid2D.velocity = new Vector2(rigid2D.velocity.x, verticalVelocity);
        } else {
            isWallSliding = false;
        }        
    }

    private void WallJump() {
        if (isWallSliding) {
                if (facingRight && Input.GetAxis("Horizontal") == -1 && Input.GetButton("Jump")) {
                    rigid2D.velocity = new Vector2(-(wallJumpHorizontalVelocity), wallJumpVerticalVelocity);
                    isWallSliding = false;
                    isWallJumping = true;
            } else if (!facingRight && Input.GetAxis("Horizontal") == 1 && Input.GetButton("Jump")) {
                    rigid2D.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
                    isWallSliding = false;
                    isWallJumping = true;
            }

                
        }
    }

        // Wall jump / slide controlls
        /*
        private void WallJump() {
            if (onWall && !onGround) {

                isWallSliding = true;
                isWallJumping = false;

                if (isWallSliding) {
                    verticalVelocity = rigid2D.velocity.y * wallSlideMultiplier;
                    rigid2D.velocity = new Vector2(rigid2D.velocity.x, verticalVelocity);

                    if (facingRight && Input.GetAxisRaw("Horizontal") == -1 || !facingRight && Input.GetAxisRaw("Horizontal") == 1) {

                        if (Input.GetButton("Jump")) {
                            if (facingRight && Input.GetAxis("Horizontal") == -1) {
                                rigid2D.velocity = new Vector2(-(wallJumpHorizontalVelocity), wallJumpVerticalVelocity);
                            } else if (!facingRight && Input.GetAxis("Horizontal") == 1) {
                                rigid2D.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
                            }

                            isWallSliding = false;
                            isWallJumping = true;
                        }
                    }
                }
            }

            if (onGround) {
                isWallJumping = false;
            }
        }
        */

        // Flips the sprite according to the direction the player is facing
        private void Flip() {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
