using UnityEngine;

public class MessageSendButton : MonoBehaviour
{
    public CommunicationsPage communicationsPage;

    public void ClickHandler()
    {
        communicationsPage.SendMessageToTeam();
    }
}
