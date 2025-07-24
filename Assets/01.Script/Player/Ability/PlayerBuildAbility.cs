using UnityEngine;

public class PlayerBuildAbility : PlayerAbility
{
    private EBuildingType _selectedType;


    private void Start()
    {
        //테스트
        _selectedType = EBuildingType.Fence;

        _owner.InputController.OnLeftMouseInput.AddListener(TryBuild);
    }

    private void Update()
    {
        if (_owner.ModeController.CurrentMode != EPlayerMode.Construction) return;
    }
    public void SetSelectedType(EBuildingType selectedType)
    {
        _selectedType = selectedType;
    }
    private async void TryBuild(EPlayerMode playerMode)
    {
        if (playerMode != EPlayerMode.Construction) return;


        Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(_owner.CurrentSelectedPos);
        string chunkID = chunk.Position.ToChunkId();

        bool success  = await BuildingManager.Instance.TryPlaceBuilding(_selectedType, chunkID, _owner.CurrentSelectedPos, Vector3.zero);

        if(success)
        {
            //todo 이펙트추가, 사운드추가
        }
        else
        {
            Debug.LogWarning("건물 배치에 실패 했습니다.");
        }
    }





}
