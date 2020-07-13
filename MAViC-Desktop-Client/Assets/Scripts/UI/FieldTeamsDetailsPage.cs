using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTeamsDetailsPage : ABackButtonClickHandler // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
{
    public SideUi sideUi;

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        sideUi.scrollRect.content = sideUi.mainMenuContentPanel.GetComponent<RectTransform>();
    }
}
