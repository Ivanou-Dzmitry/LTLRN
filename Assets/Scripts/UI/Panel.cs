using UnityEngine;
using UnityEngine.UI;

namespace LTLRN.UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private string id = "";

        public string ID { get { return id; } }

        [SerializeField] private RectTransform container = null;

        [SerializeField] private bool Resize = false;

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


            if (Resize)
                SetPanelHeight();
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

            if (canvasRoot == null)
                return;

            if(panel_01 == null || panel_02 == null)
                return;

            // Get canvas scale factor
            float scaleFactor = canvasRoot.scaleFactor;

            // Calculate available height in safe area (accounting for canvas scale)
            float safeAreaHeight = safeArea.height / scaleFactor;

            // Pixels cut off by notch/status bar (top) and home indicator (bottom).
            // Screen.safeArea origin is bottom-left, so top inset = screenHeight - (y + height).
            float topInsetPx    = Screen.height - (safeArea.y + safeArea.height);
            float bottomInsetPx = safeArea.y;

            float topInset    = topInsetPx / scaleFactor;
            float bottomInset = bottomInsetPx / scaleFactor;

            // Grow top/bottom panels by the inset so their background bleeds under the
            // notch / home indicator, while their content (anchored bottom/top respectively)
            // stays within the safe area. On phones with no notch, inset is 0 — no change.
            float panel01Height = PANEL_01_HEIGHT + topInset;
            float panel03Height = PANEL_03_HEIGHT + bottomInset;

            // If panel_03 doesn't exist in this scene, panel_02 becomes the bottom-most
            // panel — don't reserve space for a panel that isn't there.
            float reservedPanel03Height = panel_03 != null ? PANEL_03_HEIGHT : 0f;

            // Calculate panel_02 height
            panel02Height = safeAreaHeight - PANEL_01_HEIGHT - reservedPanel03Height;
            panel02Height = Mathf.Max(panel02Height, 0f);

/*            Debug.Log($"[Panel:{name}] Screen=({Screen.width}x{Screen.height}) safeArea={safeArea} " +
                      $"scaleFactor={scaleFactor} safeAreaHeight={safeAreaHeight} " +
                      $"topInset={topInset} bottomInset={bottomInset} " +
                      $"PANEL_01_HEIGHT={PANEL_01_HEIGHT} reservedPanel03Height={reservedPanel03Height} " +
                      $"-> panel01Height={panel01Height} panel02Height={panel02Height} panel03Height={panel03Height}");*/

            // Set heights
            if(panel_01 != null)
                panel_01.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel01Height);

            if (panel_02 != null)
                panel_02.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel02Height);

            if (panel_03 != null)
                panel_03.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel03Height);
        }
    }
}
