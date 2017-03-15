using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugWidget : MonoBehaviour {

    public Font debugFont;

    private GameObject debugWidget;
    private Text debugText;

    private float horizontalOffset = 2.0f;
    private float verticalOffset = 0.5f;

    private float widgetWidth = 300.0f;
    private float widgetHeight = 100.0f;
    private float scalingFactor = 0.01f;

    abstract class IDebugWidgetPage
    {
        public abstract string GetPageContents();
    }

    class ComponentPage : IDebugWidgetPage
    {
        public ComponentPage(Transform rootTransform)
        {
            root = rootTransform;
        }

        Transform root;
        public override string GetPageContents()
        {
            string contents = "";
            foreach(Component comp in root.GetComponents<Component>())
            {
                contents += string.Format("{0}\n", comp.ToString());
            }

            return contents;
        }
    }
    private List<IDebugWidgetPage> debugPages = new List<IDebugWidgetPage>();
    private int currentPage = 0;

    void Start () {

        debugWidget = new GameObject("DebugWidget_" + gameObject.name);
        debugWidget.transform.SetParent(transform, false);

        // canvas
        Canvas debugCanvas = debugWidget.AddComponent<Canvas>();
        CanvasScaler canvasScaler = debugWidget.AddComponent<CanvasScaler>();
        canvasScaler.scaleFactor = 20;
        canvasScaler.dynamicPixelsPerUnit = 3;

        // render mode
        debugCanvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.worldCamera = Camera.main;

        widgetHeight = debugFont.lineHeight * 10;

        RectTransform rect = debugCanvas.transform as RectTransform;
        rect.sizeDelta = new Vector2(widgetWidth, widgetHeight);
        rect.localScale = new Vector2(scalingFactor, scalingFactor);
        rect.localPosition = new Vector3(horizontalOffset, verticalOffset, 0.0f);
        rect.pivot = new Vector2(0.5f, 0.0f);

        CreateText(debugWidget);


        UIUtils.MoveToLayer(debugWidget.transform, LayerMask.NameToLayer("UI"));

        debugPages.Add(new ComponentPage(transform.root));
    }

    void CreateText(GameObject parent)
    {
        GameObject textObject = new GameObject("TextWidget");
        textObject.transform.SetParent(parent.transform, false);
        debugText = textObject.AddComponent<Text>();
        debugText.fontSize = 76;
        debugText.color = Color.black;
        debugText.font = debugFont;
        debugText.horizontalOverflow = HorizontalWrapMode.Overflow;
        debugText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = debugText.transform as RectTransform;
        textRect.sizeDelta = new Vector2(300, 20);
        textRect.anchorMax = new Vector2(0.0f, 0.8f);
        textRect.anchorMin = new Vector2(0.0f, 0.8f);
        textRect.pivot = Vector2.zero;
        textRect.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
    // Update is called once per frame
    void Update () {

        Vector3 relativePosition = debugWidget.transform.parent.transform.position +
                                   Camera.main.transform.right * horizontalOffset +
                                   Camera.main.transform.up * verticalOffset;

        debugWidget.transform.rotation = Camera.main.transform.rotation;
        debugWidget.transform.position = relativePosition;

        RenderCurrentPage();
    }

    void RenderCurrentPage()
    {
        debugText.text = debugPages[currentPage].GetPageContents();
    }
}
