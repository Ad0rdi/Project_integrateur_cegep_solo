/* Original author name: Donavan Sirois
 * Creation date: 2025/02/27
 * Goal: Manage the player controller. This solution contains coyote time, jump buffering, general movement and fast falling.
 * Modification listing:
 * 2025/04/29:
 *      Author Name: Adam
 *      Goal: Added multiplayer to movement
 * 2025/05/02:
 *      Author Name: Donavan Sirois
 *      Goal: Modified ground check range to fit with the new model and added the flip character
 *  2025/05/09:
 *      Author Name: Adam Turcotte
 *      Goal: Add controllers of gun and grapple. Both handle their own actions.
 */

using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : NetworkBehaviour
{
    //player inputs
    private float _horizontal;
    private bool _jumpPressed;
    private bool _jumpReleased;
    private bool _fastFall;
    private bool _shoot;
    private bool _grappling;

    //player stats
    private float _speed = 8f;
    private float _jumpPower = 16f;

    //physics component
    private Rigidbody2D _rigidBody;
    private EdgeCollider2D _collider;
    private Vector2 _move;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;

    //shooting component
    [SerializeField] private GunController gunController;
    [SerializeField] private GrappleHook grappleController;

    //state variables
    private bool _isGrounded;
    private float _coyoteTime = 0.2f;
    private float _coyoteCounter;
    private float _jumpBufferTime = 0.2f;
    private float _jumpBufferCounter;

    private bool _isFacingRight = true;


    private void Awake() // Seek the following components when the game starts
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<EdgeCollider2D>();
    }

    private void Update()
    {
        if (Managers.TimeManager.isPaused) return;
        GatherInput(); // Check the inputs from the player on every frame
        CheckStates();
        HandleJump();
        HandleShooting();
        Flip();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void GatherInput()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _jumpPressed = Input.GetButtonDown("Jump");
        _jumpReleased = Input.GetButtonUp("Jump");
        _fastFall = Input.GetButtonDown("Vertical");
        _shoot = Input.GetMouseButtonDown(0);
        _grappling = Input.GetMouseButtonDown(1);
    }

    private void CheckStates()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, 0.4f, _groundLayer);

        if (_isGrounded)
        {
            _coyoteCounter = _coyoteTime;
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
        }

        if (_jumpPressed)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleShooting()
    {
        if (_shoot) gunController.Shoot();
        if (_grappling) grappleController.Shoot();
    }

    private void HandleJump()
    {
        if (_jumpPressed) //Annule le grappling quand tu sautes
            grappleController.Cancel();

        _move.x = _rigidBody.velocity.x;
        if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
        {
            _move.y = _jumpPower;
            _rigidBody.velocity = _move;

            _jumpBufferCounter = 0f;
        }

        if (_jumpReleased && _rigidBody.velocity.y > 0f)
        {
            _move.y = _rigidBody.velocity.y * 0.5f;
            _rigidBody.velocity = _move;

            _coyoteCounter = 0f;
        }

        if (_fastFall && _rigidBody.velocity.y <= 0f)
        {
            _move.y = _rigidBody.velocity.y * 3f;
            _rigidBody.velocity = _move;
        }
    }

    private void ApplyMovement()
    {
        _move.x = _horizontal * _speed;
        _move.y = _rigidBody.velocity.y;
        _rigidBody.velocity = _move;
    }

    private void Flip()
    {
        if (_isFacingRight && _horizontal < 0f || !_isFacingRight && _horizontal > 0f)
        {
            _isFacingRight = !_isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3 direction = transform.position - collision.transform.position;
        _rigidBody.velocity = new Vector2(0, 0);
        if (collision.gameObject.TryGetComponent<Creature>(out Creature creatureComponent))
        {
            _rigidBody.AddForce(direction.normalized * -1000f);
        }
    }
}