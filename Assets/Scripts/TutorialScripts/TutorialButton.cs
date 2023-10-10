using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TutorialScripts
{
	public class TutorialButton : MonoBehaviour
	{
		[SerializeField] private Button tutorialButton;
		private const string TutorialScene = "TutorialScene";

		private void Start()
		{
			tutorialButton.onClick.AddListener(TurnTutorialScene);
		}

		private void TurnTutorialScene()
		{
			SceneManager.LoadScene(TutorialScene);
		}
	}
}