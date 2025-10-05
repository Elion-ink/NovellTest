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
    public GameObject optionButtonPrefab;

    [Header("Изображения персонажей")]
    public Image toothFairyImage;       
    public Image noahImage;             
    public Image backgroundImage;       

    [Header("Анимация панели")]
    public CanvasGroup dialoguePanel;   
    public float fadeDelay = 0.5f;      
    public float fadeDuration = 1f;     

    private DialogueNode currentNode;
    private int dialogueIndex = 0;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.alpha = 0;

        LoadNode("start");
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

        // 🖼 Смена фона
        if (backgroundImage != null && !string.IsNullOrEmpty(currentNode.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{currentNode.background}");
            if (bgSprite != null)
                backgroundImage.sprite = bgSprite;
            else
                Debug.LogWarning($"Фон '{currentNode.background}' не найден в Resources/Backgrounds/");
        }

        // ✨ Перезапуск анимации панели
        if (dialoguePanel != null)
        {
            StopAllCoroutines(); // 💡 чтобы не наслаивались
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
            dialogueText.text = line.text;

            UpdateCharacterSprite(line.name, line.emotion);
            dialogueIndex++;
        }
        else
        {
            ShowOptions();
        }
    }

    void UpdateCharacterSprite(string characterName, string emotion)
    {
        if (toothFairyImage != null) toothFairyImage.enabled = false;
        if (noahImage != null) noahImage.enabled = false;

        if (characterName == "Зубная Фея" && toothFairyImage != null)
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
        else if (characterName == "Ноа" && noahImage != null)
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
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer.transform)
            Destroy(child.gameObject);
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

        foreach (var option in currentNode.options)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer.transform);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = option.text;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => LoadNode(option.nextNode));
        }
    }

    public void OnNextButton()
    {
        ShowNextLine();
    }
}
