using System.Collections;
using UnityEngine;

public class HintCanvasController : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    // 淡入/淡出持续的时间（秒）
    [SerializeField]
    private float fadeDuration = 1.0f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// 显示提示 Canvas
    public void ShowHint()
    {
        // 仅在 Minimal 模式下显示提示
        if (GameManager.Instance.selectedTutorial == TutorialLevel.Minimal)
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        }
    }

    /// 隐藏提示 Canvas
    public void HideHint()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDisable());
    }

    /// 淡出效果完成后禁用 GameObject
    private IEnumerator FadeOutAndDisable()
    {
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeDuration));
        gameObject.SetActive(false);
    }

    /// 协程：在指定时间内平滑改变 CanvasGroup 的 alpha 值
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }
}
