using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class uiInteraction : MonoBehaviour
{

    

    private UIDocument uIDocument;
    private Button button;


    // Start is called before the first frame update
    void Start()
    {
        // uIDocument = Resources.Load();
        button = uIDocument.rootVisualElement.Q<Button>("button");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
