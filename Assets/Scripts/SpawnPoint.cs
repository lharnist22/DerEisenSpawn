using UnityEngine;

[ExecuteAlways]
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Randomize inside this radius around the point.")]
    public float radius = 2f;

    [Tooltip("Relative chance this spawner is picked (1 = normal).")]
    public float weight = 1f;

    [Tooltip("Optional: limit max alive from this point (0 = no limit).")]
    public int maxAliveFromThisPoint = 0;

    [HideInInspector] public int aliveHere = 0; // managed by the spawner

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.color = new Color(0f, 0.7f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.01f, radius));
    }
#endif
}
