using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement")]
    public float walkSpeed = 6;
    public float sprintSpeed = 10;
    float moveSpeed;
    public float groundDrag = 8;

    // set gravity in project settings to -30
    [Header("Jumping")]
    public float jumpForce = 16;
    public float jumpCooldown = 0.2f;
    public float airDrag = 0.4f;
    bool canJump = true;

    [Header("Ground Check")]
    public float extraRayGroundedCheck = 0.2f;
    public LayerMask groundLayer;
    float playerHeight;
    bool isGrounded;

    [Header("Crouching")]
    public float crouchSpeed = 2;
    public float crouchYScale = 0.5f;
    float startYScale;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 40;
    RaycastHit slopeHit;
    bool exitingSlope;

    [Header("References")]
    public Transform orientation;

    float horiInput;
    float vertInput;
    Vector3 moveDir;
    Rigidbody rb;
    Collider coll;

    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        InAir,
        Crouching
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        coll = GetComponentInChildren<Collider>();
        playerHeight = coll.transform.localScale.y * 2;
        canJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();

        GetInput();
        MovePlayer();
        SpeedControl();
        StateHandler();
    }

    void GroundedCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + extraRayGroundedCheck, groundLayer);

        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void GetInput()
    {
        horiInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canJump && isGrounded)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    void StateHandler()
    {
        // sprinting
        if (isGrounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        // walking
        else if (isGrounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
        // in air
        else
        {
            state = MovementState.InAir;
        }

        // crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
        }
    }

    void MovePlayer()
    {
        moveDir = orientation.forward * vertInput + orientation.right * horiInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            // apply downward force to prevent bumping movement while player goes up a slope
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80, ForceMode.Force);
        }

        // on ground
        if (isGrounded)
            rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);
        // in air
        else
            rb.AddForce(moveDir.normalized * moveSpeed * 10 * airDrag, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // limiting speed on ground or in air if needed
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        canJump = true;

        exitingSlope = false;
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + extraRayGroundedCheck))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }
}
