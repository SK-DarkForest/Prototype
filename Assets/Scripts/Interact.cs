using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;


public class Definitions{
    public static short notifyLimit = 5;
}
public enum CloseCases
{
    OutOfTime,
    Closed,
    DeletedByNew,
    Clicked
}

public delegate void CallbackNotify(CloseCases message);

struct Notification{
    public string Header;
    public string Body;
    public short Options;
    public CallbackNotify Callback;
    public float ShowTime;
    public bool active;
    public int id;

    public Notification(string Header, string body, short Options, CallbackNotify callback, float showTime, int id){
        this.Header = Header;
        this.Body = body;
        this.Options = Options;
        this.Callback = callback;
        this.ShowTime = showTime;
        this.active = true;
        this.id = id;
    }
}

public class Interact : MonoBehaviour
{
    // Start is called before the first frame update
    private Texture2D generatedTexture; // Dynamically generated texture
    private Material material;
    public Shader imageShader;
    public AudioSource audioSource;
    public bool interact = false;
    private Notification[] notifications = new Notification[Definitions.notifyLimit];
    private int notifics = 0;
    private GUIStyle inter;
    public int absHeight = 50;
    public int incHeight = 50;
    private Vector2 lastClicked = Vector2.zero;
    public int id = 0;
    private void callback(CloseCases message){
        Debug.Log("Closed because of: "+message);
    }
    private Notification nullNotif(){
        return new Notification(null, null, 0, null,0,0);
    }
    private Vector2 Vec2uv(Vector2 v){
        return new Vector2(v.x*16/(9*Screen.width),v.y/Screen.height);
    }
    private Vector2 uv2Vec(Vector2 uv){
        return new Vector2(uv.x*9*Screen.width/16,uv.y*Screen.height);
    }
    private float length(Vector2 p){
        return (float)Mathf.Sqrt(p.x*p.x+p.y*p.y);
    }
    void Start()
    {
        inter = new GUIStyle();
        inter.fontSize = 20;
        audioSource = GetComponent<AudioSource>();
        audioSource.enabled = false;
        material = new Material(imageShader);
        generatedTexture = new Texture2D(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        //test for out of time
        for(int i = notifications.Length-1; i >=0; i--){
            if((notifications[i].ShowTime+500<Time.timeSinceLevelLoad)&&notifications[i].active){
                if (notifications[i].Callback != null) {
                    notifications[i].Callback(CloseCases.OutOfTime);
                }
                notifics--;
                notifications[i]= nullNotif();
                notifications[i].active = false;
            }else if(notifications[i].active){
                break;
            }
        }
        this.material.SetFloat("_num", (float)this.notifics);
        this.material.SetFloat("_TimeSince", Time.timeSinceLevelLoad);
    	
        if(Input.GetKeyDown(KeyCode.A)){
           notify("Notification","This is a notification!",1,this.callback);
        }
    }
    float sdCapsuleFromShader( Vector2 p, Vector2 a, Vector2 b, float r ){
        Vector2 pa = p - a, ba = b - a;
        float h = Mathf.Clamp( Vector2.Dot(pa,ba)/Vector2.Dot(ba,ba), 0, 1 );
        return length( pa - ba*h ) - r;
    }
    void OnGUI(){
        //Interact
        Event e = Event.current;
        if (e.type == EventType.MouseDown){
            lastClicked = new Vector2(e.mousePosition.x, Screen.height - e.mousePosition.y);
            Vector2 uv = new Vector2(lastClicked.x*16/(9*Screen.width),lastClicked.y/Screen.height);
            float dist = 0;
            for(int i = 0; i < notifics; i++){
                dist = sdCapsuleFromShader(uv, new Vector2(1.1f, .08f + (float)i / 7), new Vector2(1.7f, .08f + (float)i / 7), .065f);
                if(dist < 0){
                    if(notifications[i].Callback!=null){
                        notifications[i].Callback(CloseCases.Clicked);
                    }
                    for(int j = i; j < notifics-1; j++){
                        notifications[i]=notifications[i+1];
                    }
                    notifications[notifics-1].active = false;
                    notifics--;
                    break;
                }
            }
        }
        Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), generatedTexture, material);
        if(interact){
            GUI.color = Color.white;
             GUI.Label(new Rect(Screen.width-400, Screen.height-50, 1000, 200),"<color=white><size=20>Press E to interact.</size></color>");
        }
        for(int i=0; i<notifics; i++){
            GUI.Label(new Rect(Screen.width*2/3,Screen.height-60-i*63,250,100),notifications[i].Header);
            GUI.Label(new Rect(Screen.width*2/3,Screen.height-40-i*63,250,100),notifications[i].Body);
            if (GUI.Button(new Rect(Screen.width*9/10, Screen.height-50-i*63, 30, 30), "X")){
                if(notifications[i].Callback!=null){
                    notifications[i].Callback(CloseCases.Closed);
                }
                for(int j = i; j < notifics-1; j++){
                    notifications[i]=notifications[i+1];
                }
                notifications[notifics-1].active = false;
                notifics--;
            }/*else if(IsPointInCapsuleOri(lastClicked,i)){//, new Vector2(Screen.width*2/3, Screen.height-50-i*63), new Vector2(Screen.width*9/10, Screen.height-50-i*63), 50
                GUI.Label(new Rect(100,100,100,100),"in Range");
                return;
                if(notifications[i].Callback!=null){
                    notifications[i].Callback(CloseCases.Clicked);
                }
                for(int j = i; j < notifics-1; j++){2
                    notifications[i]=notifications[i+1];
                }
                notifications[notifics-1].active = false;
                notifics--;
            }*/
        }
        /*if(GUI.Button(new Rect(100,100,100,100),"Notify")){
            notify("Notification","",1,this.callback);
        }*/ 
        //Mimimap
        //Inventar
    }
    void notify(string header, string body, short options, CallbackNotify callback){
        if(notifications[Definitions.notifyLimit-1].active){
            if (notifications[Definitions.notifyLimit - 1].Callback != null) {
                notifications[Definitions.notifyLimit - 1].Callback(CloseCases.DeletedByNew);
            }
            notifications[Definitions.notifyLimit-1].active = false;
        }else{
            notifics++;
        }
        for(int i=1;i<Definitions.notifyLimit; i++){
            notifications[Definitions.notifyLimit-i]=notifications[Definitions.notifyLimit-i-1];
        }
        notifications[0]=new Notification(header,body+this.id,options,callback,Time.timeSinceLevelLoad,this.id);
        this.id++;
        if((options&1)==1){
            audioSource.enabled = true;
            audioSource.Play();
        }
    }
}