using System;
using System.Collections.Generic;
using UnityEngine;

public class VNLClickHandler : MonoBehaviour
{
    public delegate void OnClickHandler();
    public event OnClickHandler? OnClick;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            OnClick?.Invoke();
        }
    }

    public void OnLeftMouseClickUIHandler()
    {
        OnClick?.Invoke();
    }
}