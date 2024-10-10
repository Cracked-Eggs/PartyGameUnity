using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement and Speed Settings")]
    public float walkSpeed = 8f;

    public float sprintSpeed = 14f;
    public float maxVelocityChange = 10f;

    [Header("Air & Jumping Controls")] [Range(0,1f)]public float airControl = 0.5f;
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    [Space] public float groundCheckDistance = 0.75f;

    #region Private Variables

    private Vector2 input;

    private Rigidbody rb;

    private bool sprinting;
    private bool jumping;

    private bool grounded;

    private Vector3 lastTargetVelocity;

    #endregion


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (InputManager.LockInput)
        {
            input = new Vector2(0, 0);
            sprinting = false;
            jumping = false;
            
            return;
        }
        
        //Gather input
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        sprinting = Input.GetKey(KeyCode.LeftShift);
        jumping = Input.GetKey(KeyCode.Space);
    }


    private void OnCollisionStay(Collision other)
    {
        grounded = true;
    }

    private void FixedUpdate()
    {
        if (grounded)
        {
            // Jump, have full movement etc
            if (jumping)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            else
            {
                ApplyMovement(sprinting ? sprintSpeed : walkSpeed, false);
            }
        }
        else
        {
            // Fall to the ground, and have limited air control

            if (input.magnitude > 0.5f)
            {
                // Air control
                ApplyMovement(sprinting ? sprintSpeed : walkSpeed, true);
            }
        }

        grounded = false;
    }

    private void ApplyMovement(float _speed, bool _inAir)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity) * _speed;

        if (_inAir)
            targetVelocity += lastTargetVelocity * (1 - airControl);

        Vector3 velocityChange = targetVelocity - rb.velocity;

        if (_inAir)
        {
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange * airControl,
                maxVelocityChange * airControl);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange * airControl,
                maxVelocityChange * airControl);
        }
        else
        {
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange,
                maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange,
                maxVelocityChange);
        }

        velocityChange.y = 0;
        
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
