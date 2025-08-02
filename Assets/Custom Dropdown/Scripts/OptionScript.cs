using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script for options object
/// </summary>
public class OptionScript : MonoBehaviour
{
    public AdvancedDropdown mainScript; //Reference to a main script

    public int id; //Id in list
    public string text; //Text of the option

    public TextMeshProUGUI targetText; //Text component

    public GameObject selector; //Selector object

    /// <summary>
    /// Initialize options
    /// </summary>
    /// <param name="id">Option ID</param>
    /// <param name="text">Option Text</param>
    public void InitOption(int id, string text) {
        this.id = id;
        this.text = text;
        targetText.text = text;
        selector.SetActive(id == mainScript.value);
    }

    /// <summary>
    /// Click event by pressing option
    /// </summary>
    public void ClickSelf() {
        mainScript.SelectOption(id);
    }

    /// <summary>
    /// Set select state (Called from main script)
    /// </summary>
    /// <param name="isSelected">Is selected</param>
    public void SetSelectState(bool isSelected) {
        selector.SetActive(isSelected);
    }
}
