using UnityEngine;

public class VNLClickHandler : MonoBehaviour
{
    public delegate void OnClickHandler();
    public event OnClickHandler? OnClick;

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return))
        {
            OnClick?.Invoke();
        }
    }
}
