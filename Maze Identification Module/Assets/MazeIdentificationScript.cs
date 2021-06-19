using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class MazeIdentificationScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public SpriteRenderer[] Shapes;

    private int[] Quadrants = new int[4];
    private int[] LocationDetails = new int[3];
    private int[] ButtonFunctions = { 0, 1, 2, 3 };
    private int[] Submissions = new int[4];
    private int Presses;
    private int SubmitPresses;
    private float Delay;
    private bool[][][] PossibleMazes = new bool[][][]
    {
        new bool[][]
        {
            new bool[]{ true, true, true, true },
            new bool[]{ true, true, false, true, true },
            new bool[]{ false, false, false, false },
            new bool[]{ true, false, true, false, true },
            new bool[]{ true, true, true, false },
            new bool[]{ true, true, true, true, true },
            new bool[]{ false, false, false, false },
            new bool[]{ true, false, false, false, true },
            new bool[]{ true, true, true, true }
        },
        new bool[][]
        {
            new bool[]{ true, true, true, true },
            new bool[]{ true, false, false, false, true },
            new bool[]{ false, true, false, true },
            new bool[]{ true, false, true, false, true },
            new bool[]{ true, true, true, false },
            new bool[]{ true, false, true, false, true },
            new bool[]{ false, true, false, true },
            new bool[]{ true, false, false, false, true },
            new bool[]{ true, true, true, true }
        },
        new bool[][]
        {
            new bool[]{ true, true, true, true },
            new bool[]{ true, false, false, false, true },
            new bool[]{ false, false, false, false },
            new bool[]{ true, true, true, true, true },
            new bool[]{ true, true, true, false },
            new bool[]{ true, false, true, false, true },
            new bool[]{ false, false, false, false },
            new bool[]{ true, true, false, true, true },
            new bool[]{ true, true, true, true }
        },
        new bool[][]
        {
            new bool[]{ true, true, true, true },
            new bool[]{ true, false, false, false, true },
            new bool[]{ true, false, true, false },
            new bool[]{ true, false, true, false, true },
            new bool[]{ true, true, true, false },
            new bool[]{ true, false, true, false, true },
            new bool[]{ true, false, true, false },
            new bool[]{ true, false, false, false, true },
            new bool[]{ true, true, true, true }
        }
    };
    private bool[][] Walls = { new bool[4], new bool[5], new bool[4], new bool[5], new bool[4], new bool[5], new bool[4], new bool[5], new bool[4] };
    private bool[] LitShapes = new bool[4];
    private bool Activated;
    private bool Solved;
    private bool Submitting;
    private Color RandomColour;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 5; i++)
        {
            int x = i;
            Buttons[i].OnInteract += delegate { StartCoroutine(ButtonPress(x)); return false; };
        }
        string[] QuadrantsLog = new string[4];
        for (int i = 0; i < 4; i++)
        {
            Quadrants[i] = Rnd.Range(0, 4);
            QuadrantsLog[i] = (Quadrants[i] + 1).ToString();
        }
        Walls = new bool[][]{
            new bool[]{ PossibleMazes[Quadrants[0]][0][0], PossibleMazes[Quadrants[0]][0][1], PossibleMazes[Quadrants[1]][0][2], PossibleMazes[Quadrants[1]][0][3] },
            new bool[]{ PossibleMazes[Quadrants[0]][1][0], PossibleMazes[Quadrants[0]][1][1], PossibleMazes[Quadrants[1]][1][2], PossibleMazes[Quadrants[1]][1][3], PossibleMazes[Quadrants[1]][1][4] },
            new bool[] { PossibleMazes[Quadrants[0]][2][0], PossibleMazes[Quadrants[0]][2][1], PossibleMazes[Quadrants[1]][2][2], PossibleMazes[Quadrants[1]][2][3] },
            new bool[]{ PossibleMazes[Quadrants[0]][3][0], PossibleMazes[Quadrants[0]][3][1], PossibleMazes[Quadrants[1]][3][2], PossibleMazes[Quadrants[1]][3][3], PossibleMazes[Quadrants[1]][3][4] },
            new bool[] { PossibleMazes[Quadrants[0]][4][0], PossibleMazes[Quadrants[0]][4][1], PossibleMazes[Quadrants[1]][4][2], PossibleMazes[Quadrants[1]][4][3] },
            new bool[]{ PossibleMazes[Quadrants[2]][5][0], PossibleMazes[Quadrants[2]][5][1], PossibleMazes[Quadrants[3]][5][2], PossibleMazes[Quadrants[3]][5][3], PossibleMazes[Quadrants[3]][5][4] },
            new bool[] { PossibleMazes[Quadrants[2]][6][0], PossibleMazes[Quadrants[2]][6][1], PossibleMazes[Quadrants[3]][6][2], PossibleMazes[Quadrants[3]][6][3] },
            new bool[]{ PossibleMazes[Quadrants[2]][7][0], PossibleMazes[Quadrants[2]][7][1], PossibleMazes[Quadrants[3]][7][2], PossibleMazes[Quadrants[3]][7][3], PossibleMazes[Quadrants[3]][7][4] },
            new bool[] { PossibleMazes[Quadrants[2]][8][0], PossibleMazes[Quadrants[2]][8][1], PossibleMazes[Quadrants[3]][8][2], PossibleMazes[Quadrants[3]][8][3] },
        };
        ButtonFunctions.Shuffle();
        for (int i = 0; i < 3; i++)
            LocationDetails[i] = Rnd.Range(0, 4);
        for (int i = 0; i < 4; i++)
            Debug.LogFormat("[Maze Identification #{0}] Button {1} moves you {2}.", _moduleID, i + 1, new string[] { "forwards", "clockwise", "backwards", "counter-clockwise" }[ButtonFunctions[i]]);
        Debug.LogFormat("[Maze Identification #{0}] The maze:\n{1}", _moduleID, Walls.Select(x => (x.Length == 4 ? "+" + x.Select(y => y ? "-" : " ").Join("+") + "+" : x.Select(y => y ? "|" : " ").Join(" "))).Join("\n"));
        Debug.LogFormat("[Maze Identification #{0}] The solution is {1}.", _moduleID, QuadrantsLog.Join(", "));
        RandomColour = Rnd.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
        Module.OnActivate += delegate { Activated = true; Calculate(); };
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Calculate()
    {
        LitShapes = new bool[] { false, false, false, false };
        if (Walls[LocationDetails[0] * 2][LocationDetails[1]])
            switch (LocationDetails[2])
            {
                case 0:
                    LitShapes[0] = true;
                    break;
                case 1:
                    LitShapes[3] = true;
                    break;
                case 2:
                    LitShapes[2] = true;
                    break;
                default:
                    LitShapes[1] = true;
                    break;
            }
        if (Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1] + 1])
            switch (LocationDetails[2])
            {
                case 0:
                    LitShapes[1] = true;
                    break;
                case 1:
                    LitShapes[0] = true;
                    break;
                case 2:
                    LitShapes[3] = true;
                    break;
                default:
                    LitShapes[2] = true;
                    break;
            }
        if (Walls[(LocationDetails[0] * 2) + 2][LocationDetails[1]])
            switch (LocationDetails[2])
            {
                case 0:
                    LitShapes[2] = true;
                    break;
                case 1:
                    LitShapes[1] = true;
                    break;
                case 2:
                    LitShapes[0] = true;
                    break;
                default:
                    LitShapes[3] = true;
                    break;
            }
        if (Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1]])
            switch (LocationDetails[2])
            {
                case 0:
                    LitShapes[3] = true;
                    break;
                case 1:
                    LitShapes[2] = true;
                    break;
                case 2:
                    LitShapes[1] = true;
                    break;
                default:
                    LitShapes[0] = true;
                    break;
            }
        for (int i = 0; i < 4; i++)
        {
            if (LitShapes[i])
            {
                Shapes[i].color = RandomColour;
                Shapes[i].sortingOrder = 1;
            }
            else
            {
                Shapes[i].color = new Color(0, 0, 0);
                Shapes[i].sortingOrder = 0;
            }
        }
    }

    private IEnumerator ButtonPress(int pos)
    {
        Audio.PlaySoundAtTransform("press", Buttons[pos].transform);
        if (pos != 4)
            Buttons[pos].AddInteractionPunch(0.5f);
        else
            Buttons[4].AddInteractionPunch(2);
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition -= new Vector3(0, 0.0025f, 0);
            yield return null;
        }
        if (Activated && !Solved && !Submitting && pos != 4)
        {
            switch (ButtonFunctions[pos])
            {
                case 0:
                    switch (LocationDetails[2])
                    {
                        case 0:
                            if (!Walls[LocationDetails[0] * 2][LocationDetails[1]])
                                LocationDetails[0]--;
                            break;
                        case 1:
                            if (!Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1] + 1])
                                LocationDetails[1]++;
                            break;
                        case 2:
                            if (!Walls[(LocationDetails[0] * 2) + 2][LocationDetails[1]])
                                LocationDetails[0]++;
                            break;
                        default:
                            if (!Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1]])
                                LocationDetails[1]--;
                            break;
                    }
                    break;
                case 1:
                    LocationDetails[2] = (LocationDetails[2] + 1) % 4;
                    break;
                case 2:
                    switch (LocationDetails[2])
                    {
                        case 0:
                            if (!Walls[(LocationDetails[0] * 2) + 2][LocationDetails[1]])
                                LocationDetails[0]++;
                            break;
                        case 1:
                            if (!Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1]])
                                LocationDetails[1]--;
                            break;
                        case 2:
                            if (!Walls[LocationDetails[0] * 2][LocationDetails[1]])
                                LocationDetails[0]--;
                            break;
                        default:
                            if (!Walls[(LocationDetails[0] * 2) + 1][LocationDetails[1] + 1])
                                LocationDetails[1]++;
                            break;
                    }
                    break;
                default:
                    LocationDetails[2] = (LocationDetails[2] + 3) % 4;
                    break;
            }
            Calculate();
        }
        else if (Submitting && pos != 4)
        {
            Submissions[Presses] = pos;
            Presses++;
            if (Presses == 4)
            {
                Presses = 0;
                bool Valid = true;
                for (int i = 0; i < 4; i++)
                {
                    if (Quadrants[i] != Submissions[i])
                        Valid = false;
                }
                if (Valid)
                {
                    Module.HandlePass();
                    Solved = true;
                    Submitting = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Shapes[i].color = RandomColour;
                        Shapes[i].sortingOrder = 1;
                    }
                    if (SubmitPresses >= 50)
                        Audio.PlaySoundAtTransform("heavy", Shapes[0].transform);
                    else
                        Audio.PlaySoundAtTransform("solve", Shapes[0].transform);
                    string[] SubmissionsLog = new string[4];
                    for (int i = 0; i < 4; i++)
                        SubmissionsLog[i] = (Submissions[i] + 1).ToString();
                    Debug.LogFormat("[Maze Identification #{0}] You submitted {1}, which was correct. Module solved!", _moduleID, SubmissionsLog.Join(", "));
                }
                else
                {
                    Submitting = false;
                    Module.HandleStrike();
                    string[] SubmissionsLog = new string[4];
                    for (int i = 0; i < 4; i++)
                        SubmissionsLog[i] = (Submissions[i] + 1).ToString();
                    Debug.LogFormat("[Maze Identification #{0}] You submitted {1}, which was incorrect. Strike!", _moduleID, SubmissionsLog.Join(", "));
                    Calculate();
                    SubmitPresses = 0;
                }
            }
        }
        else if (pos == 4 && !Submitting && !Solved && Activated)
        {
            Submitting = true;
            for (int i = 0; i < 4; i++)
            {
                Shapes[i].color = new Color(0, 0, 0);
                Shapes[i].sortingOrder = 0;
            }
        }
        if (pos == 4)
            SubmitPresses++;
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition += new Vector3(0, 0.0025f, 0);
            yield return null;
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 1S234' to press 1, the submit button, 2, 3 and 4 (in that order). Use '!{0} delayset 0.5' to set the delay between button presses to 0.5 seconds while traversing the maze (default is 0.1s).";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] CommandArray = command.Split(' ');
        string validcmds = "1234s";
        bool Valid = true;
        yield return null;
        for (int i = 0; i < command.Length; i++)
        {
            if (!validcmds.Contains(command[i]))
                Valid = false;
        }
        for (int i = 0; i < command.Length; i++)
        {
            if (!Valid && (CommandArray[0] != "delayset" || (CommandArray[0] == "delayset" && CommandArray.Length != 2)))
            {
                if ((CommandArray[0] == "delayset" && CommandArray.Length != 2))
                    yield return "sendtochaterror Invalid command.";
                else
                    yield return "sendtochaterror Invalid command: \"" + command[i] + "\".";
                yield break;
            }
            else if (CommandArray[0] == "delayset" && CommandArray.Length == 2)
            {
                float bruh = 0;
                if (float.TryParse(CommandArray[1], out bruh))
                {
                    if (float.Parse(CommandArray[1]) > 3 || float.Parse(CommandArray[1]) < 0.05f)
                    {
                        if (float.Parse(CommandArray[1]) > 3)
                            yield return "sendtochaterror Delay is too large.";
                        else
                            yield return "sendtochaterror Delay is too small.";
                    }
                    Delay = float.Parse(CommandArray[1]);
                    yield return "sendtochat Delay set to " + Delay + " seconds.";
                }
                else
                    yield return "sendtochaterror Invalid command. Please specify a delay.";
                yield break;
            }
            else
            {
                if (Submitting || i == 0)
                    yield return new WaitForSeconds(0.05f);
                else
                    yield return new WaitForSeconds(Delay);
                Buttons[validcmds.IndexOf(command[i])].OnInteract();
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        Buttons[4].OnInteract();
        yield return new WaitForSeconds(0.025f);
        for (int i = 0; i < 4; i++)
        {
            Buttons[Quadrants[i]].OnInteract();
            yield return new WaitForSeconds(0.025f);
        }
    }
}
