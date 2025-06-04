using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenControl : MonoBehaviour
{
    // Author: Glenn Storm
    // Chicken!

    public float moveSpeed = 3.81f;
    public float jumpForce = 32f; // nice with rb gravity scale 12
    public float runMult = 2f;
    public AnimSprite animSprite;
    public float animRateMultiplier = 1f;

    private bool facingLeft;
    private Rigidbody2D rb;
    private bool grounded;
    private bool doubleJumpReady;
    private bool doubleJumped;
    private float savedAnimRate;

    const float GROUNDEDDISTANCE = 0.1f;
    const float DOUBLEJUMPVELOCITYTHRESHOLD = 0.27f; // proportion of jump force
    const float VELOCITYLIMIT = 38.1f;


    void Start()
    {
        // validate
        rb = gameObject.GetComponent<Rigidbody2D>();
        if ( rb == null )
        {
            Debug.LogError("ChickenControl [Start] : no rigidbody 2d found. aborting.");
            enabled = false;
        }
        if (animSprite == null)
            Debug.LogWarning("ChickenControl [Start] : no Anim Sprite tool found. will ignore.");
        else
            savedAnimRate = animSprite.frameInterval;
        // initialize
    }

    void Update()
    {
        // determine grounded
        RaycastHit2D[] results = new RaycastHit2D[5]; // l,r,u,d and self
        rb.Cast(Vector2.down, results, GROUNDEDDISTANCE);
        grounded = false;
        for (int i = 0; i < results.Length; i++)
        {
            if (!results[i] || results[i].collider.gameObject == gameObject)
                continue; // ignore either self or no result
            if ( Vector2.Angle(results[i].normal, Vector2.up) < 45f )
            {
                grounded = true;
                break;
            }
        }

        // movement
        bool tryJump = false;
        if (Input.GetKeyDown(KeyCode.Space))
            tryJump = true;
        Vector2 moveInput = Vector2.zero; // store player input
        if ( Input.GetKey(KeyCode.A) )
        {
            moveInput.x -= 1f;
            facingLeft = true;
        }
        if ( Input.GetKey(KeyCode.D) )
        {
            moveInput.x += 1f;
            facingLeft = false;
        }
        if ( Input.GetKey(KeyCode.W) )
        {
            moveInput.y += 1f;
        }
        if ( Input.GetKey(KeyCode.S) )
        {
            moveInput.y -= 1f;
        }
        // handle movement TODO: handle air control
        if ( grounded && moveInput != Vector2.zero )
        {
            bool running = ( Input.GetKey( KeyCode.LeftShift ) || 
                                Input.GetKey( KeyCode.RightShift ) );
            Vector2 moveForce = ( moveInput * moveSpeed );
            if (running)
                moveForce *= runMult;
            rb.AddForce(moveForce * moveSpeed);
        }
        // determine double jump ready
        doubleJumpReady = (Mathf.Abs(rb.linearVelocity.y) < jumpForce * DOUBLEJUMPVELOCITYTHRESHOLD);
        if (doubleJumped)
            doubleJumpReady = false; // allow only one double jump per jump
        // handle jump
        if ((grounded || doubleJumpReady) && tryJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            doubleJumped = (!grounded && doubleJumpReady); // track double jumped or reset for new jump
        }
        // handle h flip
        Vector3 scale = Vector3.one;
        if (facingLeft)
            scale.x *= -1f;
        gameObject.transform.localScale = scale;

        // limit velocity
        // NOTE: using AddForce has the potential to "charge up" a lot of force
        if (rb.linearVelocity.magnitude > VELOCITYLIMIT)
            rb.linearVelocity = (rb.linearVelocity.normalized * VELOCITYLIMIT);

        // handle anim sprite tool
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;

        float moveRate = ( vel.magnitude / moveSpeed );
        moveRate *= animRateMultiplier;
        animSprite.SetFrameInterval(savedAnimRate * Mathf.Min((1f / moveRate), 999f));
    }
}
