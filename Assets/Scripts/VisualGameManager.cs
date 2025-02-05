using Unity.Netcode;
using UnityEngine;

public class VisualGameManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
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
    }

    private Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE, 0f);
    }
}
