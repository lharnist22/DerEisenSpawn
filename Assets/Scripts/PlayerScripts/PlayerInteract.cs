using TMPro;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Refs")]
    public Transform cam;                
    public TMP_Text promptText;          

    [Header("Interact")]
    public float useRange = 3f;
    public KeyCode useKey = KeyCode.F;
    public LayerMask interactMask = ~0;  

    void Start()
    {
        if (!cam && Camera.main) cam = Camera.main.transform;
        if (promptText) promptText.text = "";
    }

    void Update()
    {
        if (!cam) return;

        if (Physics.Raycast(new Ray(cam.position, cam.forward),
                            out var hit, useRange, interactMask,
                            QueryTriggerInteraction.Collide))
        {
            var purch = hit.collider.GetComponentInParent<IPurchasable>();
            if (purch != null)
            {
                int pts = PointsManager.Instance ? PointsManager.Instance.Points : 0;
                bool canBuy = PointsManager.Instance && pts >= purch.Cost;
                string afford = canBuy ? "" : " (Not enough points)";
                if (promptText) promptText.text = $"{purch.Prompt}{afford}";

                if (Input.GetKeyDown(useKey))
                {
                    purch.TryPurchase();   
                }
                return;
            }
        }
        if (promptText) promptText.text = "";
    }
}
