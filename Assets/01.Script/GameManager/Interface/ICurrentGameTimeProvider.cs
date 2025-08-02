using UnityEngine;

public interface ICurrentGameTimeProvider
{
    int CurrentDay { get; }
    int CurrentHour { get; }
    int CurrentMinute { get; }
}
