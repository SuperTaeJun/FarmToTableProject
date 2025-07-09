using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;
using UnityEngine;

public abstract class FirebaseRepositoryBase
{
    protected FirebaseAuth Auth => FirebaseAuth.DefaultInstance;
    protected FirebaseFirestore Firestore => FirebaseFirestore.DefaultInstance;


    /// ��ȯ�� �ִ� �񵿱� �۾���
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> taskFunc, string context = "")
    {
        await FirebaseManager.Instance.InitTask;

        try
        {
            // ���� �ε� UI ����
            Debug.Log($"[Firebase] ����: {context}");

            T result = await taskFunc.Invoke();

            // �ε� UI ����
            Debug.Log($"[Firebase] ����: {context}");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ����: {context} - {e.Message}");
            throw; // �ʿ� �� ����� ���� ���� ���ε� ����
        }
    }

    /// ��ȯ�� ���� �񵿱� �۾���
    protected async Task ExecuteAsync(Func<Task> taskFunc, string context = "")
    {
        await FirebaseManager.Instance.InitTask;

        try
        {
            Debug.Log($"[Firebase] ����: {context}");

            await taskFunc.Invoke();

            Debug.Log($"[Firebase] ����: {context}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ����: {context} - {e.Message}");
            throw;
        }
    }
}