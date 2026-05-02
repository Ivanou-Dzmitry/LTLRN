using UnityEngine;
using UnityEngine.UI;

namespace LTLRN.UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private string id = "";

        public string ID { get { return id; } }

        [SerializeField] private RectTransform container = null;

        private bool initialized = false;
        public bool IsInitialized { get { return initialized; } }

        private bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        private Canvas canvas = null;
        public Canvas Canvas { get { return canvas; } set { canvas = value; } }

        public enum PanelColor
        {
            Panel01 = 0,
            Panel02 = 1,
            Transparent = 2,
            Transparent50 = 3,
            Transparent10 = 4
        }

        [Header("Style")]
        [SerializeField] private Image panelImage;
        public UIColorPalette palette;
        [SerializeField] private PanelColor panelColor = PanelColor.Panel01;

        [Header("UI")]
        public Canvas canvasRoot;
        public RectTransform panel_01;
        public RectTransform panel_02;
        public RectTransform panel_03;

        // for ui layout
        [SerializeField] private float PANEL_01_HEIGHT = 395f;
        [SerializeField] private float PANEL_03_HEIGHT = 200f;

        public float panel02Height;

        public virtual void Awake()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            if (initialized) { return; }
            initialized = true;

            if (panelImage == null)
                panelImage = GetComponent<Image>();

            ApplyPanelColor();

            Close();
        }

        public virtual void Open()
        {
            if (initialized == false) { Initialize(); }
            transform.SetAsLastSibling();
            container.gameObject.SetActive(true);
            isOpen = true;
        }

        public virtual void Close()
        {
            if (initialized == false) { Initialize(); }
            container.gameObject.SetActive(false);
            isOpen = false;
        }

        public virtual float GetSize()
        {
            if (initialized == false) { Initialize(); }

            return container.rect.height;
        }

        public virtual void SetBottom(float height)
        {
            if (initialized == false) { Initialize(); }

            Vector2 offsetMin = container.offsetMin;

            container.offsetMin = new Vector2(0, height);
        }

        private void ApplyPanelColor()
        {
            if (panelImage == null || palette == null)
                return;

            panelImage.color = GetPanelColor(panelColor);
        }

        private Color GetPanelColor(PanelColor color)
        {
            return color switch
            {
                PanelColor.Panel01 => palette.Panel01,
                PanelColor.Panel02 => palette.Panel02,
                PanelColor.Transparent => palette.TransparentPanel,
                PanelColor.Transparent50 => palette.Transparent50Panel,
                PanelColor.Transparent10 => palette.Transparent10Panel,
                _ => palette.Panel01
            };
        }

        // Runtime API
        public void SetPanelColor(PanelColor color)
        {
            panelColor = color;
            ApplyPanelColor();
        }


        public void SetPanelHeight()
        {
            Rect safeArea = Screen.safeArea;

            // Get canvas scale factor
            float scaleFactor = canvasRoot.scaleFactor;

            // Calculate available height in safe area (accounting for canvas scale)
            float safeAreaHeight = safeArea.height / scaleFactor;

            // Calculate panel_02 height
            panel02Height = safeAreaHeight - PANEL_01_HEIGHT - PANEL_03_HEIGHT;
            panel02Height = Mathf.Max(panel02Height, 0f);

            // Set heights
            if(panel_01 != null)
                panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL_01_HEIGHT);
            
            if (panel_02 != null)
                panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);

            if (panel_03 != null)
                panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, PANEL_03_HEIGHT);
        }
    }
}
