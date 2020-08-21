using UnityEngine;
using UnityEngine.EventSystems;

public class SideUiLiveFootage : MonoBehaviour, IPointerClickHandler
{
    public SideUi sideUi;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sideUi.mainController.fullscreenViewObj == null)
        {
            sideUi.mainController.fullscreenViewObj = GameObject.Instantiate(sideUi.mainController.footageFullscreenViewPrefab, sideUi.mainController.wholeScreenUiObj.transform);

            sideUi.mainController.footageFullscreenView = sideUi.mainController.fullscreenViewObj.GetComponent<FootageFullscreenView>();
            sideUi.mainController.footageFullscreenView.mainController = sideUi.mainController;
            sideUi.mainController.footageFullscreenViewShowingLive = true;

            sideUi.mainController.footageFullscreenView.DisplayFullscreenImage(
                sideUi.selectedFieldTeam.GetPhotoPathFromSimulatedTime(sideUi.selectedFieldTeam.simulatedTimeLastOnline),
                sideUi.selectedFieldTeam.GetPhotoThumbnailPathFromSimulatedTime(sideUi.selectedFieldTeam.simulatedTimeLastOnline)
                );
        }
    }
}
