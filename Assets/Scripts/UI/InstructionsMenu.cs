using UnityEngine;
using UnityEngine.UI;

public class InstructionsMenu : MonoBehaviour
{
    [SerializeField] GameObject instructionsMenu;

    private Image buttonImage;
    public Sprite inactiveSprite;
    public Sprite activeSprite;
    private bool isActive = false;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    public void Activate()
    {
        if (isActive == false)
        {
            buttonImage.sprite = activeSprite;
            instructionsMenu.SetActive(true);
            isActive = true;
        }
        else
        {
            buttonImage.sprite = inactiveSprite;
            instructionsMenu.SetActive(false);
            isActive = false;
        }
    }
}
