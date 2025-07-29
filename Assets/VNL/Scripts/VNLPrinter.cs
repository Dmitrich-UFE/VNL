using UnityEngine;
using TMPro;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class VNLPrinter : MonoBehaviour
{
    
    public enum printStatus {Printing, Printed}
    private Queue<string> SymbolsQueue;

    //текстовое окно
    [SerializeField] private TMP_Text dialogueWindow;
    [SerializeField] private string str;
    public printStatus PrintStatus {get; private set;}

    void Awake()
    {
        SymbolsQueue = new Queue<string>();
    }
    
    void Start()
    {
        Print(str, 40);
    }

    //Основной метод посимвольной печати строки. 0-мгновенная печать
    public void Print(string Sentence, ushort SymbolsPerSecond)
    {
        //проверки данных
        if (dialogueWindow == null) { Debug.LogError("Component \"VNLPrinter.dialogueWindow\" is null"); return; }
        if (Sentence == null) { Debug.LogError("String data is null"); return; }
        if (Regex.IsMatch(Sentence, @"^ *$")) { Debug.LogWarning("String is empty: just spaces"); }

        //основная часть
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

    void Printing()
    {
        if (SymbolsQueue.Count == 0) 
        {
            CancelInvoke("Printing");  
            PrintStatus = printStatus.Printed;
            return; 
        }
        dialogueWindow.text += SymbolsQueue.Dequeue();
    }

}
