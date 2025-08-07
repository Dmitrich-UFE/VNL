using UnityEngine;

public class WaitForEventYieldInstruction : CustomYieldInstruction
{
    private VNLClickHandler _VNLClickHandler;
    private bool IsClicked;

    public WaitForEventYieldInstruction(VNLClickHandler externalVNLClickHandler, bool defaultIsClicked)
    {
        _VNLClickHandler = externalVNLClickHandler;
        IsClicked = defaultIsClicked;
        SubscribeOnEvent();
    }

    //подписка на событие клика: основной метод и отписка основного метода от события
    private void SubscribeOnEvent()
    {
        _VNLClickHandler.OnClick += SetTrueIsClicked;
        _VNLClickHandler.OnClick += UnSubscribeOnEvent;
    }

    //отписка метода от события клика
    private void UnSubscribeOnEvent()
    {
        _VNLClickHandler.OnClick -= SetTrueIsClicked;
    }
    
    //метод, ждущий совершения события(основной метод)
    private void SetTrueIsClicked()
    {
        IsClicked = true;
    }

    //свойство, которое определяет, ждет корутина или нет(false--не ждет)
    public override bool keepWaiting
    {
        get
        {
            return !IsClicked;
        }
    }
}