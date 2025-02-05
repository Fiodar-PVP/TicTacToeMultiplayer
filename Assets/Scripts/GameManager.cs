using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public event EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public class OnClickedGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    public static GameManager Instance { get; private set; }

    private PlayerType[,] playerTypeArray = new PlayerType[3,3];
    private PlayerType localPlayerType;
    private List<Line> lineList;
    private NetworkVariable<PlayerType> currentlyPlayablePlayerType = new NetworkVariable<PlayerType>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager!");
        }

        Instance = this;

        lineList = new List<Line>
        {
            //Horizontal
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal,
            },

            //Vertical
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2)},
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(2,1),
                orientation = Orientation.Vertical,
            },

            //Diagonals
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalA,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> {new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB,
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentlyPlayablePlayerType.OnValueChanged += CurrentlyPlayablePlayerType_OnValueChanged;
    }

    private void CurrentlyPlayablePlayerType_OnValueChanged(PlayerType previousValue, PlayerType newValue)
    {
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {

        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentlyPlayablePlayerType.Value = PlayerType.Cross;

            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if(currentlyPlayablePlayerType.Value != playerType)
        {
            return;
        }

        if (playerTypeArray[x,y] != PlayerType.None)
        {
            return;
        }

        playerTypeArray[x, y] = playerType;

        OnClickedGridPosition?.Invoke(this, new OnClickedGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (currentlyPlayablePlayerType.Value)
        {
            case PlayerType.Cross:
                currentlyPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentlyPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        if (aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType)
        {
            return true;
        }

        return false;
    }

    private void TestWinner()
    {
        foreach(Line line in lineList)
        {
            if (TestWinnerLine(line))
            {
                currentlyPlayablePlayerType.Value = PlayerType.None;

                OnGameWin?.Invoke(this, new OnGameWinEventArgs
                {
                    line = line
                });
                break;
            };
        }
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentlyPlayablePlayerType()
    {
        return currentlyPlayablePlayerType.Value;
    }
}
