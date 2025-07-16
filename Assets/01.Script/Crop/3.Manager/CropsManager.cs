using System.Collections.Generic;
using UnityEngine;

public class CropsManager : MonoBehaviour
{
    public static CropsManager Instance;

    private CropRepositroy _repo;

    private Dictionary<string, Crop> _crops;
    public Dictionary<string, Crop> Crops => _crops;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        _repo = new CropRepositroy();
    }




}
