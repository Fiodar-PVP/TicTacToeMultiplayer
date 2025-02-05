using System;
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

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public static GameManager Instance { get; private set; }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentlyPlayablePlayerType = new NetworkVariable<PlayerType>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager!");
        }

        Instance = this;
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
