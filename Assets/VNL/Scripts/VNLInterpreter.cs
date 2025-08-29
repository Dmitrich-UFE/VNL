using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using Unity.VisualScripting;
using System;

public class VNLInterpreter : MonoBehaviour
{
    [SerializeField] private string[] VNLScriptStrings;
    [SerializeField] private VNLPrinter _VNLPrinter;
    [SerializeField] private VNLDefines _VNLDefines;
    [SerializeField] private VNLClickHandler _VNLClickHandler;
    private string currentVNLDirectorName; 
    private int index;
    



    void Awake()
    {
        VNLScriptStrings = GetVNLScriptStrings("VNL 015");
    }

    
    void Update()
    {
        
    }

    //Получить строки VNLScript
    string[] GetVNLScriptStrings(string ScriptName)
    {
        return _VNLDefines.VNLScripts[ScriptName].text.Split('\n');
    }


    //Обрабатывает строчки VNLScript
    public void HandleString(int StringIndex)
    {
        string VNLScriptString = VNLScriptStrings[StringIndex];
        switch (VNLScriptString)
        {
            case string VNLDialogueStandartString when Regex.IsMatch(VNLDialogueStandartString, @"^ *"".+"" +"".+"" *$"):
                MatchCollection SplittedDialogueString = Regex.Matches(VNLDialogueStandartString, @"(?<="")[^""]+(?="")");
                _VNLPrinter.Print(SplittedDialogueString[0].Value, SplittedDialogueString[2].Value);
                break;
            
            case string VNLDialogueShortString when Regex.IsMatch(VNLDialogueShortString, @"^ *>(\S)+ +"".+"" *$"):
                string CharacterName = Regex.Match(VNLDialogueShortString, @" *(?<=>)(\S)+").Value;
                _VNLPrinter.Print(_VNLDefines.Characters[CharacterName].style + _VNLDefines.Characters[CharacterName].localName, Regex.Match(VNLDialogueShortString, @"(?<="")[^""]+(?="")").Value);
                break;

            case string VNLScriptCommand when IsVNLScriptCommand(VNLScriptCommand):
                MatchCollection ScriptLexems = Regex.Matches(VNLScriptCommand, @"[^\$\s]+");
                switch(ScriptLexems[0].Value)
                {
                    case "Import":
                        currentVNLDirectorName = ScriptLexems[1].Value;
                        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Play();
                        break;
                    
                    case "Set":
                        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Play();
                        Invoke("pausePlayingDirector", Convert.ToSingle(ScriptLexems[1].Value) - Convert.ToSingle(_VNLDefines.VNLDirectors[currentVNLDirectorName]?.time));
                        break;
                    
                    case "Download":
                        index = 0;
                        VNLScriptStrings = GetVNLScriptStrings(ScriptLexems[1].Value);
                        HandleString(index);
                        break;
                }
                break;
        }
    }

    

    //подписывает или отписывает метод переключения на следующую строку от события клика в зависимости от статуса вывода
    IEnumerator SubscribeHandler()
    {
        switch (_VNLPrinter.PrintStatus)
        {
            case VNLPrinter.printStatus.Printed:
                _VNLClickHandler.OnClick += Next;
                break;
            
            case VNLPrinter.printStatus.Printing:
                _VNLClickHandler.OnClick -= Next;
                break;

            default: break;
        }
        yield return null;
    }

    public void Next()
    {
        index++;
        HandleString(index);
    }

    private void pausePlayingDirector()
    {
        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Pause();
    }

    private bool IsVNLScriptCommand(string VNLScriptString)
    {
        return Regex.IsMatch(VNLScriptString, @"^ *\$.+");
    }
}
