using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject expandablePanelObj;
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

    protected Message _message;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(true);
        this.transform.SetAsLastSibling();
        _message.fieldTeam.mainController.mouseHoveringOverIcon = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(false);
        _message.fieldTeam.mainController.mouseHoveringOverIcon = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _message.fieldTeam.mainController.fullscreenViewObj =
                GameObject.Instantiate(_message.fieldTeam.mainController.messageFullscreenViewPrefab, _message.fieldTeam.mainController.wholeScreenUiObj.transform);
        _message.fieldTeam.mainController.messageFullscreenView = _message.fieldTeam.mainController.fullscreenViewObj.GetComponent<MessageFullscreenView>();
        _message.fieldTeam.mainController.messageFullscreenView.message = _message;
    }
}
