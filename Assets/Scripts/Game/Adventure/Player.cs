using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    private GameLogic gameLogic;

    [Header("Movement")]
    [SerializeField] private float _speed = 5f;

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

    [Header("Joystick")]
    [SerializeField] private GameObject joystick;
    private OnScreenStick stickControler;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;
    public ADV_MapManager mapManagerClass;

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

    //for direction
    private MoveDirection currentDirection;
    private MoveDirection currentDirectionLast;

    private void Awake()
    {
        //get Player components
        animator = GetComponent<Animator>();
        rigidbodyPlayer = GetComponent<Rigidbody2D>();
        colliderPlayer = GetComponent<BoxCollider2D>();

        stickControler = joystick.GetComponent<OnScreenStick>();
    }

    private void Start()
    {
        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
    }

    private void FixedUpdate()
    {
        //move only if not dialgue
        if (ADV_DialogueManager.Instance.dialogueState == ADV_DialogueManager.DialogueState.End || gameLogic.gameState == GameLogic.GameState.Play)
        {
            rigidbodyPlayer.linearVelocity = moveInputPlayer.normalized * _speed;
            stickControler.enabled = true;
        }

        if (ADV_DialogueManager.Instance.dialogueState == ADV_DialogueManager.DialogueState.Start || gameLogic.gameState == GameLogic.GameState.Pause)
        {
            rigidbodyPlayer.linearVelocity = Vector3.zero;
            stickControler.enabled = false;
        }
            
    }

    // Called automatically by PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        //move only if not diallog
        if (ADV_DialogueManager.Instance.dialogueState == ADV_DialogueManager.DialogueState.Start || gameLogic.gameState == GameLogic.GameState.Pause)
        {
            currentDirection = MoveDirection.None;
            PlayerAnimation(currentDirection);
            return;
        }

        //get movement            
        moveInputPlayer = context.ReadValue<Vector2>();

        //get direction from input
        ResolveDirection(moveInputPlayer);

        //animate player only if moving
        if (currentDirection != MoveDirection.None)
            PlayerAnimation(currentDirection);
        else
            animator.SetBool("moving", false);
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
            ADV_DialogueManager.Instance.EnterDialogueMode(inkText);
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

    //run player annimation
    private void PlayerAnimation(MoveDirection direction)
    {
        animator.SetFloat("moveX", moveInputPlayer.x);
        animator.SetFloat("moveY", moveInputPlayer.y);

        animator.SetBool("moving", direction != MoveDirection.None);
    }

    //for diallog - ENTER
    private void OnTriggerEnter2D(Collider2D collider)
    {        
        //set current collider to use
        currentCollider = collider;

        //try to get dialogue from object
        inkText = tilesUtilsClass.GetDialogueFromCollider(collider);

        if(inkText != null)
        {
            readyForDialogue = true;

            InteractIconRoutine(true);
        }                    
    }

    //for diallog - EXIT
    private void OnTriggerExit2D(Collider2D collider)
    {
        currentCollider = null;

        readyForDialogue = false;
        
        inkText = null;

        InteractIconRoutine(false);
    }

    //collision for vfx, collision etc
    private void OnCollisionEnter2D(Collision2D collision)
    {
        readyForInteract = true;

        Debug.Log($"IN: {collision.collider.name}");

        //set current collision for use
        currentCollision = collision;

        //check collision with exit IMPORTNAT
        bool isExit = mapManagerClass.ExitCheck(collision);

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
        Debug.Log($"OUT: {collision.collider.name}");
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

