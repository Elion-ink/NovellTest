using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string name;      // Имя персонажа
    public string text;      // Реплика
    public string emotion;   // Эмоция для смены спрайта
}

[System.Serializable]
public class DialogueOption
{
    public string text;       // Текст кнопки
    public string nextNode;   // К какому узлу вести
}

[System.Serializable]
public class DialogueNode
{
    public string id;                  // ID узла
    public string background;          // Фон сцены
    public DialogueLine[] dialogue;    // Реплики
    public DialogueOption[] options;   // Варианты выбора
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject optionsContainer;
    public GameObject optionButtonPrefab;

    [Header("Изображения персонажей")]
    public Image toothFairyImage;       // 🧚‍♂️ Зубная фея
    public Image noahImage;             // 👦 Ноа
    public Image backgroundImage;       // 🖼 Фон сцены

    private DialogueNode currentNode;
    private int dialogueIndex = 0;

    void Start()
    {
        LoadNode("start");
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
            {
                backgroundImage.sprite = bgSprite;
            }
            else
            {
                Debug.LogWarning($"Фон '{currentNode.background}' не найден в Resources/Backgrounds/");
            }
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

            // 🧠 Показать правильного персонажа и сменить эмоцию
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
        // Скрываем обоих, а потом показываем нужного
        if (toothFairyImage != null) toothFairyImage.enabled = false;
        if (noahImage != null) noahImage.enabled = false;

        if (characterName == "Зубная Фея" && toothFairyImage != null)
        {
            toothFairyImage.enabled = true;
            if (!string.IsNullOrEmpty(emotion))
            {
                Sprite sprite = Resources.Load<Sprite>($"Characters/ToothFairy/{emotion}");
                if (sprite != null)
                {
                    toothFairyImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Зубной Феи в Resources/Characters/ToothFairy/");
                }
            }
        }
        else if (characterName == "Ноа" && noahImage != null)
        {
            noahImage.enabled = true;
            if (!string.IsNullOrEmpty(emotion))
            {
                Sprite sprite = Resources.Load<Sprite>($"Characters/Noah/{emotion}");
                if (sprite != null)
                {
                    noahImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Ноа в Resources/Characters/Noah/");
                }
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
