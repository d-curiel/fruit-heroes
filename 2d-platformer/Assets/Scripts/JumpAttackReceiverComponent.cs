using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class JumpAttackReceiverComponent : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent JumpAttackReceived = new UnityEvent();

    public void ReceiveJumpAttack()
    {
        JumpAttackReceived?.Invoke();
    }
}
