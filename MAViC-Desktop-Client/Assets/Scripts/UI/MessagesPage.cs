using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagesPage : MonoBehaviour
{
    public SideUi sideUi;

    public GameObject messagesContainerContentPanel;

    public GameObject messageBoxPrefab;

    public void AddMessageBox(Message message)
    {
        GameObject messageBoxToAdd = Instantiate(messageBoxPrefab, messagesContainerContentPanel.transform);
        messageBoxToAdd.transform.Find("MessageText").GetComponent<Text>().text = message.messageContent;
    }
}
