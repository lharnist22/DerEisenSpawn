using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    [SerializeField] float biteCooldown = 0.5f; // seconds between this zombie’s bites
    float nextBiteTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time >= nextBiteTime)
        {
            nextBiteTime = Time.time + biteCooldown;
            var player = other.GetComponent<PlayerScript>();
            if (player != null)
            {
                Debug.Log("The zombie has bit you!");
                player.TakeBite();
            }
        }
    }
}
