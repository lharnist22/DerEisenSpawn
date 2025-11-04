using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerScript : MonoBehaviour
{
    DamageOverlay damageOverlay;
    public Transform cam;
    Rigidbody rb;
    CapsuleCollider col;
    public float move = 30f;
    public float jumpImpulse = 6f;
    public float maxHorizontalSpeed = 8f; 

    [Header("Health")]
    public int maxBites = 3;
    public string gameOverSceneName = "GameOverScene";
    int biteCount = 0;
    bool isDead = false;

    PlayerUI ui;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
    }

    void Start()
    {
#if UNITY_2023_1_OR_NEWER
        ui = ui ? ui : FindFirstObjectByType<PlayerUI>(FindObjectsInactive.Include);
        damageOverlay = damageOverlay ? damageOverlay : FindFirstObjectByType<DamageOverlay>(FindObjectsInactive.Include);
#else
    if (!ui)
    {
        var allUIs = Resources.FindObjectsOfTypeAll<PlayerUI>();
        foreach (var u in allUIs) { if (u && u.gameObject.scene.IsValid()) { ui = u; break; } }
    }
    if (!damageOverlay)
    {
        var allDO = Resources.FindObjectsOfTypeAll<DamageOverlay>();
        foreach (var d in allDO) { if (d && d.gameObject.scene.IsValid()) { damageOverlay = d; break; } }
    }
#endif

        if (!damageOverlay) Debug.LogWarning("[Player] DamageOverlay not found at Start().");
    }



    void FixedUpdate()
    {
        // --- INPUT (legacy axes) ---
        float vertical = Input.GetAxis("Vertical");     // W/S
        float horizontal = Input.GetAxis("Horizontal"); // A/D

        // --- JUMP (impulse) ---
        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }

        // --- ORIENTATION-RELATIVE MOVE VECTORS ---
        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cam != null)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized; 
            Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized; 
            fwd = camForward;
            right = camRight;
        }

        // --- DESIRED MOVE DIRECTION ---
        Vector3 desired = fwd * vertical + right * horizontal;

        if (desired.sqrMagnitude > 0.0001f)
        {
            desired.Normalize(); 
            rb.AddForce(desired * move, ForceMode.Acceleration); // Move in cam-relative dir
        }

        // --- OPTIONAL: CAP HORIZONTAL SPEED ---
        Vector3 v = rb.linearVelocity;
        Vector3 vHoriz = new Vector3(v.x, 0f, v.z);
        if (vHoriz.magnitude > maxHorizontalSpeed)
        {
            vHoriz = vHoriz.normalized * maxHorizontalSpeed;
            rb.linearVelocity = new Vector3(vHoriz.x, v.y, vHoriz.z);
        }
    }

    public void TakeBite()
    {
        if (isDead) { Debug.Log("[Player] Ignored bite: already dead."); return; }

        biteCount++;
        damageOverlay?.OnHit(biteCount, maxBites);


        if (biteCount >= maxBites)
        {
            isDead = true;
            StartCoroutine(GoGameOver());
        }
    }

    System.Collections.IEnumerator GoGameOver()
    {
        Debug.Log($"[Player] Attempting to load scene '{"GameOver"}'…");
        yield return null;

        if (Application.CanStreamedLevelBeLoaded("GameOver"))
        {
            SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"[Player] Scene '{"GameOver"}' is not in Build Settings or name is wrong.");
        }
    }
}
