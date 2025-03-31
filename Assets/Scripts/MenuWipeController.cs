using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuWipeController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false); // Start disabled
    }

    public void StartWipe()
    {
        gameObject.SetActive(true);  
        animator.enabled = true;
        animator.SetTrigger("StartWipe");  // This should trigger WipeOut
    }

    // This function is called when WipeOut finishes
    public void OnWipeOutComplete()
    {
        SceneManager.LoadScene(1); // Change scene as soon as WipeOut finishes
    }
}
