using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Net.Http.Headers;

public class MainMenu : MonoBehaviour
{

    public void enableGameObject(GameObject obj)
    {
        obj.SetActive(true);
    }
    public void disableGameObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void LoadChoseWeaponMenu(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void QuitApplication()
    {
        Debug.Log("Application Quit");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
