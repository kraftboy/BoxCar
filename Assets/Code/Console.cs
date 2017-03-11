using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Console : MonoBehaviour {

    static public Console instance;

    private bool _isOpen;
    public  bool isOpen { get { return _isOpen; } }

    private int consoleWidth;
    private int consoleHeight;
    private float closeSpeed = 0.1f;

    private InputField consoleInput;
    private Text consoleLogText;
    private bool moveToEnd = false;
    private int logLineCount;

    List<string> consoleLog = new List<string>();
    List<string> historyBuffer = new List<string>();
    private int historyPosition = 0;

    // Use this for initialization
    void Awake () {
        
        instance = this;

        SetupGUI();

        Hide();
	}

    void SetupGUI()
    {
        consoleWidth = Screen.width;
        consoleHeight = Screen.height / 2;

        Canvas canvas = GetComponent<Canvas>();
        RectTransform rect = transform as RectTransform;
        rect.offsetMin = new Vector2(0, Screen.height - consoleHeight);
        rect.offsetMax = new Vector2(0, 0);

        GameObject inputGameObject = GameObject.Find("ConsoleInput");
        consoleInput = inputGameObject.GetComponent<InputField>();
        Text consoleInputText = inputGameObject.GetComponentInChildren<Text>();
        RectTransform inputRect = consoleInput.transform as RectTransform;
        inputRect.offsetMin = new Vector2(0, 0);
        inputRect.offsetMax = new Vector2(0, -consoleHeight + 18);
        consoleInput.onEndEdit.AddListener(delegate { ProcessInput(consoleInput); });
        consoleInput.onValueChanged.AddListener(delegate { ProcessKey(consoleInput); });

        GameObject obj = GameObject.Find("ConsoleLog");
        consoleLogText = obj.GetComponent<Text>();
        consoleLogText.text = "fooo";
        RectTransform consoleLogRect = consoleLogText.transform as RectTransform;
        consoleLogRect.offsetMin = new Vector2(0, 18);
        consoleLogRect.offsetMax = new Vector2(0, -consoleHeight);

        // calculate number of lines that can be displayed
        Vector3[] worldCorners = new Vector3[4];
        consoleLogText.rectTransform.GetWorldCorners(worldCorners);
        logLineCount = (int)Mathf.Abs(((worldCorners[1].y - worldCorners[0].y) / consoleLogText.font.lineHeight));
        logLineCount += 4;

        TextGenerationSettings generationSettings = consoleLogText.GetGenerationSettings(consoleLogText.rectTransform.rect.size);
        float textHeight = consoleLogText.cachedTextGeneratorForLayout.GetPreferredHeight("", generationSettings);

        consoleLog = new List<string>(Enumerable.Repeat("", logLineCount - 1));
        consoleLog.Add((GameObject.Find("GameController").GetComponent<GameController>() as GameController).GetVersionString());
        consoleLogText.text = string.Join("\n", consoleLog.ToArray());
    }

    string consolePrompt = ">";
    string ConsolePrompt()
    {
        return consolePrompt;
    }

    void ProcessKey(InputField input)
    {
        InputField inputField = GetComponentInChildren<InputField>();
        if(inputField.text.Length < consolePrompt.Length)
        {
            inputField.text = ConsolePrompt();
            inputField.caretPosition = ConsolePrompt().Length;
        }
    }

    void ProcessInput(InputField input)
    {
        string command = consoleInput.text;

        // strip prompt
        if (consoleInput.text.IndexOf(consolePrompt) == 0)
        {
            command = consoleInput.text.Substring(consolePrompt.Length);
        }
        
        // ignore tilde
        if (command == "`")
            return;

        historyBuffer.Insert(0, command);
        consoleLog.Add(consoleInput.text);

        CommandManager cmdMgr = Toolbox.RegisterComponent<CommandManager>();
        string output = "";
        if (cmdMgr.ProcessCommand(ref output, command))
        {
            consoleLog.Add(output);
        }
        else
        {
            consoleLog.Add(string.Format("\tCommand not found: {0}", command));
        }

        while (consoleLog.Count > logLineCount)
        {
            consoleLog.RemoveAt(0);
        }

        consoleLogText.text = string.Join("\n", consoleLog.ToArray());
        FocusInputField();
    }

    private void FocusInputField()
    {
        InputField[] inputFields = GetComponentsInChildren<InputField>();
        InputField inputField = inputFields[0];
        inputField.ActivateInputField();
        inputField.text = ConsolePrompt();
        moveToEnd = true;
    }

    public void Show()
    {
        _isOpen = true;
        FocusInputField();
    }

    public void Hide()
    {
        _isOpen = false;
        consoleInput.DeactivateInputField();
    }

    void HandleVisibility()
    {
        RectTransform rect = GetComponentInChildren<Image>().transform as RectTransform;
        Vector2 wPos = rect.position;
        
        if (isOpen)
        {
            rect.offsetMin = new Vector2(0, Mathf.MoveTowards(rect.offsetMin.y, Screen.height - consoleHeight, consoleHeight * Time.deltaTime / closeSpeed));
            rect.offsetMax = new Vector2(0, Mathf.MoveTowards(rect.offsetMax.y, 0, consoleHeight * Time.deltaTime / closeSpeed));
        }
        else
        {
            rect.offsetMin = new Vector2(0, Mathf.MoveTowards(rect.offsetMin.y, Screen.height, consoleHeight * Time.deltaTime / closeSpeed));
            rect.offsetMax = new Vector2(0, Mathf.MoveTowards(rect.offsetMax.y, consoleHeight, consoleHeight * Time.deltaTime / closeSpeed));
        }
    }

    void ProcessKeys()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            historyPosition = Mathf.Min(++historyPosition, historyBuffer.Count);
            consoleInput.text = historyBuffer[historyPosition-1];
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            historyPosition = Mathf.Max(--historyPosition, 1);
            consoleInput.text = historyBuffer[historyPosition - 1];
        }
    }

    // Update is called once per frame
    void Update () {

        HandleVisibility();

        if(isOpen)
        {
            ProcessKeys();         
        }

    }

    void LateUpdate()
    {
        if(moveToEnd)
        {
            consoleInput.MoveTextEnd(false);
            moveToEnd = false;
        }
    }
}
