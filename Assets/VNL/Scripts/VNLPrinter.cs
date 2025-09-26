using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

public class VNLPrinter : MonoBehaviour, IPrinter
{
    [SerializeField] private IData VNLData;
    [SerializeField] private IVNLTagsHandler VNLTagsHandler;
    private IEnumerator yieldInstruction; 



    public void Print(string Sentence, TMP_Text Textbox)
    {

    }

    private IEnumerator Printing(Queue<string> SymbolsQueue)
    {
        while (SymbolsQueue.Count != 0)
        {
            var CurrentSymbol = SymbolsQueue.Dequeue();

            switch (CurrentSymbol)
            {
                case string VNLTag when VNLTagsHandler.IsVNLTag(VNLTag):
                    var VNLTagInfo = VNLTagsHandler.GetVNLTagInfo(VNLTag);
                    break;
                
                case string VNLStyleTag when VNLTagsHandler.IsVNLStyleTag(VNLStyleTag): 
                    var VNLStyleTagInfo = VNLTagsHandler.GetVNLStyleTagInfo(VNLStyleTag);
                    break;

                default:
                    break;
            }
            yield return yieldInstruction;
        }
    }


    //Анализирует строку, нарезает ее и возвращает очередь нарезанных подстрок
    Queue<string> GetSlicedSentence(string Sentence)
    {
        Queue<string> SymbolsQueue = new Queue<string>();
        MatchCollection Symbols = Regex.Matches(Sentence, @"(<[^>]+>|.)");

        foreach (Match Symbol in Symbols) { SymbolsQueue.Enqueue(Symbol.Value); }

        return SymbolsQueue;
    }
}