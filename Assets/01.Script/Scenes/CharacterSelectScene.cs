using UnityEngine;

public class CharacterSelectScene : MonoBehaviour
{
    [SerializeField]private GameObject CustomCharacterPrefab;
    [SerializeField] private Transform _spawnTransform;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGMType.Clothing);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Instantiate(CustomCharacterPrefab, _spawnTransform);
    }

}
