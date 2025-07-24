using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Net.Http.Headers;
using System.Collections;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject TransisiOut;
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
        StartCoroutine(Transisi(sceneName));
    }

    private IEnumerator Transisi(string sceneName)
    {
        TransisiOut.SetActive(true);
        yield return new WaitForSeconds(1f);
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
