using UnityEngine;

public class ColorTweaks : MonoBehaviour
{
    public Color[] hairShades;
    public Color[] skinTones;
    public Color[] playerColors;

    public enum ColorPalette
    {
        HairShades,
        SkinTones,
        PlayerColors
    }
    public ColorPalette palette;
    public int colorIndex;
    public bool showMe;
    public bool setThis;

    public float red;
    public float green;
    public float blue;

    public Color currentColor;


    void Start()
    {
        hairShades = new Color[8];
        skinTones = new Color[8];
        playerColors = new Color[16];

        for (int i =0; i < 8; i++)
        {
            // load hair color
            hairShades[i] = PlayerSystem.GetPlayerHairColor((PlayerHairColor)i);
            // load skin color
            skinTones[i] = PlayerSystem.GetPlayerSkinColor((PlayerSkinColor)i);
        }
        for (int i = 0; i< 16; i++)
        {
            // load player color
            playerColors[i] = PlayerSystem.GetPlayerColor((PlayerColor)i);
        }
    }

    void Update()
    {
        if (showMe)
        {
            showMe = false;
            switch (palette)
            {
                case ColorPalette.HairShades:
                    red = hairShades[colorIndex].r;
                    green = hairShades[colorIndex].g;
                    blue = hairShades[colorIndex].b;
                    break;
                case ColorPalette.SkinTones:
                    red = skinTones[colorIndex].r;
                    green = skinTones[colorIndex].g;
                    blue = skinTones[colorIndex].b;
                    break;
                case ColorPalette.PlayerColors:
                    red = playerColors[colorIndex].r;
                    green = playerColors[colorIndex].g;
                    blue = playerColors[colorIndex].b;
                    break;
            }
            currentColor = new Color(red, green, blue, 1f);
        }

        if (setThis)
        {
            setThis = false;
            switch (palette)
            {
                case ColorPalette.HairShades:
                    hairShades[colorIndex].r = red;
                    hairShades[colorIndex].g = green;
                    hairShades[colorIndex].b = blue;
                    break;
                case ColorPalette.SkinTones:
                    skinTones[colorIndex].r = red;
                    skinTones[colorIndex].g = green;
                    skinTones[colorIndex].b = blue;
                    break;
                case ColorPalette.PlayerColors:
                    playerColors[colorIndex].r = red;
                    playerColors[colorIndex].g = green;
                    playerColors[colorIndex].b = blue;
                    break;
            }
        }
    }
}
