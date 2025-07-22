using UnityEngine;
using UnityEditor;
using Firebase.Firestore;
using System.Threading.Tasks;

public class FirebaseCollectionDeleter : EditorWindow
{
    private string collectionName = "";
    private int batchSize = 100;
    private bool isDeleting = false;
    private string statusMessage = "";
    private Vector2 scrollPosition;


    [MenuItem("Tools/Firebase/Collection Deleter")]
    public static void ShowWindow()
    {
        GetWindow<FirebaseCollectionDeleter>("Firebase Collection Deleter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Firebase Collection Deleter", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // 컬렉션 이름 입력
        GUILayout.Label("Collection Name:");
        collectionName = EditorGUILayout.TextField(collectionName);

        EditorGUILayout.Space();

        // 배치 사이즈 설정
        GUILayout.Label("Batch Size (문서 수):");
        batchSize = EditorGUILayout.IntSlider(batchSize, 1, 100);

        EditorGUILayout.Space();

        // 경고 메시지
        EditorGUILayout.HelpBox("주의: 이 작업은 되돌릴 수 없습니다!", MessageType.Warning);

        EditorGUILayout.Space();

        // 삭제 버튼
        EditorGUI.BeginDisabledGroup(isDeleting || string.IsNullOrEmpty(collectionName));
        if (GUILayout.Button(isDeleting ? "삭제 중..." : "컬렉션 삭제", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("확인",
                $"정말로 '{collectionName}' 컬렉션을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다!",
                "삭제", "취소"))
            {
                DeleteCollection();
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // 상태 메시지 스크롤 영역
        GUILayout.Label("Status:");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        EditorGUILayout.TextArea(statusMessage, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // 로그 클리어 버튼
        if (GUILayout.Button("로그 클리어"))
        {
            statusMessage = "";
        }
    }

    private async void DeleteCollection()
    {
        if (FirebaseFirestore.DefaultInstance == null)
        {
            LogMessage("Firebase가 초기화되지 않았습니다!");
            return;
        }

        isDeleting = true;
        statusMessage = "";

        try
        {
            LogMessage($"'{collectionName}' 컬렉션 삭제 시작...");

            var db = FirebaseFirestore.DefaultInstance;
            var collection = db.Collection(collectionName);

            int totalDeleted = 0;

            while (true)
            {
                var query = collection.Limit(batchSize);
                var snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count == 0)
                {
                    LogMessage("삭제할 문서가 더 이상 없습니다.");
                    break;
                }

                try
                {
                    var batch = db.StartBatch();
                    int batchCount = 0;

                    foreach (var doc in snapshot.Documents)
                    {
                        batch.Delete(doc.Reference);
                        batchCount++;

                        // 안전을 위해 50개마다 배치 실행
                        if (batchCount >= 50)
                        {
                            await batch.CommitAsync();
                            totalDeleted += batchCount;
                            LogMessage($"{batchCount}개 문서 삭제 완료 (총 {totalDeleted}개)");

                            // 새 배치 시작
                            batch = db.StartBatch();
                            batchCount = 0;

                            // 배치 간 딜레이
                            await Task.Delay(200);
                        }
                    }

                    // 남은 문서들 처리
                    if (batchCount > 0)
                    {
                        await batch.CommitAsync();
                        totalDeleted += batchCount;
                        LogMessage($"{batchCount}개 문서 삭제 완료 (총 {totalDeleted}개)");
                    }
                }
                catch (System.Exception batchError)
                {
                    LogMessage($"배치 삭제 중 오류: {batchError.Message}");
                    LogMessage("배치 크기를 더 줄여보세요.");
                    break;
                }

                // UI 업데이트를 위해 잠시 대기
                await Task.Delay(300);
            }

            LogMessage($"✅ 컬렉션 '{collectionName}' 삭제 완료! 총 {totalDeleted}개 문서 삭제됨");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ 오류 발생: {e.Message}");
        }
        finally
        {
            isDeleting = false;
        }
    }

    private void LogMessage(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        statusMessage += $"[{timestamp}] {message}\n";

        // 스크롤을 맨 아래로
        scrollPosition.y = float.MaxValue;

        // 에디터 창 다시 그리기
        Repaint();

        // Unity 콘솔에도 로그
        Debug.Log($"[Firebase Deleter] {message}");
    }
}
