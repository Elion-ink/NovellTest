using System.IO;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string name;
    public string text;
    public string emotion;
    public string background; // 💡 новое поле для смены фона
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
    public float typingSpeed = 0.03f;

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
        // 💡 Переход в главное меню
        if (nodeId == "END")
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, nodeId + ".json");
        string json = File.ReadAllText(path);
        currentNode = JsonUtility.FromJson<DialogueNode>(json);
        dialogueIndex = 0;

        // Загружаем фон из корня узла
        if (backgroundImage != null && !string.IsNullOrEmpty(currentNode.background))
        {
            SetBackground(currentNode.background);
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

            // 💡 Сменить фон, если указан в этой реплике
            if (!string.IsNullOrEmpty(line.background))
            {
                SetBackground(line.background);
            }

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

    void SetBackground(string bgName)
    {
        Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");
        if (bgSprite != null)
            backgroundImage.sprite = bgSprite;
        else
            Debug.LogWarning($"Фон '{bgName}' не найден в Resources/Backgrounds/");
    }

    void UpdateCharacterSprite(string characterName, string emotion)
    {
        if (toothFairyImage != null) toothFairyImage.enabled = false;
        if (noahImage != null) noahImage.enabled = false;
        if (mikeImage != null) mikeImage.enabled = false;

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

    void ShowOptions()
    {
        ClearOptions();

        if (currentNode.options == null || currentNode.options.Length == 0)
        {
            nameText.text = "";
            dialogueText.text += "\n\n[Конец]";
            return;
        }

        for (int i = 0; i < currentNode.options.Length; i++)
        {
            optionsButtons[i].gameObject.SetActive(true);
            TextMeshProUGUI btnText = optionsButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = currentNode.options[i].text;

            DialogueOption capturedOption = currentNode.options[i];
            optionsButtons[i].onClick.AddListener(() => LoadNode(capturedOption.nextNode));
        }
    }

    public void OnNextButton()
    {
        ShowNextLine();
    }
}
