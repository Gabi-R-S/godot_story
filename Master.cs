using Godot;
using System;

public partial class Master : Node2D
{
    [Export]
    string startSceneName="scene_1";
    [Export]
    Interpreter interpreter;
    bool started = false;
    public override void _Input(InputEvent @event)
    {
        if (!started && @event.IsActionPressed("ui_accept"))
        {
            started = true;
            var filenameDic = new System.Collections.Generic.Dictionary<string, string>();
            filenameDic["scene_1"] = "scene1.scn";
            filenameDic["scene_2"] = "scene2.scn";
            filenameDic["scene_3"] = "scene3.scn";
            interpreter?.RegisterFilenames(filenameDic);
            interpreter?.StartAnimation(startSceneName);
        }
    }
}
