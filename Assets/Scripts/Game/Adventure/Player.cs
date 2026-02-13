using SuperTiled2Unity;
using System.Collections;
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
    private Collision2D currentCollision;
    [SerializeField] private GameObject interractIcon;
    private Vector3Int currentTilePosition;

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
            return;

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

        //get tilemap from collision
        currentTilemap = collision.collider.GetComponentInParent<Tilemap>();

/*        //try read custom properties from collider
        ReadCustomPropertiesFromObject(collision);*/

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

    public string[] GetCustomTileProperties(Vector3Int tilePos)
    {
        if (currentTilemap == null)
            return null;

        //get current tile
        TileBase tile = currentTilemap.GetTile(tilePos);

        if (tile == null)
            return null;

        //get custom properties from tile
        SuperTile superTile = tile as SuperTile;
        
        if (superTile == null || superTile.m_CustomProperties == null)
            return null;

        //for custom params
        string[] customData = new string[2];        

        foreach (var prop in superTile.m_CustomProperties)
        {
            //get name
            if (prop.m_Name == "Name")
                customData[0] = prop.GetValueAsString();
            //gett type
            if (prop.m_Name == "Type")
                customData[1] = prop.GetValueAsString();
        }

        if (customData[0] == null || customData[1] == null)
            return null;

        return customData;
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
        string[] customProperty = GetCustomTileProperties(targetCell);

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
        string[] customProperty = ReadCustomPropertiesFromObject(collision);

        //Debug.Log($"Trying to interact with object...{customProperty[0]}, {customProperty[1]}");

        if (customProperty != null )
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeSprite(interractIcon, "in", ICON_FADE_TIME));

            gameLogic.StartInteraction(customProperty);

            return true;
        }

        return false;
    }

    private string[] ReadCustomPropertiesFromObject(Collision2D collision)
    {
        try
        {
            SuperCustomProperties customProperties = collision.collider.GetComponentInParent<SuperCustomProperties>();
            if (customProperties != null && customProperties.m_Properties != null)
            {
                // Get specific properties by name
                string nameValue = null;
                string typeValue = null;

                foreach (var prop in customProperties.m_Properties)
                {
                    if (prop.m_Name == "Name")
                    {
                        nameValue = prop.GetValueAsString();
                    }
                    else if (prop.m_Name == "Type")
                    {
                        typeValue = prop.GetValueAsString();
                    }
                }

                if (nameValue == null || typeValue == null)
                    return null;

                return new string[] { nameValue, typeValue };
            }
            return null;
        }
        catch
        {
            Debug.LogWarning("No SuperCustomProperties found on the collided tilemap.");
            return null;
        }
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

