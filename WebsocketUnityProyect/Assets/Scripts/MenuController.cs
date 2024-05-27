using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using SimpleJSON;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_InputField usernameInput;

    public void CheckIfUsernameIsValid()
    {
        if (String.IsNullOrEmpty(usernameInput.text)) { 
            loginButton.interactable = false;
        }
        else
        {
            loginButton.interactable = true;
        }
    }
}
