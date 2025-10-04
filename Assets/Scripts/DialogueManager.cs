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
    public string background;          // Фон сцены (новое поле)
    public DialogueLine[] dialogue;    // Реплики
    public DialogueOption[] options;   // Варианты выбора
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;            // Имя персонажа
    public TextMeshProUGUI dialogueText;        // Реплика
    public GameObject optionsContainer;         // Контейнер для кнопок
    public GameObject optionButtonPrefab;       // Префаб кнопки выбора

    [Header("Изображения")]
    public Image characterImage;                // Спрайт персонажа
    public Image backgroundImage;              // 🆕 Спрайт заднего фона

    private DialogueNode currentNode;
    private int dialogueIndex = 0;

    void Start()
    {
        LoadNode("start"); // Начало диалога
    }

    void LoadNode(string nodeId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, nodeId + ".json");
        string json = File.ReadAllText(path);
        currentNode = JsonUtility.FromJson<DialogueNode>(json);
        dialogueIndex = 0;

        // 🆕 Меняем фон, если указано
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

            // 🆕 Смена эмоции персонажа
            if (characterImage != null && !string.IsNullOrEmpty(line.emotion))
            {
                Sprite sprite = Resources.Load<Sprite>($"Characters/{line.name}/{line.emotion}");
                if (sprite != null)
                {
                    characterImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Спрайт эмоции '{line.emotion}' не найден для {line.name} в Resources/Characters/{line.name}/");
                }
            }

            dialogueIndex++;
        }
        else
        {
            ShowOptions();
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
            // Конец узла
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

    // Для кнопки "Дальше"
    public void OnNextButton()
    {
        ShowNextLine();
    }
}
