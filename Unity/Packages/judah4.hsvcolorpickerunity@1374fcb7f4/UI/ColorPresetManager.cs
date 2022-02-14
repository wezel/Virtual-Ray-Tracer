using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HSVPicker
{
    public class ColorPresetManager : MonoBehaviour
    {
	    public ColorPicker picker;
	    public Image createButton;

	    [SerializeField]
	    private ColorPreset template;
	    
        private bool removeMode = false;
        private List<ColorPreset> presets;
        void Awake()
	    {
		    presets = new List<ColorPreset>(picker.Setup.MaxPresets);
		    foreach (var defaultPresetColor in picker.Setup.DefaultPresetColors)
			    CreatePreset(defaultPresetColor);

		    picker.onValueChanged.AddListener(ColorChanged);
		    SetMode(false);
	    }

        public void CreatePresetButton()
        {
	        CreatePreset(picker.CurrentColor);
        }
        
        private void CreatePreset(Color color)
        {
	        if(presets.Count >= picker.Setup.MaxPresets)
		        return;
	        
	        ColorPreset preset = Instantiate(template, transform);
	        presets.Add(preset);
	        preset.Color = color;
	        
	        createButton.transform.SetAsLastSibling();
	        if(presets.Count >= picker.Setup.MaxPresets)
		        createButton.gameObject.SetActive(false);

	        preset.gameObject.SetActive(true);
        }

	    public void PresetClicked(ColorPreset sender)
	    {
		    if (removeMode)
		    {
			    presets.Remove(sender);
			    Destroy(sender.gameObject);
			    
			    if(presets.Count < picker.Setup.MaxPresets)
				    createButton.gameObject.SetActive(true);
			    
			    return;
		    }
		    
		    picker.CurrentColor = sender.Color;
	    }
	    
	    private void ColorChanged(Color color)
	    {
		    createButton.color = color;
	    }

	    private void SetMode(bool value)
	    {
		    removeMode = value;
		    foreach (var preset in presets)
			    preset.TextEnable = value;
	    }

	    private void Update()
        {
	        if (Input.GetKeyDown(KeyCode.LeftControl)) 
		        SetMode(true);

	        if (!Input.GetKey(KeyCode.LeftControl) && removeMode)
		        SetMode(false);
        }
    }
}