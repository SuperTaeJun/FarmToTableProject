using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }
    public FirebaseUser CurrentUser { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    private Task _initTask;
    public Task InitTask => _initTask; // 외부 접근용

    public event Action OnFirebaseInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _initTask = InitFirebase(); // 초기화 Task 저장
    }

    private void Start()
    {

    }

    private async Task InitFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase 종속성 확인 완료");

            App = FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;

            await SignInAnonymously();

            IsInitialized = true;
            OnFirebaseInitialized?.Invoke();
        }
        else
        {
            Debug.LogError($"Firebase 종속성 확인 실패: {dependencyStatus}");
            throw new Exception("Firebase 초기화 실패");
        }
    }

    public async Task<bool> SignInAnonymously()
    {
        try
        {
            var authResult = await Auth.SignInAnonymouslyAsync();
            CurrentUser = authResult.User;
            Debug.Log($"Anonymous sign in successful: {CurrentUser.UserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Anonymous sign in failed: {e.Message}");
            return false;
        }
    }

    public string GetUserId()
    {
        return CurrentUser?.UserId ?? "DefaultUser";
    }
}
