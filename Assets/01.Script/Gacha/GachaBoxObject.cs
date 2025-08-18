using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GachaBoxObject : MonoBehaviour
{
    [SerializeField] private GameObject _selectedVfx;
    [SerializeField] private GameObject _hoverVfx;
    private bool isSelected = false;


    private void Start()
    {
        GachaScene.Instance.OnGachaPerformed += () => isSelected = true;
    }

    private void OnMouseDown()
    {
        if (isSelected) return;
        SoundManager.Instance.PlaySFX(SFXType.Gacha);
        _hoverVfx.SetActive(false);
        _selectedVfx.SetActive(true);
        GachaScene.Instance.OnGachaButtonClicked();
        StartCoroutine(WaitForParticleEnd());

    }
    private void OnMouseEnter()
    {
        _hoverVfx.SetActive(true);
    }
    private void OnMouseExit()
    {
        _hoverVfx.SetActive(false);
    }
    private IEnumerator WaitForParticleEnd()
    {
        // 파티클이 살아있는 동안 기다림
        while (_selectedVfx != null && _selectedVfx.activeSelf == true)
        {
            yield return null;
        }

        GachaScene.Instance.PerformGacha();
    }
}
