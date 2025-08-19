using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class FragmentInfo
{
    public GameObject fragment;
    public Vector3 localPosition;
    public Quaternion localRotation;
}
public class FractureExplosion : MonoBehaviour
{
    GameObject _frag;

    [Header("Explosion Settings")]
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public float upwardsModifier = 0f;
    [SerializeField] private PoolType _type;
    [SerializeField] private Transform _fragmentPos;
    
    public void Explode()
    {
        SoundManager.Instance.PlaySFX(SFXType.FragmentSound);
        ObjectPoolManager.Instance.Get(PoolType.SomkeL, _fragmentPos.position);
        _frag = ObjectPoolManager.Instance.Get(_type, _fragmentPos.position,transform.rotation);

        Rigidbody[] spawnRb = _frag.GetComponentsInChildren<Rigidbody>();

        foreach (var rb in spawnRb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.mass = 1f;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddExplosionForce(explosionForce, _fragmentPos.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }
        _frag.GetComponent<FragmentShrinkAndReturn>().StartShrink();
        Destroy(gameObject);
    }

}
