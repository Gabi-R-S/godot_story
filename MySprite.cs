using Godot;
using System;

public partial class MySprite : Sprite2D
{
    public void SetInfo(Texture2D texture, Rect2? rect = null)
    {
        this.Texture = texture;
        if (rect != null)
        {
            this.RegionEnabled = true;
            this.RegionRect = rect.Value;
        }
        else
        {
            this.RegionEnabled = false;
        }
        //this.Centered = true; //IDK if this is a good idea...
    }

}
