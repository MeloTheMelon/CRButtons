using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public string character;

    UIGenerator ug;
    void Start()
    {
        ug = GameObject.FindGameObjectWithTag("UIGenerator").GetComponent<UIGenerator>();
    }

    public void OnAudioButtonClick()
    {
        ug.stopAudioPlaying(); //Stop any sound playing
        ug.playAudio(this.GetComponentInChildren<Text>().text, character); //Play this buttons sound
        
    }

    public void OnMenuButtonClick()
    {
        if(character == "1Favorite")
        {
            ug.generateFavoritesMenu();
        }
        else
        {
            ug.generateButtons(character);
        }
    }

}
