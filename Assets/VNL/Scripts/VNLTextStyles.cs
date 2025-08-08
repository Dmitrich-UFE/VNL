using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

public class VNLTextStyles : MonoBehaviour
{
    private Dictionary<string, List<object>> VNLStyles;
    private System.Random rand;

    void Awake()
    {
        VNLStyles = new Dictionary<string, List<object>>();
        rand = new System.Random();
    }

    //принимает букву и возвращает букву размером <некоторое число из диапазона>
    string RandomLetterSize(string text, int minsize, int maxsize)
    {
        if (Regex.IsMatch(text, @"^ *< *.+ *> *$")) return text;
        return $"<size={rand.Next(minsize, maxsize)}px>{text}</size>";
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
        VNLStyles[DelegateName].Insert(0, "text");
    }

    //удаляет применяемый стиль
    public void Remove(string DelegateName)
    {
        VNLStyles.Remove(DelegateName);
    }

    //применяет все записанные в словарь стили к подаваемому тексту
    public string ApplyAddedStyles(string text)
    {
        if (VNLStyles.Count == 0) return text;   

        foreach (var Style in VNLStyles)
        {
            try
            {
            MethodInfo delegateInfo = typeof(VNLTextStyles).GetMethod(Style.Key, BindingFlags.NonPublic | BindingFlags.Instance);
            Style.Value[0] = text;
            text = delegateInfo.Invoke(this, Style.Value.ToArray()) as string;
            }
            catch (NullReferenceException)
            {
                Debug.LogError($"Style {Style.Key} is not exist in current context");
                continue;
            }        
            catch (TargetParameterCountException)
            {
                Debug.LogError($"Style {Style.Key} cannot be used. There are too many or too few parameters?");
                continue;
            } 
            catch (ArgumentException)
            {
                Debug.LogError($"Style {Style.Key} cannot be used. Probably style parameter(s) in tag is incorrect?");
                continue;
            }
        }
        return text;
    }
}