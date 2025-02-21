using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugText : MonoBehaviour
{
    [SerializeField]
    TextMesh mainState;
    [SerializeField]
    TextMesh fightState;
    [SerializeField]
    TextMesh travelState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMainTextState(string text)
    {
        mainState.text = text;
    }

    public void setFightTextState(string text)
    {
        fightState.text = text;
    }

    public void setTravelState(string text)
    { 
        travelState.text = text;
    }


}
