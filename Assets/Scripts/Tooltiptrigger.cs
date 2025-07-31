using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ToolTip; // Updated field name

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.SetActive(false);
    }
}
