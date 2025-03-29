using UnityEngine;

public class Home : MonoBehaviour
{
    public GameObject frog;

    private void OnEnable()
    {
        frog.SetActive(true);
    }

    private void OnDisable()
    {
        frog.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            enabled = true;
            // Get the Frogger component and call Respawn
            Frogger frogger = other.GetComponent<Frogger>();
            if (frogger != null)
            {
                frogger.Respawn();
            }
        }
    }
}