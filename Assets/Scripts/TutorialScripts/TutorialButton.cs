using System;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialScripts
{
	public class TutorialButton : MonoBehaviour
	{
		[SerializeField] private Button tutorialButton;
		[SerializeField] private GameObject objectToShowHide;
		
		private void Start() => 
					tutorialButton.onClick.AddListener(ToggleObjectVisibility);
		private void OnDisable() =>
					objectToShowHide.SetActive(false);
		private void ToggleObjectVisibility() => 
					objectToShowHide.SetActive(!objectToShowHide.activeSelf);
	}
}