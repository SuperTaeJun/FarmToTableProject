using System;
using UnityEngine;

[Serializable]
public class ForageDTO
{
    public string Type;
    public Vector3 Position;
    public Vector3 Rotation;

    public ForageDTO() { }

    public ForageDTO(Forage forage)
    {
        Type = forage.Type.ToString();
        Position = forage.Position;
        Rotation = forage.Rotation;
    }
}