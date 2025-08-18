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
        _hoverVfx.SetActive(false);
        _selectedVfx.SetActive(true);
        GachaScene.Instance.OnGachaButtonClicked();
        StartCoroutine(WaitForParticleEnd());
    }
    private void OnMouseEnter()
    {
        Debug.Log("마우스가 박스 위에 들어옴!");
        _hoverVfx.SetActive(true);
    }
    private void OnMouseExit()
    {
        Debug.Log("마우스가 박스에서 나감!");
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
