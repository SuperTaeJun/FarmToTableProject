using UnityEngine;

public class PlayerFarmingAbility : PlayerAbility
{
    private ECropType _currentSeed = ECropType.Carrot;

    private void Start()
    {
        _owner.InputController.OnFarmingInput.AddListener(HandleFarmingInput);
    }

    private void HandleFarmingInput()
    {
        //���� ���õȰſ����� �ٸ��� �۵��ؾ���
        //���õȰ� �۹����ִ� ���� �ƴϸ� ���⸮�� ���Ѷ�

        OnPlantCrop();

        //OnWaterCrop();
        //OnHarvestCrop();
    }

    public void SetCurrentSeed(int type)
    {
        _currentSeed = (ECropType)type;
    }
    private void OnPlantCrop()
    {
        // ���� ûũ ID ���
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        // �۹� �ɱ� (��� ����)
        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.PlantCrop(_currentSeed, chunkId, _owner.CurrentSelectedPos);
            Debug.Log($"{_currentSeed.ToString()}�� �ɾ����ϴ�: {_owner.CurrentSelectedPos}");
        }
    }

    private void OnWaterCrop()
    {
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.WaterCrop(chunkId, _owner.CurrentSelectedPos);
            Debug.Log($"���� �־����ϴ�: {_owner.CurrentSelectedPos}");
        }
    }

    private void OnHarvestCrop()
    {
        string chunkId = WorldManager.GetChunkId(_owner.CurrentSelectedPos);

        if (CropsManager.Instance != null)
        {
            _ = CropsManager.Instance.HarvestCrop(chunkId, _owner.CurrentSelectedPos);
            Debug.Log($"��Ȯ�߽��ϴ�: {_owner.CurrentSelectedPos}");

            //�κ��丮�� ����
        }
    }


}
