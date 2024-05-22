using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;

enum ScriptType
{
    Start,
    Move,
    SetGoalPosition,
    GetPathDir,
    SetTargetPosition,
    GetTargetDir,
    Notify,
    Number,
    Vector,
    String,
    Function,
    If,
    Else,
    IfElse,
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    Wait,
    Jump,
    Label
}

class Argument{

}

class ScriptElement {
    public ScriptType ScriptType;
    public ScriptElement[] children;
    public string[] text = new string[3];
    public Vector2 position;
    public Vector2 size;
    public Argument[] arguments;

    public ScriptElement(ScriptType scriptType, Vector2 pos) {
        this.ScriptType = scriptType;
        this.size = getSizeOf(scriptType);
        this.position = pos;
        this.children = new ScriptElement[0]; // Ensure children array is initialized
    }

    private Vector2 Vec2uv(Vector2 v) {
        return new Vector2(v.x * 16 / (9 * Screen.width), v.y / Screen.height);
    }

    private Vector2 uv2Vec(Vector2 uv) {
        return new Vector2(uv.x * 9 * Screen.width / 16, uv.y * Screen.height);
    }

    public void addChild(ScriptElement child, int index) {
        Array.Resize(ref children, children.Length + 1);
        Vector2 pos = new Vector2(this.position.x + .1f, this.position.y + .1f);
        if(this.children.Length > 0) {
            for (int i = 0; i < index; i++) {
                if(this.children[i]!=null){
                    pos.y += this.children[i].size.y;
                }
            }
        }
        child.position = pos;
        for (int i = index+1; i < this.children.Length; i++) {
            this.children[i] = this.children[i - 1];
        }
        if (index >= 0 && index < children.Length) {
            children[index] = child;
        } else {
            Debug.LogError($"Invalid index: {index}. Index must be between 0 and {children.Length - 1}.");
        }
        this.shift(index+1,new Vector2(0,0.1f));
    }

    public void update(int index) {//behaves weird on the second for-loop
        Vector2 pos = new Vector2(this.position.x + .1f, this.position.y);
        for (int i = 0; i < index; i++) {
            pos.y += this.children[i].size.y;
        }
        Debug.Log("Current: " + children[this.children.Length - 1].position);
        for (int i = index; i < this.children.Length; i++) {
            this.children[i].move(pos);
            pos.y += this.children[i].size.y;
        }
        Debug.Log("Processed: " + children[this.children.Length - 1].position);
        this.size = pos - new Vector2(this.position.x + .1f, this.position.y);
    }
    public void shift(int index, Vector2 amount){
        for (int i = index; i < this.children.Length; i++) {
            this.children[i].moveRel(amount);
        }
    }

    public bool checkPlantPossible(Vector2 pos) {
        bool x = (pos.x > this.position.x) && (pos.x < this.position.x + this.size.x);
        bool y = (pos.y > this.position.y) && (pos.y < this.position.y + this.size.y);
        return x && y;
    }

    public int[] getPlantIndex(Vector2 pos) {
        int[] index = new int[16];
        index[0] = -1;
        if (!this.checkPlantPossible(pos)) {
            return index;
        }
        float height = pos.y - this.position.y;
        int i = 0;
        while (i < this.children.Length - 1) {
            height -= this.children[i].size.y;
            if (height < 0) {
                if (this.children[i].children.Length <= 0) {
                    for (int j = i + 1; j < this.children.Length; j++) {
                        children[j].moveRel(new Vector2(0, .1f));
                    }
                    break;
                }
                int[] index2 = this.children[i].getPlantIndex(pos);
                for (int j = 1; j < index.Length; j++) {
                    index[j] = index2[j - 1];
                }
                break;
            }
            i++;
        }
        return index;
    }

    public void move(Vector2 target) {
        Vector2 offset = target - this.position;
        for (int i = 0; i < this.children.Length; i++) {
            this.children[i].moveRel(offset);
        }
        this.position = target;
    }

    public void moveRel(Vector2 offset) {
        for (int i = 0; i < this.children.Length; i++) {
            this.children[i].moveRel(offset);
        }
        this.position += offset;
    }

    public static Vector2 getSizeOf(ScriptType scriptType) {
        switch (scriptType) {
            case ScriptType.Start:
                return new Vector2(.4f, .1f);
            default:
                return new Vector2(.1f, .1f);
        }
    }

