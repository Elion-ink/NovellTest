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
    public DialogueLine[] dialogue;    // Массив реплик
    public DialogueOption[] options;   // Варианты выбора
}

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;           // UI для имени персонажа
    public TextMeshProUGUI dialogueText;       // UI для реплики
    public GameObject optionsContainer;        // Контейнер для кнопок
    public GameObject optionButtonPrefab;      // Префаб кнопки выбора
    public Image characterImage;               // Спрайт персонажа на экране

    private DialogueNode currentNode;
    private int dialogueIndex = 0;             // Для показа реплик по очереди

    void Start()
    {
        LoadNode("start"); // Начало истории
    }

    void LoadNode(string nodeId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, nodeId + ".json");
        string json = File.ReadAllText(path);
        currentNode = JsonUtility.FromJson<DialogueNode>(json);
        dialogueIndex = 0;

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

            // Меняем спрайт в зависимости от эмоции
            if(characterImage != null && !string.IsNullOrEmpty(line.emotion))
            {
                // Загружаем спрайт из Resources/Characters/имя_персонажа/эмоция
                Sprite sprite = Resources.Load<Sprite>($"Characters/{line.name}/{line.emotion}");
                if(sprite != null)
                    characterImage.sprite = sprite;
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

        if (currentNode.options.Length == 0)
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
