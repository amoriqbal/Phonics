using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AutoSizeLayoutDropdown : MonoBehaviour
{
    public bool dontTouchChildrenRect; //Is dont touch children rect

    public float topPad; //Top padding
    public float bottomPad; //Bottom padding

    public float spacing; //Spacing between objects

    public bool isHaveMaxSize; //Is have max size
    public float maxSize; //Max size of layout

    private void Update() {
        UpdateAllRect();
    }

    /// <summary>
    /// Update of layout
    /// </summary>
    public void UpdateAllRect() {
        float sizeTotal = topPad;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).gameObject.activeSelf) {
                var rect = transform.GetChild(i).GetComponent<RectTransform>();
                if (rect.GetComponent<AutoSizeLayoutDropdown>()) {
                    rect.GetComponent<AutoSizeLayoutDropdown>().UpdateAllRect();
                }
                rect.pivot = new Vector2(rect.pivot.x, 1);
                if (!dontTouchChildrenRect) {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -rect.sizeDelta.y * rect.localScale.y * (1 - rect.pivot.y) - sizeTotal);
                }
                sizeTotal += rect.sizeDelta.y * rect.localScale.y + spacing;
            }
        }
        sizeTotal -= spacing;
        sizeTotal += bottomPad;
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x,
            Mathf.Clamp(sizeTotal, float.MinValue, isHaveMaxSize ? maxSize : float.MaxValue));
    }
}
