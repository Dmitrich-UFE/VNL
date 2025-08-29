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
    



    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    void GetVNLScriptStrings(string name)
    {
        int FileIndex = 0;
        for (int i = 0; i < _VNLDefines.VNLScripts.Length; i++)
        {
            if (VNLScriptStrings[i].ToString().Equals(name))
            {
                FileIndex = i;
                break;
            }
        }
        VNLScriptStrings = _VNLDefines.VNLScripts[FileIndex].text.Split('\n');
    }

    public void HandleString(string VNLScriptString)
    {
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

            case string VNLScriptCommand when Regex.IsMatch(VNLScriptCommand, @"^ *\$.+"):
                MatchCollection ScriptLexems = Regex.Matches(VNLScriptCommand, @"[^\$\s]+");
                switch(ScriptLexems[0].Value)
                {
                    case "Import":
                        currentVNLDirectorName = ScriptLexems[1].Value;
                        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Play();
                        break;
                    
                    case "Set":
                        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Play();
                        Invoke("pausePlayingDirector", Convert.ToInt32(ScriptLexems[1].Value) - Convert.ToSingle(_VNLDefines.VNLDirectors[currentVNLDirectorName]?.time));
                        break;
                }
                break;
        }
    }

    public void Next(){}

    private void pausePlayingDirector()
    {
        _VNLDefines.VNLDirectors[currentVNLDirectorName]?.Pause();
    }
}
