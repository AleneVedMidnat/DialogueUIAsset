using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueView : DialogueViewBase
{
	List<RectTransform> UIelements;
	List<RectTransform> currentOptionLines;
	[SerializeField] CharacterList characterList;
	[SerializeField] Image characterImage;
	int currentOptionPosition = -1;
	Action advanceHandler = null;
	Action<int> SelectOption = null;

	[SerializeField] Color dulledOptionColour;
	[SerializeField] Color StandardTextColour;
	[SerializeField] TMP_FontAsset fontAsset;
	[SerializeField] float fontSize;
	[SerializeField] float spaceBetweenLines;


	PlayerInput m_input;

	private void OnEnable()
	{
		UIelements = new List<RectTransform>();
		m_input = GetComponent<PlayerInput>();
		m_input.currentActionMap.FindAction("NextLine").performed += CallNextLine;
		m_input.currentActionMap.FindAction("PickOptions").performed += OptionPicker;
	}

	void CallNextLine(InputAction.CallbackContext context)
	{
		if (currentOptionLines != null)
		{
			RemoveLines();
			SelectOption(currentOptionPosition);
		}
		else
		{
			UserRequestedViewAdvancement();
		}
		
	}

	void OptionPicker(InputAction.CallbackContext context)
	{
		if (currentOptionLines == null)
			return;

		float direction = context.ReadValue<float>();

		switch (direction)
		{
			case 1:
				if (currentOptionPosition == 0)
					break;

				currentOptionLines[currentOptionPosition].gameObject.GetComponent<TextMeshProUGUI>().color = dulledOptionColour;
				currentOptionPosition -= 1;
				currentOptionLines[currentOptionPosition].gameObject.GetComponent<TextMeshProUGUI>().color = StandardTextColour;

				break;

			case -1:
				if (currentOptionPosition == currentOptionLines.Count - 1)
					break;

				currentOptionLines[currentOptionPosition].gameObject.GetComponent<TextMeshProUGUI>().color = dulledOptionColour;
				currentOptionPosition += 1;
				currentOptionLines[currentOptionPosition].gameObject.GetComponent<TextMeshProUGUI>().color = StandardTextColour;
				break;
		}
	}

	public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			if (gameObject.activeInHierarchy == false)
			{
				onDialogueLineFinished();
				return;
			}

			RectTransform currentLineRect = OutputLine(dialogueLine);
        
        if (UIelements.Count > 0)
		{
			TextMeshProUGUI previousLine = UIelements[UIelements.Count - 1].GetComponent<TextMeshProUGUI>();
			previousLine.color = new Color(previousLine.color.r, previousLine.color.g, previousLine.color.b, previousLine.color.a / 2);
		}
        UIelements.Add(currentLineRect);


        advanceHandler = requestInterrupt;
		onDialogueLineFinished();
	}

	public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
	{
		if (SelectOption != null)
		{
			SelectOption = null;
		}
		SelectOption = onOptionSelected;

		 if (currentOptionLines == null && dialogueOptions.Length > 0)
		{
			currentOptionLines = new List<RectTransform>();
		}

		for (int i = 0; i < dialogueOptions.Length; i++)
		{
			RectTransform currentLineRect = OutputLine(dialogueOptions[i].Line);
			Debug.Log(currentLineRect.gameObject.GetComponent<TextMeshProUGUI>().color);
			currentOptionLines.Add(currentLineRect);
		}

		currentOptionPosition = 0;
		for (int i = 0; i < currentOptionLines.Count; i++)
		{
			currentOptionLines[i].GetComponent<TextMeshProUGUI>().color = dulledOptionColour;
		}
		currentOptionLines[0].GetComponent<TextMeshProUGUI>().color = StandardTextColour;

	}

	private RectTransform OutputLine(LocalizedLine dialogueLine)
	{
		CharacterInformation character = CheckIfCharacterInDatabase(dialogueLine.CharacterName);
		string newDialogueLine;
		if (character != null)
		{
			Debug.Log("charactr not null");
			newDialogueLine = "<b><color=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(character.m_characterColor) + "> " + dialogueLine.CharacterName
				+ ": </color></b>" + dialogueLine.TextWithoutCharacterName.Text;
			characterImage.sprite = character.m_characterImage; //set sprite for current character
			
		}
		else
		{
			//if the character isnt found in the database it wont display the name before the dialogue line
			//and wont update to a new character picture.
			newDialogueLine = dialogueLine.TextWithoutCharacterName.Text;
		}

		GameObject currentLine = new GameObject();
        currentLine.transform.SetParent(this.transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).transform.GetChild(0), false);
        RectTransform currentLineRect = currentLine.AddComponent<RectTransform>();
		currentLineRect.sizeDelta = new Vector2(170, 0); ;
        currentLineRect.anchorMin = new Vector2(0.5f, 0);
		currentLineRect.anchorMax = new Vector2(0.5f, 0);
		currentLineRect.pivot = new Vector2(0.5f, 0);
		currentLineRect.localPosition = Vector3.zero; //new Vector3(100f, -130f, 0f);

        TextMeshProUGUI currentLineText = currentLine.AddComponent<TextMeshProUGUI>();
		currentLineText.textInfo.lineInfo[0].lineHeight = 0;
        currentLineText.color = StandardTextColour;
        currentLineText.text = newDialogueLine;
		currentLineText.font = fontAsset;
		currentLineText.fontSize = fontSize;
        currentLineText.textInfo.lineInfo[0].lineHeight = 0;

        ContentSizeFitter currentLineSizeFit = currentLine.AddComponent<ContentSizeFitter>();
		currentLineSizeFit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		currentLineSizeFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		Canvas.ForceUpdateCanvases();

        
        currentLineRect.localPosition = new Vector3(89.5f, -130f, 0f);
        

        Debug.Log(UIelements);
		if (UIelements.Count < 0)
		{
			Debug.Log("list was zero");
		}
		else
		{
			float lineCount = currentLineText.textInfo.lineCount + spaceBetweenLines;
            float yincrease = lineCount *  currentLineText.textInfo.lineInfo[0].lineHeight;

			for (int i = 0; i < UIelements.Count; i++)
			{
				//yincrease is only going up by 3 and idk why
				UIelements[i].localPosition = new Vector3(UIelements[i].localPosition.x, UIelements[i].localPosition.y + yincrease, UIelements[i].localPosition.z);
			}
			if (currentOptionLines != null)
			{
				if (currentOptionLines.Count > 0)
				{
					for (int i = 0; i < currentOptionLines.Count; i++)
					{
						currentOptionLines[i].localPosition = new Vector3(currentOptionLines[i].localPosition.x, currentOptionLines[i].localPosition.y + yincrease, currentOptionLines[i].localPosition.z);
					}
				}
			}
			return currentLineRect;
		}
		return null;
	}

	private void RemoveLines()
	{
		if (currentOptionLines == null)
			return;

		RectTransform currentLine = currentOptionLines[currentOptionPosition];
		currentLine.position = currentOptionLines[0].position;
		currentOptionLines[currentOptionPosition].gameObject.GetComponent<TextMeshProUGUI>().color = StandardTextColour;

        TextMeshProUGUI previousLine = UIelements[UIelements.Count - 1].GetComponent<TextMeshProUGUI>();
        previousLine.color = new Color(previousLine.color.r, previousLine.color.g, previousLine.color.b, previousLine.color.a / 2);

        UIelements.Add(currentLine);
        //currentLine.localPosition = new Vector3(89.5f, -130f, 0f);
        currentOptionLines.RemoveAt(currentOptionPosition);

		for (int i = 0; i < currentOptionLines.Count - 1 ; i++)
		{

            TextMeshProUGUI lineMesh = currentLine.GetComponent<TextMeshProUGUI>();
			float lineCount = lineMesh.textInfo.lineCount - 0.5f;

            float yincrease = lineCount * lineMesh.textInfo.lineInfo[0].lineHeight;
			
			for (int j = 0; j < UIelements.Count; j++)
			{
				UIelements[j].localPosition = new Vector3(UIelements[j].localPosition.x, UIelements[j].localPosition.y - yincrease, UIelements[j].localPosition.z);

            }
		}
		for (int i = currentOptionPosition; i < currentOptionLines.Count; i++)
		{
            TextMeshProUGUI lineMesh = currentLine.GetComponent<TextMeshProUGUI>();
            float lineCount = lineMesh.textInfo.lineCount - 0.5f;

            float yincrease = lineCount * lineMesh.textInfo.lineInfo[0].lineHeight;

            currentLine.localPosition = new Vector3(currentLine.localPosition.x, currentLine.localPosition.y - yincrease, currentLine.localPosition.z);

            
        }
		for (int i = 0; i < currentOptionLines.Count; i++)
		{
			Destroy(currentOptionLines[i].gameObject);
		}

        
        currentOptionLines = null;

	}


	CharacterInformation CheckIfCharacterInDatabase(string name)
	{
		for (int i = 0; i < characterList.characters.Length; i++)
		{
			if (name == characterList.characters[i].m_characterName)
			{
				return characterList.characters[i];
			}
		}
		return null;
	}

	public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			if (gameObject.activeInHierarchy == false)
			{
				onDialogueLineFinished();
				return;
			}

			advanceHandler = null;

			onDialogueLineFinished();
		}
		public override void DismissLine(Action onDismissalComplete)
		{
			if (gameObject.activeInHierarchy == false)
			{
				onDismissalComplete();
				return;
			}

			advanceHandler = () =>
			{
				advanceHandler = null;
				onDismissalComplete();
			};
		}
		public override void UserRequestedViewAdvancement()
		{
			advanceHandler?.Invoke();
		}

	public override void DialogueComplete()
	{
		for (int i = 0; i < UIelements.Count; i++)
		{
			Destroy(UIelements[i].gameObject);
		}
		UIelements.Clear();
        gameObject.SetActive(false);
	}
}
