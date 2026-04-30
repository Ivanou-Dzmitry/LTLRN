using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EX_SliderAnimator : MonoBehaviour
{
    private Coroutine animationCoroutine;

    public void AnimateTo(float target, float duration)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Slider inactive!");
            return;
        }

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
 
        animationCoroutine = StartCoroutine(Animate(target, duration));

    }

    private IEnumerator Animate(float target, float duration)
    {
        Slider slider = GetComponent<Slider>();

        if (slider == null)
            yield return null;

        float start = 0;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            slider.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        slider.value = target;
    }
}
