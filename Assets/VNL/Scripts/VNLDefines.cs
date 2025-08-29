using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using System;

public class VNLDefines : MonoBehaviour
{
    public Dictionary<string, TextAsset> VNLScripts {get; private set;}
    [SerializeField] private TextAsset[] vnlScripts;

    public Dictionary<string, PlayableDirector> VNLDirectors {get; private set;}
    [SerializeField] private PlayableDirector[] vnlDirectors;

    public Dictionary<string, (string localName, string style)> Characters {get; private set;}
    [SerializeField] private TextAsset characters;
    

    void GetVNLScripts()
    {
        foreach(var VNLScript in vnlScripts)
        {
            try
            {
                VNLScripts.Add(VNLScript.name, VNLScript);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("The second file with same name has found. Plesae set name to unique in the TextAsset array.");
            }
            
        }
    }

    void GetVNLDirectors()
    {
        foreach(var VNLDirector in vnlDirectors)
        {
            try
            {
                VNLDirectors.Add(VNLDirector.name, VNLDirector);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("The second file with same name has found. Plesae set name to unique in the PlayableDirector array.");
            }
            
        }
    }

    void GetCharacters()
    {
        if (characters.name.Equals("Characters", StringComparison.OrdinalIgnoreCase))
        {
            string[] Characters = this.characters.text.Split('\n');

            foreach (var character in Characters)
            {
                string[] Character = character.Split('~');
                
                try
                {
                    this.Characters.Add(Character[0], (Character[1], Character[2]));
                }
                catch (ArgumentException)
                {
                    Debug.LogWarning("The second Character with same name has found. Plesae set name to unique in the Character.txt.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Please set name of txt file to \"Characters\".");
        }
        
    }
}
