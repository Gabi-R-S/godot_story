using Godot;
using System;

public partial class Textbox : Node2D
{
    bool keyboardSound = true;
    public override void _Ready() {
        textLabel = GetChild<RichTextLabel>(0);
        textLabel.Text = "";
        Hide();

    }
    public void SetColor(Color color) {
        if (textLabel == null) {
            textLabel = GetChild<RichTextLabel>(0);
            textLabel.Text = "";
        }
        
        textLabel?.AddThemeColorOverride("default_color", color);
    }
    public void SetCharactersPerSecond(float cps) { 
        letterTime = 1.0f / cps;
    }

    Action writeCallback = null;
    string fullText = "";
    int currentIndex = 0;
    float letterTime = 0.05f; //seconds per letter
    float counter = 0;
    RichTextLabel textLabel;
    


    public void Write(string text,Action callback=null) { 
    currentIndex = 0;
        fullText = text;
        writeCallback = callback;
        textLabel.Text = "";
    }

    public override void _Process(double delta)
    {
        
        if (currentIndex >= fullText.Length && writeCallback != null)
        {
            writeCallback();
            writeCallback = null;
        }
        bool lettersAdded = false;
        counter += (float)delta;
        while (counter >= letterTime)
        {
            counter -=letterTime;
            if (currentIndex < fullText.Length)
            {
                lettersAdded = true;
                currentIndex++;

                textLabel.Text = fullText.Substring(0, currentIndex);
                
            }
            
            
        }
        if (lettersAdded && keyboardSound) {
            if (AudioManager.instance != null)
            {
                if (!AudioManager.instance.IsPlayingSound("text_sound", this))
                {
                    AudioManager.instance.PlaySound("keyboard", false, this);
                }
            }
        }
    }

}
