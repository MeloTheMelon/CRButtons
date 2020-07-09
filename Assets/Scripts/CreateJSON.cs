using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CreateJSON : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            saveData();

        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            loadDataToDict();
        }
    }

    /// <summary>
    /// Read in the clips data
    /// </summary>
    private void loadDataToDict()
    {
        string path = "Assets/Resources/clips.txt";
        string content = System.IO.File.ReadAllText(path);
        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        string[] temp = content.Split('\n');
        foreach(string s in temp)
        {
            if (s.Length > 1)
            {
                string key = s.Split(':')[0];
                dict.Add(key, new List<string>());
                foreach (string val in s.Split(':')[1].Split('§'))
                {
                    dict[key].Add(val);
                }
            }
        }
    }

    /// <summary>
    /// Generate the txt file for the clips dictionary
    /// Only needed one time if new clips are added.
    /// </summary>
    private void saveData()
    {
        string path = "Assets/Resources/clips.txt";
        File.Delete(path);
        string temp = "";
        Sprite[] sprites = Resources.LoadAll<Sprite>("ButtonBackgrounds");
        foreach (Sprite s in sprites)
        {
            if (s.name != "1Favorite")
            {
                temp += s.name + ":";
                AudioClip[] audioFiles = Resources.LoadAll<AudioClip>("Audio/" + s.name);
                foreach (AudioClip a in audioFiles)
                {
                    temp += a.name + "§";
                }
                temp = temp.Remove(temp.Length - 1);
                temp += "\n";
            }
        }
        temp = temp.Remove(temp.Length - 1);

        StreamWriter writer = new StreamWriter(path, true);
        writer.Write(temp);
        writer.Close();
    }
}
