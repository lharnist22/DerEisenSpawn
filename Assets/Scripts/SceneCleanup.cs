using UnityEngine;

public static class SceneCleanup
{
    public static void KillZombiesAndGameSystems()
    {
        var lives = Object.FindObjectsOfType<ZombieLife>(true);
        foreach (var z in lives) Object.Destroy(z.gameObject);

        var zMgr = Object.FindObjectsOfType<ZombieManager>(true);
        foreach (var m in zMgr) Object.Destroy(m.gameObject);

        var pools = Object.FindObjectsOfType<ZombiePool>(true);
        foreach (var p in pools) Object.Destroy(p.gameObject);

        var pickups = Object.FindObjectsOfType<PowerupPickup>(true);
        foreach (var x in pickups) Object.Destroy(x.gameObject);

        if (PointsManager.Instance) PointsManager.Instance.ResetPoints(0);
    }
}
