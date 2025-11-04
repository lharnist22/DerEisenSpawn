using UnityEngine;

public enum PowerupType { Nuke, MaxAmmo, InstaKill }

[RequireComponent(typeof(Collider))]
public class PowerupPickup : MonoBehaviour
{
    [Header("Type & Effect")]
    public PowerupType type;
    public float instaKillDuration = 30f;  // only used for InstaKill

    [Header("Idle Motion")]
    public float hoverAmplitude = 0.1f;
    public float hoverSpeed = 2f;
    public float spinSpeed = 90f;

    [Header("Lifetime")]
    public float lifetime = 30f;

    [Header("Audio (per power-up)")]
    public AudioClip nukeClip;
    public AudioClip maxAmmoClip;
    public AudioClip instaKillClip;
    [Range(0f, 1f)] public float volume = 1f;

    Vector3 _basePos;
    float _spawnTime;

    void Awake()
    {
        _basePos = transform.position;
        _spawnTime = Time.time;

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        // Spin + hover (does not drift)
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        float y = Mathf.Sin((Time.time - _spawnTime) * hoverSpeed) * hoverAmplitude;
        transform.position = new Vector3(_basePos.x, _basePos.y + y, _basePos.z);

        // Lifetime
        if (Time.time >= _spawnTime + lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pm = PowerupManager.Instance;
        if (!pm) { Destroy(gameObject); return; }

        var clip = GetClipForType(type);
        if (clip) AudioSource.PlayClipAtPoint(clip, transform.position, volume);

        switch (type)
        {
            case PowerupType.Nuke:
                pm.DoNuke();
                break;
            case PowerupType.MaxAmmo:
                pm.DoMaxAmmo();
                break;
            case PowerupType.InstaKill:
                pm.ActivateInstaKill(instaKillDuration);
                break;
        }

        Destroy(gameObject);
    }

    AudioClip GetClipForType(PowerupType t)
    {
        switch (t)
        {
            case PowerupType.Nuke: return nukeClip;
            case PowerupType.MaxAmmo: return maxAmmoClip;
            case PowerupType.InstaKill: return instaKillClip;
            default: return null;
        }
    }
}
