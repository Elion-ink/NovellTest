using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // новый Input System

public class ItemInfo : MonoBehaviour
{
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private string description;

    public void ShowInfo()
    {
        dialogPanel.SetActive(true);
        dialogText.text = description;
    }

    private void Update()
    {
        if (!dialogPanel.activeSelf) return;

        // Пробел
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            HideInfo();

        // Левый клик мыши
        if (Mouse.current.leftButton.wasPressedThisFrame)
            HideInfo();
    }

    public void HideInfo()
    {
        dialogPanel.SetActive(false);
    }
}
