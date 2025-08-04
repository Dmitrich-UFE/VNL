using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

public class VNLTextStyles : MonoBehaviour
{
    delegate string ApplyStyle(string text);
    delegate string ApplyStyleWithRange(string text, int min, int max);
    delegate string ApplyStyleWithRangeDouble(string text, float min, float max);
    private Dictionary<string, List<object>> VNLStyles;
    private System.Random rand;

    void Awake()
    {
        VNLStyles = new Dictionary<string, List<object>>();
        rand = new System.Random();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //добавляет применяемый стиль в словарь(имя делегата и параметры для него). 
    //Если применяемый стиль уже есть, то происходит перезапись параметров
    public void Add(string DelegateName, List<object> Parameters)
    {
        if (VNLStyles.ContainsKey(DelegateName))
        {
            VNLStyles.Remove(DelegateName);
        }
        VNLStyles.Add(DelegateName, Parameters);
    }

    //удаляет применяемый стиль
    public void Remove(string DelegateName)
    {
        VNLStyles.Remove(DelegateName);
    }

    //применяет все записанные в словарь стили к подаваемому тексту
    public string ApplyAddedStyles(string text)
    {
        if (VNLStyles == null) return text;

        foreach (var Style in VNLStyles)
        {
            MethodInfo delegateInfo = typeof(VNLTextStyles).GetMethod(Style.Key);
            text = delegateInfo.Invoke(this, Style.Value.ToArray()) as string;
        }
        return text;
    }

    string DefaultStyle(string text) { return text; } 

    //принимает букву и возвращает букву размером <некоторое число из диапазона>
    string RandomLetterSize(string text, int minsize, int maxsize)
    {
        if (Regex.IsMatch(text, @"^ *< *.+ *> *$")) return text;
        return $"<size={rand.Next(minsize, maxsize)}px>{text}</size>";
    }
}
