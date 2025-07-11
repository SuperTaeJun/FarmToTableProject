using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class PlayerEffectController : MonoBehaviour
{
    public GameObject _moveEffectPrefab;

    public void SpawnMoveEffect()
    {
        GameObject.Instantiate(_moveEffectPrefab);
    }

}