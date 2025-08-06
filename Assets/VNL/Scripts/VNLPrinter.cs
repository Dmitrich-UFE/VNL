using UnityEngine;
using TMPro;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

public class VNLPrinter : MonoBehaviour
{
    public enum printStatus {Printing, Printed}
    public VNLTextStyles vnlTextStyles;
    //private Queue<string> SymbolsQueue;
    private int currentSymbolsPerSecond;

    //текстовое окно
    [SerializeField] private TMP_Text dialogueWindow;
    [SerializeField] private string str;
    [SerializeField] private VNLClickHandler _VNLClickHandler;
    [SerializeField] private int symbolsPerSecond;
    public printStatus PrintStatus {get; private set;}
    public int SymbolsPerSecond 
    {
        get => symbolsPerSecond;
        set 
        {
            if (value < 0) { Debug.LogError("Symbols count per second mustn't be less a zero"); } 
            symbolsPerSecond = value;
        }
    }

    void Awake()
    {
        //SymbolsQueue = new Queue<string>();
        currentSymbolsPerSecond = SymbolsPerSecond;
    }
    
    void Start()
    {
        PrePrint(dialogueWindow.text, 25);
        dialogueWindow.text = "";
        //Print(dialogueWindow.text);
    }

    
    //???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
//новая реализация

    void PrePrint(string Sentence, int SymbolsPerSecond)
    {
        Debug.Log("loh1");
        if (SymbolsPerSecond <= 0) 
        {
            Debug.LogError("SymbolsPerrSecond count must be positive");
            return;
        }

        Queue<string> SymbolsQueue = new Queue<string>();
        MatchCollection Symbols = Regex.Matches(Sentence, @"(<[^>]+>|.)");

        foreach (Match Symbol in Symbols) { SymbolsQueue.Enqueue(Symbol.Value); }
        Symbols = null;

        PrintStatus = printStatus.Printing;
        Debug.Log("loh2");
        StartCoroutine(Print(SymbolsQueue, SymbolsPerSecond));
    }


    IEnumerator Print(Queue<string> SymbolsQueue, int SymbolsPerSecond)
    {
        while (true)
        {

        if (SymbolsQueue.Count == 0)
        {
            PrintStatus = printStatus.Printed;
            yield break;
        }

        string currentSymbol = SymbolsQueue.Dequeue();

        Debug.Log("loh3");
        switch (currentSymbol)
        {
            case string VNLTag when IsVNLTag(VNLTag):

                var VNLTagInfo = GetVNLTagInfo(VNLTag);
                switch (VNLTagInfo.TagName)
                {
                    case "Wait":
                        if (VNLTagInfo.IsOpener)
                        {
                            bool ClickWhileWaiting = false;
                            _VNLClickHandler.OnClick += () => ClickWhileWaiting = true;
                            yield return new WaitUntil(() => ClickWhileWaiting);
                        }
                        break;

                    case "Delay":
                        if (VNLTagInfo.IsOpener)
                        {
                            yield return new WaitForSecondsRealtime(Convert.ToInt32(VNLTagInfo.TagParameter)/1000f);
                        }
                        break;

                    case "SPS":
                        if (VNLTagInfo.IsOpener)
                        {
                            StopCoroutine("Print");
                            StartCoroutine(Print(SymbolsQueue, Convert.ToInt32(VNLTagInfo.TagParameter)));
                        }
                        else
                        {
                            StopCoroutine("Print");
                            StartCoroutine(Print(SymbolsQueue, this.SymbolsPerSecond));
                        }
                        break;

                    default:
                        Debug.LogError($"Tag \"{VNLTagInfo.TagName}\" is not exist in current context");
                        Printing(currentSymbol);
                        break;
                }
                yield return new WaitForSecondsRealtime(1f/SymbolsPerSecond);
                break;

            case string VNLStyleTag when IsVNLStyleTag(VNLStyleTag):
                var VNLStyleTagInfo = GetVNLStyleTagInfo(VNLStyleTag);
                switch(VNLStyleTagInfo.StyleTagName)
                {
                    case "RandomLetterSize":
                        if (VNLStyleTagInfo.IsOpener)
                        {
                            for (int i = 0; i < VNLStyleTagInfo.TagParameters.Count; i++) 
                                VNLStyleTagInfo.TagParameters[i] = Convert.ToInt32(VNLStyleTagInfo.TagParameters[i]);
                            vnlTextStyles.Add("RandomLetterSize", VNLStyleTagInfo.TagParameters);
                        }
                        else
                        {
                            vnlTextStyles.Remove("RandomLetterSize");  
                        }
                        yield return new WaitForSecondsRealtime(1f/SymbolsPerSecond); 
                        break;
                        
                    default:
                        Debug.LogError($"Tag \"{VNLStyleTagInfo.StyleTagName}\" is not exist in current context");
                        Printing(currentSymbol);
                        yield return new WaitForSecondsRealtime(1f/SymbolsPerSecond); 
                        break;
                }
                yield return new WaitForSecondsRealtime(1f/SymbolsPerSecond);
                break;

            default:
                Debug.Log("loh4");
                Printing(currentSymbol);
                break;
        }
        yield return new WaitForSecondsRealtime(1f/SymbolsPerSecond); 

        } //окончание while true
    }

