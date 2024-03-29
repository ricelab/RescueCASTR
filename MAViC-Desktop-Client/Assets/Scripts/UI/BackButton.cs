﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ABackButtonClickHandler : MonoBehaviour
{
    public abstract void OnBackButtonClick(GameObject fromPage, GameObject toPage);
}

public class BackButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ABackButtonClickHandler backButtonClickHandler;

    public GameObject currentPageContentPanel;
    public GameObject previousPageContentPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.87f, 0.87f, 0.87f, 1.0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        currentPageContentPanel.SetActive(false);
        previousPageContentPanel.SetActive(true);

        backButtonClickHandler.OnBackButtonClick(currentPageContentPanel, previousPageContentPanel);
    }
}
