using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomLog
{
    public static void RedText(string text) => Debug.Log($"<color=red>{text}</color>");
    public static void GreenText(string text) => Debug.Log($"<color=#228B22>{text}</color>");
    public static void YellowText(string text) => Debug.Log($"<color=yellow>{text}</color>");
    public static void PurpleText(string text) => Debug.Log($"<color=purple>{text}</color>");
    public static void BlueText(string text)=> Debug.Log($"<color=blue>{text}</color>");

    public static void TextWithWordHighlight(string text, string word) => Debug.Log($"{text}:<color=yellow>{word}</color>");

    public static void RedText_Warning(string text) => Debug.LogWarning($"<color=red>{text}</color>");
    public static void GreenText_Warning(string text) => Debug.LogWarning($"<color=#228B22>{text}</color>");
    public static void PurpleText_Warning(string text) => Debug.LogWarning($"<color=purple>{text}</color>");

}
