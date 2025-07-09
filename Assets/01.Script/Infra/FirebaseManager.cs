using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviourSingleton<FirebaseManager>
{
    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    private Task _initTask;
    public Task InitTask => _initTask; // �ܺ� ����

    public event Action OnFirebaseInitialized;

    protected override void Awake()
    {
        _initTask = InitFirebase(); // �ʱ�ȭ Task ����
    }

    private async Task InitFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase ���� ����");

            App = FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;

            IsInitialized = true;
            OnFirebaseInitialized?.Invoke();
        }
        else
        {
            Debug.LogError($"Firebase ���� ����: {dependencyStatus}");
            throw new Exception("Firebase �ʱ�ȭ ����");
        }
    }
}
