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
    public string background; // смена фона в середине реплики
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
    public string music; // имя трека в Resources/Music без расширения
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

    [Header("Музыка")]
    public AudioSource musicSource;       // перетащи сюда AudioSource
    public float musicFadeTime = 1.5f;    // время плавного перехода
    public string defaultMusicName = "";  // если хочешь стартовый трек, укажи имя

    private DialogueNode currentNode;
    private int dialogueIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipTyping = false;

    // handles для корутин чтобы их правильно останавливать
    private Coroutine fadeCoroutine;
    private Coroutine musicCoroutine;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.alpha = 0;

        // настроим musicSource на безопасные значения
        if (musicSource != null)
        {
            musicSource.loop = true;
            if (musicSource.volume <= 0f) musicSource.volume = 1f; // если случайно 0
            // если хочешь стартовый трек — включаем
            if (!string.IsNullOrEmpty(defaultMusicName))
            {
                AudioClip clip = Resources.Load<AudioClip>($"Music/{defaultMusicName}");
                if (clip != null)
                {
                    musicSource.clip = clip;
                    musicSource.Play();
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] defaultMusic '{defaultMusicName}' не найден в Resources/Music/");
                }
            }
        }
        else
        {
            Debug.LogWarning("[DialogueManager] musicSource не присвоен в инспекторе.");
        }

        LoadNode("start");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();

        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            skipTyping = true;
        else if (!isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            OnNextButton();
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
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            dialoguePanel.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        dialoguePanel.alpha = to;
    }

    void LoadNode(string nodeId)
    {
        // переход в главное меню
        if (nodeId == "END")
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, nodeId + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError($"[DialogueManager] JSON-файл узла не найден: {path}");
            return;
        }

        string json = File.ReadAllText(path);

        // парсим и проверяем
        DialogueNode node = null;
        try
        {
            node = JsonUtility.FromJson<DialogueNode>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueManager] Ошибка парсинга JSON {path}: {e.Message}");
            return;
        }

        if (node == null)
        {
            Debug.LogError($"[DialogueManager] FromJson вернул null для {path}");
            return;
        }

        currentNode = node;
        dialogueIndex = 0;

        Debug.Log($"[DialogueManager] LoadNode '{nodeId}' — background='{currentNode.background}', music='{currentNode.music}'");

        // смена фона из корня узла
        if (backgroundImage != null && !string.IsNullOrEmpty(currentNode.background))
        {
            SetBackground(currentNode.background);
        }

        // смена музыки (плавно)
        if (musicSource != null && !string.IsNullOrEmpty(currentNode.music))
        {
            if (musicSource.clip == null || musicSource.clip.name != currentNode.music)
            {
                if (musicCoroutine != null) StopCoroutine(musicCoroutine);
                musicCoroutine = StartCoroutine(ChangeMusicSmoothly(currentNode.music, musicFadeTime));
            }
            else
            {
                Debug.Log("[DialogueManager] Тот же трек уже играет, смена не требуется.");
            }
        }

        // перезапуск анимации панели (останавливаем только fade-корутину)
        if (dialoguePanel != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeDialoguePanel(0f, 1f));
        }

        ShowNextLine();
        ClearOptions();
    }

    void ShowNextLine()
    {
        if (currentNode == null)
        {
            Debug.LogError("[DialogueManager] currentNode == null в ShowNextLine()");
            return;
        }

        if (dialogueIndex < currentNode.dialogue.Length)
        {
            DialogueLine line = currentNode.dialogue[dialogueIndex];
            nameText.text = line.name;
            dialogueIndex++;

            // сменить фон, если указан в самой реплике
            if (!string.IsNullOrEmpty(line.background))
            {
                SetBackground(line.background);
            }

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
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
        if (backgroundImage == null) return;
        Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");
        if (bgSprite != null)
            backgroundImage.sprite = bgSprite;
        else
            Debug.LogWarning($"[DialogueManager] Фон '{bgName}' не найден в Resources/Backgrounds/");
    }

    IEnumerator ChangeMusicSmoothly(string newMusicName, float fadeTime = 1.5f)
    {
        if (musicSource == null)
        {
            Debug.LogWarning("[DialogueManager] musicSource == null, не могу переключить музыку.");
            yield break;
        }

        AudioClip newClip = Resources.Load<AudioClip>($"Music/{newMusicName}");
        if (newClip == null)
        {
            Debug.LogWarning($"[DialogueManager] Музыка '{newMusicName}' не найдена в Resources/Music/");
            yield break;
        }

        Debug.Log($"[DialogueManager] Меняем музыку на '{newMusicName}' (fade {fadeTime}s)");

        float startVolume = musicSource.volume;
        // затухание
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.volume = 0f;

        // смена клипа и старт
        musicSource.clip = newClip;
        musicSource.Play();

        // наращивание
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeTime);
            yield return null;
        }
        musicSource.volume = startVolume;
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
                if (sprite != null) toothFairyImage.sprite = sprite;
                else Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Зубной Феи.");
            }
        }
        else if (characterName == "Noah" && noahImage != null)
        {
            noahImage.enabled = true;
            if (!string.IsNullOrEmpty(emotion))
            {
                Sprite sprite = Resources.Load<Sprite>($"Characters/Noah/{emotion}");
                if (sprite != null) noahImage.sprite = sprite;
                else Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Ноа.");
            }
        }
        else if (characterName == "Mike" && mikeImage != null)
        {
            mikeImage.enabled = true;
            if (!string.IsNullOrEmpty(emotion))
            {
                Sprite sprite = Resources.Load<Sprite>($"Characters/Mike/{emotion}");
                if (sprite != null) mikeImage.sprite = sprite;
                else Debug.LogWarning($"Не найден спрайт эмоции '{emotion}' для Майка.");
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

    // debug helper — можно вызвать из UI кнопки, чтобы сразу проверить воспроизведение
    public void TestPlayMusic(string clipName)
    {
        if (musicSource == null) { Debug.LogWarning("musicSource не присвоен"); return; }
        AudioClip clip = Resources.Load<AudioClip>($"Music/{clipName}");
        if (clip == null) { Debug.LogWarning($"тест: клип '{clipName}' не найден"); return; }
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
        musicSource.loop = true;
        Debug.Log($"[DialogueManager] TestPlayMusic: теперь играет '{clipName}'");
    }
}
