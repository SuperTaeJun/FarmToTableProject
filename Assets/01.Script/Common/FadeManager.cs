using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections.Generic;
public class FadeManager : MonoBehaviourSingleton<FadeManager>
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;


    protected override void Awake()
    {
        base.Awake();
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
        canvas.sortingOrder = 999;
        // Fade Out ���� ȭ��
        yield return StartCoroutine(Fade(0f, 1f));

        // �� �ε�
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade In �����
        yield return StartCoroutine(Fade(1f, 0f));
        canvas.sortingOrder = -1;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
