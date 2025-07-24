using UnityEngine;

public class PlayerBuildAbility : PlayerAbility
{
    private EBuildingType _selectedType;


    private void Start()
    {
        //�׽�Ʈ
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
            //todo ����Ʈ�߰�, �����߰�
        }
        else
        {
            Debug.LogWarning("�ǹ� ��ġ�� ���� �߽��ϴ�.");
        }
    }





}
