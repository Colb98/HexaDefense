using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Coroutine hideCoroutine;

    public void SetHealth(float current, float max)
    {
        fillImage.fillAmount = Mathf.Clamp01(current / max);
        Show();
    }

    public void Show(float duration = 5f)
    {
        gameObject.SetActive(true);
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    // Optionally, always face camera (billboard effect)
    //void LateUpdate()
    //{
    //    if (Camera.main != null)
    //        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    //}
}
