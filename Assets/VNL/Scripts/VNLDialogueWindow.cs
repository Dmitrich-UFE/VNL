using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VNLDialogueWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text textWindow;
    [SerializeField] private TMP_Text nameWindow;
    [SerializeField] private VNLClickHandler _VNLClickHandler;

    [SerializeField] private CanvasGroup dialogueWindow;
    private IEnumerator actualAlphaEnumerator;

    public string GetName()
    {
        return nameWindow.text;
    }

    public string GetActualText()
    {
        return textWindow.text;
    }

    public void SetName(string name)
    {
        nameWindow.text = name;
    }

    public void AddName(string name)
    {
        nameWindow.text += name;
    }

    public void SetText(string text)
    {
        textWindow.text = text;
    }

    public void AddText(string text)
    {
        textWindow.text += text;
    }

    public void Clear()
    {
        textWindow.text = "";
        nameWindow.text = "";
    }

    public void Hide()
    {
        if (actualAlphaEnumerator != null)
            StopCoroutine(actualAlphaEnumerator);
        actualAlphaEnumerator = hideWindow(1f);
        StartCoroutine(actualAlphaEnumerator);
    }

    public void Hide(float time)
    {
        if (actualAlphaEnumerator != null)
            StopCoroutine(actualAlphaEnumerator);
        actualAlphaEnumerator = hideWindow(time);
        StartCoroutine(actualAlphaEnumerator);
    }

    public void Show()
    {
        if (actualAlphaEnumerator != null)
            StopCoroutine(actualAlphaEnumerator);
        actualAlphaEnumerator = showWindow(1f);
        StartCoroutine(actualAlphaEnumerator);
    }

    public void Show(float time)
    {
        if (actualAlphaEnumerator != null)
            StopCoroutine(actualAlphaEnumerator);
        actualAlphaEnumerator = showWindow(time);
        StartCoroutine(actualAlphaEnumerator);
    }

    IEnumerator hideWindow(float time)
    {
        _VNLClickHandler.gameObject.SetActive(false);
        if (time == 0)
        {
            dialogueWindow.alpha = 0f;
            yield break;
        }

        while (dialogueWindow.alpha > 0.000001f)
        {
            dialogueWindow.alpha -= Time.deltaTime / time;
            yield return null;
        }

        dialogueWindow.gameObject.SetActive(false);
        yield break;
    }

    IEnumerator showWindow(float time)
    {
        dialogueWindow.gameObject.SetActive(true);
        if (time == 0)
        {
            dialogueWindow.alpha = 1f;
            yield break;
        }

        while (dialogueWindow.alpha < 0.999999f)
        {
            dialogueWindow.alpha += Time.deltaTime / time;
            yield return null;
        }

        _VNLClickHandler.gameObject.SetActive(true);
        yield break;
    }
}
