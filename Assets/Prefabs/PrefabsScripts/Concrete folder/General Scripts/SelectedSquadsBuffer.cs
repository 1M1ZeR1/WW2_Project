using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectedSquadsBuffer
{
    private List<AbstractSquad> squadsWhatStartChoise = new();

    public void ResetSelectedSquads() { squadsWhatStartChoise.Clear(); }

    public void AddToSelectedSquads_Guaranteed(AbstractSquad squad){if (!squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Add(squad);}
    public void AddToSelectedSquads(AbstractSquad squad)
    {
        if (squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Remove(squad);
        else squadsWhatStartChoise.Add(squad);
    }

    public int GetCount() { return squadsWhatStartChoise.Count;}
    public void Clear() { squadsWhatStartChoise.Clear(); }

    public List<AbstractSquad> GetCopyOfList() { return squadsWhatStartChoise.ToList(); }
}
