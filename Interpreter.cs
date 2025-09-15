using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Interpreter : Node2D
{
    [Export]
    PackedScene spriteModel;
    [Export]
    PackedScene textboxModel;

    Dictionary<string, string> nameToFilePaths;
    Dictionary<string, MySprite> sprites;
    Dictionary<string, Textbox> textboxes;
    Dictionary<string, CanvasItem> backgrounds;
    ColorRect fadeIn;
    [Export]
    ColorRect defaultBackground;
    Variant jsonData;
    int indexInSequence = 0;
    Godot.Collections.Array sequence;
 

    (string,string) ParseStringAtStartOfCommand(string command)//assumes first readable character is the " that delimits the string 
    {
        command= command.Trim();
        if (command.Length>0) {
            command = command.Substring(1);
        }
        GD.Print("command: "+command);
        string acc = "";
        bool escaped = false; 
        int i;
        for (i = 0; i < command.Length;i++) { 
            var chr=command[i];
            if (escaped)
            {
                if (chr == 'n')
                {
                    acc += '\n';
                }
                else { 
                acc+= chr;
                }
                    escaped = false;
            }
            else
            {
                GD.Print("here 1!");
                if (chr=='\\')
                {
                    GD.Print("here 3!");
                    escaped = true;
                }
                else if (chr == '\'')
                {
                    GD.Print("here 2!");
                    break;
                }
                else
                {

                    GD.Print("here 4: " +chr);
                    acc += chr;
                }
            }

        }


        return (acc,command.Substring(i+1));
    }

    bool IsTimeString(string str) {
        str = str.Trim().TrimEnd('s');
        if (float.TryParse(str, out float res)) {
            return true;
        }
        return false;
    }
    //for now, params are assumed to be floats.
    List<object> ParseParamsGeneral(string command)
    {
        var prts = command.Split('(', 2);
        if (prts.Length==1) {
            return new ();
        }
        var strParams = prts[1].TrimEnd(')').Split(",");


        List<object> obj = new ();

        foreach (var prm in strParams)
        {
            var acturalPrm = prm.Trim();//.TrimEnd('s');
                if (IsTimeString(acturalPrm)) { 
            acturalPrm= acturalPrm.TrimEnd('s');
            }
//            float res;
            if (float.TryParse(acturalPrm, out float res))
            { 
                obj.Add(res);
            }
            else
            {
                obj.Add(acturalPrm);
            }
        }

        return obj;
    }
    List<object> GetParamsOfSubcommandGeneral(string[] subcommands, string subcommand)
    {
        foreach (var sc in subcommands)
        {
            if (sc.Trim().Split('(')[0] == subcommand)
            {
                return ParseParamsGeneral(sc);
            }
        }


        return null;
    }



    //for now, params are assumed to be floats.
    List<float> ParseParams(string command) { 
        var prts = command.Split('(', 2);
        var strParams = prts[1].TrimEnd(')').Split(",");


        List<float> obj = new List<float>();

        foreach(var prm in strParams) {
           var acturalPrm= prm.Trim().TrimEnd('s');
           obj.Add( float.Parse(acturalPrm));
        }

        return obj;
    }
    List<float> GetParamsOfSubcommand(string[] subcommands, string subcommand) {
        foreach (var sc in subcommands)
        {
            if (sc.Trim().Split('(')[0] == subcommand)
            {
                return ParseParams(sc);
            }
        }


        return null;
    }
    bool ContainsSubCommand(string[] subcommands, string subcommand) {

        foreach (var sc in subcommands) {
            if (sc.Trim().Split('(')[0] == subcommand) {
                return true;
            }
        }
        return false;
    }

    void ProcessNext()
    {
        lock (this) { 
        try {
              
                bool allowContinue = true;
                while (allowContinue && indexInSequence < sequence.Count) { 
                var current = sequence[indexInSequence].AsString();
            var prts = current.Split(' ', 2);
            
            switch (prts[0]) {
                        case "stop":
                            {
                                prts = prts[1].Split(' ');
                                if (prts[0] == "sound")
                                {
                                    //impossible, because there is no reference.
                                }
                                else if (prts[0] == "music")
                                {
                                    AudioManager.instance?.StopMusic(prts[1]);
                                }
                            }
                            break;
                        case "play":
                            {
                                prts = prts[1].Split(' ');
                                if (prts[0] == "sound")
                                {
                                    AudioManager.instance?.PlaySound(prts[1]);
                                }
                                else if (prts[0] == "music")
                                {
                                    AudioManager.instance?.PlayMusic(prts[1]);
                                }
                            }
                            break;
                        case "text_speed": // text_speed <textbox> to(<float>)
                            { 
                                prts = prts[1].Split(' ');
                                var name = prts[0].Trim();
                                var prms = GetParamsOfSubcommand(prts,"to");
                                var speed = prms[0];
                                textboxes["name"].SetCharactersPerSecond(speed);
                            }

                            break;
                        case "show":
                    {
                        prts = prts[1].Split(' ');
                        Node2D obj;
                        var name = prts[1];
                        if (prts[0] == "sprite")
                        {
                            obj = sprites[name];
                        }
                        else
                        {
                            obj = textboxes[name];
                        }
                        obj.Show();

                        if (ContainsSubCommand(prts, "transition"))
                        {
                            Action callback = null;

                            var prms = GetParamsOfSubcommand(prts, "transition");
                            if (ContainsSubCommand(prts, "wait"))
                            {
                                allowContinue = false;
                                callback = () =>
                                {
                                    CallDeferred(nameof(ProcessNext));
                                };
                            }
                            obj.FadeIn(prms[0], callback);
                        }

                        List<float> parseParams = GetParamsOfSubcommand(prts, "at");
                        if (parseParams != null)
                        {
                            obj.Position = new Vector2(parseParams[0], parseParams[1]);
                                    if (parseParams.Count==3) { 
                                        obj.ZIndex = (int)parseParams[2];
                                    }
                        }

                        parseParams = GetParamsOfSubcommand(prts, "scale");
                        if (parseParams != null)
                        {
                            obj.Scale = new Vector2(parseParams[0], parseParams[1]);
                        }

                    }
                    break;
                case "fade_in":
                            {
                                prts = prts[1].Split(' ');
                                Action callback = null;
                                if (ContainsSubCommand(prts,"wait")) { 
                                    callback = () =>
                                        {
                                            CallDeferred(nameof(ProcessNext));
                                        };
                                    allowContinue = false;
                                }
                                fadeIn.Show();
                                fadeIn.FadeOut(float.Parse(prts[0].Trim().TrimEnd('s')),callback);
                            }
                    break;
                        case "fade_out":
                            {
                                prts = prts[1].Split(' ');
                                Action callback = null;
                                if (ContainsSubCommand(prts, "wait"))
                                {
                                    callback = () =>
                                    {
                                        CallDeferred(nameof(ProcessNext));
                                    };
                                    allowContinue = false;
                                }
                                fadeIn.Show();
                                fadeIn.FadeIn(float.Parse(prts[0].Trim().TrimEnd('s')), callback);
                            }
                            break;
                        case "wait":
                    {
                        var time = float.Parse(prts[1].Trim().TrimEnd('s'));
                        allowContinue = false;
                        System.Timers.Timer t = new System.Timers.Timer(time * 1000);
                        t.AutoReset = false;
                        t.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
                        {
                            //GD.Print("Timer elapsed");
                            // We need to call ProcessNext on the main thread
                            CallDeferred(nameof(ProcessNext));
                            t.Dispose();
                        };
                        t.Start();
                    }
                    break;
                case "write":
                        {
                            var (text, rest) = ParseStringAtStartOfCommand(prts[1]);
                       //         GD.Print(text,rest);
                            prts =rest.Split(' ');
                                foreach (var prt in prts)
                                {
                                    GD.Print(prt);
                                }

                            var prms= GetParamsOfSubcommandGeneral(prts, "in");
                            if (prms != null)
                            {
                                var name = (string)prms[0];
                                var textbox = textboxes[name];
                                Action callback = null;
                                if (ContainsSubCommand(prts, "wait"))
                                {
                                    callback = () =>
                                    {
                                        CallDeferred(nameof(ProcessNext));
                                    };
                                    allowContinue = false;
                                }
                                if (ContainsSubCommand(prts, "show"))
                                {
                                    textbox.Show();
                                }
                                    List<float> parseParams = GetParamsOfSubcommand(prts, "at");
                                    if (parseParams != null)
                                    {
                                        textbox.Position = new Vector2(parseParams[0], parseParams[1]);
                                        if (parseParams.Count == 3)
                                        {
                                            textbox.ZIndex = (int)parseParams[2];
                                        }
                                    }
                                    textbox.Write(text,callback);
                            }
                        }                 
                    break;
                case "hide":
                    {
                        prts = prts[1].Split(' ');
                        var name = prts[1];
                        Node2D obj;
                        if (prts[0] == "sprite")
                        {

                            obj = sprites[name];

                        }
                        else
                        {
                            obj = textboxes[name];
                        }
                        if (ContainsSubCommand(prts, "transition"))
                        {
                            Action callback = null;

                            var prms = GetParamsOfSubcommand(prts, "transition");
                            if (ContainsSubCommand(prts, "wait"))
                            {
                                allowContinue = false;
                                callback = () =>
                                {
                                    CallDeferred(nameof(ProcessNext));
                                };
                            }
                            obj.FadeOut(prms[0], callback);
                        }
                        else { 
                            obj.Hide();
                        }



                    }
                    break;
                case "move":
                    { 
                        prts = prts[1].Split(' ');
                        var name = prts[1];
                        Node2D obj;
                        if (prts[0] == "sprite")
                        {
                            obj = sprites[name];
                        }
                        else {
                            obj = textboxes[name];
                        }

                        if (ContainsSubCommand(prts, "show"))
                        {
                            obj.Show();
                        }

                                Vector2 movePos = Vector2.Zero;
                        List<float> parseParams = GetParamsOfSubcommand(prts, "to");
                        if (parseParams != null)
                        {
                            movePos = new Vector2(parseParams[0], parseParams[1]);
                        }

                        if (ContainsSubCommand(prts, "transition"))
                        {
                            Action callback = null;

                            var prms = GetParamsOfSubcommand(prts, "transition");
                            if (ContainsSubCommand(prts, "wait"))
                            {
                                allowContinue = false;
                                callback = () =>
                                {
                                    CallDeferred(nameof(ProcessNext));
                                };
                            }
                            obj.MoveTo(movePos, prms[0], callback);
                            if (parseParams.Count == 3)
                            {
                                        obj.ZIndex = (int)parseParams[2];
                             }
                                }
                        else {
                            obj.Position = movePos;
                                    if (parseParams.Count == 3)
                                    {
                                        obj.ZIndex = (int)parseParams[2];
                                    }
                                }



                    }

                    break;
                        case "scene":
                            StartAnimation(prts[1]);
                            break;
                default:
                    break;
            }

            indexInSequence++;
                }
        }
        catch (Exception e) {
            GD.PrintErr("Error processing command at line "+(indexInSequence) +": " + e.Message);
           GD.PrintErr(e.StackTrace);
            }
        }
    }



    void LoadBackgrounds(bool fullReset = false) {
        backgrounds = new();

        var bgData = jsonData.AsGodotDictionary()["background_layers"].AsGodotArray();
        foreach (var bg in bgData) {
            var bgInfo=bg.AsGodotDictionary();
            if (bgInfo["type"].ToString() == "color") { 
                var clr= bgInfo["color"].AsGodotArray();
                Color color= new Color(clr[0].AsSingle(), clr[1].AsSingle(), clr[2].AsSingle(), clr[3].AsSingle());
                var rect = new ColorRect();
                rect.Color = color;
                rect.Size = GetViewportRect().Size;
                AddChild(rect);
                backgrounds.Add(bgInfo["name"].AsString(),rect);
                if (bgInfo["show"].AsBool() == false) { 
                    rect.Hide();
                }
            }
        }
        if (fadeIn==null||fullReset)
        {
            fadeIn = new ColorRect();
            fadeIn.Color = new Color(0, 0, 0, 1);
            fadeIn.Size = GetViewportRect().Size;
            fadeIn.Hide();
            fadeIn.ZIndex = 1000;
            AddChild(fadeIn);
        }
    }

    void LoadTextboxes() {
        textboxes=new();
        var boxData = jsonData.AsGodotDictionary()["textboxes"].AsGodotArray();
        foreach (var box in boxData) { 
            var boxInfo=box.AsGodotDictionary();
            var name= boxInfo["name"].AsString();
            Vector2 pos;
            if (boxInfo.ContainsKey("position"))
            {
                var posInfo = boxInfo["position"].AsGodotArray();
                pos = new Vector2(posInfo[0].AsSingle(), posInfo[1].AsSingle());
            }
                        else {
                pos = new Vector2(0, 0);
            }
            
            var textbox = textboxModel.Instantiate<Textbox>();
            textbox.Position = pos;

            if (boxInfo.ContainsKey("color"))
            { 
                var color= boxInfo["color"].AsGodotArray();
                textbox.SetColor(new Color(color[0].AsSingle(), color[1].AsSingle(), color[2].AsSingle(), color[3].AsSingle()));

            }
            if (boxInfo.ContainsKey("text_speed"))
            {
                textbox.SetCharactersPerSecond(boxInfo["text_speed"].AsSingle());
            }
                textboxes.Add(name,textbox);
            textbox.Hide();
            AddChild(textbox);
        }

    }


    void LoadSprites()
    {
        sprites= new();
        Dictionary<string, Texture2D> textures= new();
        var spritesData = jsonData.AsGodotDictionary()["sprites"].AsGodotArray();
        
        foreach (var sprite in spritesData)
        {
            var spriteInfo = sprite.AsGodotDictionary();
            
            var path = spriteInfo["texture"].AsString();
            var name = spriteInfo["name"].AsString();
            var rectData =
            spriteInfo.ContainsKey("rectangle")?spriteInfo["rectangle"].AsGodotArray():null ;

            Rect2? rect = rectData == null ? null: new Rect2(
                rectData[0].AsSingle(), 
                rectData[1].AsSingle(),
                rectData[2].AsSingle(),
                rectData[3].AsSingle()
                );

            if (!textures.ContainsKey(path))
            {
                textures.Add(path,GD.Load<Texture2D>(path));
            }
            var texture = textures[path];



            var mySprite = spriteModel.Instantiate<MySprite>();
            mySprite.SetInfo(texture,rect);
            mySprite.Hide();

            AddChild(mySprite);
            sprites[name]=mySprite;
        }
    }
    public void RegisterFilenames(Dictionary<string,string> nameToFilePaths) { 
        
        this.nameToFilePaths=new Dictionary<string, string>(nameToFilePaths);
    }
    public void StartAnimation(string firstSceneName,bool fullReset=false) {
        Start(nameToFilePaths[firstSceneName],fullReset);
    }
    void Start(string fileName, bool fullReset = false)
    {
        var file = FileAccess.Open(fileName, FileAccess.ModeFlags.Read);
        var content = file.GetAsText();
        var json = new Json();
        var res = json.Parse(content);
        foreach (var c in GetChildren())
        {
            if (c != fadeIn||fullReset)
            {
                c.QueueFree();
            }
        }
        if (res == Error.Ok)
        {
            GD.Print("Here!");
            jsonData = json.Data;
            sequence = jsonData.AsGodotDictionary()["sequence"].AsGodotArray();

            LoadBackgrounds(fullReset);
            LoadSprites();
            LoadTextboxes();

            indexInSequence = 0;
            ProcessNext();
        }
        else {
            GD.PrintErr("Error parsing file!");
        }
    }


}
