using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTeamsDetailsPage : ABackButtonClickHandler // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
{
    public SideUi sideUi;

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        sideUi.scrollRect.content = sideUi.fieldTeamDetailsContentPanel.GetComponent<RectTransform>();

        foreach(Transform child in sideUi.messagesPage.messagesContainerContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in sideUi.cluesPage.cluesContainerContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
