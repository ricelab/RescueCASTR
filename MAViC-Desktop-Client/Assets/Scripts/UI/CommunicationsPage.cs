using UnityEngine;
using UnityEngine.UI;

public class CommunicationsPage : MonoBehaviour
{
    public SideUi sideUi;

    public GameObject containerContentPanel;

    public InputField messageInputField;

    public GameObject clueBoxPrefab;
    public GameObject incomingMessageBoxPrefab;
    public GameObject outgoingMessageBoxPrefab;

    public void AddCommunicationBox(Communication communication)
    {
        // First try Clue
        Clue clue = communication as Clue;
        if (clue != null)
        {
            AddClueBox(clue);
            return;
        }

        // Then try Message
        Message message = communication as Message;
        if (message != null)
        {
            AddMessageBox(message);
            return;
        }
    }

    public void AddClueBox(Clue clue)
    {
        GameObject clueBoxObjToAdd = Instantiate(clueBoxPrefab, containerContentPanel.transform);
        ClueBox clueBoxToAdd = clueBoxObjToAdd.GetComponent<ClueBox>();

        clueBoxToAdd.clue = clue;

        clueBoxObjToAdd.transform.Find("ClueText").GetComponent<Text>().text = clue.textDescription;
        clueBoxObjToAdd.transform.Find("ClueTimeText").GetComponent<Text>().text = clue.simulatedTime.dateTime.ToString("MM/dd/yyyy HH:mm:ss");

        //Texture2D texture = Utility.LoadImageFile(sideUi.selectedFieldTeam.recordingDirectoryPath + "clues-photos/" + clue.photoFileName); ;
        //clueBoxToAdd.transform.Find("Image").GetComponent<Image>().sprite =
        //    Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        clueBoxToAdd.LoadImage(sideUi.mainController.resourcesUrl /* + sideUi.selectedFieldTeam.recordingDirectoryPath */ + "clues-photos/" + clue.photoFileName);
    }

    public void AddMessageBox(Message message)
    {
        GameObject messageBoxObjToAdd = Instantiate(
            message.messageDirection == MessageDirection.FromTeamToCommand ? incomingMessageBoxPrefab : outgoingMessageBoxPrefab,
            containerContentPanel.transform
            );
        MessageBox messageBoxToAdd = messageBoxObjToAdd.GetComponent<MessageBox>();

        messageBoxToAdd.message = message;

        messageBoxObjToAdd.transform.Find("MessageText").GetComponent<Text>().text = message.messageContent;
        messageBoxObjToAdd.transform.Find("MessageTimeText").GetComponent<Text>().text = message.simulatedTime.dateTime.ToString("MM/dd/yyyy HH:mm:ss");
    }

    public void SendMessageToTeam(string messageText = null)
    {
        Message message = new Message();
        message.fieldTeam = sideUi.selectedFieldTeam;
        message.simulatedTime = sideUi.mainController.currentSimulatedTime;
        message.instantiateBySimulatedTime = true;
        message.location = sideUi.selectedFieldTeam.predictedCurrentLocation;
        message.messageDirection = MessageDirection.FromCommandToTeam;
        message.messageContent = messageText == null || messageText == "" ? messageInputField.text : messageText;
        message.Start();

        sideUi.selectedFieldTeam.AddCommunication(message);

        messageInputField.text = "";

        sideUi.communicationsPageScrollRect.ScrollToBottom();
    }
}
