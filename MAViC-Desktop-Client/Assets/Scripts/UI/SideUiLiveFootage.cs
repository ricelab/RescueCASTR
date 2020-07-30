using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SideUiLiveFootage : MonoBehaviour, IPointerClickHandler
{
    public SideUi sideUi;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sideUi.mainController.fullscreenViewObj == null)
        {
            sideUi.mainController.fullscreenViewObj = GameObject.Instantiate(sideUi.mainController.fullscreenViewPrefab, sideUi.mainController.wholeScreenUiObj.transform);

            sideUi.mainController.fullscreenView = sideUi.mainController.fullscreenViewObj.GetComponent<FullscreenView>();
            sideUi.mainController.fullscreenView.mainController = sideUi.mainController;
            sideUi.mainController.fullscreenViewShowingLiveFootage = true;

            sideUi.mainController.fullscreenView.DisplayFullscreenImage(
                sideUi.selectedFieldTeam.GetPhotoPathFromSimulatedTime(sideUi.mainController.currentSimulatedTime),
                sideUi.selectedFieldTeam.GetPhotoThumbnailPathFromSimulatedTime(sideUi.mainController.currentSimulatedTime)
                );
        }
    }
}
