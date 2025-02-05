using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public class OnClickedGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

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
            currentlyPlayablePlayerType.Value = PlayerType.Cross;
        }
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
}
