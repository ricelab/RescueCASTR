using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CluesPage : MonoBehaviour
{
    public SideUi sideUi;

    public GameObject cluesContainerContentPanel;

    public GameObject clueBoxPrefab;

    public void AddClueBox(Clue clue)
    {
        GameObject clueBoxToAdd = Instantiate(clueBoxPrefab, cluesContainerContentPanel.transform);

        clueBoxToAdd.transform.Find("ClueText").GetComponent<Text>().text = clue.textDescription;

        Texture2D texture = Utility.LoadImageFile(sideUi.selectedFieldTeam.recordingDirectoryPath + "clues-photos/" + clue.photoFileName); ;
        clueBoxToAdd.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
