using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float updateDelay = .2f;

    private Vector2 mousePosition;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateDelay) 
        {
            var mousePos = Input.mousePosition;

            mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

            //SocketManager.SendPlayerPosition(mousePosition); //Fix
            
            timer = 0;
        }
    }
}