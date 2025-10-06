using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Prologue : MonoBehaviour
{
    public TextMeshProUGUI narratorText;
    public float delayBeforeNextScene = 5f;

    void Start()
    {
        StartCoroutine(ShowPrologue());
    }

    IEnumerator ShowPrologue()
    {
        narratorText.text = "Do you believe in fairy tales?";
        yield return new WaitForSeconds(2.5f);
        narratorText.text = "Do you brush your teeth every day?";
        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene("MainScene");
    }
}
