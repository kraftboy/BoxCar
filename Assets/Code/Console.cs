using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Console : MonoBehaviour {

    static public Console instance;

    private bool _isOpen;
    public bool isOpen { get { return _isOpen; } }

    private int consoleWidth;
    private int consoleHeight;
    private float closeSpeed = 0.1f;

    private InputField consoleInput;
    private Text consoleLogText;
    private int logLineCount;

    List<string> consoleLog = new List<string>();
    List<string> historyBuffer = new List<string>();
    private int historyPosition = 0;

    // Use this for initialization
    void Awake() {

        instance = this;

        Hide();

        SetupGUI();

    }

    void SetupGUI()
    {
        consoleWidth = Camera.main.pixelWidth;
        consoleHeight = Camera.main.pixelHeight / 2;

        GameObject panelObject = GameObject.Find("ConsolePanel");
        if (panelObject)
        {
            RectTransform rectTransform = panelObject.transform as RectTransform;
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, -consoleHeight);
        }
        else
        {
            Debug.Log("ConsolePanel not found.");
        }

        GameObject inputGameObject = GameObject.Find("ConsoleInput");
        if (inputGameObject)
        {
            consoleInput = inputGameObject.GetComponent<InputField>();
            consoleInput.onEndEdit.AddListener(delegate { ProcessInput(consoleInput); });
            consoleInput.onValueChanged.AddListener(delegate { ProcessKey(consoleInput); });

            Text consoleInputText = inputGameObject.GetComponentInChildren<Text>();
        }
        else
        {
            Debug.Log("ConsoleInput not found.");
        }

        GameObject logObject = GameObject.Find("ConsoleLog");
        if (logObject)
        {
            consoleLogText = logObject.GetComponent<Text>();
            consoleLogText.text = "fooo";
            RectTransform consoleLogRect = consoleLogText.transform as RectTransform;
            consoleLogRect.offsetMin = new Vector2(0, 18);
            consoleLogRect.offsetMax = new Vector2(0, 0);

            // calculate number of lines that can be displayed
            Vector3[] worldCorners = new Vector3[4];
            consoleLogText.rectTransform.GetWorldCorners(worldCorners);
            logLineCount = (int)Mathf.Abs(((worldCorners[1].y - worldCorners[0].y) / consoleLogText.font.lineHeight));
            logLineCount += 3;

            TextGenerationSettings generationSettings = consoleLogText.GetGenerationSettings(consoleLogText.rectTransform.rect.size);
            float textHeight = consoleLogText.cachedTextGeneratorForLayout.GetPreferredHeight("", generationSettings);

            consoleLog = new List<string>(Enumerable.Repeat("", logLineCount - 1));
            consoleLog.Add((GameObject.Find("GameController").GetComponent<GameController>() as GameController).GetVersionString());
            consoleLogText.text = string.Join("\n", consoleLog.ToArray());
        }
        else
        {
            Debug.Log("ConsoleLog not found.");
        }

        UIUtils.MoveToLayer(transform, LayerMask.NameToLayer("UI"));

        CommandManager cmdMgr = Toolbox.RegisterComponent<CommandManager>();
        cmdMgr.AddCommand("clear", ClearConsole);
    }

    string ClearConsole(string[] args)
    {
        consoleLogText.text = "";
        return "";
    }

    string consolePrompt = ">";
    string ConsolePrompt()
    {
        return consolePrompt;
    }

    void ProcessKey(InputField input)
    {
        InputField inputField = GetComponentInChildren<InputField>();
        if (inputField.text.Length < consolePrompt.Length)
        {
            inputField.text = ConsolePrompt();
            inputField.caretPosition = ConsolePrompt().Length;
        }
    }

    void ProcessKeys()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        bool setFromHistory = false;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            setFromHistory = true;
            ++historyPosition;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            setFromHistory = true;
            --historyPosition;
        }

        if (!setFromHistory)
            return;

        historyPosition = Mathf.Clamp(historyPosition, 0, historyBuffer.Count);

        string setTextTo;
        if (historyPosition == 0)
        {
            setTextTo = "";
        }
        else
        {
            setTextTo = historyBuffer[historyPosition - 1];
        }

        consoleInput.text = ConsolePrompt() + setTextTo;
        consoleInput.MoveTextEnd(false);
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
        if (command == "`" || command == "")
        {
            FocusInputField();
            return;
        }


        historyBuffer.Insert(0, command);
        consoleLog.Add(consoleInput.text);

        CommandManager cmdMgr = Toolbox.RegisterComponent<CommandManager>();
        string output = "";
        string commandString = command.Split().First();
        string[] args = command.Split().Skip(1).ToArray();
        if (cmdMgr.ProcessCommand(ref output, commandString, args))
        {
            consoleLog.Add(output);
            historyPosition = 0;
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
        ResetConsoleInputText();
        FocusInputField();
    }

    private void FocusInputField()
    {
        InputField inputField = GetComponentInChildren<InputField>();
        if (inputField)
        {
            inputField.ActivateInputField();
            inputField.text = ConsolePrompt();
            StartCoroutine(MoveTextEnd_NextFrame());
        }
    }

    IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0; // Skip the first frame in which this is called.
        consoleInput.MoveTextEnd(false); // Do this during the next frame.
    }

    public void ResetConsoleInputText()
    {
        InputField inputField = GetComponentInChildren<InputField>();
        inputField.text = ConsolePrompt();
        FocusInputField();
    }

    public void Show()
    {
        _isOpen = true;
        FocusInputField();
    }

    public void Hide()
    {
        _isOpen = false;
        if(consoleInput)
        {
            consoleInput.DeactivateInputField();
        }
    }

    void HandleVisibility()
    {
        RectTransform rect = GetComponentInChildren<Image>().transform as RectTransform;
        Vector2 wPos = rect.position;

        consoleHeight = Camera.main.pixelHeight / 2;

        if (isOpen)
        {
            rect.offsetMin = new Vector2(0, Mathf.MoveTowards(rect.offsetMin.y, Camera.main.pixelHeight - consoleHeight, consoleHeight * Time.deltaTime / closeSpeed));
            rect.offsetMax = new Vector2(0, Mathf.MoveTowards(rect.offsetMax.y, 0, consoleHeight * Time.deltaTime / closeSpeed));
        }
        else
        {
            rect.offsetMin = new Vector2(0, Mathf.MoveTowards(rect.offsetMin.y, Camera.main.pixelHeight, consoleHeight * Time.deltaTime / closeSpeed));
            rect.offsetMax = new Vector2(0, Mathf.MoveTowards(rect.offsetMax.y, consoleHeight, consoleHeight * Time.deltaTime / closeSpeed));
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
}
