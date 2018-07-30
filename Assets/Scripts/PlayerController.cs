using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float Fr;
    private float g;
    private float m;
    private float y;

    private float moveInput = 0f;
    private bool facingRight = true;
    public float horizontalVelocityMax = 10f;
    private float horizontalVelocity;
    public float horizontalAcceleration = 0.5f;
    public float horizontalFriction = 2f;
    public float horizontalFrictionStopping = 0.2f;
    public float horizontalFrictionTurning = 0.2f;
    private float timeAcceleration = 0;
    private float timeFriction = 0;
    
    private bool onGround = false;
    public Transform groundCheck;
    private float groundCheckRadius = 0.2f;
    public LayerMask ground;
    private float onGroundRemember;
    public float onGroundRememberTime = 0.2f;

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

        Move();
    }

    void Update () {
        Jump();
    }

    // Checks if the player is on ground
    private bool IsOnGround() {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);
    }

    // Movement controlls
    private void Move() {
        /*
        if (Mathf.Abs(moveInput) < 0.01f) {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingStopping, Time.deltaTime * horizontalAcceleration);
        } else if (Mathf.Sign(moveInput) != Mathf.Sign(horizontalVelocity)) {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingTurning, Time.deltaTime * horizontalAcceleration);
        } else {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDamping, Time.deltaTime * horizontalAcceleration);
        }        
        */

        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Acceleration
        if (moveInput != 0) {
            timeAcceleration = timeAcceleration + Time.deltaTime;
            horizontalVelocity = ((horizontalAcceleration * (timeAcceleration * moveInput)) + rigid2D.velocity.x);        
        } else {
            timeAcceleration = 0;
        }

        // Speedcap
        if (moveInput * horizontalVelocity > horizontalVelocityMax) {
            horizontalVelocity = moveInput * horizontalVelocityMax;
        }

        // Movement decreases slowly over time due to friction if
        // directional buttons are released
        if (moveInput == 0 && horizontalVelocity != 0 ) {

            timeFriction = timeFriction + Time.deltaTime;

            if (facingRight) {
                if (horizontalVelocity <= 0) {
                    horizontalVelocity = 0;
                    timeFriction = 0;
                } else {
                    horizontalVelocity = -(horizontalFriction * timeFriction) + rigid2D.velocity.x;
                }
            } else {
                if (horizontalVelocity >= 0) {
                    horizontalVelocity = 0;
                    timeFriction = 0;
                } else {
                    horizontalVelocity = (horizontalFriction * timeFriction) + rigid2D.velocity.x;
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

    // Flips the sprite according to the direction the player is facing
    private void Flip() {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
