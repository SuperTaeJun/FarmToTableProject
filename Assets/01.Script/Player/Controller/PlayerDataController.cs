using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerDataController
{
    private Player _owner;
    private PlayerDataRepository _repository;
    private float _saveInterval = 10f;
    private float _saveTimer;

    // 이벤트
    public event Action OnDataSaved;
    public event Action OnDataLoaded;
    public event Action<string> OnDataError;

    public PlayerDataController(Player owner)
    {
        _owner = owner;
        _repository = new PlayerDataRepository();
    }

    public void Update()
    {
        // 자동 저장 타이머
        _saveTimer += Time.deltaTime;
        if (_saveTimer >= _saveInterval)
        {
            _ = SavePlayerDataAsync(); // 비동기 저장
            _saveTimer = 0f;
        }
    }

    public async Task SavePlayerDataAsync()
    {
        try
        {
            var currentData = await _repository.LoadPlayerDataAsync();
            if (currentData == null)
            {
                currentData = new PlayerDataDto();
            }

            // Player의 현재 상태를 데이터에 반영
            currentData.SetPosition(_owner.transform.position);
            currentData.SetRotation(_owner.transform.rotation);
            currentData.LastSaved = DateTime.UtcNow;
            
            // 차량 언락 정보도 함께 저장
            if (VehicleManager.Instance != null)
            {
                currentData.UnlockedVehicleTypes = VehicleManager.Instance.GetUnlockedVehicleTypesAsInt();
            }

            await _repository.SavePlayerDataAsync(currentData);
            Debug.Log($"플레이어 데이터 저장됨: {_owner.transform.position}");
            
            OnDataSaved?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 저장 실패: {ex.Message}");
            OnDataError?.Invoke($"데이터 저장 실패: {ex.Message}");
        }
    }

    public async Task LoadPlayerDataAsync()
    {
        try
        {
            var playerData = await _repository.LoadPlayerDataAsync();
            
            if (playerData != null)
            {
                // 위치 정보 로드
                _owner.CharacterController.gameObject.SetActive(false);
                _owner.transform.position = playerData.GetPosition();
                _owner.transform.rotation = playerData.GetRotation();
                _owner.CharacterController.gameObject.SetActive(true);
                Debug.Log($"플레이어 위치 로드됨: {_owner.transform.position}");
                
                // 차량 언락 정보를 VehicleManager에 전달
                if (VehicleManager.Instance != null && playerData.UnlockedVehicleTypes != null)
                {
                    VehicleManager.Instance.LoadVehicleUnlockData(playerData.UnlockedVehicleTypes);
                    Debug.Log($"차량 언락 정보 로드됨: {playerData.UnlockedVehicleTypes.Count}개");
                }
                
                OnDataLoaded?.Invoke();
            }
            else
            {
                Debug.Log("저장된 플레이어 데이터가 없습니다.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
            OnDataError?.Invoke($"데이터 로드 실패: {ex.Message}");
        }
    }

    public void OnVehicleDataChanged()
    {
        _ = SavePlayerDataAsync();
    }

}