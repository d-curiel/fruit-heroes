using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Movement2D : MonoBehaviour
{

    [Header("Components")]
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private LayerMask _movingPlatformLayer;

    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration;
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _groundLinearDrag;
    [SerializeField] private bool _canMove = true;
    private float _horizontalDirection;
    [SerializeField]
    private bool _lastDirection;
    private bool _changinDirection => (_rigidbody2D.velocity.x > 0f && _horizontalDirection < 0f) || (_rigidbody2D.velocity.x < 0f && _horizontalDirection > 0f);

    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private bool _jumpButtonPressed = false;
    [SerializeField] private int _extraJumpsValue = 1;
    private bool _canJump => _jumpButtonPressed && (_extraJumpsValue > 0);

    [Header("Wall Movement Variables")]
    [SerializeField] private float _wallSlideModifier = 0.5f;
    private bool _wallSlide => _onWall && !_onGround && _rigidbody2D.velocity.y < 0f;

    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;
    private bool _onMovingPlatform;


    [Header("Wall Collision Variables")]
    [SerializeField] private float _wallRaycastLength;
    [SerializeField] private float _wallExitRaycastLength;
    [SerializeField] private bool _onWall;
    [SerializeField] private bool _onRightWall;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {

        CheckCollisions();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _extraJumpsValue = _extraJumps;
        } else
        {
            FallMultiplier();
            ApplyAirLinearDrag();
        }
        if (_canJump) Jump();
        CheckFlip();
        Animate();
        if (_wallSlide)
        {
            WallSlide();
        }
        if (_onWall){
            StickToWall();
            _extraJumpsValue = _extraJumps;
        }
    }

    private void MoveCharacter()
    {
        _rigidbody2D.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rigidbody2D.velocity.x) > _maxMoveSpeed)
            _rigidbody2D.velocity = new Vector2(Mathf.Sign(_rigidbody2D.velocity.x) * _maxMoveSpeed, _rigidbody2D.velocity.y);
    }

    /**
     * Esta función detecta si nuestro personaje está en movimiento y aplica
     * un drag para que no parezca que se desliza.
     */
    private void ApplyGroundLinearDrag()
    {
        if(Mathf.Abs(_horizontalDirection) < 0.4f || _changinDirection)
        {
            _rigidbody2D.drag = _groundLinearDrag;
        } else
        {
            _rigidbody2D.drag = 0f;
        }
    }
    
    /**
     * Esta función aplica una fuerza hacia abajo para que el personaje
     * en salto no flote.
     */
    private void ApplyAirLinearDrag()
    {
         _rigidbody2D.drag = _airLinearDrag;
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        _horizontalDirection = context.ReadValue<Vector2>().x;


    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        _jumpButtonPressed = context.ReadValueAsButton();
    }


    private void Jump()
    {
        if (!_onGround && !_onWall)
        {
            _extraJumpsValue--;
        }
        _jumpButtonPressed = false;
        ApplyAirLinearDrag();
        _rigidbody2D.velocity = new Vector2((_onWall ? _onRightWall ? -1f : 1f : _horizontalDirection) , 0f);
        _rigidbody2D.AddForce((_onWall ? _onRightWall ? new Vector2(-1, 1) : new Vector2(1, 1) : Vector2.up) * _jumpForce, ForceMode2D.Impulse);

    }

    private void FallMultiplier()
    {
        if(_rigidbody2D.velocity.y < 0)
        {
            _rigidbody2D.gravityScale = _fallMultiplier;
        } else if (_rigidbody2D.velocity.y > 0)
        {
            _rigidbody2D.gravityScale = _lowJumpFallMultiplier;
        } else
        {
            _rigidbody2D.gravityScale = 1f;
        }
    }

    private void WallSlide()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, -_maxMoveSpeed * _wallSlideModifier);
    }

    private void StickToWall()
    {
        //Empuja al personaje contra la pared

        if(_onRightWall && _horizontalDirection >= 0 && !_onGround && !_canJump)
        {
            _rigidbody2D.velocity = new Vector2(1, _rigidbody2D.velocity.y);

        } else if (!_onRightWall  && _horizontalDirection <= 0 && !_onGround && !_canJump)
        {
            _rigidbody2D.velocity = new Vector2(-1, _rigidbody2D.velocity.y);
        }

    }
    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                                Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        _onMovingPlatform = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _movingPlatformLayer) ||
                                Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _movingPlatformLayer);
        _onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer) ||
             Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer);
    }

    private void Animate()
    {
        _spriteRenderer.flipX = _lastDirection;
    }

    private void CheckFlip()
    {
        if (_onWall)
        {

            _lastDirection = !_onRightWall;
        } else
        {
            if(_horizontalDirection != 0)
            {
                _lastDirection = _horizontalDirection < 0f;
            }
        }
    }

    private void OnDrawGizmos()

    {
        Gizmos.color = Color.green;

        //Ground Check
        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);

        //Wall Check
        Gizmos.DrawLine(transform.position, transform.position +  Vector3.right * _wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position +  Vector3.left * _wallRaycastLength);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((_movingPlatformLayer.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            gameObject.transform.SetParent(collision.gameObject.transform, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((_movingPlatformLayer.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            gameObject.transform.parent = null;
        }
    }

    private void OnDisable()
    {
        _rigidbody2D.velocity = Vector2.zero;
    }

}
