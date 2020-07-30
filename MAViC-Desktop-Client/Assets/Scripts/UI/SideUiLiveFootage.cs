using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SideUiLiveFootage : MonoBehaviour, IPointerClickHandler
{
    public SideUi sideUi;
    public GameObject fullscreenViewPrefab;

    private GameObject _fullscreenViewObj;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_fullscreenViewObj == null)
        {
            _fullscreenViewObj = GameObject.Instantiate(fullscreenViewPrefab, sideUi.mainController.wholeScreenUiObj.transform);

            sideUi.fullscreenView = _fullscreenViewObj.GetComponent<FullscreenView>();
            sideUi.fullscreenView.mainController = sideUi.mainController;
            sideUi.fullscreenView.DisplayFullscreenImage(
                sideUi.selectedFieldTeam.GetPhotoPathFromSimulatedTime(sideUi.mainController.currentSimulatedTime),
                sideUi.selectedFieldTeam.GetPhotoThumbnailPathFromSimulatedTime(sideUi.mainController.currentSimulatedTime)
                );
        }
    }
}
