using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static ADV_InteractionBase;

public class Player : MonoBehaviour
{
    private GameLogic gameLogic;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private PathFinder pathFinder;

    private float currentSpeed;
    private List<Vector2> _path;
    private int _pathIndex;
    private bool hasTarget;

    // Stuck detection: if player doesn't move for this long, cancel the path.
    private const float StuckTimeout     = 0.4f;
    private const float StuckDistSq      = 0.01f; // 0.1 units — less than one frame at walkSpeed
    private const float StuckGracePeriod = 2f;    // no stuck checks for 2s after scene load
    private float       _stuckTimer;
    private Vector2     _stuckCheckPos;
    private float       _stuckGraceEnd;

    [Header("Marker")]
    [SerializeField] private CenterPanelClick centerPanelClick;

    //player staff
    private Rigidbody2D rigidbodyPlayer;
    private BoxCollider2D colliderPlayer;
    private Vector2 moveInputPlayer;
    private Animator animator;

    public bool readyForInteract = false;
    public bool readyForDialogue = false;

    //important objects
    private Collision2D currentCollision; //for interract
    private Collider2D currentCollider; //for dialog

    [Header("Interaction")]
    private Tilemap currentTilemap;
    [SerializeField] private GameObject interractIcon;
    private Vector3Int currentTilePosition;

    [Header("Dialog")]
    private TextAsset inkText;

    [Header("Fight")]
    public Button attackButton;
    public PolygonCollider2D[] attackColliders;
    [SerializeField] private float kickForce = 5;

    [Header("Light")]
    [SerializeField] private Transform playerLight;

    [Header("Joystick")]
    //[SerializeField] private GameObject joystick;
    //private OnScreenStick stickControler;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;
    public ADV_MapManager mapManagerClass;
    public ADV_InteractionManager interactionManagerClass;

    //for icon fade
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

    public enum PlayerState
    {
        Idle,
        Walk,
        Attack,
        Interract,
        Talk
    }

    public enum PlayerLocation
    {
        Map,
        Room
    }



    //for direction
    private MoveDirection currentDirection;
    private MoveDirection currentDirectionLast;

    public PlayerState currentPlayerState;
    public PlayerLocation currentPlayerLocation;

    private void Awake()
    {
        //get Player components
        animator = GetComponent<Animator>();
        rigidbodyPlayer = GetComponent<Rigidbody2D>();
        colliderPlayer = GetComponent<BoxCollider2D>();

        //stickControler = joystick.GetComponent<OnScreenStick>();
    }

    private void Start()
    {
        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();

        currentPlayerLocation = PlayerLocation.Map;

        //set initial speed
        currentSpeed = walkSpeed;

        // Suppress stuck detection briefly so physics can settle after spawn.
        _stuckGraceEnd = Time.time + StuckGracePeriod;
        _stuckCheckPos = rigidbodyPlayer.position;
    }

    private void FixedUpdate()
    {
        //get current dialogue state and game state
        var dialogueState = ADV_DialogueManager.Instance.dialogueState;

        //get current game state
        var gameState = gameLogic.gameState;

        // Possible = dialogue available but not started — player can still move freely.
        bool canMove = (dialogueState == ADV_DialogueManager.DialogueState.End ||
            dialogueState == ADV_DialogueManager.DialogueState.None ||
            dialogueState == ADV_DialogueManager.DialogueState.Possible) &&
                gameState == GameLogic.GameState.Play;

        //stop movement if dialogue or pause
        if (!canMove)
        {
            rigidbodyPlayer.linearVelocity = Vector2.zero;

            currentDirection = MoveDirection.None;
            animator.SetBool("moving", false);

            return;
        }

        //move player to target position if set
        if (!hasTarget || _path == null || _pathIndex >= _path.Count)
        {
            rigidbodyPlayer.linearVelocity = Vector2.zero;
            hasTarget = false;
            return;
        }

        //move player to target position
        Vector2 waypoint = _path[_pathIndex];
        Vector2 dir = waypoint - rigidbodyPlayer.position;

        //advance to next waypoint when close enough
        if (dir.magnitude < 0.15f)
        {
            _pathIndex++;

            if (_pathIndex >= _path.Count)
            {
                // reached final waypoint
                rigidbodyPlayer.linearVelocity = Vector2.zero;
                currentDirection = MoveDirection.None;
                animator.SetBool("moving", false);
                hasTarget = false;
                centerPanelClick.HideMarker();
            }

            return;
        }

        ResolveDirection(dir);

        currentPlayerState = PlayerState.Walk;

        PlayerAnimation(dir);

        rigidbodyPlayer.linearVelocity = dir.normalized * currentSpeed;

        // Stuck detection: cancel path if position barely changed for StuckTimeout seconds.
        // Skip during grace period after scene load so physics can settle.
        if (Time.time < _stuckGraceEnd)
        {
            _stuckCheckPos = rigidbodyPlayer.position;
            _stuckTimer    = 0f;
        }
        else if ((rigidbodyPlayer.position - _stuckCheckPos).sqrMagnitude > StuckDistSq)
        {
            _stuckCheckPos = rigidbodyPlayer.position;
            _stuckTimer    = 0f;
        }
        else
        {
            _stuckTimer += Time.fixedDeltaTime;
            if (_stuckTimer >= StuckTimeout)
            {
                hasTarget = false;
                _stuckTimer = 0f;
                rigidbodyPlayer.linearVelocity = Vector2.zero;
                animator.SetBool("moving", false);
                centerPanelClick.HideMarker();
            }
        }
    }

