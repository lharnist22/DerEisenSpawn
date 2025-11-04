using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public Transform player;
    public float sensitivity = 0.12f;
    public bool keepCursorLocked = true;
    public float eyeHeight = 1.6f;        
    public Vector3 localOffset = Vector3.zero; // This is so incredibly helpful for camera position
    float yaw;
    float pitch;
    // These are the minimum and maximum I can look up or down
    public float minPitch = -85f;
    public float maxPitch = 85f;

    void OnEnable()
    {
        if (keepCursorLocked)
        {
            // Found this to help debug -- https://discussions.unity.com/t/cursor-lockstate-not-working/145392
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (player)
        {
            yaw = player.eulerAngles.y;
        }

        pitch = transform.localEulerAngles.x;

        if (pitch > 180f)
        {
            pitch -= 360f;
        }
    }

    void Update()
    {
        if (!player || Mouse.current == null) return;

        // Toggle cursor with Esc (Debug mode to mess with other values)
        // Found this to help debug -- https://discussions.unity.com/t/cursor-lockstate-not-working/145392
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // These links helped me here specifically https://gamedev.stackexchange.com/questions/104693/how-to-use-input-getaxismouse-x-y-to-rotate-the-camera?

        // Mouse deltas → yaw/pitch
        Vector2 d = Mouse.current.delta.ReadValue();
        yaw += d.x * sensitivity;
        pitch -= d.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply yaw to the body
        Vector3 bodyAngles = player.eulerAngles;
        bodyAngles.y = yaw;
        player.eulerAngles = bodyAngles;

        // Apply pitch+yaw to camera
        Vector3 camAngles = transform.eulerAngles;
        camAngles.x = pitch;
        camAngles.y = yaw;
        camAngles.z = 0f;
        transform.eulerAngles = camAngles;

        Vector3 basePos = player.position + Vector3.up * eyeHeight;

        // Rotate the localOffset by yaw around Y using cos/sin (2D rotation in XZ)
        float rad = yaw * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);

        // local (x,y,z) → world offset 
        Vector3 off = new Vector3(localOffset.x * c + localOffset.z * s /* world X */, localOffset.y /* world Y */, -localOffset.x * s + localOffset.z * c   /* world Z */);

        transform.position = basePos + off;
    }
}
