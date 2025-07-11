using UnityEngine;

public class CharacterSelectScene : MonoBehaviour
{
    [SerializeField]private GameObject CustomCharacterPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject.Instantiate(CustomCharacterPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