    /// <summary>Immediately stops movement and clears the current path.</summary>
    public void CancelPath()
    {
        hasTarget   = false;
        _path       = null;
        _stuckTimer = 0f;
        rigidbodyPlayer.linearVelocity = Vector2.zero;
        animator.SetBool("moving", false);
        centerPanelClick.HideMarker();
    }

    /// <summary>
    /// Sets a movement target. Returns false if the position is inside an obstacle or unreachable.
    /// </summary>
    public bool SetTarget(Vector2 worldPos, bool run = false)
    {
        if (pathFinder == null)
        {
            // Fallback: direct move without pathfinding.
            _path = new List<Vector2> { worldPos };
            _pathIndex = 0;
            hasTarget = true;
            currentSpeed = run ? walkSpeed * runMultiplier : walkSpeed;
            return true;
        }

        List<Vector2> path = pathFinder.FindPath(rigidbodyPlayer.position, worldPos);

        if (path == null || path.Count == 0)
            return false;

        _path          = path;
        _pathIndex     = 0;
        hasTarget      = true;
        _stuckTimer    = 0f;
        _stuckCheckPos = rigidbodyPlayer.position;
        currentSpeed   = run ? walkSpeed * runMultiplier : walkSpeed;
        return true;
    }

    // Called automatically by PlayerInput
/*    public void OnMove(InputAction.CallbackContext context)
    {
        //move only if not diallog
        if (ADV_DialogueManager.Instance.dialogueState == ADV_DialogueManager.DialogueState.Start || gameLogic.gameState == GameLogic.GameState.Pause)
        {
            currentDirection = MoveDirection.None;
            //PlayerAnimation(currentDirection);            
            return;
        }

        //get movement            
        moveInputPlayer = context.ReadValue<Vector2>();

        //get direction from input
        ResolveDirection(moveInputPlayer);

        //animate player only if moving
        if (currentDirection != MoveDirection.None)
        {
            //PlayerAnimation(currentDirection);
            currentPlayerState = PlayerState.Walk;
        }
        else
        {
            animator.SetBool("moving", false);
            currentPlayerState = PlayerState.Idle;
        }
            
    }*/

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            StartAttack();

        if (context.canceled)
            StopAttack();
    }

    public void StartAttack()
    {
        var dialogueState = ADV_DialogueManager.Instance.dialogueState;
        
        //noanimate attack if dialogue possible or end
        if (dialogueState != ADV_DialogueManager.DialogueState.None)
            return;

        animator.SetBool("attacking", true);        
        currentPlayerState = PlayerState.Attack;        
    }

    public void StopAttack()
    {
        currentPlayerState = PlayerState.Idle;
        animator.SetBool("attacking", false);
    }


    //for interraction with dialogue
    public void OnInterract(InputAction.CallbackContext context)
    {        
        if (!context.performed)
            return;        

        //for dialogues system
        ADV_DialogueManager.Instance.OnCommunicate();
    }


    public void TryToDialogue()
    {
        if (!readyForDialogue)
            return;

        if (inkText != null)
        {
            ADV_DialogueManager.Instance.EnterDialogueMode(inkText);
            currentPlayerState = PlayerState.Talk;
        }
            
    }

