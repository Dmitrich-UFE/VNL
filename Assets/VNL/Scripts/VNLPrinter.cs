using UnityEngine;
using TMPro;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class VNLPrinter : MonoBehaviour
{
    public enum printStatus {Printing, Printed}
    [SerializeField] private TMP_Text dialogueWindow;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private VNLTextStyles vnlTextStyles;
    [SerializeField] private VNLClickHandler _VNLClickHandler;
    
    [SerializeField] private int symbolsPerSecond;
    private Queue<string> SymbolsQueue;
    private int currentSymbolsPerSecond;

    public printStatus PrintStatus {get; private set;}
    public int SymbolsPerSecond 
    {
        get => symbolsPerSecond;
        set 
        {
            if (value < 0) 
                Debug.LogError("Symbols count per second mustn't be less a zero");
            else 
                symbolsPerSecond = value;
        }
    }
    
    void Start()
    {
        currentSymbolsPerSecond = SymbolsPerSecond;
        SymbolsQueue = PrePrint(dialogueWindow.text);
        dialogueWindow.text = "";
        StartCoroutine(Printing());
    }

    //Запуск метода для печати
    public void Print(string nameText, string Sentence)
    {
        this.nameText.text = nameText;
        currentSymbolsPerSecond = SymbolsPerSecond;
        SymbolsQueue = PrePrint(dialogueWindow.text);
        dialogueWindow.text = "";
        StartCoroutine(Printing());
    }

    //Анализирует строку, нарезает ее и возвращает очередь нарезанных подстрок
    Queue<string> PrePrint(string Sentence)
    {

        Queue<string> SymbolsQueue = new Queue<string>();
        MatchCollection Symbols = Regex.Matches(Sentence, @"(<[^>]+>|.)");

        foreach (Match Symbol in Symbols) { SymbolsQueue.Enqueue(Symbol.Value); }

        return SymbolsQueue;
    }

    //Анализирует каждую структуру(элемент) в очереди на наличие тегов, которые необходимо обработать 
    IEnumerator Printing()
    {
        _VNLClickHandler.OnClick += InvokeFastPrint;
        PrintStatus = printStatus.Printing;

        while (SymbolsQueue.Count != 0)
        {
   
        string currentSymbol = SymbolsQueue.Dequeue();

        switch (currentSymbol)
        {
            case string VNLTag when IsVNLTag(VNLTag):

                var VNLTagInfo = GetVNLTagInfo(VNLTag);

                switch (VNLTagInfo.TagName)
                {
                    case "Wait":
                        if (VNLTagInfo.IsOpener)
                        {
                            _VNLClickHandler.OnClick -= InvokeFastPrint;
                            yield return new WaitForEventYieldInstruction(_VNLClickHandler, false);
                        }
                        break;

                    case "Delay":
                        if (VNLTagInfo.IsOpener)
                        {
                            yield return new WaitForSecondsRealtime(Convert.ToInt32(VNLTagInfo.TagParameter)/1000f);
                        }
                        break;

                    case "SPS":
                        currentSymbolsPerSecond = VNLTagInfo.IsOpener ? 
                        Convert.ToInt32(VNLTagInfo.TagParameter) : 
                        this.SymbolsPerSecond;
                    break;
                        
                    default:
                        Debug.LogError($"Tag \"{VNLTagInfo.TagName}\" is not exist in current context");
                        PrintSymbol(currentSymbol);
                        break;
                }
                yield return new WaitForSecondsRealtime(1f/currentSymbolsPerSecond);
                break;

            case string VNLStyleTag when IsVNLStyleTag(VNLStyleTag):
                var VNLStyleTagInfo = GetVNLStyleTagInfo(VNLStyleTag);
        
                if (VNLStyleTagInfo.IsOpener) 
                    vnlTextStyles.Add(VNLStyleTagInfo.StyleTagName, VNLStyleTagInfo.TagParameters);
                else 
                    vnlTextStyles.Remove(VNLStyleTagInfo.StyleTagName);
                yield return new WaitForSecondsRealtime(1f/currentSymbolsPerSecond);
                break;

            default:
                _VNLClickHandler.OnClick += InvokeFastPrint;
                PrintSymbol(currentSymbol);
                break;
        }
        yield return new WaitForSecondsRealtime(1f/currentSymbolsPerSecond); 

        } //окончание while

        _VNLClickHandler.OnClick -= InvokeFastPrint;
        PrintStatus = printStatus.Printed;
        yield break;
    }

    //вспомогательный метод. Выводит символ непосредственно в текстовое поле
    void PrintSymbol(string text)
    {
        dialogueWindow.text += vnlTextStyles.ApplyAddedStyles(text);
    }

    //метод, который запустит метод мгновенного вывода фрагмента текста
    void InvokeFastPrint()
    {
        _VNLClickHandler.OnClick -= InvokeFastPrint;
        FastPrint();
    }

    //мгновенный вывод фрагмента текста
    void FastPrint()
    {
        StringBuilder PrintableText = new StringBuilder();

        while (SymbolsQueue.Count != 0)
        {
            var CurrentSymbol = SymbolsQueue.Peek();
            
            switch (CurrentSymbol)
            {
                case string VNLTag when IsVNLTag(VNLTag):
                    var VNLTagInfo = GetVNLTagInfo(VNLTag);
                    if (VNLTagInfo.TagName.Equals("Wait") || VNLTagInfo.TagName.Equals("Delay"))
                    {
                        dialogueWindow.text += PrintableText.ToString();
                        return;
                    }
                    else if (VNLTagInfo.TagName.Equals("SPS"))
                    {
                        currentSymbolsPerSecond = VNLTagInfo.IsOpener ? 
                        Convert.ToInt32(VNLTagInfo.TagParameter) : 
                        this.SymbolsPerSecond;
                        SymbolsQueue.Dequeue();
                    }
                    else
                    {
                        PrintableText.Append(SymbolsQueue.Dequeue());
                    }
                    break;
                
                case string VNLStyleTag when IsVNLStyleTag(VNLStyleTag): 
                    var VNLStyleTagInfo = GetVNLStyleTagInfo(VNLStyleTag);
                    SymbolsQueue.Dequeue();

                    if (VNLStyleTagInfo.IsOpener) 
                        vnlTextStyles.Add(VNLStyleTagInfo.StyleTagName, VNLStyleTagInfo.TagParameters);
                    else 
                        vnlTextStyles.Remove(VNLStyleTagInfo.StyleTagName);
                    break;

                default:
                    PrintableText.Append(vnlTextStyles.ApplyAddedStyles(SymbolsQueue.Dequeue())); 
                    break;
            }
        }

        dialogueWindow.text += PrintableText.ToString();
    }


    //Получение информации о теге VNL
    (string TagName, bool IsOpener, object TagParameter) GetVNLTagInfo(string VNLTag)
    {
        string TagName = Regex.Match(VNLTag, @"(?<=(VNL *))\w+").Value;
        bool IsOpener = !Regex.IsMatch(VNLTag, @"^ *< */ *VNL *\w+ *> *$");
        string RawTagParameter = Regex.Match(VNLTag, @"(?<=\= *)((\d+)|""(\S+)"")").Value;

        object TagParameter = null;
        if (!String.IsNullOrEmpty(RawTagParameter) && Regex.IsMatch(RawTagParameter, @"""[^"", !]+"""))
        {
            TagParameter = Regex.Replace(RawTagParameter, @"""", @"");
        }
        else if (!String.IsNullOrEmpty(RawTagParameter))
        {
            TagParameter = Convert.ToInt32(RawTagParameter);
        }

        //если тег закрывающий, TagParameter вернет null
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
            MatchCollection MatchParameters = Regex.Matches(VNLStyleTag, @"(?<= *\( *| *, *)(\d+|(""[^"", !]+""))(?= *\) *| *, *)");
            foreach (Match MatchParameter in MatchParameters)
            {
                if (Regex.IsMatch(MatchParameter.Value, @"""[^"", !]+"""))
                {
                    TagParameters.Add(Regex.Replace(MatchParameter.Value, @"""", @""));
                }
                else
                {
                    TagParameters.Add(Convert.ToInt32(MatchParameter.Value));
                }
            } 
            //TagParameters.Add(MatchParameter.Value);
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