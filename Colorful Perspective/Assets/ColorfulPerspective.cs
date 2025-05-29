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

    bool IsCubeAmountPressedEven;

    int FirstSerialNumberDigit;
    int ThirdAndSixthSerialNumberDigit;
    int BatteryHolder;
    int PressCubeAmount = 0;
    int CurrentTableRotation = 0;

    Direction CurrentFacePerspective;
    Direction NextFacePerspective;

    Material CurrentColor;

    List<int> CurrentPressedCubeIndexList = new List<int>();

    List<string> CurrentFaceCubeNames = new List<string>();
    List<Material> CurrentFaceColors = new List<Material>();

    List<Direction> TablePerspective0;
    List<Material> TableColors0;

    List<Direction> TablePerspective90;
    List<Material> TableColors90;

    List<Direction> TablePerspective180;
    List<Material> TableColors180;

    List<Direction> TablePerspective270;
    List<Material> TableColors270;

    List<KMSelectable> CorrectCubes = new List<KMSelectable>();

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;


    public enum Direction
    {
        Front,
        Left,
        Right,
        Up,
        Down
    }

    public enum CubeColor
    {
        Black = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Cyan = 4,
        Magenta = 5,
        Yellow = 6,
        White = 7
    }

    public class RGB
    {
        public bool r, g, b;

        public RGB(bool r, bool g, bool b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

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
        Func<CubeColor, Material> M = delegate (CubeColor c)
        {
            return (CubeColors[(int)c]);
        };

        TablePerspective0 = new List<Direction>
        {
            Direction.Right, Direction.Up, Direction.Left, Direction.Down,
            Direction.Down, Direction.Right, Direction.Up, Direction.Left,
            Direction.Up, Direction.Down, Direction.Left, Direction.Right,
            Direction.Right, Direction.Up, Direction.Down, Direction.Left,
        };

        TableColors0 = new List<Material>
        {
            M(CubeColor.Blue), M(CubeColor.Green), M(CubeColor.Yellow), M(CubeColor.White),
            M(CubeColor.Red), M(CubeColor.Cyan), M(CubeColor.Black), M(CubeColor.Magenta),
            M(CubeColor.Magenta), M(CubeColor.Blue), M(CubeColor.Red), M(CubeColor.White),
            M(CubeColor.Black), M(CubeColor.Yellow), M(CubeColor.Cyan), M(CubeColor.Green),
        };

        TablePerspective90 = new List<Direction>
        {
            Direction.Down, Direction.Right, Direction.Left, Direction.Down,
            Direction.Right, Direction.Left, Direction.Down, Direction.Right,
            Direction.Left, Direction.Up, Direction.Right, Direction.Up,
            Direction.Up, Direction.Down, Direction.Up, Direction.Left,
        };

        TableColors90 = new List<Material>
        {
            M(CubeColor.Black), M(CubeColor.Magenta), M(CubeColor.Red), M(CubeColor.Blue),
            M(CubeColor.Yellow), M(CubeColor.Blue), M(CubeColor.Cyan), M(CubeColor.Green),
            M(CubeColor.Cyan), M(CubeColor.Red), M(CubeColor.Black), M(CubeColor.Yellow),
            M(CubeColor.Green), M(CubeColor.White), M(CubeColor.Magenta), M(CubeColor.White),
        };

        TablePerspective180 = new List<Direction>
        {
            Direction.Right, Direction.Up, Direction.Down, Direction.Left,
            Direction.Left, Direction.Right, Direction.Up, Direction.Down,
            Direction.Right, Direction.Down, Direction.Left, Direction.Up,
            Direction.Up, Direction.Right, Direction.Down, Direction.Left,
        };

        TableColors180 = new List<Material>
        {
            M(CubeColor.Green), M(CubeColor.Cyan), M(CubeColor.Yellow), M(CubeColor.Black),
            M(CubeColor.White), M(CubeColor.Red), M(CubeColor.Blue), M(CubeColor.Magenta),
            M(CubeColor.Magenta), M(CubeColor.Black), M(CubeColor.Cyan), M(CubeColor.Red),
            M(CubeColor.White), M(CubeColor.Yellow), M(CubeColor.Green), M(CubeColor.Blue),
        };

        TablePerspective270 = new List<Direction>
        {
            Direction.Right, Direction.Down, Direction.Up, Direction.Down,
            Direction.Down, Direction.Left, Direction.Down, Direction.Right,
            Direction.Left, Direction.Up, Direction.Right, Direction.Left,
            Direction.Up, Direction.Right, Direction.Left, Direction.Up,
        };

        TableColors270 = new List<Material>
        {
            M(CubeColor.White), M(CubeColor.Magenta), M(CubeColor.White), M(CubeColor.Green),
            M(CubeColor.Yellow), M(CubeColor.Black), M(CubeColor.Red), M(CubeColor.Cyan),
            M(CubeColor.Green), M(CubeColor.Cyan), M(CubeColor.Blue), M(CubeColor.Yellow),
            M(CubeColor.Blue), M(CubeColor.Red), M(CubeColor.Magenta), M(CubeColor.Black),
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

    void MainGameplayLoop()
    {
        HandleColorAndPerspectiveChange();
        GetCurrentPerspectiveColors();
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
        int matchingColorIndex = -1;
        if (FirstSerialNumberDigit % 2 == 0)
        {
            GetFaceRight();
            for (int i = 0; i < CurrentFaceColors.Count; i++)
            {
                if (CurrentFaceColors[i].color == TableColors0[i].color)
                {
                    matchingColorIndex = i;
                    break;
                }
            }
        }
        else
        {
            GetFaceLeft();
            for (int i = CurrentFaceColors.Count - 1; i >= 0; i--)
            {
                if (CurrentFaceColors[i].color == TableColors0[i].color)
                {
                    matchingColorIndex = i;
                    break;
                }
            }
        }
        if (matchingColorIndex != -1) CurrentFacePerspective = TablePerspective0[matchingColorIndex];
        else CurrentFacePerspective = Direction.Front;
        GetCurrentPerspectiveColors();
        Debug.LogFormat("[Colorful Perspective #{0}] Starting Perspective {1}", ModuleId, CurrentFacePerspective);
    }

    void GetStartingColor()
    {
        Func<CubeColor, Material> M = delegate (CubeColor c)
        {
            return (CubeColors[(int)c]);
        };

        int col = 0;
        int row = 0;
        Dictionary<int, Material> StartColorDict = new Dictionary<int, Material>
        {
            { 11, M(CubeColor.Green) },
            { 12, M(CubeColor.Black) },
            { 13, M(CubeColor.Blue) },
            { 14, M(CubeColor.White) },
            { 21, M(CubeColor.Red) },
            { 22, M(CubeColor.Cyan) },
            { 23, M(CubeColor.Yellow) },
            { 24, M(CubeColor.Magenta) },
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

        if (ThirdAndSixthSerialNumberDigit <= 10)
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

    void GetCurrentPerspectiveColors()
    {
        switch (CurrentFacePerspective)
        {
            case Direction.Left:
                GetFaceLeft();
                break;
            case Direction.Down:
                GetFaceDown();
                break;
            case Direction.Front:
                GetFaceFront();
                break;
            case Direction.Up:
                GetFaceUp();
                break;
            case Direction.Right:
                GetFaceRight();
                break;
        }
    }

    void GetFaceLeft()
    {
        CurrentFaceColors.Clear();
        CurrentFaceCubeNames.Clear();
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

    #region Direction and Color Changes

    void HandleColorAndPerspectiveChange()
    {
        Func<float, bool> IsOne = delegate (float value)
        {
            return value > 0.5f;
        };

        RGB NextColor = new RGB(IsOne(CurrentColor.color.r), IsOne(CurrentColor.color.g), IsOne(CurrentColor.color.b));

        foreach (int index in CurrentPressedCubeIndexList)
        {
            if (CurrentTableRotation == 0)
            {
                Debug.Log(TableColors0[index].color);
                if (TableColors0[index].color.r == 1) NextColor.r = !NextColor.r;
                if (TableColors0[index].color.g == 1) NextColor.g = !NextColor.g;
                if (TableColors0[index].color.b == 1) NextColor.b = !NextColor.b;
                NextFacePerspective = TablePerspective0[index];
            }
            else if (CurrentTableRotation == 1)
            {
                if (TableColors90[index].color.r == 1) NextColor.r = !NextColor.r;
                if (TableColors90[index].color.g == 1) NextColor.g = !NextColor.g;
                if (TableColors90[index].color.b == 1) NextColor.b = !NextColor.b;
                NextFacePerspective = TablePerspective90[index];
            }
            else if (CurrentTableRotation == 2)
            {
                if (TableColors180[index].color.r == 1) NextColor.r = !NextColor.r;
                if (TableColors180[index].color.g == 1) NextColor.g = !NextColor.g;
                if (TableColors180[index].color.b == 1) NextColor.b = !NextColor.b;
                NextFacePerspective = TablePerspective180[index];
            }
            else
            {
                if (TableColors270[index].color.r == 1) NextColor.r = !NextColor.r;
                if (TableColors270[index].color.g == 1) NextColor.g = !NextColor.g;
                if (TableColors270[index].color.b == 1) NextColor.b = !NextColor.b;
                NextFacePerspective = TablePerspective270[index];
            }
        }
        HandlePerspectiveChange();

        if (!IsCubeAmountPressedEven)
        {
            NextColor.r = !NextColor.r;
            NextColor.g = !NextColor.g;
            NextColor.b = !NextColor.b;
        }

        Dictionary<int, int> lookupTableNewColor = new Dictionary<int, int>
        {
            { 0, 0 }, //Black
            { 100, 1 }, //Red
            { 10, 2 }, //Green
            { 1, 3 }, //Blue
            { 11, 4 }, //Cyan
            { 101, 5 }, //Magenta
            { 110, 6 }, //Yellow
            { 111, 7 }, //White
        };

        int key = (NextColor.r ? 100 : 0) + (NextColor.g ? 10 : 0) + (NextColor.b ? 1 : 0);
        CurrentColor = CubeColors[lookupTableNewColor[key]];
        Debug.LogFormat("[Colorful Perspective #{0}] New Color is: {1}", ModuleId, CurrentColor.name);
        Debug.LogFormat("[Colorful Perspective #{0}] New Perspective is: {1}", ModuleId, CurrentFacePerspective);

        CurrentPressedCubeIndexList.Clear();
    }

    void HandlePerspectiveChange()
    {
        Debug.Log("sdfsregbesdbsbsdb");
        if (IsCubeAmountPressedEven)
        {
            switch (NextFacePerspective)
            {
                case Direction.Left:
                    NextFacePerspective = Direction.Right;
                    break;
                case Direction.Right:
                    NextFacePerspective = Direction.Left;
                    break;
                case Direction.Up:
                    NextFacePerspective = Direction.Down;
                    break;
                case Direction.Down:
                    NextFacePerspective = Direction.Up;
                    break;
            }
        }

        Dictionary<string, Direction> DirectionTransitions = new Dictionary<string, Direction>
        {
            { "LeftLeft", Direction.Right },
            { "LeftRight", Direction.Front },
            { "RightLeft", Direction.Front },
            { "RightRight", Direction.Left },
            { "UpUp", Direction.Down },
            { "UpDown", Direction.Front },
            { "DownUp", Direction.Front },
            { "DownDown", Direction.Up }
        };

        string key = CurrentFacePerspective.ToString() + NextFacePerspective.ToString();
        if (key == "LeftLeft" || key == "RightRight" || key == "UpUp" || key == "DownDown")
        {
            if (IsCubeAmountPressedEven) CurrentTableRotation++; else CurrentTableRotation += 3;
        }
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

    #region Prepare Cubes to press

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

        if (CorrectCubes.Count % 2 == 0)
        {
            IsCubeAmountPressedEven = true;
        }
        else
        {
            IsCubeAmountPressedEven = false;
        }
    }

    void ColorNotOnFaceFallback()
    {
        foreach (Material color in CubeColors)
        {
            if (CurrentFaceColors.Any(material => material.color == color.color))
            {
                Debug.LogFormat("[Colorful Perspective #{0}] Color not present on {1} face, changed color to {2}", ModuleId, CurrentFacePerspective, color.name);
                CurrentColor = color;
                return;
            }
        }
    }

    #endregion

    void InputHandler(KMSelectable cube)
    {
        cube.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, cube.transform);
        int cubeIndex = Array.IndexOf(CubesInteractable, cube);
        if (CorrectCubes.Contains(CubesInteractable[cubeIndex]))
        {
            CurrentPressedCubeIndexList.Add(CurrentFaceCubeNames.FindIndex(name => name == CubesInteractable[cubeIndex].gameObject.name));
            CorrectCubes.Remove(CubesInteractable[cubeIndex]);
            CubesInteractable[cubeIndex].gameObject.SetActive(false);
            PressCubeAmount++;
        }
        else
        {
            Strike();
        }

        if (PressCubeAmount == 64)
        {
            Solve();
        }

        if (CorrectCubes.Count == 0)
        {
            MainGameplayLoop();
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