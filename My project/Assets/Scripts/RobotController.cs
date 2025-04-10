using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float hAcceleration = .5f; // Acceleration on movement key press
    public float hFriction = 2f;
    public float maxSpeed = 5f;
    public float totalWeight = 0f;
    public float maxSwayAngle = 25f;
    public float maxSwayImpulse = 30f;
    public float swaySpeedRatio = .7f;
    // private bool swayImpulse = true;
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.5f;

    [Header("Body Settings")]
    public Transform bodySprite;
    public Vector2 bodyColliderSize = new Vector2(0.8f, 1.2f);

    [Header("Crouch Settings")]
    public float crouchAmount = 0.8f;
    public float crouchSpeed = 5f;
    public float crouchColliderReduction = 0.6f; // How much to reduce collider height when crouching

    private Rigidbody2D rb;
    private CircleCollider2D wheelCollider;
    private BoxCollider2D bodyCollider;
    private bool isGrounded;
    private float horizontalInput;

    private bool isCrouching = false;
    private Vector3 originalBodyPosition;
    private Vector2 originalBodyColliderSize;
    private Vector2 originalBodyColliderOffset;

    [Header("Attachment Points")]
    public Transform rightAttachPoint;
    public Transform leftAttachPoint;
    public Transform topAttachPoint;
    public float attachRange = 1f;
    public LayerMask itemLayer;

    [Header("Attached Packages (Per Side)")]
    private List<GameObject> rightPackages = new List<GameObject>();
    private List<GameObject> leftPackages = new List<GameObject>();
    private List<GameObject> topPackages = new List<GameObject>();

    private bool canToggleAttach = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wheelCollider = GetComponent<CircleCollider2D>();
        //swayImpulse = true;

        if (wheelCollider == null)
            Debug.LogError("No CircleCollider2D found on the robot!");

        if (bodySprite == null)
        {
            bodySprite = transform.Find("BodySprite");
            if (bodySprite == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<SpriteRenderer>() != null && child.name != "WheelSprite")
                    {
                        bodySprite = child;
                        break;
                    }
                }
            }
        }

        if (bodySprite != null)
        {
            originalBodyPosition = bodySprite.localPosition;
            SetupBodyCollider();
        }
        else
        {
            Debug.LogWarning("Body sprite not found!");
        }

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void SetupBodyCollider()
    {
        bodyCollider = bodySprite.GetComponent<BoxCollider2D>();

        if (bodyCollider == null)
        {
            bodyCollider = bodySprite.gameObject.AddComponent<BoxCollider2D>();
            bodyCollider.size = bodyColliderSize;
            bodyCollider.offset = Vector2.zero;
        }

        originalBodyColliderSize = bodyCollider.size;
        originalBodyColliderOffset = bodyCollider.offset;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // debug message
        if (Input.GetKeyDown(KeyCode.P)) {Debug.Log("curent input direction: " + horizontalInput + ". current speed: " + rb.velocity.x + ". current rotation: " + rb.rotation);}

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            OneSoundEffects robot = GetComponent<OneSoundEffects>();
            robot.PlayJumpAudio();

            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = true;
        }
        
        // Handle crouching
        HandleCrouch();

        if (canToggleAttach)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ToggleAttachment(rightPackages, rightAttachPoint);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ToggleAttachment(leftPackages, leftAttachPoint);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ToggleAttachment(topPackages, topAttachPoint);
            }
        }
        
        
       
        
    }

    void FixedUpdate()
    {
        PlayerMovementH();
        
        
        PlayerSway();
        

        CheckGrounded();

        if (bodySprite != null)
        {
            bodySprite.rotation = Quaternion.identity;
        }
    }

    void CheckGrounded()
    {
        if (wheelCollider == null) return;

        Vector2 circleBottom = (Vector2)transform.position - new Vector2(0, wheelCollider.radius);

        RaycastHit2D hit = Physics2D.Raycast(circleBottom, Vector2.down, groundCheckDistance);
        RaycastHit2D hitLeft = Physics2D.Raycast(circleBottom - new Vector2(wheelCollider.radius * 0.5f, 0), Vector2.down, groundCheckDistance);
        RaycastHit2D hitRight = Physics2D.Raycast(circleBottom + new Vector2(wheelCollider.radius * 0.5f, 0), Vector2.down, groundCheckDistance);

        isGrounded = (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.gameObject != bodySprite.gameObject) ||
                     (hitLeft.collider != null && hitLeft.collider.gameObject != gameObject && hitLeft.collider.gameObject != bodySprite.gameObject) ||
                     (hitRight.collider != null && hitRight.collider.gameObject != gameObject && hitRight.collider.gameObject != bodySprite.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        OneSoundEffects robot = GetComponent<OneSoundEffects>();
        robot.PlayBumpAudio();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 contact = collision.GetContact(i).point;
            Vector2 center = transform.position;

            if (contact.y < center.y - wheelCollider.radius * 0.8f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void HandleCrouch()
    {
        if (bodySprite != null)
        {
            if (Input.GetKey(KeyCode.S))
            {
                OneSoundEffects robot = GetComponent<OneSoundEffects>();
                if (!isCrouching) {
                    robot.PlayCrouchAudio();
                }
                
                isCrouching = true;
                Vector3 targetPos = new Vector3(originalBodyPosition.x, originalBodyPosition.y - crouchAmount, originalBodyPosition.z);
                bodySprite.localPosition = Vector3.Lerp(bodySprite.localPosition, targetPos, Time.deltaTime * crouchSpeed);

                if (bodyCollider != null)
                {
                    Vector2 crouchSize = originalBodyColliderSize;
                    crouchSize.y *= crouchColliderReduction;
                    bodyCollider.size = crouchSize;

                    Vector2 crouchOffset = originalBodyColliderOffset;
                    crouchOffset.y -= (originalBodyColliderSize.y - crouchSize.y) / 2;
                    bodyCollider.offset = crouchOffset;
                }
            }
            else if (isCrouching)
            {
                OneSoundEffects robot = GetComponent<OneSoundEffects>();
                robot.PlayUncrouchAudio();
                
                bodySprite.localPosition = Vector3.Lerp(bodySprite.localPosition, originalBodyPosition, Time.deltaTime * crouchSpeed);

                if (bodyCollider != null)
                {
                    bodyCollider.size = Vector2.Lerp(bodyCollider.size, originalBodyColliderSize, Time.deltaTime * crouchSpeed);
                    bodyCollider.offset = Vector2.Lerp(bodyCollider.offset, originalBodyColliderOffset, Time.deltaTime * crouchSpeed);
                }

                if (Vector3.Distance(bodySprite.localPosition, originalBodyPosition) < 0.01f)
                {
                    isCrouching = false;
                    bodySprite.localPosition = originalBodyPosition;

                    if (bodyCollider != null)
                    {
                        bodyCollider.size = originalBodyColliderSize;
                        bodyCollider.offset = originalBodyColliderOffset;
                    }
                }
            }
        }
    }

    void ToggleAttachment(List<GameObject> packageList, Transform attachPoint)
    {
        Collider2D item = Physics2D.OverlapCircle(attachPoint.position, attachRange, itemLayer);

        if (item != null && !packageList.Contains(item.gameObject))
        {
            AttachItem(item.gameObject, attachPoint, packageList);
        }
        else if (packageList.Count > 0)
        {
            DetachLastItem(packageList);
        }

        StartCoroutine(AttachCooldown());
    }

    void AttachItem(GameObject item, Transform attachPoint, List<GameObject> packageList)
    {
        item.transform.SetParent(attachPoint);
        item.transform.localPosition = new Vector3(0, packageList.Count * 0.5f, 0);

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        packageList.Add(item);
        Debug.Log($"Item attached to {attachPoint.name} | Total: {packageList.Count}");
    }

    void DetachLastItem(List<GameObject> packageList)
    {
        GameObject last = packageList.Last();
        packageList.RemoveAt(packageList.Count - 1);

        last.transform.SetParent(null);

        Rigidbody2D rb = last.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        Debug.Log($"Item detached | Remaining: {packageList.Count}");
    }

    IEnumerator AttachCooldown()
    {
        canToggleAttach = false;
        yield return new WaitForSeconds(0.2f);
        canToggleAttach = true;
    }

    void PlayerMovementH()
    {
        float h_velocity = 0;
        int momentumDir = Math.Sign(rb.velocity.x);
        float h_speed = Math.Abs(rb.velocity.x);
        if (horizontalInput != 0)
        {
            LoopSoundEffects robot = GetComponent<LoopSoundEffects>();
            robot.PlayMoveAudio();

            h_velocity = (hAcceleration / (1 + totalWeight));
            if (h_speed + h_velocity >= maxSpeed / (1 + totalWeight)){
               h_velocity = 0;
               rb.velocity = new Vector2(maxSpeed / (1 + totalWeight) * horizontalInput, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x + (h_velocity * horizontalInput), rb.velocity.y);
            }
        }
        else
        {
            LoopSoundEffects robot = GetComponent<LoopSoundEffects>();
            robot.StopAudio();

            h_velocity = momentumDir * hFriction * (1 + totalWeight);
            if (h_speed - (momentumDir * h_velocity) > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x - h_velocity, rb.velocity.y);
            }
            else
            {
                h_velocity =  0;
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }



    void PlayerSway(){
        float playerRotation = rb.rotation;
        
        if (horizontalInput != 0){
            // Commented section is for possible "bounce" in player sway at initial movement
            //if(swayImpulse){}
            
            // Math.Log((Math.Abs(rb.velocity.x) , 2)

            // player sway on movement press
            if (Math.Abs(rb.rotation) <= maxSwayAngle) 
            {
                playerRotation += (Math.Abs(rb.velocity.x)) * (swaySpeedRatio) * horizontalInput;
                if (playerRotation <= maxSwayAngle){
                    rb.rotation = playerRotation;
                }
                else
                {
                    playerRotation = horizontalInput * maxSwayAngle;
                    rb.rotation = playerRotation;
                } 
            }
            
        }
        else if (playerRotation != 0) 
        {
            playerRotation += -Math.Sign(playerRotation) * ((float) Math.Pow(2, Math.Abs(playerRotation) / 16) - 1) * swaySpeedRatio;
            rb.rotation = playerRotation;
        }

    }
}
