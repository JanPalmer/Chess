using System.Collections.Generic;
using UnityEngine;

public class SpriteLibrary : MonoBehaviour
{
    // References for all the sprites that a chesspiece can be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    private Dictionary<string, Sprite> spriteDictionary;

    public void Initialize()
    {
        spriteDictionary = new Dictionary<string, Sprite>(){
            { "black_queen", black_queen },
            { "black_knight", black_knight },
            { "black_bishop", black_bishop },
            { "black_king", black_king },
            { "black_rook", black_rook },
            { "black_pawn", black_pawn },
            { "white_queen", white_queen },
            { "white_knight", white_knight },
            { "white_bishop", white_bishop },
            { "white_king", white_king },
            { "white_rook", white_rook },
            { "white_pawn", white_pawn },
        };
    }

    public Sprite SelectSprite(string name)
    {
        if (spriteDictionary.TryGetValue(name, out var sprite))
        {
            return sprite;
        }
        else
        {
            return null;
        }
    }

    public override string ToString()
    {
        string result = string.Empty;

        foreach (var sprite in spriteDictionary)
        {
            result += $"{sprite.Key} - {sprite.Value != null}\n";
            //Debug.Log($"{sprite.Key} - {sprite.Value != null}");
        }

        return result;
    }
}
