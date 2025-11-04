using System;
using UnityEngine;

public class WeaponEquip : MonoBehaviour
{
    [Header("Refs")]
    public Transform socket;        
    public GunRayCast gun;          

    [Header("Starter (optional)")]
    public GameObject starterViewInScene;
    public WeaponData starterWeaponData;

    [Header("Default View Offsets (fallback)")]
    public Vector3 localOffset = new Vector3(0.20f, -0.25f, 0.80f);
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    [Header("Auto-wiring (optional)")]
    [SerializeField] string socketPathHint = "CameraRoot/WeaponSocket"; 

    GameObject currentView;

    void Awake()
    {
        if (socket == null)
        {
            var t = transform.Find(socketPathHint);
            if (t != null) socket = t;
            else
            {
                foreach (var tr in GetComponentsInChildren<Transform>(true))
                {
                    if (tr.name.Equals("WeaponSocket", StringComparison.OrdinalIgnoreCase))
                    {
                        socket = tr;
                        break;
                    }
                }
            }
        }

        if (gun == null)
        {
            gun = GetComponent<GunRayCast>();
            if (gun == null)
                gun = GetComponentInChildren<GunRayCast>(true);
            if (gun == null)
                gun = GetComponentInParent<GunRayCast>();
        }
    }

    void Start()
    {
        if (socket == null)
        {
            Debug.LogError("[WeaponEquip] Missing socket Transform. Assign it in the inspector or set a valid socketPathHint.");
            enabled = false;
            return;
        }
        if (gun == null)
        {
            Debug.LogError("[WeaponEquip] Missing GunRayCast reference on player. Assign it in the inspector.");
            enabled = false;
            return;
        }

        if (starterViewInScene)
        {
            if (starterViewInScene.scene.IsValid() && starterViewInScene.scene.isLoaded)
            {
                starterViewInScene.transform.SetParent(socket, worldPositionStays: false);
                currentView = starterViewInScene;
            }
            else
            {
                currentView = Instantiate(starterViewInScene, socket, false);
            }

            if (starterWeaponData)
                ApplyStatsToGun(starterWeaponData, initializeAmmo: true);

            ApplyOffsets(currentView.transform, starterWeaponData);
            EnsureVisible(currentView);

            var muzz = FindMuzzle(currentView.transform);
            gun.muzzle = muzz ? muzz : socket;
            return;
        }

        if (socket.childCount > 0)
        {
            currentView = socket.GetChild(0).gameObject;
            ApplyOffsets(currentView.transform, starterWeaponData);
            EnsureVisible(currentView);

            var muzz = FindMuzzle(currentView.transform);
            gun.muzzle = muzz ? muzz : socket;

            if (starterWeaponData)
                ApplyStatsToGun(starterWeaponData, initializeAmmo: true);
        }
    }

    public void Equip(WeaponData data, GameObject viewPrefab, bool resetAmmoFromData)
    {
        if (!socket || !gun || !data || !viewPrefab)
        {
            Debug.LogError("[WeaponEquip] Equip() missing data/viewPrefab or socket/gun.");
            return;
        }

        ApplyStatsToGun(data, initializeAmmo: resetAmmoFromData);

        CleanSocket();

        currentView = Instantiate(viewPrefab, socket, false);
        ApplyOffsets(currentView.transform, data);
        EnsureVisible(currentView);

        var muzz = FindMuzzle(currentView.transform);
        gun.muzzle = muzz ? muzz : socket;

        AmmoUI.NotifyChange(gun.currentInMag, gun.magazineSize, gun.reserveAmmo);
    }

    void CleanSocket()
    {
        if (socket == null) return;
        for (int i = socket.childCount - 1; i >= 0; i--)
        {
            var child = socket.GetChild(i).gameObject;
            Destroy(child);
        }
        currentView = null;
    }

    void ApplyStatsToGun(WeaponData data, bool initializeAmmo)
    {
        if (gun == null || data == null) return;

        gun.weaponData = data;
        gun.magazineSize = data.magazineSize;
        gun.fireCooldown = data.fireCooldown;
        gun.reloadTime = data.reloadTime;

        if (initializeAmmo)
        {
            gun.currentInMag = data.magazineSize;
            gun.reserveAmmo = Mathf.Max(0, data.startingReserve);
        }
    }

    void ApplyOffsets(Transform t, WeaponData dataOrNull)
    {
        if (!t) return;

        // reset
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        // per-weapon (if present) else fallback
        Vector3 off = dataOrNull ? dataOrNull.viewLocalOffset : localOffset;
        Vector3 eur = dataOrNull ? dataOrNull.viewLocalEuler : localEuler;
        Vector3 scl = dataOrNull ? dataOrNull.viewLocalScale : localScale;

        t.localPosition = off;
        t.localRotation = Quaternion.Euler(eur);
        t.localScale = scl;
    }

    Transform FindMuzzle(Transform root)
    {
        if (!root) return null;
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
        {
            if (tr.name.IndexOf("muzzle", StringComparison.OrdinalIgnoreCase) >= 0)
                return tr;
        }
        return null;
    }

    void EnsureVisible(GameObject view)
    {
        if (!view) return;

        // Put on same layer as socket (camera culling)
        int layer = socket ? socket.gameObject.layer : gameObject.layer;
        foreach (var tr in view.GetComponentsInChildren<Transform>(true))
            tr.gameObject.layer = layer;

        // Enable all renderers
        foreach (var r in view.GetComponentsInChildren<Renderer>(true))
            r.enabled = true;
    }
}
