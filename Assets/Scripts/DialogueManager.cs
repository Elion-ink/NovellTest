using System.IO;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string name;
    public string text;
    public string emotion;
}

[System.Serializable]
public class DialogueOption
{
    public string text;
    public string nextNode;
}

[System.Serializable]
public class DialogueNode
{
    public string id;
    public string background;
    public DialogueLine[] dialogue;
    public DialogueOption[] options;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject optionsContainer;
    public Button[] optionsButtons;
    public GameObject optionButtonPrefab;

    [Header("Изображения персонажей")]
    public Image toothFairyImage;
    public Image noahImage;
    public Image mikeImage;
    public Image backgroundImage;

    [Header("Анимация панели")]
    public CanvasGroup dialoguePanel;
    public float fadeDelay = 0.5f;
    public float fadeDuration = 1f;

    [Header("Меню паузы")]
    public GameObject pauseMenu;

    [Header("Эффект печати")]
    public float typingSpeed = 0.03f; // скорость печати

    private DialogueNode currentNode;
    private int dialogueIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipTyping = false;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.alpha = 0;

        LoadNode("start");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            skipTyping = true;
        }
        else if (!isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            OnNextButton();
        }
    }

    void TogglePauseMenu()
    {
        if (pauseMenu == null) return;

        bool isActive = pauseMenu.activeSelf;
        pauseMenu.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
    }

    IEnumerator FadeDialoguePanel(float from, float to)
    {
        yield return new WaitForSeconds(fadeDelay);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            dialoguePanel.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        dialoguePanel.alpha = to;
    }

    void LoadNode(string nodeId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, nodeId + ".json");
        string json = File.ReadAllText(path);
        currentNode = JsonUtility.FromJson<DialogueNode>(json);
        dialogueIndex = 0;

        if (backgroundImage != null && !string.IsNullOrEmpty(currentNode.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{currentNode.background}");
            if (bgSprite != null)
                backgroundImage.sprite = bgSprite;
            else
                Debug.LogWarning($"Фон '{currentNode.background}' не найден в Resources/Backgrounds/");
        }

        if (dialoguePanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeDialoguePanel(0f, 1f));
        }

        ShowNextLine();
        ClearOptions();
    }

    void ShowNextLine()
    {
        if (dialogueIndex < currentNode.dialogue.Length)
        {
            DialogueLine line = currentNode.dialogue[dialogueIndex];

            nameText.text = line.name;
            dialogueIndex++;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(line.text));
            UpdateCharacterSprite(line.name, line.emotion);
        }
        else
        {
            ShowOptions();
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        foreach (char letter in fullText)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
                break;
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void UpdateCharacterSprite(string characterName, string emotion)
{
    // Сначала выключаем всех
    if (toothFairyImage != null) toothFairyImage.enabled = false;
    if (noahImage != null) noahImage.enabled = false;
    if (mikeImage != null) mikeImage.enabled = false;

    // Зубная фея
    if (characterName == "Fairy" && toothFairyImage != null)
    {
        toothFairyImage.enabled = true;
        if (!string.IsNullOrEmpty(emotion))
        {
            Sprite sprite = Resources.Load<Sprite>($"Characters/ToothFairy/{emotion}");
            if (sprite != null)
                toothFairyImage.sprite = sprite;
            else
                Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Зубной Феи.");
        }
    }
    // Ноа
    else if (characterName == "Noah" && noahImage != null)
    {
        noahImage.enabled = true;
        if (!string.IsNullOrEmpty(emotion))
        {
            Sprite sprite = Resources.Load<Sprite>($"Characters/Noah/{emotion}");
            if (sprite != null)
                noahImage.sprite = sprite;
            else
                Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Ноа.");
        }
    }
    // Майк
    else if (characterName == "Mike" && mikeImage != null)
    {
        mikeImage.enabled = true;
        if (!string.IsNullOrEmpty(emotion))
        {
            Sprite sprite = Resources.Load<Sprite>($"Characters/Mike/{emotion}");
            if (sprite != null)
                mikeImage.sprite = sprite;
            else
                Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Майка.");
        }
    }
}


    void ClearOptions()
    {
        foreach (Button btn in optionsButtons)
        {
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }
    }

    // 🔹 Исправленный метод ShowOptions с локальной переменной для правильного замыкания
    void ShowOptions()
    {
        //optionsContainer.transform.GetComponent<VerticalLayoutGroup>().enabled = true;
        ClearOptions();

        if (currentNode.options == null || currentNode.options.Length == 0)
        {
            nameText.text = "";
            dialogueText.text += "\n\n[Конец]";
            return;
        }

for(int i = 0; i < currentNode.options.Length; i++)
{
    //GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer.transform);
    optionsButtons[i].gameObject.SetActive(true);
    TextMeshProUGUI btnText = optionsButtons[i].GetComponentInChildren<TextMeshProUGUI>();
    btnText.text = currentNode.options[i].text;

    //Button btn = optionsButtons[i];

    DialogueOption capturedOption = currentNode.options[i];
    optionsButtons[i].onClick.AddListener(() => LoadNode(capturedOption.nextNode));
}
        //optionsContainer.transform.GetComponent<VerticalLayoutGroup>().enabled = false;
    }



    public void OnNextButton()
    {
        ShowNextLine();
    }
}
