using UnityEngine;

public class WaitForEventYieldInstruction : CustomYieldInstruction
{
    private VNLClickHandler _VNLClickHandler;
    private bool IsClicked;

    public WaitForEventYieldInstruction(VNLClickHandler externalVNLClickHandler, bool defaultisClicked)
    {
        _VNLClickHandler = externalVNLClickHandler;
        IsClicked = defaultisClicked;
        SubscribeOnEvent();
    }

    private void SubscribeOnEvent()
    {
        _VNLClickHandler.OnClick += SetTrueIsClicked;
        _VNLClickHandler.OnClick += UnSubscribeOnEvent;
        
    }

    private void UnSubscribeOnEvent()
    {
        _VNLClickHandler.OnClick -= SetTrueIsClicked;
    }

    private void SetTrueIsClicked()
    {
        IsClicked = true;
    }


    public override bool keepWaiting
    {
        get
        {
            return !IsClicked;
        }
    }




}
