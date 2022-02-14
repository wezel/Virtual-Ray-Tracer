using System;
using _Project.Ray_Tracer.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Toolbar
{
    /// <summary>
    /// A UI class that manages a dropdown menu for adding objects to the scene.
    /// </summary>
    public class CreateObjectDropdown : MonoBehaviour
    {
        [SerializeField]
        private bool interactable;
        /// <summary>
        /// Whether this <see cref="CreateObjectDropdown"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set 
            {
                interactable = value;
                openButton.interactable = interactable;
            }
        }
        
        [SerializeField]
        private Button itemPrefab;
        [SerializeField]
        private Button openButton;
        [SerializeField]
        private VerticalLayoutGroup items;
        
        public bool DropDownHovered { get; set; }

        /// <summary>
        /// Open the dropdown menu.
        /// </summary>
        public void Open()
        {
            openButton.gameObject.SetActive(false);
            items.gameObject.SetActive(true);
        }

        /// <summary>
        /// Close the dropdown menu.
        /// </summary>
        public void Close()
        {
            items.gameObject.SetActive(false);
            openButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Toggle the dropdown menu. If the dropdown is closed it will now be opened and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (!items.gameObject.activeSelf)
                Open();
            else
                Close();
        }

        private void OnClick(RTSceneManager.ObjectType type)
        {
            RTSceneManager.Get().CreateObject(type);
            Close();
        }

        private void Awake()
        {
            openButton.onClick.AddListener(Toggle);

            // Update interactability based on serialized value in inspector.
            Interactable = interactable;
            
        }

        private void Start()
        {
            // Add the button that opens the dropdown.
            Button itemButton = Instantiate(openButton, items.transform);
            itemButton.onClick.AddListener(Toggle);

            // Add the buttons that create the objects.
            Array objectTypes = Enum.GetValues(typeof(RTSceneManager.ObjectType));
            foreach(RTSceneManager.ObjectType objectType in objectTypes)
            {
                itemButton = Instantiate(itemPrefab, items.transform);
                itemButton.GetComponentInChildren<TextMeshProUGUI>().text = objectType.ToString();
                itemButton.onClick.AddListener(() => OnClick(objectType));
            }
            items.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30 * (objectTypes.Length + 1));
        }

        private void Update()
        {
            // Close the dropdown if it opened and we click outside of it.
            if (!DropDownHovered && Input.GetMouseButtonDown(0) && items.gameObject.activeSelf)
                Close();
        }
    }
}