using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMeta : MonoBehaviour
{
    [HideInInspector] public SpawnPoint spawnPoint;
    [HideInInspector] public bool countedAlive; 
}



public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance { get; private set; }

    public event Action<int> OnRoundChanged;

    readonly HashSet<PooledZombie> alive = new HashSet<PooledZombie>();

    [SerializeField] int totalAlive; // for Inspector debugging

    [Header("Spawn Points")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public float navmeshSampleRadius = 3f;
    public Transform spawnParent;
    public Transform player;

    [Header("Wave Settings")]
    public int currentRound = 0;
    public float secondsBeforeFirstWave = 2f;
    public float secondsBetweenWaves = 5f;      
    public int hardModeExtra = 0;               
    public float hardModeMultiplier = 1.0f;     
    public int waveCap = 80;                    

    [Header("Pool Ref")]
    public ZombiePool pool;

    [Header("Balancing")]
    public WeaponData pistolData;         
    public float hardModeHPScale = 1f;    
    bool hardMode = false;   




    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {

        RaiseRoundChanged(currentRound);

        if (!player)
        {
            var p = FindFirstObjectByType<PlayerScript>();
            if (p) player = p.transform;
        }

        bool hardMode = PlayerPrefs.GetInt("HardMode", 0) == 1;
        if (hardMode)
        {
            hardModeExtra = 6;         
            hardModeMultiplier = 1.15f; 
        }

        StartCoroutine(WaveLoop());
    }

    void RaiseRoundChanged(int round)
    {
        currentRound = round;
        OnRoundChanged?.Invoke(currentRound);
        // Debug.Log($"[Waves] Round set -> {currentRound}");
    }
    //Wave / Round system here with Enum
    System.Collections.IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(secondsBeforeFirstWave);

        while (true)
        {
            // 1) Next round
            RaiseRoundChanged(currentRound + 1);
            OnRoundChanged?.Invoke(currentRound); 


            // 2) Compute count for this round
            int count = ComputeZombiesForRound(currentRound);
            Debug.Log($"[Waves] Round {currentRound} — spawning {count} zombies");

            // 3) Here's a smooth trickle to avoid a spawn hitch.
            yield return StartCoroutine(SpawnRound(count, batchSize: 6, batchDelay: 0.25f));

            // 4) Wait until all zombies are dead
            while (alive.Count > 0) yield return null;

            // 5) Time for downtime
            yield return new WaitForSeconds(secondsBetweenWaves);
        }
    }


    public void RegisterAlive(PooledZombie z)
    {
        if (z == null) return;
        if (alive.Add(z))
            totalAlive = alive.Count;
    }

    public void UnregisterAlive(PooledZombie z)
    {
        if (z == null) return;
        if (alive.Remove(z))
            totalAlive = alive.Count;
    }

    // Formula for spawning zombies based off round number here (I capped it at 80)

    int ComputeZombiesForRound(int r)
    {
        // Base quadratic that hits 80 at r=12
        float baseCount = 0.46103896f * r * r + 0.73376623f * r + 4.8051948f;

        baseCount = baseCount * hardModeMultiplier + hardModeExtra;

        int result = Mathf.RoundToInt(Mathf.Min(baseCount, waveCap));

        return Mathf.Max(result, 1);
    }

    System.Collections.IEnumerator SpawnRound(int total, int batchSize, float batchDelay)
    {
        int remaining = total;
        while (remaining > 0)
        {
            int n = Mathf.Min(batchSize, remaining);
            for (int i = 0; i < n; i++)
                SpawnOne();

            remaining -= n;
            if (remaining > 0) yield return new WaitForSeconds(batchDelay);
        }
    }

    void SpawnOne()
    {
        var sp = PickWeightedSpawnPoint();
        if (sp == null) { Debug.LogWarning("No eligible SpawnPoint found."); return; }

        var zmb = pool.Get();

        var agent = zmb.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(true);
        if (!agent) { Debug.LogError("Pooled zombie missing NavMeshAgent."); pool.Return(zmb); return; }

        agent.enabled = false;

        const int maxTries = 6;
        bool placed = false;
        Vector3 chosen = sp.transform.position;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 desired = RandomPointAround(sp);
            if (UnityEngine.AI.NavMesh.SamplePosition(desired, out var hit, navmeshSampleRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                chosen = hit.position;
                placed = true;
                break;
            }
        }

        if (!placed) { Debug.LogWarning($"SpawnPoint '{sp.name}' had no nearby NavMesh."); pool.Return(zmb); return; }

        zmb.transform.position = chosen;
        agent.enabled = true;
        agent.Warp(chosen);

        var chase = zmb.GetComponentInChildren<ZombieNavChase>(true);
        if (chase) chase.player = player;

        sp.aliveHere++;
        zmb.originPool = pool;

        var meta = zmb.GetComponentInChildren<ZombieMeta>(true);
        if (meta) meta.spawnPoint = sp;
        if (meta) meta.countedAlive = true;  
        totalAlive++;
        totalAlive++;

        var life = zmb.GetComponentInChildren<ZombieLife>(true);
        if (life)
        {
            float hpScale = hardMode ? hardModeHPScale : 1f;
            life.ConfigureForRound(currentRound, pistolData, hpScale);
            life.manager = this; 
        }
        if (life)
        {
            float hpScale = hardMode ? hardModeHPScale : 1f;
            life.manager = this;
            life.ConfigureForRound(currentRound, pistolData, hpScale);
        }


        zmb.OnSpawned();
        RegisterAlive(zmb);

        var test = zmb.GetComponentInChildren<ZombieMeta>(true);
        if (test)
        {
            test.spawnPoint = sp;
            test.countedAlive = true; 
        }


    }

    SpawnPoint PickWeightedSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;

        float total = 0f;
        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            if (sp.maxAliveFromThisPoint > 0 && sp.aliveHere >= sp.maxAliveFromThisPoint) continue;
            total += Mathf.Max(0f, sp.weight);
        }
        if (total <= 0f) return null;

        float r = UnityEngine.Random.value * total;
        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            if (sp.maxAliveFromThisPoint > 0 && sp.aliveHere >= sp.maxAliveFromThisPoint) continue;

            float w = Mathf.Max(0f, sp.weight);
            if (r <= w) return sp;
            r -= w;
        }
        return null;
    }

    Vector3 RandomPointAround(SpawnPoint sp)
    {
        Vector2 circle = UnityEngine.Random.insideUnitCircle * sp.radius;
        Vector3 p = sp.transform.position;
        return new Vector3(p.x + circle.x, p.y, p.z + circle.y);
    }


    public void NotifyZombieDied()
    {
        totalAlive = Mathf.Max(0, totalAlive - 1);
        // Debug.Log($"[Waves] Zombie died. Alive now: {totalAlive}");
    }
}