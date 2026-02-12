using SuperTiled2Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.DebugUI;

public class Player : MonoBehaviour
{
    private GameLogic gameLogic;

    [Header("Movement")]
    [SerializeField] private float _speed = 5f;

    private Rigidbody2D rigidbodyPlayer;
    private BoxCollider2D colliderPlayer;
    private Vector2 moveInputPlayer;
    private Animator animator;

    private bool readyForInteract = false;

    [Header("Interaction")]
    private Tilemap currentTilemap;
    private Vector3Int currentTilePosition;

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
        colliderPlayer = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
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

        //Debug.Log($"Input: {moveInputPlayer} | Direction: {currentDirection}");
    }

    public void OnInterract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Interact();

        if (context.started)
            Debug.Log("Pressed");

        if (context.canceled)
            Debug.Log("Released");
    }

    public void Interact()
    {
        if (!readyForInteract)
            return;

        TryInteract();
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        readyForInteract = true;       

        currentTilemap = collision.collider.GetComponentInParent<Tilemap>();
        if (currentTilemap == null)
            return;

        // Use contact point directly
        ContactPoint2D contact = collision.GetContact(0);

        Vector3Int cellPosition = currentTilemap.WorldToCell(contact.point);

        //Debug.Log($"Contact point: {contact.point}");
        //Debug.Log($"Cell: {cellPosition}");        

        //GetCustomTileProperties(cellPosition);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        readyForInteract = false;

    }

    public (string name, string type) GetCustomTileProperties(Vector3Int tilePos)
    {
        if (currentTilemap == null)
            return (null, null);

        TileBase tile = currentTilemap.GetTile(tilePos);
        if (tile == null)
            return (null, null);

        SuperTile superTile = tile as SuperTile;
        if (superTile == null || superTile.m_CustomProperties == null)
            return (null, null);

        string name = null;
        string type = null;

        foreach (var prop in superTile.m_CustomProperties)
        {
            if (prop.m_Name == "Name")
                name = prop.GetValueAsString();

            if (prop.m_Name == "Type")
                type = prop.GetValueAsString();
        }

        return (name, type);
    }


    public void TryInteract()
    {
        Vector3Int playerCell = currentTilemap.WorldToCell(transform.position);

        Vector3Int offset = Vector3Int.zero;

        switch (currentDirection)
        {
            case MoveDirection.Up:
                offset = Vector3Int.zero;
                break;
            case MoveDirection.Down:
                offset = Vector3Int.down;
                break;
            case MoveDirection.Left:
                offset = Vector3Int.left;
                break;
            case MoveDirection.Right:
                offset = Vector3Int.right;
                break;
        }

        Vector3Int targetCell = playerCell + offset;

        Debug.Log($"offset: {offset}");
        Debug.Log($"PlayerCell: {playerCell}");
        Debug.Log($"TargetCell: {targetCell}");

        var (tileName, tileType) = GetCustomTileProperties(targetCell);

        if (tileName != null)
        {
            //Debug.Log(tileName);  // stump
            //Debug.Log(tileType);  // inanimate

            gameLogic.StartInteraction(tileName, tileType);
        }
    }

}

