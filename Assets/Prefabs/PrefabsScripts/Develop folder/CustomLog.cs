using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomLog
{
    public static void RedText(string text) => Debug.Log($"<color=red>{text}</color>");
    public static void GreenText(string text) => Debug.Log($"<color=#228B22>{text}</color>");
    public static void YellowText(string text) => Debug.Log($"<color=yellow>{text}</color>");

    public static void TextWithWordHighlight(string text, string word) => Debug.Log($"{text}:<color=yellow>{word}</color>");
}
