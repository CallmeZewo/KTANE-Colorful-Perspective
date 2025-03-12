using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class ColorfulPerspective : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] CubesInteractable;

    //Black Red Green Blue Cyan Magenta Yellow White
    public Material[] CubeColors;
    public Material NoColor;

    KMSelectable[,,] CubeGrid = new KMSelectable[4, 4, 4];

    int FirstSerialNumberDigit;
    int ThirdAndSixthSerialNumberDigit;
    int BatteryHolder;
    int PressedCubes = 0;

    string CurrentFacePerspective;
    string NextFacePerspective;

    Material CurrentColor;

    List<string> CurrentFaceCubeNames = new List<string>();
    List<Material> CurrentFaceColors = new List<Material>();

    List<string> TablePerspective0;
    List<Material> TableColors0;

    List<string> TablePerspective90;
    List<Material> TableColors90;

    List<string> TablePerspective180;
    List<Material> TableColors180;

    List<string> TablePerspective270;
    List<Material> TableColors270;

    List<KMSelectable> CorrectCubes = new List<KMSelectable>();

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        
        foreach (KMSelectable cubes in CubesInteractable) {
            cubes.OnInteract += delegate () { InputHandler(cubes); return false; };
        }
    }

    void OnDestroy()
    { //Shit you need to do when the bomb ends

    }

    void Activate()
    { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module

        #region Arrow table

        TablePerspective0 = new List<string>
        {
            "Right", "Up", "Left", "Down",
            "Down", "Right", "Up", "Left",
            "Up", "Down", "Left", "Right",
            "Right", "Up", "Down", "Left",
        };

        TableColors0 = new List<Material>
        {
            CubeColors[3], CubeColors[2], CubeColors[6], CubeColors[7],
            CubeColors[1], CubeColors[4], CubeColors[0], CubeColors[5],
            CubeColors[5], CubeColors[3], CubeColors[1], CubeColors[7],
            CubeColors[0], CubeColors[6], CubeColors[4], CubeColors[2],
        };

        TablePerspective90 = new List<string>
        {
            "Down", "Right", "Left", "Down",
            "Right", "Left", "Down", "Right",
            "Left", "Up", "Right", "Up",
            "Up", "Down", "Up", "Left",
        };

        TableColors90 = new List<Material>
        {
            CubeColors[0], CubeColors[5], CubeColors[1], CubeColors[3],
            CubeColors[6], CubeColors[3], CubeColors[4], CubeColors[2],
            CubeColors[4], CubeColors[1], CubeColors[0], CubeColors[6],
            CubeColors[2], CubeColors[7], CubeColors[5], CubeColors[7],
        };

        TablePerspective180 = new List<string>
        {
            "Right", "Up", "Down", "Left",
            "Left", "Right", "Up", "Down",
            "Right", "Down", "Left", "Up",
            "Up", "Right", "Down", "Left",
        };

        TableColors180 = new List<Material>
        {
            CubeColors[2], CubeColors[4], CubeColors[6], CubeColors[0],
            CubeColors[7], CubeColors[1], CubeColors[3], CubeColors[5],
            CubeColors[5], CubeColors[0], CubeColors[4], CubeColors[1],
            CubeColors[7], CubeColors[6], CubeColors[2], CubeColors[3],
        };

        TablePerspective270 = new List<string>
        {
            "Right", "Down", "Up", "Down",
            "Down", "Left", "Down", "Right",
            "Left", "Up", "Right", "Left",
            "Up", "Right", "Left", "Up",
        };

        TableColors270 = new List<Material>
        {
            CubeColors[7], CubeColors[5], CubeColors[7], CubeColors[2],
            CubeColors[6], CubeColors[0], CubeColors[1], CubeColors[4],
            CubeColors[2], CubeColors[4], CubeColors[3], CubeColors[6],
            CubeColors[3], CubeColors[1], CubeColors[5], CubeColors[0],
        };

        #endregion

        Setup();
        Starting();
    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    void Setup()
    {
        SetRandomCubeColor();
        CubeColorsIntoGrid();
        FirstSerialNumberDigit = Bomb.GetSerialNumberNumbers().First();
        ThirdAndSixthSerialNumberDigit = (Bomb.GetSerialNumber()[2] - '0') + (Bomb.GetSerialNumber()[5] - '0');
        BatteryHolder = Bomb.GetBatteryHolderCount();
    }

    void Starting()
    {
        GetStartingPerspective();
        GetStartingColor();
        GetCorrectCubeList();
    }

    #region Setup Methodes

    void SetRandomCubeColor()
    {
        List<Material> colorPool = new List<Material>();

        foreach (var color in CubeColors)
        {
            colorPool.AddRange(Enumerable.Repeat(color, 8));
        }

        for (int i = colorPool.Count - 1; i > 0; i--)
        {
            int j = Rnd.Range(0, i + 1);
            Material temp = colorPool[i];
            colorPool[i] = colorPool[j];
            colorPool[j] = temp;
        }

        for (int i = 0; i < CubesInteractable.Length; i++)
        {
            CubesInteractable[i].GetComponent<Renderer>().material = colorPool[i];
        }
    }

    void CubeColorsIntoGrid()
    {
        int index = 0;
        for (int z = 0; z < 4; z++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    CubeGrid[x, y, z] = CubesInteractable[index];
                    index++;
                }
            }
        }
    }

    #endregion

    #region Starting Values

    void GetStartingPerspective()
    {   
        if (FirstSerialNumberDigit % 2 == 0)
        {
            GetFaceRight();
        }
        else
        {
            GetFaceLeft();
            CurrentFaceColors.Reverse();
        }

        int matchingColorIndex = CurrentFaceColors.FindIndex(color => TableColors0.Any(material => material.color == color.color));
        if (matchingColorIndex != -1) CurrentFacePerspective = TablePerspective0[matchingColorIndex];
        else CurrentFacePerspective = "Front";
        Debug.LogFormat("[Colorful Perspective #{0}] Starting Perspective {1}", ModuleId, CurrentFacePerspective);
    }

    void GetStartingColor()
    {
        int col = 0;
        int row = 0;
        Dictionary<int, Material> StartColorDict = new Dictionary<int, Material>
        {
            { 11, CubeColors[2] },
            { 12, CubeColors[0] },
            { 13, CubeColors[3] },
            { 14, CubeColors[7] },
            { 21, CubeColors[1] },
            { 22, CubeColors[4] },
            { 23, CubeColors[6] },
            { 24, CubeColors[5] },
        };

        if (BatteryHolder <= 1)
        {
            col = 1;
        }
        else if (BatteryHolder == 2)
        {
            col = 2;
        }
        else if (BatteryHolder == 3)
        {
            col = 3;
        }
        else
        {
            col = 4;
        }

        if (ThirdAndSixthSerialNumberDigit < 10)
        {
            row = 1;
        }
        else
        {
            row = 2;
        }

        CurrentColor = StartColorDict[row * 10 + col];
        Debug.LogFormat("[Colorful Perspective #{0}] Starting Color is {1}", ModuleId, CurrentColor.name);
    }

    #endregion

    #region Get Face Colors into List

    void GetFaceLeft()
    {
        CurrentFaceCubeNames.Clear();
        CurrentFaceColors.Clear();
        for (int z = 3; z >= 0; z--)
        {
            for (int y = 0; y < 4; y++)
            {
                bool foundCube = false;

                for (int x = 0; x < 4; x++)
                {
                    if (CubeGrid[x, y, z].gameObject.activeSelf)
                    {
                        CurrentFaceColors.Add(CubeGrid[x, y, z].GetComponent<Renderer>().material);
                        CurrentFaceCubeNames.Add(CubeGrid[x, y, z].gameObject.name);
                        foundCube = true;
                        break;
                    }
                }
                if (!foundCube)
                {
                    CurrentFaceColors.Add(NoColor);
                    CurrentFaceCubeNames.Add("None");
                }
            }
        }
    }

    void GetFaceDown()
    {
        CurrentFaceColors.Clear();
        CurrentFaceCubeNames.Clear();
        for (int y = 3; y >= 0; y--)
        {
            for (int x = 0; x < 4; x++)
            {
                bool foundCube = false;

                for (int z = 0; z < 4; z++)
                {
                    if (CubeGrid[x, y, z].gameObject.activeSelf)
                    {
                        CurrentFaceColors.Add(CubeGrid[x, y, z].GetComponent<Renderer>().material);
                        CurrentFaceCubeNames.Add(CubeGrid[x, y, z].gameObject.name);
                        foundCube = true;
                        break;
                    }
                }
                if (!foundCube)
                {
                    CurrentFaceColors.Add(NoColor);
                    CurrentFaceCubeNames.Add("None");
                }
            }
        }
    }

    void GetFaceFront()
    {
        CurrentFaceColors.Clear();
        CurrentFaceCubeNames.Clear();
        for (int z = 3; z >= 0; z--)
        {
            for (int x = 0; x < 4; x++)
            {
                bool foundCube = false;

                for (int y = 3; y >= 0; y--)
                {
                    if (CubeGrid[x, y, z].gameObject.activeSelf)
                    {
                        CurrentFaceColors.Add(CubeGrid[x, y, z].GetComponent<Renderer>().material);
                        CurrentFaceCubeNames.Add(CubeGrid[x, y, z].gameObject.name);
                        foundCube = true;
                        break;
                    }
                }
                if (!foundCube)
                {
                    CurrentFaceColors.Add(NoColor);
                    CurrentFaceCubeNames.Add("None");
                }
            }
        }
    }

    void GetFaceUp()
    {
        CurrentFaceColors.Clear();
        CurrentFaceCubeNames.Clear();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                bool foundCube = false;

                for (int z = 3; z >= 0; z--)
                {
                    if (CubeGrid[x, y, z].gameObject.activeSelf)
                    {
                        CurrentFaceColors.Add(CubeGrid[x, y, z].GetComponent<Renderer>().material);
                        CurrentFaceCubeNames.Add(CubeGrid[x, y, z].gameObject.name);
                        foundCube = true;
                        break;
                    }
                }
                if (!foundCube)
                {
                    CurrentFaceColors.Add(NoColor);
                    CurrentFaceCubeNames.Add("None");
                }
            }
        }
    }

    void GetFaceRight()
    {
        CurrentFaceColors.Clear();
        CurrentFaceCubeNames.Clear();
        for (int z = 3; z >= 0; z--)
        {
            for (int y = 3; y >= 0; y--)
            {
                bool foundCube = false;

                for (int x = 3; x >= 0; x--)
                {
                    if (CubeGrid[x, y, z].gameObject.activeSelf)
                    {
                        CurrentFaceColors.Add(CubeGrid[x, y, z].GetComponent<Renderer>().material);
                        CurrentFaceCubeNames.Add(CubeGrid[x, y, z].gameObject.name);
                        foundCube = true;
                        break;
                    }
                }
                if (!foundCube)
                {
                    CurrentFaceColors.Add(NoColor);
                    CurrentFaceCubeNames.Add("None");
                }
            }
        }
    }

    #endregion

    #region Direction Changes

    void HandleDirectionChange()
    {
        Dictionary<string, string> DirectionTransitions = new Dictionary<string, string>
        {
            { "LeftLeft", "Right" },
            { "LeftRight", "Front" },
            { "RightLeft", "Front" },
            { "RightRight", "Left" },
            { "UpUp", "Down" },
            { "UpDown", "Front" },
            { "DownUp", "Front" },
            { "DownDown", "Up" }
        };

        string key = CurrentFacePerspective + NextFacePerspective;
        if (DirectionTransitions.ContainsKey(key))
        {
            CurrentFacePerspective = DirectionTransitions[key];
        }
        else
        {
            CurrentFacePerspective = NextFacePerspective;
        }
    }

    #endregion

    void GetCorrectCubeList()
    {
        if (!CurrentFaceColors.Any(material => material.color == CurrentColor.color))
        {
            ColorNotOnFaceFallback();
        }

        CorrectCubes.Clear();
        for (int i = 0; i < CubesInteractable.Length; i++)
        {
            if (CurrentFaceCubeNames.Contains(CubesInteractable[i].name) && CubesInteractable[i].gameObject.GetComponent<Renderer>().material.color== CurrentColor.color)
            {
                CorrectCubes.Add(CubesInteractable[i]);
            }
        }

        string[] correctCubeNames = CorrectCubes.Select(cube => cube.name).ToArray();

        Debug.LogFormat("[Colorful Perspective #{0}] Correct Cubes for this Perspective: {1}", ModuleId, string.Join(", ", correctCubeNames).ToString());
    }

    void ColorNotOnFaceFallback()
    {
        foreach (Material color in CubeColors)
        {
            Debug.Log(CurrentFaceColors.Any(material => material.color == color.color));
            if (CurrentFaceColors.Any(material => material.color == color.color))
            {
                Debug.LogFormat("[Colorful Perspective #{0}] Color not present on {1} face, changed color to {2}", ModuleId, CurrentFacePerspective, color.name);
                CurrentColor = color;
                return;
            }
        }
    }

    void InputHandler(KMSelectable cube)
    {
        int cubeIndex = Array.IndexOf(CubesInteractable, cube);
        if (CorrectCubes.Contains(CubesInteractable[cubeIndex]))
        {
            CorrectCubes.RemoveAt(0);
            CubesInteractable[cubeIndex].gameObject.SetActive(false);
            PressedCubes++;
        }

        if (CorrectCubes.Count == PressedCubes)
        {
            PressedCubes = 0;
        }
    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }
    /*
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
    */
}