    public void display() {
        if (this.children == null) {
            Debug.LogError("Children array is null!");
            return;
        }
        Vector2 size = new Vector2(.4f, .1f);
        Vector2 displayPos = uv2Vec(position);
        Vector2 displaySize = uv2Vec(size);

        /*GUI.Box(new Rect(displayPos.x, displayPos.y, displaySize.x, .1f), "");
        GUI.Box(new Rect(displayPos.x, displayPos.y, .1f, displaySize.y + .2f), "");
        GUI.Box(new Rect(displayPos.x, displayPos.y + displaySize.y + .1f, displaySize.x, .1f), "");*/
        GUI.Box(new Rect(displayPos.x, displayPos.y, displaySize.x, displaySize.y), "");

        for (int i = 0; i < this.children.Length; i++) {
            if (this.children[i] == null) {
                Debug.LogError($"Child at index {i} is null!");
                continue;
            }
            children[i].display();
        }
    }
    public string transpile(){
        string inner = "";
        for (int i = 0; i < children.Length; i++){
            inner += children[i].transpile();
            if(this.ScriptType == ScriptType.If){
                continue;
            }
        }
        switch (this.ScriptType)
        {
            case ScriptType.Start: return "function start(){"+inner+"};";
            case ScriptType.If: return "if("+inner+"){"+children[1].transpile()+"}";
            case ScriptType.Function: return ""+this.text;
            default:
                return "";
        }
    }
}

public class ProgrammingUI : MonoBehaviour
{
    // Start is called before the first frame update
    public float radius = 20f;
    public Shader imageShader;
    private Texture2D generatedTexture; // Dynamically generated texture
    private Material material;
    private ComputeBuffer computeBuffer;
    private ScriptElement start;
    private int inc;
    static Func<int, int, int> CreateDynamicMethod()
    {
        // Define a dynamic assembly and module
        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        // Define a dynamic type
        TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicType", TypeAttributes.Public);

        // Define a dynamic method that accepts two integers and returns their sum
        MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttributes.Public | MethodAttributes.Static, typeof(int),
                                                               new Type[] { typeof(int), typeof(int) });

        // Get an ILGenerator and emit IL code
        ILGenerator ilGenerator = methodBuilder.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);          // Load first argument onto the stack
        ilGenerator.Emit(OpCodes.Ldarg_1);          // Load second argument onto the stack
        ilGenerator.Emit(OpCodes.Add);              // Add the two arguments
        ilGenerator.Emit(OpCodes.Ret);               // Return the result
        // Create the type
        Type dynamicType = typeBuilder.CreateType();
        // Get the MethodInfo for the dynamic method
        MethodInfo dynamicMethod = dynamicType.GetMethod("Add");
        // Create a delegate for the dynamic method
        return (Func<int, int, int>)Delegate.CreateDelegate(typeof(Func<int, int, int>), dynamicMethod);
    }
    void Start()
    {
        material = new Material(imageShader);
        generatedTexture = new Texture2D(Screen.width, Screen.height);
        float[] floatArray = { 1.0f, 2.0f, 3.0f, 4.0f };

        // Create the compute buffer with the length of the array and size of a float
        computeBuffer = new ComputeBuffer(floatArray.Length, sizeof(float));
        computeBuffer.SetData(floatArray);

        // Pass the compute buffer to the shader
        material.SetBuffer("floatArrayBuffer", computeBuffer);
        //-------Assembly-------
        Func<int, int, int> dynamicFunction = CreateDynamicMethod();

        // Test the dynamic function
        int result = dynamicFunction(10, 20);
        Debug.Log("Result: " + result);
        this.start = new ScriptElement(ScriptType.Start, new Vector2(.1f,.1f));
        //this.start.addChild(new ScriptElement(ScriptType.Function,Vector2.zero),0);
    }

    // Update is called once per frame
    void Update(){
        float timeSinceSceneLoad = Time.timeSinceLevelLoad;
        this.material.SetFloat("_TimeSince", timeSinceSceneLoad);
        if(Math.Floor(timeSinceSceneLoad)>inc){
            inc = (int)Math.Floor(timeSinceSceneLoad);
            ScriptElement child = new ScriptElement(ScriptType.Function, new Vector2(0,0));
            child.text[0] = "Worked";
            this.start.addChild(child,this.start.children.Length);
            for(int i = 0; i < this.start.children.Length; i++){
                Debug.Log("Index: "+i+" Position"+this.start.children[i].position+"");
            }
        }
        if(Input.GetKeyDown(KeyCode.T)){
            Debug.Log("Transpiled: "+this.start.transpile());
        }
    }
    void OnDestroy()
    {
        if (computeBuffer != null)
            computeBuffer.Release();
    }
    private bool isDragging = false;
    private Vector2 offset;
    private Vector2 position = Vector2.zero;

    void OnGUI()
    {
        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), generatedTexture, material);
        this.start.display();
    }
    void DrawCircle(Vector2 center, float radius)
    {
        // Draw a circle using a series of small line segments
        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector2 startPoint = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            angle = (i + 1) * 2 * Mathf.PI / segments;
            Vector2 endPoint = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            // Draw a small rectangle to simulate a line
            GUI.DrawTexture(new Rect(startPoint.x, startPoint.y, 1f, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(endPoint.x, endPoint.y, 1f, 1f), Texture2D.whiteTexture);
        }
    }
}