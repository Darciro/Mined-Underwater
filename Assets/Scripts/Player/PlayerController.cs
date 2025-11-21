using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float leftBoundPadding;
    [SerializeField] private float rightBoundPadding;
    [SerializeField] private float upBoundPadding;
    [SerializeField] private float downBoundPadding;

    private ProjectileComponent playerProjectile;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 moveDirection;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    private Rigidbody2D rb;
    private Animator animator;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int MoveY = Animator.StringToHash("MoveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        playerProjectile = GetComponent<ProjectileComponent>();

        SetupBounds();
    }

    private void Update()
    {
        ReadInput();
        FireProjectile();
    }

    private void FixedUpdate()
    {
        Move();
        // MovePlayer();
    }

    private void Move()
    {
        rb.MovePosition(rb.position + moveDirection * (maxSpeed * Time.fixedDeltaTime));
    }

    private void ReadInput()
    {
        moveDirection = moveAction.ReadValue<Vector2>().normalized;
        // if (moveDirection == Vector2.zero) return;

        animator.SetFloat(moveX, moveDirection.x);
        animator.SetFloat(MoveY, moveDirection.y);
    }

    private void SetupBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
    }

    /* private void MovePlayer()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            rb.AddForce(input * acceleration, ForceMode2D.Force);
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            rb.AddForce(rb.linearVelocity * -deceleration, ForceMode2D.Force);
        }

        // Clamp position within bounds
        Vector3 clampedPos = rb.position;
        clampedPos.x = Math.Clamp(clampedPos.x, minBounds.x + leftBoundPadding, maxBounds.x - rightBoundPadding);
        clampedPos.y = Math.Clamp(clampedPos.y, minBounds.y + downBoundPadding, maxBounds.y - upBoundPadding);
        rb.position = clampedPos;
    } */

    private void FireProjectile()
    {
        playerProjectile.isFiring = fireAction.IsPressed();
    }
}
