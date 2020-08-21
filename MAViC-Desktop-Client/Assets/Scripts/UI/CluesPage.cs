using UnityEngine;
using UnityEngine.UI;

public class CluesPage : MonoBehaviour
{
    public SideUi sideUi;

    public GameObject cluesContainerContentPanel;

    public GameObject clueBoxPrefab;

    public void AddClueBox(Clue clue)
    {
        GameObject clueBoxObjToAdd = Instantiate(clueBoxPrefab, cluesContainerContentPanel.transform);
        ClueBox clueBoxToAdd = clueBoxObjToAdd.GetComponent<ClueBox>();

        clueBoxToAdd.clue = clue;

        clueBoxObjToAdd.transform.Find("ClueText").GetComponent<Text>().text = clue.textDescription;
        clueBoxObjToAdd.transform.Find("ClueTimeText").GetComponent<Text>().text = clue.simulatedTime.dateTime.ToString("MM/dd/yyyy HH:mm:ss");

        //Texture2D texture = Utility.LoadImageFile(sideUi.selectedFieldTeam.recordingDirectoryPath + "clues-photos/" + clue.photoFileName); ;
        //clueBoxToAdd.transform.Find("Image").GetComponent<Image>().sprite =
        //    Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        clueBoxToAdd.LoadImage(sideUi.mainController.resourcesUrl /* + sideUi.selectedFieldTeam.recordingDirectoryPath */ + "clues-photos/" + clue.photoFileName);
    }
}
