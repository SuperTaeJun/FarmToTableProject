using Firebase.Firestore;

[FirestoreData]
public class BlockDto
{
    [FirestoreProperty]
    public int Type { get; set; }

    [FirestoreProperty]
    public int X { get; set; }

    [FirestoreProperty]
    public int Y { get; set; }

    [FirestoreProperty]
    public int Z { get; set; }
}