using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using Unity.VisualScripting;
using System;
using UnityEngine.Timeline;

public class VNLInterpreter : MonoBehaviour
{
    [SerializeField] private VNLPrinter _VNLPrinter;
    [SerializeField] private VNLDefines _VNLDefines;
    [SerializeField] private VNLClickHandler _VNLClickHandler;
    [SerializeField] private VNLDialogueWindow _VNLDialogueWindow;
    [SerializeField] private string firstScript;
    [SerializeField] private PlayableDirector timelineDirector;

    private string[] VNLScriptStrings;
    private string currentVNLDirectorName; 
    private string actualInfo;
    private int index;

    



    void Awake()
    {
        _VNLDefines.InitializeResources();
        VNLScriptStrings = GetVNLScriptStrings(firstScript);
    }

    
    void Start()
    {
        index = 0;
        HandleString(index);
        StartCoroutine(CheckStatus());
    }

    //Получить строки VNLScript
    string[] GetVNLScriptStrings(string ScriptName)
    {
        return _VNLDefines.VNLScripts[ScriptName].text.Split('\n');
    }


    //Обрабатывает строчки VNLScript
    public void HandleString(int StringIndex)
    {
        try
        {
            string VNLScriptString = VNLScriptStrings[StringIndex];

            switch (VNLScriptString)
            {
                case string VNLDialogueStandartString when IsVNLDialogueStandartString(VNLDialogueStandartString):
                    MatchCollection SplittedDialogueString = Regex.Matches(VNLDialogueStandartString, @"(?<="")[^""]+(?="")");
                    _VNLPrinter.Print(SplittedDialogueString[0].Value, SplittedDialogueString[2].Value);
                    break;
            
                case string VNLDialogueNamedString when IsVNLDialogueNamedString(VNLDialogueNamedString):
                    string CharacterName = Regex.Match(VNLDialogueNamedString, @" *(?<=>)(\S)+").Value;
                    _VNLPrinter.Print(_VNLDefines.Characters[CharacterName].style + _VNLDefines.Characters[CharacterName].localName, Regex.Match(VNLDialogueNamedString, @"(?<="")[^""]+(?="")").Value);
                    break;

                case string VNLDialogueShortString when Regex.IsMatch(VNLDialogueShortString, @"^ *"".+"" *"):
                    string SplittedShortDialogueString = Regex.Match(VNLDialogueShortString, @"(?<="")[^""]+(?="")").Value;
                    _VNLPrinter.Print(" ", SplittedShortDialogueString);
                    break;

                case string VNLScriptCommand when IsVNLScriptCommand(VNLScriptCommand):
                    _VNLPrinter.PrintStatus = VNLPrinter.printStatus.DoingCommand;
                    _VNLDialogueWindow.Clear();
                    MatchCollection ScriptLexems = Regex.Matches(VNLScriptCommand, @"[^\$\s]+");
                    switch(ScriptLexems[0].Value)
                    {
                        case "Import":
                            currentVNLDirectorName = ScriptLexems[1].Value;
                            timelineDirector = _VNLDefines.VNLDirectors[currentVNLDirectorName];
                            timelineDirector.time = 0f;
                            Next();
                            break;

                        case "Set":
                            timelineDirector.Play();
                            Invoke("pausePlayingDirector", Convert.ToSingle(ScriptLexems[1].Value) - Convert.ToSingle(timelineDirector.time));
                            break;

                        case "Download":
                            index = 0;
                            VNLScriptStrings = GetVNLScriptStrings(ScriptLexems[1].Value);
                            HandleString(index);
                            break;
                    
                        case "Info":
                            actualInfo = ScriptLexems[1].Value;
                            Next();
                            break;
                    }
                    break;

                default:
                    Debug.Log($"VNLInterpreter hasn't could handle this string: {VNLScriptString}, index: {index}");
                    break;
            }
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Returning to main menu");
        }
    }

    

    //подписывает или отписывает метод переключения на следующую строку от события клика в зависимости от статуса вывода
    IEnumerator CheckStatus()
    {
        while(true)
        {
        
        switch (_VNLPrinter.PrintStatus)
        {
            case VNLPrinter.printStatus.Printed:
                _VNLClickHandler.OnClick += Next;
                yield return new WaitForEventYieldInstruction(_VNLClickHandler, false);
                break;
            
            default:
                _VNLClickHandler.OnClick -= Next;
                break;
        }

        yield return null;
        }
    }

    public void Next()
    {
        index++;
        HandleString(index);
    }

    private void pausePlayingDirector()
    {
        timelineDirector.Pause();
        Next();
    }

    private bool IsVNLDialogueStandartString(string VNLDialogueStandartString)
    {
        return Regex.IsMatch(VNLDialogueStandartString, @"^ *"".+"" +"".+"" *");
    }

    

    private bool IsVNLDialogueNamedString(string VNLDialogueNamedString)
    {
        return Regex.IsMatch(VNLDialogueNamedString, @"^ *>(\S)+ +"".+"" *");
    }

    private bool IsVNLScriptCommand(string VNLScriptString)
    {
        return Regex.IsMatch(VNLScriptString, @"^ *\$.+");
    }
}
