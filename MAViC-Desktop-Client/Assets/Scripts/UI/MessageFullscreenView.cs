using UnityEngine.UI;

public class MessageFullscreenView : FullscreenView
{
    public Text messageText;

    public Message message
    {
        get
        {
            return _message;
        }
        set
        {
            _message = value;

            messageText.text = _message.messageContent;
        }
    }

    private Message _message;

    public override MainController GetMainController()
    {
        return _message.fieldTeam.mainController;
    }
}
