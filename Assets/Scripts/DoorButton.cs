using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Название сцены, куда переходить

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
