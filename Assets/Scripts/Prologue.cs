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
        narratorText.text = "…В начале всё было погружено во тьму.";
        yield return new WaitForSeconds(2.5f);
        narratorText.text = "Но даже в глубине этой тьмы уже шевелилась история...";
        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene("MainScene"); // <- сюда твоя основная сцена
    }
}
