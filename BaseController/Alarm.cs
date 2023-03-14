using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.ModAPI.Contracts;
using Sandbox.Game;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using SpaceEngineers.UWBlockPrograms.BaseController;

public class MessageOfType
{
    public List<string> messages;
    public bool was;
    public String prefix;

    public MessageOfType(String prefix)
    {
        this.messages = new List<String>();
        this.was = true;
        this.prefix = prefix;
    }

    override
    public string ToString()
    {
        return (prefix == "" || messages.Count == 0 ? "" : prefix + ":\n") + String.Join("\n", messages);
    }

    public String next()
    {
        String result = null;
        if (was || messages.Count > 0)
        {
            var command = new JsonObject("");
            command.Add(new JsonPrimitive("Action", "BaseStatus"));
            command.Add(new JsonPrimitive("Type", this.prefix));
            command.Add(new JsonPrimitive("Value", this.ToString()));
            result = command.ToString();
        }

        this.was = this.messages.Count > 0;
        this.messages.Clear();
        return result;
    }

    public void add(String mess)
    {
        this.messages.Add(mess);
    }
}

public class Messaging
{
    private bool alarmActive;
    private MessageOfType alarmMess;
    private MessageOfType warningMess;
    private MessageOfType infoMess;

    private Dictionary<String, MessageOfType> Messages;

    readonly string[] animFrames = new[] { "|---", "-|--", "--|-", "---|", "--|-", "-|--" };
    int runningFrame;
    int runningMult;

    private Program parent;

    public Messaging(Program parent)
    {
        this.parent = parent;
        alarmActive = false;
        Messages = new Dictionary<String, MessageOfType>();

        Messages["Alarm"] = alarmMess = new MessageOfType("Alarm");
        Messages["Warn"] = warningMess = new MessageOfType("Warn");
        Messages["Info"] = infoMess = new MessageOfType("Info");

        runningFrame = 0;
    }

    public void alarm(String message)
    {
        alarmMess.add(message);
    }
    public void info(String message)
    {
        infoMess.add(message);
    }
    public void warn(String message)
    {
        warningMess.add(message);
    }

    void updateLoader()
    {
        var command = new JsonObject("");
        runningFrame = (runningFrame + 1) % animFrames.Count();
        command.Add(new JsonPrimitive("Action", "BaseStatus"));
        command.Add(new JsonPrimitive("Type", "Loader"));
        command.Add(new JsonPrimitive("Value", animFrames[runningFrame]));
        parent.IGC.SendBroadcastMessage(parent.commandChannelTag, command.ToString());
    }

    public void next()
    {
        updateLoader();

        foreach (var message in Messages.Values)
        {
            var status = message.next();
            if (status != null)
            {
                parent.IGC.SendBroadcastMessage(parent.commandChannelTag, status);
            }
        };
    }

}