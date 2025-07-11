using UnityEngine;

public class CharacterSelectScene : MonoBehaviour
{
    [SerializeField]private GameObject CustomCharacterPrefab;
    [SerializeField] private Transform _spawnTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject.Instantiate(CustomCharacterPrefab, _spawnTransform);
    }

    void Update()
    {
        
    }
}
