using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private int maxJumps = 2;

    [Header("Swimming")]
    [SerializeField] private float swimGravity = 0.3f;
    [SerializeField] private float swimUpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 0.3f;
    private bool canShoot = true;

    [Header("Slow Effect")]
    [SerializeField] private float slowedSpeedMultiplier = 0.3f;
    [SerializeField] private float slowDuration = 1f;

    private float originalSpeed;
    private bool isSlowed;




    [Header("Hit")]
    [SerializeField] private float hitCooldown = 0.5f;
    private bool canBeHit = true;

    private float horizontal;
    private bool isFacingRight = true;
    private bool isSwimming;

    private int jumpCount;
    private Rigidbody2D rb;
    private float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        originalSpeed = speed;
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        bool grounded = IsGrounded();

        // Reset jumps only when grounded & falling
        if (!isSwimming && grounded && rb.linearVelocity.y <= 0f)
        {
            jumpCount = 0;
        }

        // ATTACK
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
            Shoot();
        }

        // JUMP / SWIM UP
        if (Input.GetButtonDown("Jump"))
        {
            if (isSwimming)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, swimUpForce);
            }
            else if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
                animator.SetTrigger("Jump");
            }
        }

        // Short hop
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f && !isSwimming)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Animator Bools
        animator.SetBool("isWalking", horizontal != 0 && grounded && !isSwimming);
        animator.SetBool("isSwimming", isSwimming);
        animator.SetBool("isGrounded", grounded);

        Flip();
    }

    void FixedUpdate()
    {
        float currentSpeed = isSlowed ? speed : originalSpeed;
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
    }

    // WATER
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isSwimming = true;
            rb.gravityScale = swimGravity;
            jumpCount = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isSwimming = false;
            rb.gravityScale = defaultGravity;
        }
    }

    //  HIT OBSTACLES


    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;

        if (layer == LayerMask.NameToLayer("Obstacles"))
        {
            Debug.Log("Applying slow");
            //TakeDamage(1);
            ApplySlow();
        }

        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            //TakeDamage(1);
        }
    }



    private void Shoot()
    {
        canShoot = false;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        bullet.GetComponent<Bullet>().SetDirection(dir);

        Invoke(nameof(ResetShoot), shootCooldown);
    }


    private void ApplySlow()
    {
        if (isSlowed) return;

        isSlowed = true;
        speed = originalSpeed * slowedSpeedMultiplier;

        Debug.Log("SLOWED! Speed is now: " + speed);

        Invoke(nameof(ResetSpeed), slowDuration);
    }


    private void ResetSpeed()
    {
        speed = originalSpeed;
        isSlowed = false;
        Debug.Log("Speed reset to: " + speed);
    }

    private void ResetShoot()
    {
        canShoot = true;
    }


    private void ResetHit()
    {
        canBeHit = true;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            0.1f,
            groundLayer
        );
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) ||
            (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }
}
