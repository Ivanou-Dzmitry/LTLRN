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
            Transparent = 2
        }

        [Header("Style")]
        [SerializeField] private Image panelImage;
        [SerializeField] private UIColorPalette palette;
        [SerializeField] private PanelColor panelColor = PanelColor.Panel01;

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

            container.offsetMin = new Vector2 (0, height);
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
                _ => palette.Panel01
            };
        }

        // Runtime API
        public void SetPanelColor(PanelColor color)
        {
            panelColor = color;
            ApplyPanelColor();
        }
    }
}