/*    private bool DialogueStart()
    {
        if (inkText != null)
            gameLogic.DialogueRoutine(inkText.text);

        return true;
    }*/

    //for collision effects and etc
    public void StartInteract()
    {
        //only interact if ready. Collision with collider
        if (!readyForInteract)
            return;

        if (!TryToInteractWithTile())
        {
            TryToInteractWithObject(currentCollision);
        }            
    }

    private void ResolveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
        {
            currentDirection = MoveDirection.None;
            return;
        }

        MoveDirection newDirection;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            newDirection = input.x > 0
                ? MoveDirection.Right
                : MoveDirection.Left;
        }
        else
        {
            newDirection = input.y > 0
                ? MoveDirection.Up
                : MoveDirection.Down;
        }

        // update only if changed
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            currentDirectionLast = currentDirection;

            UpdateLightDirection();
        }
    }

    private void UpdateLightDirection()
    {
        float angle = currentDirection switch
        {
            MoveDirection.Up => 0f,
            MoveDirection.Down => 180f,
            MoveDirection.Left => 90f,
            MoveDirection.Right => -90f,
            _ => 180f
        };

        playerLight.localRotation = Quaternion.Euler(0, 0, angle);
    }

    //run player annimation
    /*    private void PlayerAnimation(MoveDirection direction)
        {
            animator.SetFloat("moveX", moveInputPlayer.x);
            animator.SetFloat("moveY", moveInputPlayer.y);

            animator.SetBool("moving", direction != MoveDirection.None);
        }*/

    private void PlayerAnimation(Vector2 dir)
    {
        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);

        animator.SetBool("moving", true);
    }

    //for diallog - ENTER
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log($"IN Trigger: {collider.name}");

        //set current collider to use
        currentCollider = collider;

        //try to get dialogue from object
        inkText = tilesUtilsClass.GetDialogueFromCollider(collider);

        //run diallogue if found
        if (inkText != null && currentPlayerState != PlayerState.Attack)
        {
            readyForDialogue = true;

            //dialogue possible
            ADV_DialogueManager.Instance.dialogueState = ADV_DialogueManager.DialogueState.Possible;

            InteractIconRoutine(true);

            // Player reached the NPC trigger zone — stop moving so we don't bump into the solid body.
            CancelPath();
        }

        InteractionType interactionType = interactionManagerClass.GetInteraction(collider);

        if (interactionType == InteractionType.Collectible)
            interactionManagerClass.RunInteraction(collider);
    }

    //only if contact wit hitbox
    public void HandleAttackHit(Collider2D collider)
    {
        InteractionType interactionType = interactionManagerClass.GetInteraction(collider);

        if (collider != null && currentPlayerState == PlayerState.Attack)
        {
            switch (interactionType)
            {
                case InteractionType.Destructible:
                case InteractionType.Enemy:
                    interactionManagerClass.RunInteraction(collider);
                    break;
            }
        }
    }

    //for diallog - EXIT
    private void OnTriggerExit2D(Collider2D collider)
    {
        //Debug.Log($"OUT Trigger: {collider.name}");

        currentCollider = null;

        readyForDialogue = false;
        
        inkText = null;

        InteractIconRoutine(false);

        //no diallogue
        ADV_DialogueManager.Instance.dialogueState = ADV_DialogueManager.DialogueState.None;
    }

    //collision for vfx, collision etc
    private void OnCollisionEnter2D(Collision2D collision)
    {
        readyForInteract = true;

        //Debug.Log($"IN Collision: {collision.collider.name}");

        //set current collision for use
        currentCollision = collision;

        //check collision with exit IMPORTNAT
        bool isExit = mapManagerClass.ExitCheck(collision);

        bool isRoom = mapManagerClass.RoomCheck(collision);

        // Cancel path on level/room transition — old waypoints are in the previous level's space.
        if (isExit || isRoom)
            CancelPath();

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
        currentCollision = null;
        //Debug.Log($"OUT Collision: {collision.collider.name}");
    }

    private bool InteractIconRoutine(bool value)
    {
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
        Vector3Int playerCell = Vector3Int.zero;

        try
        {
            //get player cell position
            playerCell = currentTilemap.WorldToCell(transform.position);
        }catch
        {            
            return false;
        }

        //set initial offset to zero
        Vector3Int offset = Vector3Int.zero;

        //directions
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

        if (customProperty != null && customProperty[0] != "")
        {
            gameLogic.StartInteraction(customProperty);

            return true;
        }

        return false;
    }


    public bool TryToInteractWithObject(Collision2D collision)
    {
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        if (customProperty != null )
        {
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

