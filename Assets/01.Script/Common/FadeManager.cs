using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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
    public void FadeScreenWithEvent(Action midAction = null)
    {
        StartCoroutine(FadeRoutine(midAction));
    }
    private IEnumerator FadeRoutine(Action midAction = null)
    {
        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
        canvas.sortingOrder = 999;
        // Fade Out °ËÀº È­¸é
        yield return StartCoroutine(Fade(0f, 1f));
        midAction?.Invoke();

        // Fade In ¹à¾ÆÁü
        yield return StartCoroutine(Fade(1f, 0f));
        canvas.sortingOrder = -1;

    }
    private IEnumerator FadeRoutine(string sceneName,Action onComplete = null)
    {
        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
        canvas.sortingOrder = 999;
        // Fade Out °ËÀº È­¸é
        yield return StartCoroutine(Fade(0f, 1f));

        // ¾À ·Îµå
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade In ¹à¾ÆÁü
        yield return StartCoroutine(Fade(1f, 0f));
        canvas.sortingOrder = -1;

        onComplete?.Invoke();
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
