using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieNavChase : MonoBehaviour
{
    public Transform player;
    [Header("Repathing")]
    public float repathInterval = 0.25f, repathIfPlayerMoved = 0.75f, forceRepathEvery = 1.5f;
    [Header("Target Sampling")]
    public float targetSampleRadius = 1.5f, maxTargetSampleRadius = 8f, attackRange = 1.5f;
    public bool verboseLogs = false;

    NavMeshAgent agent;
    bool agentConfigured;                      
    Vector3 lastPlayerWorld = Vector3.positiveInfinity, lastGoodDest;
    float nextCheck, nextForceRepath, nextResolve;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        lastGoodDest = transform.position;
        nextForceRepath = Time.time + forceRepathEvery;
        TryResolvePlayer(initial: true);
    }

    void Update()
    {
        ConfigureAgentOnce();                  // <-- safe-guard; no-op until agent is ready
        if (!agentConfigured) return;

        agent.stoppingDistance = attackRange * 0.9f;

        if (Time.time >= nextResolve) { nextResolve = Time.time + 0.5f; TryResolvePlayer(); }

        bool timeToCheck = Time.time >= nextCheck;
        bool playerMoved = lastPlayerWorld.Equals(Vector3.positiveInfinity) ||
                           (player.position - lastPlayerWorld).sqrMagnitude >= repathIfPlayerMoved * repathIfPlayerMoved;
        bool forceTick = Time.time >= nextForceRepath;

        if (timeToCheck || playerMoved || forceTick)
        {
            nextCheck = Time.time + repathInterval;
            nextForceRepath = Time.time + forceRepathEvery;

            if (!TrySampleNearPlayer(out var target))
            {
                if (NavMesh.Raycast(transform.position, player.position, out var edge, NavMesh.AllAreas))
                {
                    SetDest(edge.position, "[Chase] Using edge");
                    return;
                }
                UseLastGood("[Chase] Sample failed; using lastGood");
                return;
            }

            var path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                SetDest(target, "[Chase] Set path to sampled");
                lastPlayerWorld = player.position;
            }
            else if (path.status == NavMeshPathStatus.PathPartial)
            {
                var corners = path.corners;
                if (corners != null && corners.Length >= 2)
                {
                    SetDest(corners[corners.Length - 1], "[Chase] Partial; last corner");
                    lastPlayerWorld = player.position;
                }
                else if (NavMesh.Raycast(transform.position, target, out var cut, NavMesh.AllAreas))
                {
                    SetDest(cut.position, "[Chase] Partial; cut pos");
                    lastPlayerWorld = player.position;
                }
                else
                {
                    UseLastGood("[Chase] Partial; using lastGood");
                }
            }
            else
            {
                UseLastGood($"[Chase] {path.status}; using lastGood");
            }
        }

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
            agent.isStopped = true;
        else if (agent.isStopped)
            agent.isStopped = false;
    }

    void ConfigureAgentOnce()
    {
        if (agentConfigured) return;
        if (!agent) return;
        if (!agent.enabled) return;
        if (!agent.isOnNavMesh) return;

        agent.autoRepath = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;              
        agentConfigured = true;
    }

    bool TrySampleNearPlayer(out Vector3 sampled)
    {
        sampled = default;
        if (!player) return false;

        float r = Mathf.Max(0.1f, targetSampleRadius);
        while (true)
        {
            if (NavMesh.SamplePosition(player.position, out var hit, r, NavMesh.AllAreas))
            {
                sampled = hit.position;
                return true;
            }
            r *= 1.7f;
            if (r > maxTargetSampleRadius) break;
        }
        return false;
    }

    void SetDest(Vector3 target, string log)
    {
        agent.isStopped = false;
        agent.SetDestination(target);
        lastGoodDest = target;
    }

    void UseLastGood(string log)
    {
        if (lastGoodDest == Vector3.zero) return;
        agent.isStopped = false;
        agent.SetDestination(lastGoodDest);
    }

    void TryResolvePlayer(bool initial = false)
    {
        if (IsRealPlayer(player)) return;
        var p = FindFirstObjectByType<PlayerScript>();   // Unity 2023+
        if (p) player = p.transform;
    }

    static bool IsRealPlayer(Transform t) => t && t.GetComponent<PlayerScript>() != null;
}
