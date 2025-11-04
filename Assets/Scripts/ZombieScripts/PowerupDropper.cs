using UnityEngine;
using UnityEngine.AI;

public class PowerupDropper : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public PowerupPickup pickupPrefab;
        public float weight = 1f;
    }

    [Range(0f, 1f)] public float dropChance = 0.25f;
    public Entry[] table;

    [Header("Spawn tuning")]
    public Transform dropOrigin;           
    public float hoverYOffset = 0.3f;      
    public LayerMask groundMask = ~0;      
    public float rayStartHeight = 2.0f;    
    public float rayMaxDistance = 6.0f;    

    public float navmeshSampleRadius = 1.5f; 

    public void TryDrop()
    {
        if (table == null || table.Length == 0) { Debug.Log("[Dropper] No table entries."); return; }
        float roll = Random.value;
        if (roll > dropChance) { Debug.Log($"[Dropper] Rolled {roll:F2} > dropChance {dropChance:F2} → no drop"); return; }

        // Weighted pick
        float total = 0f; foreach (var e in table) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) { Debug.Log("[Dropper] Total weight <= 0."); return; }

        float r = Random.value * total;
        PowerupPickup chosen = null;
        foreach (var e in table)
        {
            r -= Mathf.Max(0f, e.weight);
            if (r <= 0f) { chosen = e.pickupPrefab; break; }
        }
        if (!chosen) chosen = table[table.Length - 1].pickupPrefab;

        Vector3 pos = ComputeDropPosition();
        var go = Instantiate(chosen, pos, Quaternion.identity);
        Debug.Log($"[Dropper] SPAWNED {chosen.name} at {pos}");
    }

    Vector3 ComputeDropPosition()
    {
        if (dropOrigin)
        {
            if (TryProjectToGround(dropOrigin.position, out Vector3 g1))
                return g1 + Vector3.up * hoverYOffset;
            return dropOrigin.position + Vector3.up * hoverYOffset;
        }

        if (TryGetRenderCenter(out Vector3 center))
        {
            if (TryProjectToGround(center, out Vector3 g2))
                return g2 + Vector3.up * hoverYOffset;
            return center + Vector3.up * hoverYOffset;
        }

        Vector3 p = transform.position;
        if (TryProjectToGround(p, out Vector3 g3))
            return g3 + Vector3.up * hoverYOffset;

        return p + Vector3.up * hoverYOffset;
    }

    bool TryGetRenderCenter(out Vector3 center)
    {
        center = Vector3.zero;
        var rends = GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return false;

        Bounds b = new Bounds(rends[0].bounds.center, Vector3.zero);
        for (int i = 0; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);
        center = b.center;
        return true;
    }

    bool TryProjectToGround(Vector3 src, out Vector3 hitPoint)
    {
        Vector3 origin = src + Vector3.up * rayStartHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayMaxDistance + rayStartHeight, groundMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            return true;
        }

        if (NavMesh.SamplePosition(src, out NavMeshHit nHit, navmeshSampleRadius, NavMesh.AllAreas))
        {
            hitPoint = nHit.position;
            return true;
        }

        hitPoint = default;
        return false;
    }

    public void TryDropAt(Vector3 worldPos)
    {
        // 1) Sanity / chance gate
        if (table == null || table.Length == 0)
        {
            Debug.Log("[Dropper] No table entries.");
            return;
        }

        float roll = Random.value;
        if (roll > dropChance)
        {
            Debug.Log($"[Dropper] Rolled {roll:F2} > dropChance {dropChance:F2} → no drop");
            return;
        }

        float totalWeight = 0f;
        foreach (var e in table)
            totalWeight += Mathf.Max(0f, e.weight);

        if (totalWeight <= 0f)
        {
            Debug.Log("[Dropper] Total weight <= 0.");
            return;
        }

        float rand = Random.value * totalWeight;
        PowerupPickup chosen = null;
        foreach (var e in table)
        {
            rand -= Mathf.Max(0f, e.weight);
            if (rand <= 0f)
            {
                chosen = e.pickupPrefab;
                break;
            }
        }
        if (!chosen)
            chosen = table[table.Length - 1].pickupPrefab;

        Vector3 inputPos = worldPos;
        Vector3 spawnPos = inputPos;

        if (TryProjectToGround(spawnPos, out var ground))
            spawnPos = ground;

        spawnPos += Vector3.up * hoverYOffset;

        Debug.Log($"[Dropper] InputPos={inputPos} FinalPos={spawnPos}");
        Instantiate(chosen, spawnPos, Quaternion.identity);
    }
}
