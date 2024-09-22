using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CharacterInformation
{
	public string m_characterName;
	public Sprite m_characterImage;
	public Color m_characterColor;

	public CharacterInformation(string characterName, Sprite characterImage, Color characterColor) 
	{ 
		m_characterName = characterName;
		m_characterImage = characterImage;
		m_characterColor = characterColor;
	}

}

[CreateAssetMenu(fileName = "Characters", menuName = "ScriptableObjects/CharacterList", order = 1)]
public class CharacterList : ScriptableObject
{
    public CharacterInformation[] characters;
}


