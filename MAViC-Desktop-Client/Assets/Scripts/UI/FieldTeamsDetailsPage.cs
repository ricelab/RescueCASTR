using UnityEngine;

public class FieldTeamsDetailsPage : ABackButtonClickHandler // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
{
    public SideUi sideUi;

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        foreach(Transform child in sideUi.messagesPage.messagesContainerContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in sideUi.cluesPage.cluesContainerContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in sideUi.cluesAndMessagesPage.containerContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
