using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gui : MonoBehaviour
{
    // Start is called before the first frame update
    public int angleOffset = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void drawDisk(Vector2 centerPosition,int radius, int angleOffset){
        for(int i=0; i<26; i++){
            float angle = (Mathf.Deg2Rad*(i-angleOffset)*10);
            Vector2 charPosition = new Vector2(centerPosition.x - Mathf.Cos(angle) * radius,
                                                centerPosition.y - Mathf.Sin(angle) * radius);
            GUI.backgroundColor = Color.clear;
            GUI.Label(new Rect(charPosition.y, charPosition.x, 100, 20), ((char)(i+65)).ToString());
        }
        for(int i=0; i<10; i++){
            float angle = (Mathf.Deg2Rad*((i-angleOffset)+26)*10);
            Vector2 charPosition = new Vector2(centerPosition.x - Mathf.Cos(angle) * radius,
                                                centerPosition.y - Mathf.Sin(angle) * radius);
            GUI.backgroundColor = Color.clear;
            GUI.Label(new Rect(charPosition.y, charPosition.x, 100, 20), ((char)(i+48)).ToString());
        }
    }
    void OnGUI(){
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
        headingStyle.fontSize = 30;
        headingStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(Screen.width / 2 - 50, 20, 100, 40), "Cipher", headingStyle);
        int temp = 0;
        if(Input.inputString!="")
        {
        char tmp=Input.inputString[0];
        temp=(int)tmp;
        if(temp>96&&temp<123){
            temp-=96;
        }else if(temp>47&&temp<58){
            temp -=21;
        }
        
        angleOffset=(temp-1);
        print(temp);
        }
        int radius = 80;
        Vector2 centerPosition = new Vector2(Screen.height/2,Screen.width/2);
        drawDisk(centerPosition, radius, 0);
        drawDisk(centerPosition, radius+20, angleOffset);
    }
}
