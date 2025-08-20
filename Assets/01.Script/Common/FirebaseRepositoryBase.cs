using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;
using UnityEngine;

public abstract class FirebaseRepositoryBase
{
    protected FirebaseAuth Auth => FirebaseAuth.DefaultInstance;
    protected FirebaseFirestore Firestore => FirebaseFirestore.DefaultInstance;
    protected string UserId => FirebaseManager.Instance.GetUserId();


    /// 반환값이 있는 비동기 작업 실행
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> taskFunc, string context = "")
    {
        await FirebaseManager.Instance.InitTask;

        try
        {
            // 로딩 UI 표시
            Debug.Log($"[Firebase] 시작: {context}");
            T result = await taskFunc.Invoke();
            // 로딩 UI 종료
            Debug.Log($"[Firebase] 완료: {context}");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] 실패: {context} - {e.Message}");
            throw; // 필요 시 예외를 상위로 던져서 호출부에서 처리
        }
    }

    /// 반환값이 없는 비동기 작업 실행
    protected async Task ExecuteAsync(Func<Task> taskFunc, string context = "")
    {
        await FirebaseManager.Instance.InitTask;

        try
        {
            Debug.Log($"[Firebase] 시작: {context}");

            await taskFunc.Invoke();

            Debug.Log($"[Firebase] 완료: {context}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] 실패: {context} - {e.Message}");
            throw;
        }
    }
}
