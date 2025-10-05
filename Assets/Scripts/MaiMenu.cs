using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Метод для кнопки Start
    public void StartGame()
    {
        // Заменяй "GameScene" на имя своей игровой сцены
        SceneManager.LoadScene("GameScene");
    }

    // Метод для кнопки Back или Quit
    public void BackToMenu()
    {
        // Если это главная сцена – выходим из игры
        Application.Quit();

        // Если это внутриигровое меню – возвращаемся в главное меню:
        // SceneManager.LoadScene("MainMenu");
    }
}
