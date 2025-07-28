using UnityEngine;
using TMPro;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class VNLPrinter : MonoBehaviour
{
    //текстовое окно
    [SerializeField] private TMP_Text dialogueWindow;
    [SerializeField] private string str;
    public enum Status {Printing, Printed}
    
    
    void Start()
    {
        Print(str, 1);
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
            Queue<string> SymbolsQueue = new Queue<string>();

            string pattern = @"(<[^>]+>|.)";
            MatchCollection Symbols = Regex.Matches(Sentence, pattern);

            foreach (Match Symbol in Symbols)
            {
                SymbolsQueue.Enqueue(Symbol.Value);
            }
            Symbols = null;


            Invoke("Printing", 1/SymbolsPerSecond);

            

            void Printing()
            {
                if (SymbolsQueue.Peek() != null) 
                {
                     dialogueWindow.text += SymbolsQueue.Dequeue();
                     Invoke("Printing", 1/SymbolsPerSecond);
                }
            }





        }
    }
}
