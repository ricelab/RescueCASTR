using UnityEngine;
using UnityEngine.UI;

public class MessageInputField : MonoBehaviour
{
    public InputField inputField;
    public CommunicationsPage communicationsPage;

    private bool wasFocused;

    public void Start()
    {
        wasFocused = inputField.isFocused;
    }

    public void Update()
    {
        if (wasFocused && Input.GetKeyDown(KeyCode.Return))
        {
            communicationsPage.SendMessageToTeam();
        }

        wasFocused = inputField.isFocused;
    }
}
