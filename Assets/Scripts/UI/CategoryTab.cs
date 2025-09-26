using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGameHub.UI
{
    /// <summary>
    /// Category tab for filtering games
    /// </summary>
    public class CategoryTab : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI tabText;
        [SerializeField] private Button tabButton;
        [SerializeField] private Image tabBackground;
        [SerializeField] private Image tabIcon;
        
        [Header("Visual States")]
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color normalTextColor = Color.black;
        
        // Private variables
        private string categoryName;
        private bool isSelected;
        
        // Events
        public System.Action<string> OnTabSelected;

        public string CategoryName => categoryName;

        private void Awake()
        {
            if (tabButton != null)
            {
                tabButton.onClick.RemoveAllListeners();
                tabButton.onClick.AddListener(OnTabClicked);
            }
        }

        public void Setup(string category, bool selected = false)
        {
            categoryName = category;
            
            // Set tab text
            if (tabText != null)
            {
                tabText.text = GetDisplayNameForCategory(category);
            }
            
            // Load category icon
            LoadCategoryIcon(category);
            
            // Set initial selection state
            SetSelected(selected);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            // Update background color
            if (tabBackground != null)
            {
                tabBackground.color = isSelected ? selectedColor : normalColor;
            }
            
            // Update text color
            if (tabText != null)
            {
                tabText.color = isSelected ? selectedTextColor : normalTextColor;
            }
            
            // Update button interactability (optional)
            if (tabButton != null)
            {
                tabButton.interactable = !isSelected;
            }
        }

        private void LoadCategoryIcon(string category)
        {
            if (tabIcon == null) return;
            
            // Load icon from Resources
            string iconName = GetIconNameForCategory(category);
            Sprite iconSprite = Resources.Load<Sprite>($"CategoryIcons/{iconName}");
            
            if (iconSprite != null)
            {
                tabIcon.sprite = iconSprite;
                tabIcon.gameObject.SetActive(true);
            }
            else
            {
                tabIcon.gameObject.SetActive(false);
            }
        }

        private string GetDisplayNameForCategory(string category)
        {
            return category switch
            {
                "All" => "All Games",
                "Puzzle" => "Puzzle",
                "Action" => "Action",
                "Memory" => "Memory",
                "Strategy" => "Strategy",
                "Reflex" => "Reflex",
                "Casual" => "Casual",
                "Word" => "Word",
                "Math" => "Math",
                _ => category
            };
        }

        private string GetIconNameForCategory(string category)
        {
            return category switch
            {
                "All" => "all_games",
                "Puzzle" => "puzzle",
                "Action" => "action",
                "Memory" => "memory",
                "Strategy" => "strategy",
                "Reflex" => "reflex",
                "Casual" => "casual",
                "Word" => "word",
                "Math" => "math",
                _ => "default"
            };
        }

        private void OnTabClicked()
        {
            if (!isSelected)
            {
                OnTabSelected?.Invoke(categoryName);
            }
        }
    }
}