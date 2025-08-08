using System;
using System.Collections.Generic;
using UnityEngine;

public class VNLClickHandler : MonoBehaviour
{
    public delegate void OnClickHandler();
    public event OnClickHandler? OnClick;

    //public delegate void OnClickHandlerFastPrint(Queue<string> SymbolsQueue);
    //public event Action<Queue<string>> OnClickFastPrint;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            OnClick?.Invoke();
            //OnClickFastPrint?.Invoke(new Queue<string>());
        }
    }

    public void OnLeftMouseClickUIHandler()
    {
        OnClick?.Invoke();
        //OnClickFastPrint?.Invoke(new Queue<string>());
    }
}