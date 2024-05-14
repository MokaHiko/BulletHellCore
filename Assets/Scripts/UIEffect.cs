using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Flags]
enum ScriptedAnimation
{
    None = 0,
    FadeIn = 1 << 0,
    Stretch = 1 << 1,

    Bounce = 1 << 2,
    FadeOut = 1 << 3,
};

public class UIEffect : MonoBehaviour
{
    [SerializeField]
    ScriptedAnimation animation_flags;

    [SerializeField]
    bool effect_children = false;
    
    [SerializeField]
    bool destroy_after_animation = false;

    [Header("FadeIn")]
    [SerializeField]
    float fade_in_duration = 1.0f;

    [SerializeField]
    float stretch_duration = 1.0f;

    [SerializeField]
    float fade_out_duration = 3.0f;

    [Header("Bounce")]
    [SerializeField]
    float bounce_duration = 1.0f;
    float bounce_strength = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        float duration = 0;

        if ((animation_flags & ScriptedAnimation.FadeIn) == ScriptedAnimation.FadeIn)
        {
            duration += fade_in_duration;
            StartCoroutine(FadeInEffect(fade_in_duration));
        }

        if ((animation_flags & ScriptedAnimation.Stretch) == ScriptedAnimation.Stretch)
        {
            duration += stretch_duration;
            StartCoroutine(StretchEffect(stretch_duration));
        }

        if ((animation_flags & ScriptedAnimation.Bounce) == ScriptedAnimation.Bounce)
        {
            duration += bounce_duration;
            StartCoroutine(BounceEffect(bounce_duration));
        }

        if (destroy_after_animation)
        {
            Invoke(nameof(Die), duration);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Die(GameObject root = null)
    {
        GameObject destroyTarget = root ? root : gameObject;

        if ((animation_flags & ScriptedAnimation.FadeOut) == ScriptedAnimation.FadeOut)
        {
            StartCoroutine(FadeOut(fade_out_duration, () => Destroy(destroyTarget)));
        }
        else
        {
            Destroy(destroyTarget);
        }
    }

    IEnumerator StretchEffect(float duration, Action action = null)
    {
        float time = 0.0f;

        Vector3 start_scale = GetComponent<RectTransform>().localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            Vector3 scale = Vector3.Lerp(new Vector3(0.0f, start_scale.y, start_scale.z), start_scale, time / duration);
            GetComponent<RectTransform>().localScale = scale;
            yield return null;
        }

        action?.Invoke();
    }
    IEnumerator BounceEffect(float duration, Action action = null)
    {
        float time = 0.0f;

        Vector3 start_position = GetComponent<RectTransform>().position;

        float offset = bounce_strength;
        float x = UnityEngine.Random.Range(-offset, offset);
        float y = UnityEngine.Random.Range(0, offset);

        Vector3 target_position = start_position + new Vector3(x, y, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            Vector3 position = Vector3.Lerp(start_position, target_position, time / duration);
            GetComponent<RectTransform>().position = position;
            yield return null;
        }

        action?.Invoke();
    }

    IEnumerator FadeInEffect(float duration, Action action = null)
    {
        float time = 0.0f;

        TryGetComponent<CanvasGroup>(out CanvasGroup group);
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1.0f, time / duration);
            GetComponent<CanvasRenderer>().SetAlpha(alpha);

            if(effect_children)
            {
                if(group != null)
                {
                    group.alpha = alpha;
                }
            }

            yield return null;
        }

        action?.Invoke();
    }
    IEnumerator FadeOut(float duration, Action action = null)
    {
        float time = 0.0f;

        TryGetComponent<CanvasGroup>(out CanvasGroup group);
        TryGetComponent<CanvasRenderer>(out CanvasRenderer canvas_renderer);
        TryGetComponent<TMP_Text>(out TMP_Text tmp_text);

        float start_alpha = 1.0f;
        if (canvas_renderer != null)
        {
            start_alpha  = canvas_renderer.GetAlpha();
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(start_alpha, 0.0f, time / duration);

            if (canvas_renderer != null)
            {
                canvas_renderer.SetAlpha(alpha);
            }

            if (tmp_text != null)
            {
                tmp_text.alpha = alpha;
            }

            if(effect_children)
            {
                if(group != null)
                {
                    group.alpha = alpha;
                }
            }

            yield return null;
        }

        action?.Invoke();
    }

}
