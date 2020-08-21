using UnityEngine;
using UnityEngine.UI;

public class MessagesPage : MonoBehaviour
{
    public SideUi sideUi;

    public GameObject messagesContainerContentPanel;

    public GameObject messageBoxPrefab;

    public void AddMessageBox(Message message)
    {
        GameObject messageBoxObjToAdd = Instantiate(messageBoxPrefab, messagesContainerContentPanel.transform);
        MessageBox messageBoxToAdd = messageBoxObjToAdd.GetComponent<MessageBox>();

        messageBoxToAdd.message = message;

        messageBoxObjToAdd.transform.Find("MessageText").GetComponent<Text>().text = message.messageContent;
        messageBoxObjToAdd.transform.Find("MessageTimeText").GetComponent<Text>().text = message.simulatedTime.dateTime.ToString("MM/dd/yyyy HH:mm:ss");
    }
}
