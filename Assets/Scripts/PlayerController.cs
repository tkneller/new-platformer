using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement
    private float moveInput = 0;
    private int   direction = 1;
    public float  horizontalVelocityMax = 10f;
    private float horizontalVelocity;
    private float verticalVelocity = 0;
    public float  horizontalAcceleration = 0.5f;
    private float horizontalAccelerationTime = 0;
    public float  horizontalFriction = 0.2f;
    private float horizontalFrictionTime = 0;
    public float  horizontalFrictionStopping = 0.2f;
    public float  horizontalFrictionTurning = 0.2f;

    // Dash
    public float  dashVelocity = 30f;
    public float  dashTime = 5f;
    private float dashTimer = 0;
    private bool  isDashing = false;

    // Ground check
    private bool     isOnGround = false;
    public Transform groundCheck;
    private float    groundCheckRadius = 0.2f;
    public LayerMask ground;
    private float    isOnGroundRemember;
    public float     isOnGroundRememberTime = 0.2f;

    // Wall check
    private bool     isOnWall = false;
    public Transform wallCheck;
    public float     wallCheckRadius = 0.2f;
    public LayerMask wall;

    // Wall slide / jump
    public float wallSlideMultiplier = 0.5f;
    public float wallJumpHorizontalVelocity = 10f;
    public float wallJumpVerticalVelocity = 25f;
    public bool  isWallSliding = false;
    public bool  isWallJumping = false;

    // Jump
    public float  jumpVelocity = 10f;
    public float  cutJumpHeight = 0.5f; 
    private float jumpPressedRemember;
    public float  jumpPressedRememberTime = 0.2f;
    private bool  isJumping = false;

    // Air dash
    public float airDashVelocity = 15f;
    private bool isAirDashing = false;

    private Rigidbody2D rigid2D;

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
        WallSlide();
        WallJump();
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

    // Movement controlls
    private float Move() {
        moveInput = Input.GetAxisRaw("Horizontal");
      
        // Deactivactivates movement when player is sliding down a wall
        if (isWallSliding || isDashing) {
            return moveInput;  
        }

        // Acceleration
        if (moveInput != 0) {
            horizontalAccelerationTime = horizontalAccelerationTime + Time.deltaTime;
            horizontalVelocity         = (horizontalAcceleration - horizontalFriction) 
                                       * (horizontalAccelerationTime * moveInput) 
                                       + rigid2D.velocity.x;
        }
        else {
            horizontalAccelerationTime = 0;
        }

        // Speedcap
        if (moveInput * horizontalVelocity > horizontalVelocityMax) {
            horizontalVelocity = moveInput * horizontalVelocityMax;
        }

        // Stopping
        if (Mathf.Abs(moveInput) < 0.01f) {

            horizontalFrictionTime = horizontalFrictionTime + Time.deltaTime;

            if (direction == 1) {

                if (horizontalVelocity    <= 0) {
                    horizontalVelocity     = 0;
                    horizontalFrictionTime = 0;
                }
                else {
                    horizontalVelocity = -(horizontalFrictionStopping * horizontalFrictionTime) + rigid2D.velocity.x;
                }
            }
            else {

                if (horizontalVelocity >= 0) {
                    horizontalVelocity     = 0;
                    horizontalFrictionTime = 0;
                }
                else {
                    horizontalVelocity = (horizontalFrictionStopping * horizontalFrictionTime) + rigid2D.velocity.x;
                }
            }
        }   

        rigid2D.velocity = new Vector2(horizontalVelocity, rigid2D.velocity.y);

        // Flip sprite according to the direction the player is facing
        if (moveInput > 0 && direction == -1) {
            Flip();
        }
        else if (moveInput < 0 && direction == 1) {
            Flip();
        }

        return moveInput;
    }

    // Dash controlls
    private void Dash() {

        if (isDashing) {
            dashTimer = dashTimer + Time.deltaTime;
        }
        else {
            dashTimer = 0;
        }

        if (dashTimer < dashTime) {
            if (Input.GetButtonDown("Dash")) {
                isDashing = true;
                rigid2D.velocity = new Vector2(rigid2D.velocity.x + (direction * dashVelocity), rigid2D.velocity.y);
            }
            else if (Input.GetButtonUp("Dash")) {
                isDashing = false;
            }
        } else {
            isDashing = false;
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

    // Air dash controlls
    private void AirDash() {
        
        if (!isOnGround) {

            if (Input.GetButton("Dash") && isAirDashing == false) {
                rigid2D.velocity = new Vector2(rigid2D.velocity.x + airDashVelocity, 0);
                isAirDashing     = true;
            }
        }
        else {
            isAirDashing = false;
        } 
    }
}
