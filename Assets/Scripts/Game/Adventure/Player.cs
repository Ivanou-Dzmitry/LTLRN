using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;

    private Rigidbody2D rigidbodyPlayer;
    private Vector2 moveInputPlayer;
    private Animator animator;

    public enum MoveDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    private MoveDirection currentDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbodyPlayer = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {

        rigidbodyPlayer.linearVelocity = moveInputPlayer.normalized * _speed;
    }

    // Called automatically by PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInputPlayer = context.ReadValue<Vector2>();

        //get direction from input
        ResolveDirection(moveInputPlayer);

        if(currentDirection != MoveDirection.None)
            PlayerAnimation(currentDirection);
        else
            animator.SetBool("moving", false);

        Debug.Log($"Input: {moveInputPlayer} | Direction: {currentDirection}");
    }

    private void ResolveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
        {
            currentDirection = MoveDirection.None;
            return;
        }

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            currentDirection = input.x > 0
                ? MoveDirection.Right
                : MoveDirection.Left;
        }
        else
        {
            currentDirection = input.y > 0
                ? MoveDirection.Up
                : MoveDirection.Down;
        }
    }

    private void PlayerAnimation(MoveDirection direction)
    {
        animator.SetFloat("moveX", moveInputPlayer.x);
        animator.SetFloat("moveY", moveInputPlayer.y);

        animator.SetBool("moving", direction != MoveDirection.None);

        //animator.SetBool("IsMoving", direction != MoveDirection.None);
    }
}

