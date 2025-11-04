using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Routing")]
    public ZombieLife life;          
    public bool isHead = false;      

    [Header("Points (optional)")]
    public bool awardPointsOnHit = true;
    public int pointsOnHit = 10;

    void Awake()
    {
        if (!life) life = GetComponentInParent<ZombieLife>();
    }

    public void ApplyHit(int baseDamage, float headshotMult = 2f)
    {
        if (!life) return;

        int dmg = isHead ? Mathf.RoundToInt(baseDamage * headshotMult) : baseDamage;
        life.ApplyDamage(dmg);

        if (awardPointsOnHit)
            PointsManager.Instance?.AddPoints(pointsOnHit);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!life) life = GetComponentInParent<ZombieLife>();
    }
#endif
}
