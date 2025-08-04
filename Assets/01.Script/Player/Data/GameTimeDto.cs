using Firebase.Firestore;

[FirestoreData]
public class GameTimeDto
{
    [FirestoreProperty]
    public int CurrentDay { get; set; }

    [FirestoreProperty]
    public int CurrentHour { get; set; }

    [FirestoreProperty]
    public int CurrentMinute { get; set; }
}