    void Printing(string text)
    {
        Debug.Log("Loh5");
        dialogueWindow.text += vnlTextStyles.ApplyAddedStyles(text);
    }


    //Получение информации о теге VNL
    (string TagName, bool IsOpener, object TagParameter) GetVNLTagInfo(string VNLTag)
    {
        string TagName = Regex.Match(VNLTag, @"(?<=(VNL *))\w+").Value;
        bool IsOpener = !Regex.IsMatch(VNLTag, @"^ *< */ *VNL *\w+ *> *$");
        object TagParameter = Regex.Match(VNLTag, @"(?<=\= *)((\d+)|""(\S+)"")").Value;

        //если тег закрывающий, TagParameter вернет String.Empty, т.е. ""
        return (TagName, IsOpener, TagParameter); 
    }

    //Получение информации о теге VNLStyle
    (string StyleTagName, bool IsOpener, List<object> TagParameters) GetVNLStyleTagInfo(string VNLStyleTag)
    {
        string StyleTagName = Regex.Match(VNLStyleTag, @"(?<=(VNLStyle *))\w+").Value;
        bool IsOpener = !Regex.IsMatch(VNLStyleTag, @"^ *< */ *VNLStyle *\w+ *> *$");

        //для открывающего тега
        if (IsOpener == true)
        {
            List<object> TagParameters = new List<object>();
            MatchCollection MatchParameters = Regex.Matches(VNLStyleTag, @"(?<= *\( *| *, *)(([^, !])+)(?= *\) *| *, *)");
            foreach (Match MatchParameter in MatchParameters) TagParameters.Add(MatchParameter.Value);
            Debug.Log(TagParameters.Count);
            return (StyleTagName, IsOpener, TagParameters);
        }

        //для закрывающего тега
        return (StyleTagName, IsOpener, new List<object>());
    }

    //Проверки тега на соответствие шаблонам VNL. Бракует большинство неверно написанных тегов, но теги с неправильным названием и/или параметрами пропускает
    //Проверка: это тег VNL?
    bool IsVNLTag(string Tag)
    {
        return Regex.IsMatch(Tag, @"^ *< *VNL *\w+ *(= *\S+)? *> *$") ^ Regex.IsMatch(Tag, @"^ *< *\/ *VNL *\w+ *> *$");
    }

    //Проверка: это тег VNLStyle?
    bool IsVNLStyleTag(string VNLTag)
    {
        return Regex.IsMatch(VNLTag, @"^ *< *VNLStyle *\w+ *\(( *\S+ *)*\) *> *$") ^ Regex.IsMatch(VNLTag, @"^ *< */ *VNLStyle *\w+ *> *$");
    }



}