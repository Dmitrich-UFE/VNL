using UnityEngine;
using TMPro;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class VNLPrinter : MonoBehaviour
{
    public enum printStatus {Printing, Printed}
    public VNLTextStyles vnlTextStyles;
    private Queue<string> SymbolsQueue;
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
        SymbolsQueue = new Queue<string>();
        currentSymbolsPerSecond = SymbolsPerSecond;
    }
    
    void Start()
    {
        Print(dialogueWindow.text);
    }

    //Основной метод посимвольной печати строки. 0-мгновенная печать
    //разбивает строку на символы и теги и запускает вспомогательный метод
    public void Print(string Sentence)
    {
        //проверки данных
        if (dialogueWindow == null) { Debug.LogError("Component \"VNLPrinter.dialogueWindow\" is null"); return; }
        if (Sentence == null) { Debug.LogError("String data is null"); return; }
        if (Regex.IsMatch(Sentence, @"^ *$")) { Debug.LogWarning("String is empty: just spaces"); }

        //основная часть
        Clear();

        if (SymbolsPerSecond == 0) { dialogueWindow.text = Sentence; }
        else
        {
            string pattern = @"(<[^>]+>|.)";
            MatchCollection Symbols = Regex.Matches(Sentence, pattern);

            foreach (Match Symbol in Symbols)
            {
                SymbolsQueue.Enqueue(Symbol.Value);
            }
            Symbols = null;

            InvokeRepeating("Printing", 0f, 1f/SymbolsPerSecond);
            PrintStatus = printStatus.Printing;
        }
    }

    //вспомогательный метод для печати строки
    //выводит нарезанные фрагменты текста на экран
    void Printing()
    {
        //условие завершения вывода
        if (SymbolsQueue.Count == 0) 
        {
            CancelInvoke("Printing");  
            PrintStatus = printStatus.Printed;
            return; 
        }

        //исследование тегов
        switch (SymbolsQueue.Peek())
        {
            //ожидание клика
            case string WaitTag when Regex.IsMatch(WaitTag, @"^ *< *wait *> *$", RegexOptions.IgnoreCase):
                CancelInvoke("Printing"); 
                SymbolsQueue.Dequeue();
                _VNLClickHandler.OnClick += Continue;
                break;

            //задержка вывода
            case string DelayTag when Regex.IsMatch(DelayTag, @"^ *< *delay *= *[^+-]\d+ *> *$", RegexOptions.IgnoreCase):
                CancelInvoke("Printing"); 
                SymbolsQueue.Dequeue();
                Delay(Convert.ToUInt32(Regex.Match(DelayTag, @"(?<=\=) *\d+").Value));
                break;

            //печать с определенным количеством символов в секунду
            case string CurrentSymbolsPerSecondTag when Regex.IsMatch(CurrentSymbolsPerSecondTag, @"^ *< */? *sps *(= *[^+-]\d+ *)?> *$", RegexOptions.IgnoreCase): 
                if (Regex.IsMatch(CurrentSymbolsPerSecondTag, @"^ *< *sps *(= *[^+-]\d+ *){1}> *$", RegexOptions.IgnoreCase))
                {
                    SymbolsQueue.Dequeue();
                    CancelInvoke("Printing");    
                    CurrentSPSPrint(Convert.ToUInt32(Regex.Match(CurrentSymbolsPerSecondTag, @"(?<=\=) *\d+").Value));
                }
                else if (Regex.IsMatch(CurrentSymbolsPerSecondTag, @"^ *< */? *sps *> *$", RegexOptions.IgnoreCase))
                {
                    SymbolsQueue.Dequeue();
                    CancelInvoke("Printing");
                    CurrentSPSPrint(Convert.ToUInt32(SymbolsPerSecond));
                }
                else 
                    goto default;

                break;

            //стиль: RandomLetterSize
            case string VNLStyleTagRandomLetterSize when Regex.IsMatch(VNLStyleTagRandomLetterSize, @"^ *< */? *VNLStyle *RandomLetterSize *(\( *\d+ *, *\d+ *\))? *> *$", RegexOptions.IgnoreCase):
                //Debug.Log("я застрял");
                //открывающий тег
                if ((Regex.IsMatch(VNLStyleTagRandomLetterSize, @"^ *< *VNLStyle *RandomLetterSize\(\d+, * \d+ *\) *> *$", RegexOptions.IgnoreCase)))
                {
                    SymbolsQueue.Dequeue();
                    MatchCollection MatchParameters = Regex.Matches(VNLStyleTagRandomLetterSize, @"(\d+)");
                    List<object> ParametersList = new List<object>();
                    foreach (Match m in MatchParameters) ParametersList.Add(Convert.ToInt32(m.Value));
                    vnlTextStyles.Add("RandomLetterSize", ParametersList);
                    MatchParameters = null;
                }
                //закрывающий тег
                else if ((Regex.IsMatch(VNLStyleTagRandomLetterSize, @"^ *< */ *VNLStyle *RandomLetterSize *> *$", RegexOptions.IgnoreCase)))
                {
                    SymbolsQueue.Dequeue();
                    vnlTextStyles.Remove("RandomLetterSize");                   
                }
                //если тег не совпал с паттернами(например, </VNLStyle RandomLetterSize(2, 55)>)
                else
                {
                    goto default;
                }

                break;

                
            //другие теги:
            //оформляются подобным образом. Если нужен клик для срабатывания тега, то обязательно подписываемся на событие _VNLClickHandler.OnClick
            
            //если кастомные теги не обнаружены, то просто выводим текст со стандартными тегами
            default: dialogueWindow.text += vnlTextStyles.ApplyAddedStyles(SymbolsQueue.Dequeue()); break;
        }
    }

    //метод для очистки данных перед выводом следующей строки
    void Clear()
    {
        dialogueWindow.text = "";
        SymbolsQueue.Clear();
    }

    //вызов продолжения вывода текста
    void Continue()
    {
        _VNLClickHandler.OnClick -= Continue;
        InvokeRepeating("Printing", 0f, 1f/currentSymbolsPerSecond);
    }

    //задержка текста в миллисекундах
    void Delay(uint milliseconds)
    {
        InvokeRepeating("Printing", milliseconds/1000f, 1f/currentSymbolsPerSecond);
    }

    //вывод текста со скоростью не по умолчанию
    void CurrentSPSPrint(uint symbolsPerSecond)
    {
        currentSymbolsPerSecond = Convert.ToInt32(symbolsPerSecond);
        InvokeRepeating("Printing", 0f, 1f/symbolsPerSecond);
    }
}