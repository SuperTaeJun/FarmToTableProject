using System.Collections.Generic;
using Firebase.Firestore;

/// <summary>
/// 작물 청크 문서 DTO - Firebase 저장용
/// 월드 레포지토리와 같은 구조로 사용자별 익명 인증 적용
/// </summary>
[FirestoreData]
public class CropChunkDocumentDto
{
    [FirestoreProperty]
    public string ChunkId { get; set; }

    [FirestoreProperty]
    public List<CropDto> Crops { get; set; } = new List<CropDto>();
}