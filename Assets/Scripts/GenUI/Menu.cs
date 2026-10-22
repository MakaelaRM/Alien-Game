
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void SelectMenu()
    {
        SceneManager.LoadScene("Menu");
        //SceneManager.LoadScene(0);
        //SceneManager.LoadSceneAsync(0);
    }

    public void Play()
    {
        SceneManager.LoadScene("Town");
        //SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked!");
        Application.Quit();
    }
}