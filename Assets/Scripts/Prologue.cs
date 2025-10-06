using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Prologue : MonoBehaviour
{
    public TextMeshProUGUI narratorText;
    public float delayBeforeNextScene = 5f; // через сколько секунд перейти

    void Start()
    {
        StartCoroutine(ShowPrologue());
    }

    IEnumerator ShowPrologue()
    {
        narratorText.text = "Вы верите в сказки?";
        yield return new WaitForSeconds(2.5f);
        narratorText.text = "Чистите зубы каждый день?";
        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene("MainScene"); // <- сюда твоя основная сцена
    }
}
