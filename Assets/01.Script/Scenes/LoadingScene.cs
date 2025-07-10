using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    void Start()
    {
        Invoke("MoveScene", 3);
    }

    private void MoveScene()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }
}
