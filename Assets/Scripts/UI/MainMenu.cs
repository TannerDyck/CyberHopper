using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public MenuWipeController wipeTransition;

    public void PlayGame()
    {
        wipeTransition.StartWipe(); // Start the wipe animation
    }
}

