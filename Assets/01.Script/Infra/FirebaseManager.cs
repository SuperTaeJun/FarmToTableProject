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
    public Task InitTask => _initTask; // 외부 공개

    public event Action OnFirebaseInitialized;

    protected override void Awake()
    {
        _initTask = InitFirebase(); // 초기화 Task 저장
    }

    private async Task InitFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase 연결 성공");

            App = FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;

            IsInitialized = true;
            OnFirebaseInitialized?.Invoke();
        }
        else
        {
            Debug.LogError($"Firebase 연결 실패: {dependencyStatus}");
            throw new Exception("Firebase 초기화 실패");
        }
    }
}
