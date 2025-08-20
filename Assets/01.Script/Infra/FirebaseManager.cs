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
    public Task InitTask => _initTask;

    public event Action OnFirebaseInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _initTask = InitFirebase(); // 초기화 Task 저장
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 종속성 확인 실패: {dependencyStatus}");
            throw new Exception("Firebase 초기화 실패");
        }

        Debug.Log("Firebase 종속성 확인 완료");
        App = FirebaseApp.DefaultInstance;
        Auth = FirebaseAuth.DefaultInstance;
        Firestore = FirebaseFirestore.DefaultInstance;

        // 상태 변경 이벤트로 CurrentUser 최신화
        Auth.StateChanged += OnAuthStateChanged;
        OnAuthStateChanged(this, null);

        if (Auth.CurrentUser != null)
        {
            CurrentUser = Auth.CurrentUser;
            Debug.Log($"Reusing existing anonymous user: {CurrentUser.UserId}");
        }
        else
        {
            await SignInAnonymously();
        }

        IsInitialized = true;
        OnFirebaseInitialized?.Invoke();
    }

    private void OnDestroy()
    {
        if (Auth != null) Auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (Auth == null) return;
        if (Auth.CurrentUser != CurrentUser)
        {
            CurrentUser = Auth.CurrentUser;
            if (CurrentUser != null)
                Debug.Log($"Auth state changed -> {CurrentUser.UserId}");
        }
    }

    public async Task<bool> SignInAnonymously()
    {
        try
        {
            var result = await Auth.SignInAnonymouslyAsync();
            CurrentUser = result.User;
            Debug.Log($"Anonymous sign in successful: {CurrentUser.UserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Anonymous sign in failed: {e.Message}");
            return false;
        }
    }

    public string GetUserId() => CurrentUser?.UserId ?? "DefaultUser";

}
