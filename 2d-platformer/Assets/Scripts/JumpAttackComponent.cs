using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpAttackComponent : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;

    [Header("Layers")]
    [SerializeField]
    private LayerMask _targetLayers;

    [Header("Raycast Config")]
    [SerializeField]
    private float _headRaycastLength;
    [SerializeField]
    private Vector3 _headRaycastOffset;
    [SerializeField]
    private float _feetRaycastLength;
    [SerializeField]
    private Vector3 _feetRaycastOffset;


    private bool _isJumping;
    private bool _isStatic;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        // Verificar si el objeto está saltando o cayendo
        _isJumping = CheckIsJumping();
        _isStatic = CheckIsStatic();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Comprobar que contra lo que hemos chocado, nos interesa, usando una LayerMask
        //Comprobar si el objetivo puede recibir un ataque en salto
        JumpAttackReceiverComponent jumpAttackReceiver = collision.gameObject.GetComponent<JumpAttackReceiverComponent>();
        if (IsInTargetLayers(collision.gameObject.layer) && jumpAttackReceiver != null)
        {
            Debug.Log("IsStatic: " + _isStatic);
            if (!_isStatic)
            {
                bool isHit;
                if (_isJumping)
                {

                    //Comprobar si lo estamos tocando con la "cabeza" con un Raycast
                    isHit = Physics2D.Raycast(transform.position + _headRaycastOffset, Vector2.up, _headRaycastLength, _targetLayers) ||
                                            Physics2D.Raycast(transform.position - _headRaycastOffset, Vector2.up, _headRaycastLength, _targetLayers);
                } else
                {
                    //Comprobar si lo estamos tocando con los "pies" con un Raycast
                    isHit = Physics2D.Raycast(transform.position + _feetRaycastOffset, Vector2.down, _feetRaycastLength, _targetLayers) ||
                                            Physics2D.Raycast(transform.position - _feetRaycastOffset, Vector2.down, _feetRaycastLength, _targetLayers);

                }
                if (isHit)
                {
                    jumpAttackReceiver.ReceiveJumpAttack();
                }
            }
        }
    }

    private bool CheckIsJumping()
    {
        return _rb.velocity.y > 0.1f;
    }

    private bool CheckIsStatic()
    {
        return _rb.velocity.y == 0;
    }

    private bool IsInTargetLayers(int objectLayer)
    {
        // Comprobar si el layer del objeto está en la LayerMask
        return _targetLayers == (_targetLayers | (1 << objectLayer));
    }

    private void OnDrawGizmos()

    {
        Gizmos.color = Color.red;

        //Feet Check
        Gizmos.DrawLine(transform.position + _feetRaycastOffset, transform.position + _feetRaycastOffset + Vector3.down * _feetRaycastLength);
        Gizmos.DrawLine(transform.position - _feetRaycastOffset, transform.position - _feetRaycastOffset + Vector3.down * _feetRaycastLength);

        //Head Check
        Gizmos.DrawLine(transform.position + _headRaycastOffset, transform.position + _headRaycastOffset + Vector3.up * _headRaycastLength);
        Gizmos.DrawLine(transform.position - _headRaycastOffset, transform.position - _headRaycastOffset + Vector3.up * _headRaycastLength);
    }
}
