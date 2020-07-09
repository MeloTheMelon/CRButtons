using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FavorButton : MonoBehaviour
{

    public Sprite starEmpty, starFull; //Favor icon, filled and not filled

    private bool isFavored = false;

    private string characterName;
    private string voiceLine;

    private UIGenerator uig;


    private void Start()
    {
        characterName = this.gameObject.transform.parent.GetComponent<Image>().sprite.name; //Get the character name
        voiceLine = this.gameObject.transform.parent.gameObject.GetComponentInChildren<Text>().text; //Get the voice line 
        uig = GameObject.FindGameObjectsWithTag("UIGenerator")[0].GetComponent<UIGenerator>(); //Get a reference to the UI generator
        isFavored = uig.checkIfFavorite(characterName, voiceLine); //Check if the voice line is favored or not
        if(isFavored)
            this.GetComponent<Button>().GetComponent<Image>().sprite = starFull; //Set the favored sprite accordingly
    }

    /// <summary>
    /// Toggle favored and update the icon
    /// </summary>
    public void onFavorButtonClick()
    {
        isFavored = !isFavored;
        updateIcon();
    }

    /// <summary>
    /// Set the favored status and update the icon
    /// </summary>
    /// <param name="setFavored"></param>
    public void setFavorBool(bool setFavored)
    {
        if (isFavored != setFavored)
        {
            isFavored = setFavored;
            updateIcon();
        }
    }

    /// <summary>
    /// Sets the favored icon accordingly
    /// </summary>
    private void updateIcon()
    {
        if (isFavored)
        {
            this.GetComponent<Button>().GetComponent<Image>().sprite = starFull;
            uig.editFavoriteSoundsData(characterName, voiceLine, true);
        }
        else
        {
            this.GetComponent<Button>().GetComponent<Image>().sprite = starEmpty;
            uig.editFavoriteSoundsData(characterName, voiceLine, false);
        }
    }



}
