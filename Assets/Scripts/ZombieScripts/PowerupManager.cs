// PowerupManager.cs
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    public bool InstaKillActive => Time.time < _instaKillUntil;
    float _instaKillUntil;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ActivateInstaKill(float duration)
    {
        _instaKillUntil = Mathf.Max(_instaKillUntil, Time.time + duration);
    }

    public void DoMaxAmmo()
    {
        var guns = FindAll<GunRayCast>();
        foreach (var g in guns)
        {
            if (!g || !g.weaponData) continue;

            g.reserveAmmo = Mathf.Max(g.reserveAmmo, g.weaponData.startingReserve);

            int need = g.magazineSize - g.currentInMag;
            if (need > 0 && g.reserveAmmo > 0)
            {
                int load = Mathf.Min(need, g.reserveAmmo);
                g.currentInMag += load;
                g.reserveAmmo -= load;
            }
            AmmoUI.NotifyChange(g.currentInMag, g.magazineSize, g.reserveAmmo);
        }
    }

    public void DoNuke()
    {
        var enemies = FindAll<ZombieLife>();
        foreach (var e in enemies)
            if (e) e.DieByNuke();
    }
    static T[] FindAll<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        // New API (fast; no sort; include inactive)
        return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        // Legacy API (includes inactive when true)
        return Object.FindObjectsOfType<T>(true);
#endif
    }
}
