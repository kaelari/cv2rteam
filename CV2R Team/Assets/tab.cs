using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tab : MonoBehaviour
{

    public Selectable nextField;
    public InputField component;
    // Use this for initialization
    void Start()
    {
        component = GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (component.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            nextField.Select();
        }
    }
}
