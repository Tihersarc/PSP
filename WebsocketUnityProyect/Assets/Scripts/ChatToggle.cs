using UnityEngine;

public class ChatToggle : MonoBehaviour
{
    public GameObject chatPanel; // El panel del chat que queremos modificar
    public Vector3 largeScale = new Vector3(1, 1, 1); // Escala grande
    public Vector3 smallScale = new Vector3(0.5f, 0.5f, 0.5f); // Escala pequeña

    private int state = 1; // 0: Grande, 1: Pequeño, 2: Deshabilitado

    void Start()
    {
        if (chatPanel != null)
        {
            SetChatScaleAndState();
        }
        else
        {
            Debug.LogError("Chat Panel is not assigned.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            state = (state + 1) % 3;
            SetChatScaleAndState();
        }
    }

    private void SetChatScaleAndState()
    {
        switch (state)
        {
            case 0:
                chatPanel.SetActive(true);
                chatPanel.transform.localScale = largeScale;
                break;
            case 1:
                chatPanel.transform.localScale = smallScale;
                break;
            case 2:
                chatPanel.SetActive(false);
                break;
        }
    }
}
