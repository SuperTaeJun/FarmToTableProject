using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("MoveScene", 3);
    }

    private void MoveScene()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }
}
