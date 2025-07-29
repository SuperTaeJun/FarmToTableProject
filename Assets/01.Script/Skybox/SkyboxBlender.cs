using UnityEngine;

public class SkyboxBlender : MonoBehaviour
{
    [Header("Skybox")]
    [SerializeField] private Material skyboxMaterial;
    [Header("Directional Light")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float maxIntensity = 3f;        // 12시 인텐시티
    [SerializeField] private float minIntensity = 0.5f;      // 24시(자정) 인텐시티
    [SerializeField] private Color dayColor = Color.white;   // 낮 색상
    [SerializeField] private Color nightColor = new Color(0.3f, 0.3f, 0.7f); // 밤 색상

    [Header("Ambient Light")]
    [SerializeField] private Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightAmbientColor = new Color(0.02f, 0.02f, 0.08f);

    [SerializeField] private Color dayFogColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightFogColor = new Color(0.02f, 0.02f, 0.08f);



    private void Start()
    {
        RenderSettings.skybox = skyboxMaterial; // 인스턴스 생성
        GameTimeManager.Instance.OnTimeChanged.AddListener(UpdateLighting);

        // 디렉셔널 라이트가 할당되지 않았다면 자동으로 찾기
        if (directionalLight == null)
        {
            directionalLight = RenderSettings.sun;
        }

        // 초기 설정
        if (GameTimeManager.Instance != null)
        {
            UpdateLighting(GameTimeManager.Instance.CurrentHour, GameTimeManager.Instance.CurrentMinute);
        }
    }
    private void UpdateLighting(int hour, int minute)
    {
        if (GameTimeManager.Instance == null) return;

        float currentHour = GameTimeManager.Instance.CurrentHourFloat;

        // 라이트용 블렌드 계산 (12시에 1, 0시에 0)
        float lightBlendValue = (Mathf.Cos(((currentHour - 12f) / 24f) * 2f * Mathf.PI) + 1f) / 2f;

        // 스카이박스용 블렌드 계산 (12시에 0, 0시에 1 - 라이트와 반대)
        float skyboxBlendValue = 1f - lightBlendValue;

        // 스카이박스 적용
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetFloat("_Blend", skyboxBlendValue);
        }

        // 디렉셔널 라이트 적용
        if (directionalLight != null)
        {
            UpdateDirectionalLight(currentHour, lightBlendValue);
        }

        UpdateAmbientLight(lightBlendValue);
        UpdateFogLight(lightBlendValue);
    }

    private void UpdateDirectionalLight(float currentHour, float blendValue)
    {
        // 인텐시티 계산 (12시에 최대, 24시에 최소)
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, blendValue);

        // 색상 계산 (낮/밤 색상 블렌딩)
        directionalLight.color = Color.Lerp(nightColor, dayColor, blendValue);

        // 태양의 자연스러운 움직임 계산
        // 고도각 (Elevation) - 6시(-90도) → 12시(60도) → 18시(-90도)
        float elevationAngle;
        if (currentHour >= 6f && currentHour <= 18f)
        {
            // 낮 시간: 부드러운 포물선 형태
            float timeFromSunrise = (currentHour - 6f) / 12f; // 0~1
            elevationAngle = Mathf.Sin(timeFromSunrise * Mathf.PI) * 60f - 30f; // -30도에서 60도까지
        }
        else
        {
            // 밤 시간: 낮은 각도 유지
            elevationAngle = -60f;
        }

        // 방위각 (Azimuth) - 동쪽에서 서쪽으로 회전
        float azimuthAngle = (currentHour - 6f) * 15f; // 6시(0도) → 18시(180도)

        // 최종 회전 적용
        directionalLight.transform.rotation = Quaternion.Euler(elevationAngle, azimuthAngle, 0f);
    }
    private void UpdateAmbientLight(float blendValue)
    {
        // Ambient Light 색상 변경
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, blendValue);
    }
    private void UpdateFogLight(float blendValue)
    {
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, blendValue);
    }
}