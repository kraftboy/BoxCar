using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour
{
    
    public GameObject gameCamera;
    public GameObject hero;

    private Console console;

    [Header("Speed")]
    public float heroMaxSpeed = 10.0f;
    public float turnSpeed = 90.0f;
    [Header("Acceleration")]
    public float heroAcceleration = 20.0f;
    public float heroDecceleration = 20.0f;
    [Header("Lean")]
    public float leanForwardMax = 15.0f;
    public float leanSidewaysMax = 15.0f;


    private float heroSpeed = .0f;
    private float speedProjection = .0f;

    private List<string> debugMessages = new List<string>();

    // Use this for initialization
    void Start()
    {
        MainCamera mainCam = gameCamera.GetComponent<MainCamera>();
        mainCam.hero = hero;
        console = GameObject.Find("Console").GetComponent<Console>();
    }

    void Awake()
    {
        CommandManager cmdManager = Toolbox.RegisterComponent<CommandManager>();
        cmdManager.AddCommand("version", GetVersionString);
    }

    public string GetVersionString(params string[] args)
    {
        return string.Format("{0}: {1}", Application.productName, Application.version);
    }

    public float GetSpeedProjection()
    {
        return heroSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateConsole();
        
        debugMessages.Clear();

        if (console.isOpen)
        {
            return;
        }

        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        if (hInput == 0.0f && vInput == 0.0f)
        {
            heroSpeed -= heroDecceleration * Time.deltaTime;
        }

        Vector3 inputVector = new Vector3(hInput, 0.0f, vInput);
        Vector3 cameraForwardOnGround = Vector3.ProjectOnPlane(gameCamera.transform.forward, Vector3.up).normalized;
        Vector3 cameraRightOnGround = Vector3.ProjectOnPlane(gameCamera.transform.right, Vector3.up).normalized;

        Quaternion cameraQuat = Quaternion.LookRotation(cameraForwardOnGround, Vector3.up);

        Vector3 targetDirection = new Vector3();
        targetDirection = new Vector3(hInput, 0.0f, vInput).normalized;
        speedProjection = Vector3.Project(targetDirection, hero.transform.forward).magnitude;

        targetDirection = cameraQuat * targetDirection;

        Vector3 targetInHeroSpace = hero.transform.InverseTransformVector(targetDirection);

        heroSpeed += speedProjection * heroAcceleration * Time.deltaTime;
        heroSpeed = Mathf.Clamp(heroSpeed, 0.0f, heroMaxSpeed);

        Debug.DrawRay(hero.transform.position, targetDirection * 2.5f, Color.red, 0.0f, false);
        AddDebugMsg("H: {0: 0.00} V: {1:0.00}", hInput, vInput);
        AddDebugMsg("SpeedProjection: {0: 0.00}", speedProjection);
        AddDebugMsg("Speed: {0}", heroSpeed);
        AddDebugMsg("Position: {0}", hero.transform.position.ToString("F3"));
        AddDebugMsg("Cam Position: {0}", gameCamera.transform.position.ToString("F3"));
        AddDebugMsg("TargetInHeroSpace: {0}", targetInHeroSpace.ToString("F3"));

        if (targetDirection != Vector3.zero)
        {
            Quaternion turnQuat = Quaternion.LookRotation(targetDirection, Vector3.up);
            hero.transform.rotation = Quaternion.Slerp(hero.transform.rotation, turnQuat, Time.deltaTime * Mathf.Deg2Rad * turnSpeed * inputVector.magnitude);
        }

        CharacterController controller = hero.GetComponent<CharacterController>();

        Vector3 move = hero.transform.forward * heroSpeed * Time.deltaTime;
        move += new Vector3(0.0f, -9.8f * Time.deltaTime, 0.0f);

        controller.Move(move);

        float leanForward = leanForwardMax * (heroSpeed / heroMaxSpeed);
        float leanSideways = leanSidewaysMax * -targetInHeroSpace.x;
        AddDebugMsg("leanForward: {0}", leanForward);
        AddDebugMsg("leanSideways: {0}", leanSideways);

        Quaternion rotateForward = new Quaternion(Mathf.Deg2Rad * leanForward, 0.0f, Mathf.Deg2Rad * leanSideways, 1.0f);
        Transform child = GameObject.Find("Wheel").transform;
        child.localRotation = Quaternion.Slerp(child.localRotation, rotateForward, Time.deltaTime * Mathf.Deg2Rad * 180.0f);
    }

    void UpdateConsole()
    {
        if (!console) return;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (console.isOpen)
            {
                console.Hide();
            }
            else
            {
                console.Show();
            }
        }

        if (console.isOpen)
        {
            return;
        }
    }

    void AddDebugMsg(string format, params object[] msgParams)
    {
        debugMessages.Add(string.Format(format, msgParams));
    }

    /*
    void OnGUI()
    {
        Camera mainCamera = camera.GetComponent<Camera>();
        int offset = 20;

        Vector3 position = mainCamera.WorldToScreenPoint(hero.transform.position);
        Rect screenRect = new Rect(position.x + 15, position.y - 10 - (debugMessages.Count * offset), 300, offset);

        foreach (string msg in debugMessages)
        {
            GUI.Label(screenRect, string.Copy(msg));
            screenRect.y += offset;
        }
    }
    */
}
