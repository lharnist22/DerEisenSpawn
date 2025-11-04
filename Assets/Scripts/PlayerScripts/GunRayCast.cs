using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GunRayCast : MonoBehaviour
{

    // In GunRayCast.cs
    public void SetMuzzle(Transform t) { muzzle = t; }

    [Header("Refs")]
    public Transform cam;                       
    public Transform muzzle;
    public WeaponData weaponData;               
    public LayerMask hitMask = ~0;

    [Header("Shooting")]
    public float range = 100f;
    public float fireCooldown = 0.2f;           // Firerate pretty much

    [Header("Ammo")]
    public int magazineSize = 8;                
    public int reserveAmmo = 80;                
    public int currentInMag = 0;                
    public bool autoReloadOnEmpty = true;

    [Header("Reload")]
    public float reloadTime = 1.5f;             // seconds
    bool isReloading = false;

    [Header("VFX")]
    public GameObject bulletHitEffect;
    public float hitEffectTime = 2f;

    LineRenderer line;
    float nextFireTime;

    void Start()
    {
        if (!cam && Camera.main) cam = Camera.main.transform;

        if (weaponData) magazineSize = Mathf.Max(1, weaponData.magazineSize);

        int load = Mathf.Min(magazineSize, reserveAmmo > 0 ? magazineSize : magazineSize);
        currentInMag = magazineSize;

        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
        line.enabled = false;

        AmmoUI.NotifyChange(currentInMag, magazineSize, reserveAmmo);
    }

    void Update()
    {
        bool wantsFire =
            weaponData && weaponData.fireMode == FireMode.Auto
            ? Input.GetButton("Fire1")            // hold for full-auto
            : Input.GetButtonDown("Fire1");       // click for semi

        if (wantsFire) TryFire();
        if (Input.GetKeyDown(KeyCode.R)) TryReload();
    }


    void TryFire()
    {
        if (isReloading) return;
        if (Time.time < nextFireTime) return;

        // Dry-fire if empty
        if (currentInMag <= 0)
        {
            if (autoReloadOnEmpty) TryReload();
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        currentInMag--;
        AmmoUI.NotifyChange(currentInMag, magazineSize, reserveAmmo);

        ShootRay();

        if (currentInMag == 0 && autoReloadOnEmpty)
            TryReload();
    }

    void ShootRay()
    {
        if (!cam) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Collide))
        {
            ShowLine(hit.point);

            int baseDmg = Mathf.RoundToInt(weaponData ? weaponData.bodyDamage : 10f);
            float hsMult = weaponData ? weaponData.headshotMultiplier : 2f;

                var life = hit.collider.GetComponentInParent<ZombieLife>();
                if (life) life.ApplyDamage(baseDmg);

            if (bulletHitEffect)
            {
                var fx = Instantiate(bulletHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, hitEffectTime);
            }
        }
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentInMag >= magazineSize) return;     // already full
        if (reserveAmmo <= 0) return;                 

        StartCoroutine(ReloadCo());
    }

    System.Collections.IEnumerator ReloadCo()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - currentInMag;
        int toLoad = Mathf.Min(needed, reserveAmmo);

        currentInMag += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
        AmmoUI.NotifyChange(currentInMag, magazineSize, reserveAmmo);
    }

    public void AddAmmo(int amount)
    {
        reserveAmmo = Mathf.Max(0, reserveAmmo + amount);
        AmmoUI.NotifyChange(currentInMag, magazineSize, reserveAmmo);
    }

    void ShowLine(Vector3 endPoint)
    {
        line.enabled = true;
        line.SetPosition(0, muzzle ? muzzle.position : cam.position);
        line.SetPosition(1, endPoint);
        StartCoroutine(HideLineNextFrame());
    }
    System.Collections.IEnumerator HideLineNextFrame()
    {
        yield return null;
        line.enabled = false;
    }
}
