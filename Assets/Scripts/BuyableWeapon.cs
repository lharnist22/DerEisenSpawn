using UnityEngine;

public class BuyableWeapon : MonoBehaviour, IPurchasable
{
    [SerializeField] int cost = 1400;
    [SerializeField] string weaponDisplay = "AK-47";
    [SerializeField] WeaponData weaponData;            // your AK47Data asset
    [SerializeField] GameObject weaponViewPrefab;      // ← the AK model prefab to hold
    [SerializeField] bool refillAmmoIfOwned = true;
    [SerializeField] int refillReserveAmount = 90;

    public int Cost => cost;
    public string Prompt => $"Press [F] to Buy {weaponDisplay} (${cost})";

    public bool TryPurchase()
    {
        if (!PointsManager.Instance) return false;

        var equip = FindFirstObjectByType<WeaponEquip>(FindObjectsInactive.Exclude);
        var gun = FindFirstObjectByType<GunRayCast>(FindObjectsInactive.Exclude);
        if (!equip || !gun || !weaponData)
        {
            Debug.LogError("[Buy] Missing WeaponEquip / GunRayCast / WeaponData");
            return false;
        }

        bool alreadyEquipped = (gun.weaponData == weaponData);

        // Re-buy = ammo refill
        if (alreadyEquipped && refillAmmoIfOwned)
        {
            if (!PointsManager.Instance.TrySpend(cost)) return false;

            gun.reserveAmmo += Mathf.Max(0, refillReserveAmount);
            int need = gun.magazineSize - gun.currentInMag;
            if (need > 0 && gun.reserveAmmo > 0)
            {
                int load = Mathf.Min(need, gun.reserveAmmo);
                gun.currentInMag += load;
                gun.reserveAmmo -= load;
            }
            AmmoUI.NotifyChange(gun.currentInMag, gun.magazineSize, gun.reserveAmmo);
            Debug.Log("[Buy] Refilled ammo.");
            return true;
        }

        // New purchase = spend + equip (spawns the model under WeaponSocket)
        if (!PointsManager.Instance.TrySpend(cost)) return false;

        if (!weaponViewPrefab)
        {
            Debug.LogError("[Buy] weaponViewPrefab is NOT assigned on BuyableWeapon.");
            return false;
        }

        equip.Equip(weaponData, weaponViewPrefab, resetAmmoFromData: true);
        Debug.Log("[Buy] Equipped AK-47.");
        return true;
    }
}
