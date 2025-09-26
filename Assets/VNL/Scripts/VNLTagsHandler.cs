using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

public class VNLTagsHandler : MonoBehaviour, IVNLTagsHandler
{
    //Проверки тега на соответствие шаблонам VNL. Бракует большинство неверно написанных тегов, но теги с неправильным названием и/или параметрами пропускает
    //Проверка: это тег VNL?
    public bool IsVNLTag(string Tag)
    {
        return Regex.IsMatch(Tag, @"^ *< *VNL *\w+ *(= *\S+)? *> *$") ^ Regex.IsMatch(Tag, @"^ *< *\/ *VNL *\w+ *> *$");
    }

    //Проверка: это тег VNLStyle?
    public bool IsVNLStyleTag(string Tag)
    {
        return Regex.IsMatch(Tag, @"^ *< *VNLStyle *\w+ *\(( *\S+ *)*\) *> *$") ^ Regex.IsMatch(Tag, @"^ *< */ *VNLStyle *\w+ *> *$");
    }

    //Получение информации о теге VNL
    public (string TagName, bool IsOpener, object TagParameter) GetVNLTagInfo(string VNLTag)
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
    public (string StyleTagName, bool IsOpener, List<object> TagParameters) GetVNLStyleTagInfo(string VNLStyleTag)
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
}
