using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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
    private Collision2D currentCollision;
    [SerializeField] private GameObject interractIcon;
    private Vector3Int currentTilePosition;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;
    public ADV_MapManager mapManagerClass;

    //for icaon fade
    private Coroutine fadeRoutine;
    private const float ICON_FADE_TIME = .3f;

    public enum MoveDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    private MoveDirection currentDirection;
    private MoveDirection currentDirectionLast;

    private void Awake()
    {
        //get components
        animator = GetComponent<Animator>();
        rigidbodyPlayer = GetComponent<Rigidbody2D>();
        colliderPlayer = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
    }

    private void FixedUpdate()
    {
        //move only if not interacting
        if (gameLogic.interractState == GameLogic.InterractState.End)        
            rigidbodyPlayer.linearVelocity = moveInputPlayer.normalized * _speed;
    }

    // Called automatically by PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        //move only if not interacting
        if (gameLogic.interractState == GameLogic.InterractState.Start)
        {
            currentDirection = MoveDirection.None;
            PlayerAnimation(currentDirection);
            return;
        }
            

        moveInputPlayer = context.ReadValue<Vector2>();

        //get direction from input
        ResolveDirection(moveInputPlayer);

        //animate player only if moving
        if (currentDirection != MoveDirection.None)
            PlayerAnimation(currentDirection);
        else
            animator.SetBool("moving", false);
    }

    public void OnInterract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Interact();
    }

    public void Interact()
    {
        //only interact if ready. Collision with collider
        if (!readyForInteract)
            return;
       
        if(!TryToInteractWithTile())
            TryToInteractWithObject(currentCollision);
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

            currentDirectionLast = currentDirection;
        }
        else
        {
            currentDirection = input.y > 0
                ? MoveDirection.Up
                : MoveDirection.Down;

            currentDirectionLast = currentDirection;
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

        currentCollision = collision;

        mapManagerClass.ExitCheck(collision);

        //get tilemap from collision
        currentTilemap = collision.collider.GetComponentInParent<Tilemap>();

        //only if tilemap found
        if (currentTilemap == null)
            return;

        // Use contact point directly
        ContactPoint2D contact = collision.GetContact(0);

        Vector3Int cellPosition = currentTilemap.WorldToCell(contact.point);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        InteractIconRoutine(false);
    }

    public bool InteractIconRoutine(bool value)
    {
        readyForInteract = value;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        string mode = "";

        if (value)
            mode = "in";
        else
            mode = "out";

        fadeRoutine = StartCoroutine(FadeSprite(interractIcon, mode, ICON_FADE_TIME));

        return true;
    }


    public bool TryToInteractWithTile()
    {
        Vector3Int playerCell;

        try
        {
            //get player cell position
            playerCell = currentTilemap.WorldToCell(transform.position);
        }catch
        {
            Debug.LogWarning("Player cell is not on a tilemap cell.");
            return false;
        }

        //set initial offset to zero
        Vector3Int offset = Vector3Int.zero;

        switch (currentDirectionLast)
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

        //calculate target cell based on player cell and direction
        Vector3Int targetCell = playerCell + offset;

        //try to get custom properties from target cell
        string[] customProperty = tilesUtilsClass.GetCustomTileProperties(currentTilemap, targetCell);

        if (customProperty != null)
        {
            InteractIconRoutine(true);

            gameLogic.StartInteraction(customProperty);

            return true;
        }

        return false;
    }


    public bool TryToInteractWithObject(Collision2D collision)
    {
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        //Debug.Log($"Trying to interact with object...{customProperty[0]}, {customProperty[1]}");

        if (customProperty != null )
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeSprite(interractIcon, "in", ICON_FADE_TIME));

            //send data to game logic
            gameLogic.StartInteraction(customProperty);

            return true;
        }

        return false;
    }


    private IEnumerator FadeSprite(GameObject obj, string direction, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        if (sr == null)
            yield break;

        float startAlpha = sr.color.a;
        float targetAlpha = direction == "in" ? 1f : 0f;

        // Enable before fade-in
        if (direction == "in")
            obj.gameObject.SetActive(true);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            Color c = sr.color;
            c.a = newAlpha;
            sr.color = c;

            yield return null;
        }

        // Ensure exact final value
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;

        // Disable after fade-out
        if (direction == "out")
            obj.gameObject.SetActive(false);
    }

}

