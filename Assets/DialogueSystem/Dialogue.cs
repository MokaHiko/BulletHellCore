using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public string identifier;
    public Sprite characterBanner;
    public Sprite characterPP; // PP is Profile Picture
    public bool stopTime;

    [TextArea(3, 10)]
    public string[] sentences;

}
