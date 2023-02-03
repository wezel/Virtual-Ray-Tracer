using System;
using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Scripts;
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

        private List<Button> buttons = new List<Button>();
        
        public bool DropDownHovered { get; set; }

        /// <summary>
        /// Open the dropdown menu.
        /// </summary>
        public void Open()
        {
            openButton.gameObject.SetActive(false);
            items.gameObject.SetActive(true);

            // Check if this object is unlocked or not
            int idx = 0;
            Array objectTypes = Enum.GetValues(typeof(RTSceneManager.ObjectType));
            foreach (RTSceneManager.ObjectType objectType in objectTypes)
            {
                bool enabled = GlobalManager.TutorialPoints >= (int)objectType || GlobalManager.Get().CheatMode;
                var transforms = buttons[idx].GetComponentsInChildren<RectTransform>(true);

                if (transforms.Length > 2) transforms[2].gameObject.SetActive(!enabled); // hide or show unlock panel
                buttons[idx].interactable = enabled;

                // Check if the light limit is reached
                if (enabled && objectType.ToString().Contains("Light"))
                {
                    RTSceneManager sceneManager = RTSceneManager.Get();
                    RTScene scene = sceneManager.Scene;
                    if ((objectType.ToString().Contains("Point") || objectType.ToString().Contains("Spot")) &&
                        scene.PointLights.Count + scene.SpotLights.Count >= sceneManager.PointSpotLightLimit)
                    {
                        buttons[idx].interactable = false;
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().text = "Point/Spot light limit reached!";
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().fontSize = 14;
                    }
                    else if (objectType.ToString().Contains("Area") && scene.AreaLights.Count >= sceneManager.AreaLightLimit)
                    {
                        buttons[idx].interactable = false;
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().text = "Area light limit reached!";
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().fontSize = 14;
                    }
                    else // Reset the button (text and fontSize) in case the text was changed before.
                    {
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().text = objectType.ToString();
                        buttons[idx].GetComponentInChildren<TextMeshProUGUI>().fontSize = itemPrefab.GetComponentInChildren<TextMeshProUGUI>().fontSize;
                    }
                }

                ++idx;
            }

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
            RTSceneManager sceneManager = RTSceneManager.Get();
            if      (type == RTSceneManager.ObjectType.PointLight) sceneManager.Scene.EnablePointLights = true;
            else if (type == RTSceneManager.ObjectType.SpotLight)  sceneManager.Scene.EnableSpotLights  = true;
            else if (type == RTSceneManager.ObjectType.AreaLight)  sceneManager.Scene.EnableAreaLights  = true;
            sceneManager.CreateObject(type);
            GlobalManager.ObjectsCreated++;
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
            foreach (RTSceneManager.ObjectType objectType in objectTypes)
            {
                itemButton = Instantiate(itemPrefab, items.transform);
                itemButton.GetComponentInChildren<TextMeshProUGUI>().text = objectType.ToString();

                // Set object unlock price
                itemButton.GetComponentsInChildren<TextMeshProUGUI>()[1].text = ((int)objectType).ToString();

                itemButton.onClick.AddListener(() => OnClick(objectType));
                buttons.Add(itemButton);
            }

            items.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 30 * (objectTypes.Length + 1));
        }

        private void Update()
        {
            // Close the dropdown if it opened and we click outside of it.
            if (!DropDownHovered && Input.GetMouseButtonDown(0) && items.gameObject.activeSelf)
                Close();
        }
    }
}