using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteEntry
{
    public string id;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Sprites Taker/SpriteDictionary")]
public class SpriteDatabase : ScriptableObject
{
    public List<string> id = new();
    public List<Sprite> sprites = new();

    private Dictionary<string, Sprite> spritesDict = new();

    public void StartWork()
    {
        for(int i = 0; i < id.Count; i++)
        {
            spritesDict.Add(id[i],sprites[i]);
        }
    }

    public Sprite GetSprite(string id)
    {
        spritesDict.TryGetValue(id, out Sprite sprite);
        return sprite;
    }
}
