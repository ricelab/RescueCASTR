using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour, IPointerClickHandler
{
    public MainController mainController;
    public Text text;

    public void OnPointerClick(PointerEventData eventData)
    {
        text.text = "Starting up...";
        mainController.BeginSimulation();
    }
}
