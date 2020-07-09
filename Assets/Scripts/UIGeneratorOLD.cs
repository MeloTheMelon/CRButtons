
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGeneratorOLD : MonoBehaviour
{
    public float speed;

    public GameObject menuButtonTemplate;
    public GameObject audioButtonTemplate;

    public GameObject returnButton;
    
    private AudioSource currentlyPlaying;
    private List<GameObject> shownObjects = new List<GameObject>();

    private int buttonWidth;
    private int buttonHeight;

    private Vector2 firstButtonPos;
    private int buttonsPerRow;
    private int dividerSize;

    private string currentMenu = "main";

    private Vector2 scrollStart;
    private Vector3 scrollStartMouse;

    private DeviceOrientation currentOrientation;
    private Vector2 currentScreen;

    // Start is called before the first frame update
    void Start()
    {
        currentOrientation = Input.deviceOrientation;
        currentScreen = new Vector2(Screen.width, Screen.height);
        buttonWidth = (int) menuButtonTemplate.GetComponent<RectTransform>().rect.width;
        buttonHeight = (int)menuButtonTemplate.GetComponent<RectTransform>().rect.height;

        buttonsPerRow = (Screen.width - 50) / (buttonWidth + 50);   //Calculate how many buttons per row
        dividerSize = (Screen.width - buttonsPerRow * buttonWidth) / (buttonsPerRow + 1); //Calculate the size of the space between buttons
        firstButtonPos.x = (int) ((-Screen.width / 2) + dividerSize + (buttonWidth / 2)); //First Button X Positon
        firstButtonPos.y = (int) ((Screen.height / 2) - dividerSize - (buttonHeight / 2) - ((int) returnButton.GetComponent<RectTransform>().rect.height)); // First Y Position 

        Debug.Log("First X: " + firstButtonPos.x + ". First Y: " + firstButtonPos.y + ". Div: "+dividerSize);

        //generateButtons("Nott");
        generateMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentScreen.x != Screen.width || currentScreen.y != Screen.height || currentOrientation != Input.deviceOrientation)
        {
            currentScreen = new Vector2(Screen.width, Screen.height);
            currentOrientation = Input.deviceOrientation;
            reloadMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            clearMenu();
            generateMenu();
        }
        scrolling();
        mouseScrolling();
    }

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

    public void generateMenu()
    {
        currentMenu = "main"; //Set the current Menu string, for reloading the scene
        returnButton.SetActive(false); //Hide return button
        Sprite[] sprites = Resources.LoadAll<Sprite>("ButtonBackgrounds"); //Load all backgrounds
        Vector2 lastButtonPos = new Vector2(1.337f, 0); //Init so it starts with the first position
        foreach (Sprite s in sprites)
        {
            GameObject temp = Instantiate(menuButtonTemplate); //Create Button
            temp.GetComponent<Image>().sprite = s; //Set Background Image
            temp.GetComponent<ButtonScript>().character = s.name; //Set Character-Name for button load function
            temp.transform.SetParent(this.transform); //Add to canvas
            shownObjects.Add(temp); //Add to currently showing list
            lastButtonPos = getNextButtonPosition(lastButtonPos); //Get the right position
            temp.transform.localPosition = lastButtonPos; //Position correctly
        }

    }
    public void generateButtons(string characterName)
    {
        clearMenu(); //Remove all menu buttons
        currentMenu = characterName; //Set the menu string for reloading the menu
        returnButton.SetActive(true); //Show return button
        AudioClip[] audioFiles = Resources.LoadAll<AudioClip>("Audio/"+characterName); //Load all audio files
        Sprite buttonSprite = Resources.Load<Sprite>("ButtonBackgrounds/"+characterName); //Load Button Background
        Vector2 lastButtonPos = new Vector2(1.337f, 0); //Init so it starts with the first position
        foreach(AudioClip a in audioFiles)
        {
            GameObject temp = Instantiate(audioButtonTemplate); //Create Button
            temp.GetComponent<Image>().sprite = buttonSprite; //set Background Image 
            temp.GetComponentInChildren<Text>().text = a.name; //Set Description
            temp.GetComponent<AudioSource>().clip = a; //Set Audio File
            temp.transform.SetParent(this.transform); //Add to canvas
            shownObjects.Add(temp); //Add to currently showing list
            lastButtonPos = getNextButtonPosition(lastButtonPos); //Get the right position for the button
            temp.transform.localPosition = lastButtonPos; //Place it at the right position
        }

    }

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
                //Calculate the difference in the scroll
                Vector2 dif = Input.touches[0].position  - scrollStart;
                Vector2 moveBy = new Vector2(0,(dif.y * speed * Time.deltaTime));
                //Check if you can scroll in that direction at this point
                if ((moveBy.y < 0 && this.transform.localPosition.y >= 0) || (moveBy.y > 0 && this.transform.position.y + (shownObjects[shownObjects.Count - 1].transform.localPosition.y - dividerSize - (buttonHeight/2)) < 0 )) 
                    //Move the list
                    this.transform.Translate(moveBy);
                //Update the last scroll position
                scrollStart = Input.touches[0].position;
            }
        }
    }

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

    public void clearMenu()
    {
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

    public void reloadMenu()
    {
        Debug.Log(currentMenu);
        buttonsPerRow = (Screen.width - 50) / (buttonWidth + 50);   //Calculate how many buttons per row
        dividerSize = (Screen.width - buttonsPerRow * buttonWidth) / (buttonsPerRow + 1); //Calculate the size of the space between buttons
        firstButtonPos.x = (int)((-Screen.width / 2) + dividerSize + (buttonWidth / 2)); //First Button X Positon
        firstButtonPos.y = (int)((Screen.height / 2) - dividerSize - (buttonHeight / 2) - ((int)returnButton.GetComponent<RectTransform>().rect.height)); // First Y Position 

        Debug.Log("First X: " + firstButtonPos.x + ". First Y: " + firstButtonPos.y + ". Div: " + dividerSize);

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

}
