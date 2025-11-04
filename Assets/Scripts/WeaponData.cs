using UnityEngine;

public enum FireMode { Semi, Auto }



[CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WeaponData")]
public class WeaponData : ScriptableObject


{ 
    // WeaponData.cs
    [Header("View Tuning (local to WeaponSocket)")]
    public Vector3 viewLocalOffset = new Vector3(0.20f, -0.25f, 0.80f);
    public Vector3 viewLocalEuler = Vector3.zero;
    public Vector3 viewLocalScale = Vector3.one;

    [Header("Display")]
    public string displayName = "Weapon";

    [Header("Ammo")]
    public int magazineSize = 8;
    public int startingReserve = 80;

    [Header("Damage")]
    public float bodyDamage = 10f;
    public float headshotMultiplier = 2f;

    [Header("Firing")]
    public FireMode fireMode = FireMode.Semi; // ← NEW
    public float fireCooldown = 0.15f;
    public float reloadTime = 1.8f;
}
