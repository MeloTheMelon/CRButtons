
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{
    public float speed;

    public GameObject menuButtonTemplate;
    public GameObject audioButtonTemplate;

    public GameObject returnButton;

    public bool useGenericDesign;

    private AudioSource currentlyPlaying;
    private List<GameObject> shownObjects = new List<GameObject>();

    private int buttonWidth;
    private int buttonHeight;

    private Sprite genericButton;

    private Vector2 firstButtonPos;
    private int buttonsPerRow;
    private int dividerSize;

    private string currentMenu = "main";
    private bool isChangingScene = false;
    private bool returnButtonCooldown = false;

    private bool firstScroll = true;

    private Vector2 scrollStart;
    private Vector3 scrollStartMouse;

    private DeviceOrientation currentOrientation;
    private Vector2 currentScreen;

    private Dictionary<string, List<string>> audioDict;

    //Simple Dict to generate the text above each character menu
    private IDictionary<string, string> actorNames = new Dictionary<string, string>()
                                            {
                                                {"Beau","Beauregard - Marisha Ray"},
                                                {"Caduceus","Caduceus - Taliesin Jaffe"},
                                                {"Caleb","Caleb - Liam O'Brien"},
                                                {"Fjord","Fjord - Travis Willingham"},
                                                {"Grog","Grog - Travis Willingham"},
                                                {"Jester","Jester - Laura Bailey"},
                                                {"Keyleth","Keyleth - Marisha Ray"},
                                                {"Kiri","Kiri - Matthew Mercer"},
                                                {"Matt","Matthew Mercer"},
                                                {"Misc","Miscellaneous"},
                                                {"Mollymauk","Mollymauk - Taliesin Jaffe"},
                                                {"Nott","Nott - Sam Riegel"},
                                                {"Percy","Percy - Taliesin Jaffe"},
                                                {"Pike","Pike - Ashley Johnson"},
                                                {"PumatSol","Pumat Sol - Matthew Mercer"},
                                                {"Scanlan","Scanlan - Sam Riegel"},
                                                {"Suntree","Suntree - Matthew Mercer"},
                                                {"Taryon", "Taryon Darrington - Sam Riegel"},
                                                {"Vax","Vax'ildan - Liam O'Brien"},
                                                {"Vex","Vex'ahlia - Laura Bailey"},
                                                {"Victor","Victor - Matthew Mercer"},
                                                {"Yasha","Yasha - Ashely Johnson"},
                                                {"1Favorite","Favorites"}
                                            };

    public List<string[]> favoriteSounds;

    // Start is called before the first frame update
    void Start()
    {
        //Screen.fullScreen = false;

        audioDict = getAudioDict();
        if (!PlayerPrefs.HasKey("fav"))
            PlayerPrefs.SetString("fav", "");
        favoriteSounds = loadFavorData();
        
        genericButton = Resources.Load<Sprite>("Basic");

        currentOrientation = Input.deviceOrientation;
        currentScreen = new Vector2(Screen.width, Screen.height);
        buttonWidth = (int) menuButtonTemplate.GetComponent<RectTransform>().rect.width;
        buttonHeight = (int)menuButtonTemplate.GetComponent<RectTransform>().rect.height;

        buttonsPerRow = (Screen.width - 50) / (buttonWidth + 50);   //Calculate how many buttons per row
        dividerSize = (Screen.width - buttonsPerRow * buttonWidth) / (buttonsPerRow + 1); //Calculate the size of the space between buttons
        firstButtonPos.x = (int) ((-Screen.width / 2) + dividerSize + (buttonWidth / 2)); //First Button X Positon
        firstButtonPos.y = (int) ((Screen.height / 2) - dividerSize - (buttonHeight / 2) - ((int) returnButton.GetComponent<RectTransform>().rect.height)); // First Y Position 
        generateMenu();
    }

    // Update is called once per frame
    void Update()
    {
        //Re-Adjust to different screen size, or if screen switches from portrait to landscape
        if(currentScreen.x != Screen.width || currentScreen.y != Screen.height || currentOrientation != Input.deviceOrientation)
        {
            currentScreen = new Vector2(Screen.width, Screen.height);
            currentOrientation = Input.deviceOrientation;
            reloadMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && currentMenu != "main" && !isChangingScene && !returnButtonCooldown)
        {
            StartCoroutine("returnButtonCooldownTimer"); //Short cooldown fixes a problem with registering 2 button presses if there is only one.
            clearMenu();    //Reset the Menu
            generateMenu(); //Load the new Menu
        }else if (Input.GetKeyDown(KeyCode.Escape) && currentMenu == "main" && !isChangingScene && !returnButtonCooldown)
        {
            //If return button gets pressed in the main menu, the application closes.
            Application.Quit(); 
        }

        scrolling();    //Touch Scrolling
        mouseScrolling(); //Mobile Scrolling
    }

    /// <summary>Calculate the position of the next button, depending on the last placed button</summary>
    private Vector2 getNextButtonPosition(Vector2 lastButtonPosition)
    {
        
        Vector2 newPos = lastButtonPosition;
        if (lastButtonPosition.x == 1.337f)
        {
            newPos = firstButtonPos;
        }
        else
        {
            newPos.x += dividerSize + buttonWidth;
            if (newPos.x >= Screen.width / 2)
            {
                newPos.x = firstButtonPos.x;
                newPos.y -= dividerSize + buttonHeight;
            }
        }
        return newPos;
    }

    /// <summary>
    /// Play a specific audio line, depending on name of the audio file and which character
    /// </summary>
    public void playAudio(string name, string characterName)
    {
        if (currentMenu != "main")
        {
            AudioClip ac = Resources.Load<AudioClip>("Audio/" + characterName + "/" + name.Replace("\r",""));
            this.gameObject.GetComponent<AudioSource>().clip = ac;
            this.gameObject.GetComponent<AudioSource>().Play();
        }

    }

    /// <summary>
    /// Generates the main menu 
    /// </summary>
    public void generateMenu()
    {
        isChangingScene = true;
        currentMenu = "main"; //Set the current Menu string, for reloading the scene
        returnButton.SetActive(false); //Hide return button
        Sprite[] sprites = Resources.LoadAll<Sprite>("ButtonBackgrounds"); //Load all backgrounds
        Vector2 lastButtonPos = new Vector2(1.337f, 0); //Init so it starts with the first position
        foreach (Sprite s in sprites)
        {
            GameObject temp = Instantiate(menuButtonTemplate); //Create Button
            if (useGenericDesign)
            {
                temp.GetComponent<Image>().sprite = genericButton;
            }
            else {
                temp.GetComponent<Image>().sprite = s; //Set Background Image
            }
            temp.GetComponent<ButtonScript>().character = s.name; //Set Character-Name for button load function

            string buttonText = "";
            if(s.name == "1Favorite")
            {
                buttonText = "Favorite";
                temp.GetComponent<Image>().sprite = s;
            }else if (s.name == "PumatSol")
            {
                buttonText = "Pumat Sol";
            }
            else
            {
                buttonText = s.name;
            }

            temp.GetComponentInChildren<Text>().text = buttonText; //Set Text for Menu Button
            temp.transform.SetParent(this.transform); //Add to canvas
            shownObjects.Add(temp); //Add to currently showing list
            lastButtonPos = getNextButtonPosition(lastButtonPos); //Get the right position
            temp.transform.localPosition = lastButtonPos; //Position correctly
            isChangingScene = false;
        }

    }

    /// <summary>
    /// Generate a button based on the character-name, texture depends on the name
    /// </summary>
    public void generateButtons(string characterName)
    {
        isChangingScene = true;
        clearMenu(); //Remove all menu buttons
        currentMenu = characterName; //Set the menu string for reloading the menu
        returnButton.GetComponentInChildren<Text>().text = actorNames[characterName];
        returnButton.SetActive(true); //Show return button
        Sprite buttonSprite = Resources.Load<Sprite>("ButtonBackgrounds/"+characterName); //Load Button Background
        if (useGenericDesign)
            buttonSprite = genericButton;
        Vector2 lastButtonPos = new Vector2(1.337f, 0); //Init so it starts with the first position
        foreach(string a in audioDict[characterName])
        {
            GameObject temp = Instantiate(audioButtonTemplate); //Create Button
            temp.GetComponent<Image>().sprite = buttonSprite; //set Background Image 
            temp.GetComponentInChildren<Text>().text = a; //Set Description
            temp.GetComponent<ButtonScript>().character = characterName; //Set Character-Name for button load function
            //temp.GetComponent<AudioSource>().clip = a; //Set Audio File
            temp.transform.SetParent(this.transform); //Add to canvas
            shownObjects.Add(temp); //Add to currently showing list
            lastButtonPos = getNextButtonPosition(lastButtonPos); //Get the right position for the button
            temp.transform.localPosition = lastButtonPos; //Place it at the right position
        }
        isChangingScene = false;

    }

    /// <summary>
    /// Generate the favorites Menu, based on the buttons marked as favorites
    /// </summary>
    public void generateFavoritesMenu()
    {
        string characterName = "1Favorite";
        clearMenu(); //Remove all menu buttons
        currentMenu = characterName; //Set the menu string for reloading the menu
        returnButton.GetComponentInChildren<Text>().text = actorNames[characterName];
        returnButton.SetActive(true); //Show return button
        Sprite buttonSprite = Resources.Load<Sprite>("ButtonBackgrounds/" + characterName); //Load Button Background
        Vector2 lastButtonPos = new Vector2(1.337f, 0); //Init so it starts with the first position

        foreach(string[] s in favoriteSounds)
        {
            GameObject temp = Instantiate(audioButtonTemplate); //Create Button

            if (useGenericDesign)
                temp.GetComponent<Image>().sprite = genericButton;
            else
                temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("ButtonBackgrounds/" + s[0]); //set Background Image 

            temp.GetComponent<ButtonScript>().character = s[0]; //Set Character-Name for button load function
            temp.GetComponentInChildren<Text>().text = s[1]; //Set Description
            temp.transform.SetParent(this.transform); //Add to canvas
            shownObjects.Add(temp); //Add to currently showing list
            lastButtonPos = getNextButtonPosition(lastButtonPos); //Get the right position for the button
            temp.transform.localPosition = lastButtonPos; //Place it at the right position
        }

    }

    /// <summary>
    /// Move the Menus up and down by swiping on the touch screen.
    /// Basic mobile scrolling.
    /// </summary>
    public void scrolling()
    {
        
        if(Input.touchCount > 0)
        {
            //Check if there is a touch input
            if(Input.touches[0].phase == TouchPhase.Began)
            {
                scrollStart = Input.touches[0].position;
            }else if(Input.touches[0].phase == TouchPhase.Moved)
            {
                if (!firstScroll)
                {
                    //Calculate the difference in the scroll
                    Vector2 dif = Input.touches[0].position - scrollStart;
                    Vector2 moveBy = new Vector2(0, (dif.y * speed * Time.deltaTime));
                    //Check if you can scroll in that direction at this point
                    if ((moveBy.y < 0 && this.transform.localPosition.y >= 0) || (moveBy.y > 0 && this.transform.position.y + (shownObjects[shownObjects.Count - 1].transform.localPosition.y - dividerSize - (buttonHeight / 2)) < 0))
                        //Move the list
                        this.transform.Translate(new Vector2 (0, Math.Min(moveBy.y,30)));
                }
                else
                {
                    firstScroll = false;
                }
                //Update the last scroll position
                scrollStart = Input.touches[0].position;
            }
        }
    }

    /// <summary>
    /// Same as scrolling but for the mouse instead of touch.
    /// Only used for Debugging.
    /// </summary>
    public void mouseScrolling()
    {
        if (Input.GetMouseButtonDown(0))
        {
            scrollStartMouse = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            //Calculate the difference in the scroll
            Vector3 dif = Input.mousePosition - scrollStartMouse;
            Vector2 moveBy = new Vector2(0, (dif.y * speed * Time.deltaTime));
            //Check if you can scroll in that direction at this point
            if ((moveBy.y < 0 && this.transform.localPosition.y >= 0) || (moveBy.y > 0 && this.transform.position.y + (shownObjects[shownObjects.Count - 1].transform.localPosition.y - dividerSize - (buttonHeight / 2)) < 0))
                //Move the list
                this.transform.Translate(moveBy);
            //Update the last scroll position
            scrollStartMouse = Input.mousePosition;
        }
    }

    /// <summary>
    /// Delete the currently shown menu.
    /// </summary>
    public void clearMenu()
    {
        this.gameObject.GetComponent<AudioSource>().Stop();
        //Destroy all buttons that are currently shown
        foreach(GameObject o in shownObjects)
        {
            Destroy(o);
        }
        //Reset the button list
        shownObjects = new List<GameObject>();
        //Reset the button list position
        this.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Reload the currently shown menu.
    /// Do this after screen resizes.
    /// </summary>
    public void reloadMenu()
    {
        buttonsPerRow = (Screen.width - 50) / (buttonWidth + 50);   //Calculate how many buttons per row
        dividerSize = (Screen.width - buttonsPerRow * buttonWidth) / (buttonsPerRow + 1); //Calculate the size of the space between buttons
        firstButtonPos.x = (int)((-Screen.width / 2) + dividerSize + (buttonWidth / 2)); //First Button X Positon
        firstButtonPos.y = (int)((Screen.height / 2) - dividerSize - (buttonHeight / 2) - ((int)returnButton.GetComponent<RectTransform>().rect.height)); // First Y Position 

        clearMenu();

        if (currentMenu == "main")
        {
            generateMenu();
        }
        else
        {
            generateButtons(currentMenu);
        }

    }

    public void setCurrentlyPlayingAudiosource(AudioSource audioS)
    {
        //Set the currently playing audiosource
        currentlyPlaying = audioS;
    }
    
    public void stopAudioPlaying()
    {
        //Stop audio from playing
        if(currentlyPlaying != null)
        currentlyPlaying.Stop();
    }

    public void onReturnButtonClick()
    {
        //Return back to main menu
        clearMenu();
        generateMenu();
    }

    /// <summary>
    /// Reads in the clips file.
    /// Saves resources over reading and searching through the audio clips when creating character menues
    /// </summary>
    /// <returns>A Dictionairy with character names as key and the names of the character clips as values</returns>
    public Dictionary<string, List<string>> getAudioDict()
    {
        string path = "clips";
        string content = (Resources.Load(path) as TextAsset).text;
        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        string[] temp = content.Split('\n');
        foreach (string s in temp){
            if (s.Length > 1){
                string key = s.Split(':')[0];
                dict.Add(key, new List<string>());
                foreach (string val in s.Split(':')[1].Split('§')){
                    dict[key].Add(val);
                }
            }
        }
        return dict;
    }

    /// <summary>
    /// Loads the favorite data.
    /// </summary>
    /// <returns>Creates a list of string tuples with consisting of character name and clip name</returns>
    private List<string[]> loadFavorData()
    {
        List<string[]> favorTuples = new List<string[]>();
        string content = PlayerPrefs.GetString("fav");
        content = content.Replace("\r", "");
        if (content.Length > 1)
        {
            string[] temp = content.Split('\n');
            foreach (string s in temp)
            {
                string[] tupel = s.Split('§');
                favorTuples.Add(tupel);
            }
        }
        return favorTuples;
    }
        
    /// <summary>
    /// Add or remove a specific sound name from the favorites list
    /// </summary>
    /// <param name="addOrRemove">true for adding and false for removing</param>
    public void editFavoriteSoundsData(string characterName, string soundName, bool addOrRemove)
    {
        
        if (addOrRemove)
        {
            favoriteSounds.Add(new string[] {characterName, soundName});
        }
        else
        {
            favoriteSounds.RemoveAll(s => s[0] == characterName && s[1] == soundName);
        }

        saveFavoriteSoundData();

    }

    /// <summary>
    /// Saves the favorite sounds list in PlayerPrefs
    /// </summary>
    private void saveFavoriteSoundData()
    {
        string content = "";
        
        foreach(string[] s in favoriteSounds)
        {
            content += s[0] + "§" + s[1] + "\n";
        }
        content = content.Remove(content.Length - 1);

        PlayerPrefs.SetString("fav", content);
    }

    /// <summary>
    /// Checks if a sound is favored or not. Needed to set the favored star icon on or off
    /// </summary>
    /// <returns>True if favored, false if not</returns>
    public bool checkIfFavorite(string characterName, string soundName)
    {
        return favoriteSounds.Any(p => p[0] == characterName && p[1] == soundName);
    }

    /// <summary>
    /// Short 0.5f second cooldown to fix a bug that caused the app to close if the return button was clicked twice quickly
    /// </summary>
    IEnumerator returnButtonCooldownTimer()
    {
        returnButtonCooldown = true;
        yield return new WaitForSeconds(0.5f);
        returnButtonCooldown = false;
    }
}
