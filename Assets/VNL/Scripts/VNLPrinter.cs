using UnityEngine;
using TMPro;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class VNLPrinter : MonoBehaviour
{
    public enum printStatus {Printing, Printed}
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
        Print(str);
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
            case string customTag when Regex.IsMatch(customTag, @"^ *< *wait *> *$", RegexOptions.IgnoreCase):
                CancelInvoke("Printing"); 
                SymbolsQueue.Dequeue();
                _VNLClickHandler.OnClick += Continue;
                break;

            //задержка вывода
            case string customTag2 when Regex.IsMatch(customTag2, @"^ *< *delay *= *[^+-]\d+ *> *$", RegexOptions.IgnoreCase):
                CancelInvoke("Printing"); 
                SymbolsQueue.Dequeue();
                Delay(Convert.ToUInt32(Regex.Match(customTag2, @"(?<=\=) *\d+").Value));
                break;

            //печать с определенным количеством символов в секунду
            case string customTag3 when Regex.IsMatch(customTag3, @"^ *< */? *sps *(= *[^+-]\d+ *)?> *$", RegexOptions.IgnoreCase):
                CancelInvoke("Printing"); 
                SymbolsQueue.Dequeue();

                if (Regex.IsMatch(customTag3, @"^ *< *sps *(= *[^+-]\d+ *)?> *", RegexOptions.IgnoreCase))
                    CurrentSPSPrint(Convert.ToUInt32(Regex.Match(customTag3, @"(?<=\=) *\d+").Value));

                if (Regex.IsMatch(customTag3, @" *< */? *sps *> *", RegexOptions.IgnoreCase))
                    CurrentSPSPrint(Convert.ToUInt32(SymbolsPerSecond));
                break;
                
            //другие теги:
            //оформляются подобным образом. Если нужен клик для срабатывания тега, то обязательно подписываемся на событие _VNLClickHandler.OnClick
            
            //если кастомные теги не обнаружены, то просто выводим текст со стандартными тегами
            default: dialogueWindow.text += SymbolsQueue.Dequeue(); break;
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