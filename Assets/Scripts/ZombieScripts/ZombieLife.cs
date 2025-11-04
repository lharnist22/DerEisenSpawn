using UnityEngine;
using UnityEngine.AI;

public class ZombieLife : MonoBehaviour
{
    [HideInInspector] public PowerupDropper dropper;
    [HideInInspector] public ZombieManager manager;
    [HideInInspector] public PooledZombie pooled;

    [Header("Drop / Death Position")]
    [Tooltip("Marker that follows the rig (pelvis/chest). If null we fall back automatically.")]
    public Transform deathDropOrigin;

    [Header("Health & Points")]
    public int maxHP = 100;
    public int DebugHP => hp;
    public bool awardPointsPerHit = true;
    public int pointsPerHit = 10;
    NavMeshAgent _agent;
    int hp;

    void Awake()
    {
        if (!manager) manager = ZombieManager.Instance;
        if (!pooled) pooled = GetComponentInParent<PooledZombie>();
        if (!dropper) dropper = GetComponentInChildren<PowerupDropper>(true);
        if (!dropper) Debug.LogWarning($"[ZombieLife] No PowerupDropper found on {name}");
        if (!deathDropOrigin) Debug.Log($"[ZombieLife] deathDropOrigin not assigned on {name}; will auto-fallback.");
    }

    void OnEnable() { hp = maxHP; }

    public void ApplyDamage(int dmg)
    {
        if (dmg <= 0) return;

        if (PowerupManager.Instance && PowerupManager.Instance.InstaKillActive)
            dmg = hp;

        if (awardPointsPerHit)
            PointsManager.Instance?.AddPoints(pointsPerHit);

        hp -= dmg;
        if (hp <= 0) Die();
    }

    public void Die()
    {
        manager?.NotifyZombieDied();
        PointsManager.Instance?.AddPoints(50);

        // Safely stop/clear the agent only if it's alive & on a NavMesh
        var agent = _agent != null ? _agent : GetComponent<NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Disable the agent so later pooling/moves don't spam NavMesh errors
        if (agent != null && agent.isActiveAndEnabled)
            agent.enabled = false;

        // Drops
        Vector3 deathPos = GetDeathWorldPosWithDiagnostics();
        if (dropper) dropper.TryDropAt(deathPos);
        else Debug.LogWarning("[ZombieLife] dropper is null, no TryDropAt!");

        // Return to pool or deactivate
        if (pooled && pooled.originPool != null)
        {
            pooled.originPool.Return(pooled);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void DieByNuke() => Die();

Vector3 GetDeathWorldPosWithDiagnostics()
    {
        if (deathDropOrigin)
        {
            Debug.Log($"[DeathPos] Using deathDropOrigin {deathDropOrigin.name} @ {deathDropOrigin.position}");
            return deathDropOrigin.position;
        }

        if (TryGetComponentInChildren<Animator>(out var anim) && anim.isHuman)
        {
            var hips = anim.GetBoneTransform(HumanBodyBones.Hips);
            if (hips)
            {
                Debug.Log($"[DeathPos] Using Animator Hips @ {hips.position}");
                return hips.position;
            }
        }

        if (TryGetComponentInChildren<NavMeshAgent>(out var agent))
        {
            Debug.Log($"[DeathPos] Using NavMeshAgent {agent.gameObject.name} @ {agent.transform.position}");
            return agent.transform.position;
        }

        if (TryGetRenderCenter(out var center))
        {
            Debug.Log($"[DeathPos] Using RenderCenter @ {center}");
            return center;
        }

        Debug.Log($"[DeathPos] Fallback transform @ {transform.position}");
        return transform.position;
    }

    bool TryGetComponentInChildren<T>(out T comp) where T : Component
    {
        comp = GetComponentInChildren<T>(true);
        return comp;
    }

    bool TryGetRenderCenter(out Vector3 center)
    {
        var rends = GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            center = transform.position;
            return false;
        }
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        center = b.center;
        return true;
    }

    public void ConfigureForRound(int round, WeaponData pistol, float hpMultiplier = 1f)
    {
        if (round < 1) round = 1;

        int mag = Mathf.Max(1, pistol ? pistol.magazineSize : 8);
        float dmg = pistol ? pistol.bodyDamage : 10f;

        int shots = 6 + Mathf.Max(0, round - 1) * mag;

        float hpF = shots * dmg * Mathf.Max(0.01f, hpMultiplier);

        maxHP = Mathf.CeilToInt(hpF);
        hp = maxHP;

        Debug.Log($"[ZombieLife] R{round}: shots={shots}, dmg/shot={dmg} → HP={maxHP}");
    }

}
