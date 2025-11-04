using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentOnlyMovement : MonoBehaviour
{
    void OnEnable()
    {
        // If a Rigidbody exists, let NavMeshAgent own motion
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;           // no physics translation
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // If an Animator exists, avoid root-motion moving the body
        if (TryGetComponent(out Animator anim))
            anim.applyRootMotion = false;
    }
}
