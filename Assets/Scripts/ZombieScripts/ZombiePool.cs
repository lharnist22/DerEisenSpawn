using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePool : MonoBehaviour
{
    public PooledZombie prefab;
    public int initialSize = 10;

    readonly Queue<PooledZombie> q = new Queue<PooledZombie>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var z = Instantiate(prefab, transform);
            var agent = z.GetComponentInChildren<NavMeshAgent>(true);

            if (agent)
            {
                agent.enabled = false;
            }

            z.gameObject.SetActive(false);
            z.originPool = this;
            q.Enqueue(z);
        }
    }


    public PooledZombie Get()
    {
        PooledZombie z;
        if (q.Count > 0)
        {
            z = q.Dequeue();
        }

        else
        {
            z = Instantiate(prefab, transform);
            var agent = z.GetComponentInChildren<NavMeshAgent>(true);
            
            if (agent)
            {
                agent.enabled = false;
            }
        }

        z.originPool = this;
        z.gameObject.SetActive(true); // safe: agent is still disabled
        return z;
    }

    public void Return(PooledZombie z)
    {
        var meta = z.GetComponentInChildren<ZombieMeta>(true);

        if (meta && meta.spawnPoint != null)
        {
            meta.spawnPoint.aliveHere--;
            meta.spawnPoint = null;
        }

        ZombieManager.Instance?.UnregisterAlive(z);

        if (meta) 
        {
            meta.countedAlive = false;
        }

        if (meta && meta.countedAlive)
        {
            meta.countedAlive = false;                    // prevent double-decrement
            ZombieManager.Instance?.NotifyZombieDied();   // wave progress
        }

        var agent = z.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(true);
        
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        z.gameObject.SetActive(false);
        q.Enqueue(z);
    }
}
