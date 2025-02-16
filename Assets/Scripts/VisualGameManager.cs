using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VisualGameManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }

        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:    eulerZ = 0;  break;
            case GameManager.Orientation.Vertical:      eulerZ = 90;  break;
            case GameManager.Orientation.DiagonalA:     eulerZ = 45;  break;
            case GameManager.Orientation.DiagonalB:     eulerZ = -45;  break;
        }
        Transform lineTransform =
            Instantiate(
                lineCompletePrefab,
                GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
                Quaternion.Euler(0f, 0f, eulerZ)
                );
        lineTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(lineTransform.gameObject);
    }

    private void GameManager_OnClickedGridPosition(object sender, GameManager.OnClickedGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }

        Transform prefabTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        prefabTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(prefabTransform.gameObject);
    }

    private Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE, 0f);
    }
}
