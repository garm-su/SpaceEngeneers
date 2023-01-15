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

public class MessageOfType
{
    public List<string> messages;
    public bool was;

    public MessageOfType()
    {
        this.messages = new List<String>();
        this.was = false;
    }

    public void next()
    {
        this.was = this.messages.Count > 0;
        this.messages.Clear();
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

    readonly string[] animFrames = new[] { "|---", "-|--", "--|-", "---|", "--|-", "-|--" };
    int runningFrame;
    int runningMult;

    public Messaging()
    {
        alarmActive = false;
        alarmMess = new MessageOfType();
        warningMess = new MessageOfType();
        infoMess = new MessageOfType();
        runningFrame = 0;
        runningMult = 10;
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

    public void next()
    {
        runningFrame = (runningFrame + 1) % (animFrames.Count() * runningMult);
        warningMess.next();
        infoMess.next();
        alarmMess.next();
    }

    public String getInfo()
    {
        return String.Join("\n", infoMess.messages);
    }

    public string getError()
    {
        var isAlarms = alarmMess.messages.Count() > 0;
        var prefix = (isAlarms ? "Alrm" : "Warn") + ": ";
        return animFrames[runningFrame / runningMult] + "\n" + prefix + String.Join("\n" + prefix, (isAlarms ? alarmMess : warningMess).messages);
    }
}