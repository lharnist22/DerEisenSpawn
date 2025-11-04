using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PooledZombie : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseSpeed = 2f;
    public Transform target;   //This is player so that the Zombies actually follow the player

    [HideInInspector] public ZombiePool originPool;

    float speed;
    Renderer rend;

    void Awake() 
    { 
        rend = GetComponentInChildren<Renderer>(); 
    }

    public void OnSpawned()
    {
        speed = baseSpeed * Random.Range(0.8f, 1.6f);             
        float s = Random.Range(0.85f, 1.25f);                     
        transform.localScale = new Vector3(s, s, s);
        if (rend) 
        {
            rend.material.color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.5f, 1f); // This is so that each zombie has a random color (random green color)
        } 
    }

    void Update()
    {
        if (!target)
        {
            Debug.Log("Player does not exist");
            return;
        }
        Vector3 dir = target.position - transform.position; 
        dir.y = 0f;
        
        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            transform.position += dir * speed * Time.deltaTime;
            transform.forward = Vector3.Lerp(transform.forward, dir, 0.2f);
        }
    }


    public void Despawn()
    {
        if (originPool)
        {
            originPool.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
