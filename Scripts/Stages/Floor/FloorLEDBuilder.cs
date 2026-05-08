using Local.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class FloorLEDBuilder : MonoBehaviour
{
    [SerializeField]
    private GameObject LEDCubePrefab;

    [SerializeField]
    private LEDNode LEDHalfCornerCubePrefab, LEDOneSidedCornerCubePrefab, LEDTwoSidedCornerCubePrefab, LEDThreeSidedCornerCubePrefab;
    
    [SerializeField]
    private OnePureEffectSpawner transparentBlockSpawner;

    [SerializeField]
    private CornerLEDCubePool cornerLEDCubePool;

    [SerializeField]
    private GameObject cubeExplosionPrefab;

    [SerializeField]
    private Transform explosionParent;

    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;

    [SerializeField]
    private FloorGeoDataChannel floorGeoDataChannel;

    [SerializeField]
    [Range(0f, 0.5f)]
    private float thres1 = 0.5f;


    private FloorBlockSet cubeSet;
    private FloorLiquidBuilder liquidBuilder;

    public readonly float HalfCubeHeight = 0.5f;

    private static int MAX_STAGE_SIZE = 20;
    private static int MAX_MAP_SIZE = 2 * MAX_STAGE_SIZE - 1;
    private static int MIDDLE_MAP_INDEX = MAX_MAP_SIZE / 2;
    private static int MIN_HEIGHT = int.MinValue;

    private static readonly Dictionary<Vector3, Quaternion> lookCenterRotation = new Dictionary<Vector3, Quaternion>()
        {
            { Vector3.right, Quaternion.Euler(0, 0, 0) },
            { Vector3.forward, Quaternion.Euler(0, 90, 0) },
            { -Vector3.right, Quaternion.Euler(0, 180, 0) },
            { -Vector3.forward, Quaternion.Euler(0, 270, 0) },
        };

    private LEDNode[,] nodeArray = new LEDNode[2 * MAX_STAGE_SIZE - 1, 2 * MAX_STAGE_SIZE - 1];
    private (LEDNode, WorldDirection)[,] slopMap = new (LEDNode, WorldDirection)[2 * MAX_STAGE_SIZE - 1, 2 * MAX_STAGE_SIZE - 1];
    private int[,] heightMap = new int[2 * MAX_STAGE_SIZE - 1, 2 * MAX_STAGE_SIZE - 1];
    private int[,] tempLocalHeightMap;
    private List<(LEDNode, LEDNode)> prevSlops = new List<(LEDNode, LEDNode)>(), slops = new List<(LEDNode, LEDNode)>();

    private Dictionary<int, LEDNode[]> insideOutLayer = new Dictionary<int, LEDNode[]>();
    private List<Vector3Int> insideOut;
    private List<(int, int, int, int)> textOnFloor = new List<(int, int, int, int)>(); 

    private HashSet<LEDNode> onFloorObjectLocatableNode = new HashSet<LEDNode>();
    private HashSet<LEDNode> onFloorEdgeLocatableNode = new HashSet<LEDNode>();
    private HashSet<LEDNode> onFloorTextureLocatableNode = new HashSet<LEDNode>();
    private HashSet<LEDNode> onLiquidObjectLocatableNode = new HashSet<LEDNode>();
    private HashSet<LEDNode> ESDLocatableNodes = new HashSet<LEDNode>();
    private List<Vector2Int> ESDLocations = new List<Vector2Int>();

    private int explosiveCount = 0;
    private int spaceBetweenExplosive = 15;
    private int ESDPadding = 2;
    private int size, mapSize, prevSize;
    private List<GameObject> onFloorLEDButtons = new List<GameObject>();
    
    // flags for changing theme
    private bool changeTheme = false;
    private bool changeCurrentLayerTheme = false;
    private IEnumerator sequentialInstantitation;

    private int layer, inLayerIndex;
    private int numFloorBlocks = 0;

    public int Size
    {
        get => size;
    }

    public float FloorRatio
    {
        get => (float)numFloorBlocks / (mapSize * mapSize);
    }

    public FloorBlockSet CubeSet
    {
        get => cubeSet;
        set => cubeSet = value;
    }

    private void Awake()
    {
        prevSize = 0;
        liquidBuilder = GetComponentInChildren<FloorLiquidBuilder>();
    }

    private void OnEnable()
    {
        floorGeoDataChannel.RegisterLEDBuilder(this);
    }

    private void OnDisable()
    {
        floorGeoDataChannel.UnregisterLEDBuilder(this);
    }

    public void InitializeFloor()
    {
        if (cubeSet.CubePrefab == null)
        {
            Debug.LogError("Cube prefab is not assigned!");
            return;
        }

        insideOut = InsideOutCoordinates(MAX_STAGE_SIZE);
        InstantiateLEDCubes(insideOut);
        cubeSet.CornerLEDCubePool.Initialize();
    }

    public void ChangeFloorTheme()
    {
        if (cubeSet.CubePrefab == null)
        {
            Debug.LogError("Cube prefab is not assigned!");
            return;
        }

        insideOut = InsideOutCoordinates(MAX_STAGE_SIZE);
        cubeSet.CornerLEDCubePool.Initialize();
        changeTheme = true;

        sequentialInstantitation = InstantiateLEDCubesSequentially(insideOut);
        StartCoroutine(sequentialInstantitation);
    }

    public void TurnOnLayerWorkTrigger()
    {
        if (!changeTheme)
        {
            return;
        }

        changeCurrentLayerTheme = true;
    }


    public void BuildFloor(int[,] localHeightMap)
    {
        if (changeTheme)
        {
            changeTheme = false;
            changeCurrentLayerTheme = false;
            StopCoroutine(sequentialInstantitation);
            FinishInstantitation();
        }

        mapSize = localHeightMap.GetLength(0); 
        size = MapSizeToStageSize(mapSize);

        (LEDNode, WorldDirection)[,] localSlopMap = FindPropLocatableNodeAndArrangeSlops(localHeightMap);

        Copy(localSlopMap, slopMap);

        InstantiateCornerLEDCubes(localHeightMap);

        // prevent GetHeightOf() returns value of new heightMap.
        // tempLocalHeightMap is used to store the localHeightMap before it is transformed.
        tempLocalHeightMap = localHeightMap;
    }

    public void BuildFloor(int[,] localHeightMap, int targetStageSize)
    {
        int targetMapSize = StageSizeToMapSize(targetStageSize);
        int[,] interpolatedHeightMap = InterpolateLocalHeightMap(localHeightMap, targetMapSize);

        FillUpBlocks(interpolatedHeightMap);
        BuildFloor(interpolatedHeightMap);
    }

    public void BuildFloor(int targetStageSize, float threshold =0.5f, int padding=0, bool doubleDeck=false)
    {
        int targetMapSize = StageSizeToMapSize(targetStageSize);
        int[,] localHeightMap;

        if (padding == 0)
        {
             localHeightMap = GenerateLocalHeightMap(targetMapSize, threshold, doubleDeck); 
        }
        else
        {
            localHeightMap = GenerateLocalHeightMap(targetMapSize, padding, doubleDeck);
        }


        FillUpBlocks(localHeightMap);
        BuildFloor(localHeightMap);
    }

    public void BuildFlatFloor(int targetStageSize, int padding=0)
    {
        int targetMapSize = StageSizeToMapSize(targetStageSize);
        int[,] localHeightMap = GenerateFlatLocalHeightMap(targetMapSize, padding);
        BuildFloor(localHeightMap);
    }

    public void TransformFloor(Vector3 anchorPos)
    {
        Copy(tempLocalHeightMap, heightMap);

        transform.position = anchorPos;
        DisableUnusedNodes();
        ApplyHeightMapToCubes();
        ReflectSlopMapInHeightMap();

        DestroyPrevSlopsAndEnableNewSlops();

        prevSize = size;
    }

    public void TriggerFloorExplosion()
    {
        explosionParent.position = Vector3.zero;
        explosionParent.gameObject.SetActive(false);
        explosionParent.gameObject.SetActive(true);
        FMODAudioManager.instance.PlayOneShot(SFXTags.StageBoomed);
    }

    private void InstantiateLEDCubes(List<Vector3Int> coordinates)
    {
        insideOutLayer.Clear();
        explosionParent.gameObject.SetActive(false);
        int layer = GetLayerToAdd();

        if (layer == 0)
        {
            insideOutLayer[0] = new LEDNode[1];
        }

        int InLayerIndex = 0;

        foreach (var point in coordinates)
        {

            LEDNode node = Instantiate(cubeSet.CubePrefab, new Vector3(point.x, transform.position.y, point.y),
                Quaternion.identity, transform).GetComponent<LEDNode>();

            node.gameObject.SetActive(false);


            AddNodeToArray(node, point.x, point.y);


            layer = point.z;

            if (!insideOutLayer.ContainsKey(layer))
            {
                InLayerIndex = 0;
                insideOutLayer[point.z] = new LEDNode[8 * layer];
            }

            insideOutLayer[layer][InLayerIndex] = node;

            InLayerIndex++;

            if (explosiveCount == 0)
            {
                GameObject explosive = Instantiate(cubeSet.ExplosivePrefab, node.transform.position, Quaternion.identity, explosionParent);
            }

            int addition = RandomExtenstion.IsHappen(0.5f) ? 2 : 1;

            explosiveCount = (explosiveCount + addition) % spaceBetweenExplosive;
        }

        LinkNode();
    }

    private IEnumerator InstantiateLEDCubesSequentially(List<Vector3Int> coordinates)
    {
        insideOutLayer.Clear();
        explosionParent.gameObject.SetActive(false);
        layer = GetLayerToAdd();

        if (layer == 0)
        {
            insideOutLayer[0] = new LEDNode[1];
        }

        inLayerIndex = 0;
        
        foreach (var point in coordinates)
        {
            layer = point.z;

            if (!insideOutLayer.ContainsKey(layer))
            {
                inLayerIndex = 0;
                insideOutLayer[point.z] = new LEDNode[8 * layer];
                yield return new WaitUntil(() => changeCurrentLayerTheme);
                changeCurrentLayerTheme = false;
            }
      
            LEDNode node = Instantiate(cubeSet.CubePrefab, new Vector3(point.x, transform.position.y, point.y),
            Quaternion.identity, transform).GetComponent<LEDNode>();

            node.gameObject.SetActive(false);
            AddNodeToArray(node, point.x, point.y);


           

            insideOutLayer[layer][inLayerIndex] = node;

            inLayerIndex++;

            if (explosiveCount == 0)
            {
                GameObject explosive = Instantiate(cubeSet.ExplosivePrefab, node.transform.position, Quaternion.identity, explosionParent);
            }

            int addition = RandomExtenstion.IsHappen(0.5f) ? 2 : 1;

            explosiveCount = (explosiveCount + addition) % spaceBetweenExplosive;

        }

        LinkNode();
    }


    private void FinishInstantitation()
    {
        foreach (var point in insideOut)
        {
            if (layer > point.z)
            {
                continue;
            }

            layer = point.z;

            if (!insideOutLayer.ContainsKey(layer))
            {
                inLayerIndex = 0;
                insideOutLayer[point.z] = new LEDNode[8 * layer];
            }

            LEDNode node = Instantiate(cubeSet.CubePrefab, new Vector3(point.x, transform.position.y, point.y),
                Quaternion.identity, transform).GetComponent<LEDNode>();

            node.gameObject.SetActive(false);

            AddNodeToArray(node, point.x, point.y);



            insideOutLayer[layer][inLayerIndex] = node;

            inLayerIndex++;

            if (explosiveCount == 0)
            {
                GameObject explosive = Instantiate(cubeSet.ExplosivePrefab, node.transform.position, Quaternion.identity, explosionParent);
            }

            int addition = RandomExtenstion.IsHappen(0.5f) ? 2 : 1;

            explosiveCount = (explosiveCount + addition) % spaceBetweenExplosive;

        }

        LinkNode();
}



private void LinkNode()
    {
        for (int x = 0; x < 2 * MAX_STAGE_SIZE - 1; x++)
        {
            for (int z = 0; z < 2 * MAX_STAGE_SIZE - 1; z++)
            {
                LEDNode node =  nodeArray[x, z];

                node.left = (x > 0) ? nodeArray[x - 1, z] : null;
                node.right = (x < 2 * MAX_STAGE_SIZE - 2) ? nodeArray[x + 1, z] : null;
                node.top = (z < 2 * MAX_STAGE_SIZE - 2) ? nodeArray[x, z + 1] : null;
                node.bottom = (z > 0) ? nodeArray[x, z - 1] : null;
            }
        }
    }

    private void InstantiateCornerLEDCubes(int[,] localHeightMap)
    {
        // Assuming 'size' correctly represents the bounds of the slopMap to be instantiated
        int offset = MAX_STAGE_SIZE - size; // Calculate offset based on the size difference
        int length = StageSizeToMapSize(size);


        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                int mapX = x + offset;
                int mapZ = z + offset;

                var (prefab, direction) = slopMap[mapX, mapZ];

                if (prefab != null)
                {
                    // Calculate the world position; adjust as necessary for your coordinate system
                    Vector3 position = new Vector3(x - size + 1, localHeightMap[x, z] + 1, z - size + 1);

                    // Use WorldDirectionChannelSO to get the rotation
                    Quaternion rotation = worldDirectionChannel.Rotation(direction);


                    LEDNode node = cubeSet.CornerLEDCubePool.Get(prefab);
                    LEDNode nodeInSameLoc = nodeArray[mapX, mapZ];

                    // corner cube(node) will be activated after TransformFloor() is called.
                    // And this will replace cube(nodeInSameLoc).
                    node.transform.parent = transform;
                    node.gameObject.SetActive(false);
                    node.transform.SetLocalPositionAndRotation(position, rotation);
                    // Optional: Keep track of these instances if needed
                    slops.Add((node, nodeInSameLoc));
                }
            }
        }
    }


    private List<Vector3Int> InsideOutCoordinates(int size, int layer = 0)
    {
        List<Vector3Int> coordinates = new List<Vector3Int>();
        int x, y;

        while (layer < size)
        {
            for (y = -layer; y <= layer; y++)
            {
                for (x = -layer; x <= layer; x++)
                {
                    if (Mathf.Abs(x) == layer || Mathf.Abs(y) == layer)
                    {
                        coordinates.Add(new Vector3Int(x, y, layer));
                    }
                }
            }
            layer++;
        }

        return coordinates;
    }

    private int GetLayerToAdd()
    {
        return insideOutLayer.Count;
    }

    private void AddNodeToArray(LEDNode node, int x, int y)
    {
        int xIndex = x + MAX_STAGE_SIZE - 1;
        int yIndex = y + MAX_STAGE_SIZE - 1;
    
       LEDNode nodeInSameLoc = nodeArray[xIndex, yIndex];
        if (nodeInSameLoc != null)
        {
            if (nodeInSameLoc.gameObject.activeSelf)
            {
                // change shader
                transparentBlockSpawner.Spawn().transform.position = nodeInSameLoc.transform.position;
            }

            if (nodeInSameLoc.HasReplacement())
            {
                nodeInSameLoc.replacement.Drop();
                Destroy(nodeInSameLoc.gameObject);
            }
            else
            {
                nodeArray[xIndex, yIndex].Drop();
            }
        }
        nodeArray[x + MAX_STAGE_SIZE - 1, y + MAX_STAGE_SIZE - 1] = node;
    }

    private Vector2Int WorldPosToMapIndex(Vector3 worldPos)
    { 
        worldPos.y = 0f;

        Vector3 rotatedPos = Quaternion.Inverse(transform.rotation) * worldPos;
        int rotatedX = Mathf.RoundToInt(rotatedPos.x);
        int rotatedZ = Mathf.RoundToInt(rotatedPos.z);
        return new Vector2Int(rotatedX + MAX_STAGE_SIZE - 1, rotatedZ + MAX_STAGE_SIZE - 1);
    }

    private void FillUpBlocks(int[,] localHeightMap)
    {
        int size = localHeightMap.GetLength(0);

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (localHeightMap[x, z] != 0)
                {
                    // ������ �ø����μ� ����޴� �������� �ٽ� Ȯ���ؾ���. ���� ������� ���ȣ��?
                    EvaluateAndRaiseBlock(ref localHeightMap, x, z);
                }
            }
        }
    }

    private void EvaluateAndRaiseBlockRecursively(ref int[,] localHeightMap, int x, int z)
    {

    }

    private void EvaluateAndRaiseBlock(ref int[,] localHeightMap, int x, int z)
    {
        int length = localHeightMap.GetLength(0);
        int currentHeight = localHeightMap[x, z];

        // Array to hold the height of adjacent blocks: Left, Right, Down, Up
        int[] adjacentHeights = new int[4]
        {
        (x > 0) ? localHeightMap[x - 1, z] : currentHeight, // Left
        (x < length - 1) ? localHeightMap[x + 1, z] : currentHeight, // Right
        (z > 0) ? localHeightMap[x, z - 1] : currentHeight, // Down
        (z < length - 1) ? localHeightMap[x, z + 1] : currentHeight, // Up
        };

        int higherAdjacentCount = 0;

        for (int i = 0; i < adjacentHeights.Length; i++)
        {
            if (adjacentHeights[i] > currentHeight)
            {
                higherAdjacentCount++;
            }
        }

        bool shouldRaise = false;
        if (higherAdjacentCount >= 3)
        {
            // Case 2: Three or more adjacents are higher
            shouldRaise = true;
        }
        else if (higherAdjacentCount == 2)
        {
            // Case 1: Check if two higher adjacents form a straight line with the current block
            if ((adjacentHeights[0] > currentHeight && adjacentHeights[1] > currentHeight) ||
                (adjacentHeights[2] > currentHeight && adjacentHeights[3] > currentHeight))
            {
                shouldRaise = true;
            }
        }

        // If the block should be raised
        if (shouldRaise)
        {
            Debug.DrawRay(new Vector3(x - this.size + 1, currentHeight, z - this.size + 1), Vector3.up * 10, Color.red, 10f);
            localHeightMap[x, z] = 0; // Raise the block

            // Check adjacent blocks again since the current block's height has changed
            if (x > 0 && localHeightMap[x - 1, z] < 0) EvaluateAndRaiseBlock(ref localHeightMap, x - 1, z);
            if (x < length - 1 && localHeightMap[x + 1, z] < 0) EvaluateAndRaiseBlock(ref localHeightMap, x + 1, z);
            if (z > 0 && localHeightMap[x, z - 1] < 0) EvaluateAndRaiseBlock(ref localHeightMap, x, z - 1);
            if (z < length - 1 && localHeightMap[x, z + 1] < 0) EvaluateAndRaiseBlock(ref localHeightMap, x, z + 1);
        }
    }

    private (LEDNode, WorldDirection)[,] FindPropLocatableNodeAndArrangeSlops(int[,] localHeightMap)
    {
        onFloorObjectLocatableNode.Clear();
        onFloorTextureLocatableNode.Clear();
        ESDLocatableNodes.Clear();
        ESDLocations.Clear();
        onLiquidObjectLocatableNode.Clear();
        numFloorBlocks = 0;

        int length = localHeightMap.GetLength(0);
        (LEDNode, WorldDirection)[,] localSlopMap = new (LEDNode, WorldDirection)[length, length];

        int offset = MAX_STAGE_SIZE - size;

        // �ٱ� �׵θ� (�������� �ٷ� �ٱ�)
        for (int x = ESDPadding; x < length - ESDPadding; x++)
        {
            int xArrayIndex = x + offset;
            int zArrayIndex = -1 + offset;

            ESDLocatableNodes.Add(nodeArray[xArrayIndex, zArrayIndex]);
            ESDLocations.Add(new Vector2Int(xArrayIndex, zArrayIndex));

            zArrayIndex = length + offset;
            ESDLocatableNodes.Add(nodeArray[xArrayIndex, zArrayIndex]);
            ESDLocations.Add(new Vector2Int(xArrayIndex, zArrayIndex));
        }

        for (int z = ESDPadding; z < length - ESDPadding; z++)
        {
            int xArrayIndex = -1 + offset;
            int zArrayIndex = z + offset;

            ESDLocatableNodes.Add(nodeArray[xArrayIndex, zArrayIndex]);
            ESDLocations.Add(new Vector2Int(xArrayIndex, zArrayIndex));

            xArrayIndex = length + offset;
            ESDLocatableNodes.Add(nodeArray[xArrayIndex, zArrayIndex]);
            ESDLocations.Add(new Vector2Int(xArrayIndex, zArrayIndex));
        }


        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                // �������� ���� �׵θ�
                bool onBoundary = x == 0 || x == length - 1 || z == 0 || z == length - 1;

                // Collect heights of adjacent and diagonal blocks in a 3x3 matrix centered on (x, z)
                int[,] adjacentHeights = GetAdjacentHeights(localHeightMap, x, z);

                int directHigherCount = CountDirectHigherAdjacent(adjacentHeights, out int directLowerCount);
                int diagonalHigherCount = CountDiagonalHigherAdjacent(adjacentHeights);

                if (directHigherCount == 0)
                {
                    if (localHeightMap[x, z] == 0)
                    {
                        LEDNode node = nodeArray[x + offset, z + offset];

                        // ��� ���� 0 �� ���� ���� ���⿡ �����ϹǷ� OnEdge�� ���� �ʱ�ȭ�� �ʿ�� ����.
                        node.OnEdge = directLowerCount > 0;
                        numFloorBlocks++;
                        onFloorObjectLocatableNode.Add(node);
                        onFloorTextureLocatableNode.Add(node);
                    }
                    else if (!onBoundary && localHeightMap[x, z] == -1 && x != length - 1 )
                    {
                        LEDNode node = nodeArray[x + offset, z + offset];
                        onLiquidObjectLocatableNode.Add(node);
                    }
                }

                // Determine prefab and its rotation based on adjacent heights
                localSlopMap[x, z] = DeterminePrefabAndRotation(adjacentHeights, directHigherCount, diagonalHigherCount);
            }
        }

        int centerIndex = MAX_STAGE_SIZE - 1;

        // remove center node and adjacents
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                onFloorObjectLocatableNode.Remove(nodeArray[centerIndex + i, centerIndex + j]);
            }
        }

        return localSlopMap;
    }

    private void ReflectSlopMapInHeightMap()
    {
        int length = 2 * size - 1;
        int offset = MAX_STAGE_SIZE - size;

        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                int mapX = x + offset;
                int mapZ = z + offset;

                if (slopMap[mapX, mapZ].Item1 != null)
                {
                    // reflect the corner cube in the height map
                    heightMap[mapX, mapZ] += 1;
                }
            }
        }
    }

    private (LEDNode, WorldDirection) DeterminePrefabAndRotation(int[,] adjacentHeights, int directHigherCount, int diagonalHigherCount)
    {
        // Part 1: Directly adjacent checks
        if (directHigherCount == 1)
        {
            if (diagonalHigherCount > 0 && CheckTriangleCase(adjacentHeights, out WorldDirection direction))
            {
                return (cubeSet.ThreeSidedCornerCubePrefab, direction);
            }
            WorldDirection rotation = GetHigherAdjacentDirection(adjacentHeights, true);
            return (cubeSet.HalfCornerCubePrefab, rotation);
        }
        else if (directHigherCount == 2)
        {
            // Two consecutive blocks higher
            WorldDirection rotation = GetRotationForTwoConsecutiveHigher(adjacentHeights);
            return (cubeSet.ThreeSidedCornerCubePrefab, rotation);
        }

        // Part 2: Diagonal checks
        if (diagonalHigherCount == 1)
        {
            WorldDirection rotation = GetRotationForSingleDiagonalHigher(adjacentHeights);
            return (cubeSet.OneSidedCornerCubePrefab, rotation);
        }
        else if (diagonalHigherCount == 2)
        {
            WorldDirection rotation = GetRotationForTwoDiagonalHigher(adjacentHeights);
            return (cubeSet.TwoSidedCornerCubePrefab, rotation);
        }

        return (null, WorldDirection.North);
    }

    private int CountDirectHigherAdjacent(int[,] adjacentHeights, out int lowerCount)
    {
        int count = 0;
        lowerCount = 0;
        int centerHeight = adjacentHeights[1, 1];

        // Directly adjacent indices (top, right, bottom, left)
        int[,] directions = new int[,] { { 0, 1 }, { 1, 2 }, { 2, 1 }, { 1, 0 } };
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dz = directions[i, 1];
            if (adjacentHeights[dx, dz] > centerHeight)
            {
                count++;
            }

            if (adjacentHeights[dx, dz] < centerHeight)
            {
                lowerCount++;
            }
        }

        return count;
    }

    private int CountDiagonalHigherAdjacent(int[,] adjacentHeights)
    {
        int count = 0;
        int centerHeight = adjacentHeights[1, 1];

        // Diagonally adjacent indices
        int[,] directions = new int[,] { { 0, 0 }, { 0, 2 }, { 2, 0 }, { 2, 2 } };
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dz = directions[i, 1];
            if (adjacentHeights[dx, dz] > centerHeight)
            {
                count++;
            }
        }

        return count;
    }

    private bool CheckTriangleCase(int[,] adjacentHeights, out WorldDirection direction)
    {
        int centerHeight = adjacentHeights[1, 1];
        int leftHeight = adjacentHeights[1, 0];
        int topHeight = adjacentHeights[0, 1];
        int rightHeight = adjacentHeights[1, 2];
        int bottomHeight = adjacentHeights[2, 1];
        int topLeftHeight = adjacentHeights[0, 0];
        int topRightHeight = adjacentHeights[0, 2];
        int bottomRightHeight = adjacentHeights[2, 2];
        int bottomLeftHeight = adjacentHeights[2, 0];

        if (centerHeight < leftHeight)
        {
            if (centerHeight < topRightHeight)
            {
                direction = WorldDirection.East;
                return true;
            }

            if (centerHeight < bottomRightHeight)
            {
                direction = WorldDirection.North;
                return true;
            }
        }

        if (centerHeight < rightHeight)
        {
            if (centerHeight < bottomLeftHeight)
            {
                direction = WorldDirection.West;
                return true;
            }

            if (centerHeight < topLeftHeight)
            {
                direction = WorldDirection.South;
                return true;
            }
        }

        if (centerHeight < topHeight)
        {
            if (centerHeight < bottomLeftHeight)
            {
                direction = WorldDirection.East;
                return true;
            }

            if (centerHeight < bottomRightHeight)
            {
                direction = WorldDirection.South;
                return true;
            }
        }

        if (centerHeight < bottomHeight)
        {
            if (centerHeight < topLeftHeight)
            {
                direction = WorldDirection.North;
                return true;
            }

            if (centerHeight < topRightHeight)
            {
                direction = WorldDirection.West;
                return true;
            }
        }

        direction = WorldDirection.North;
        return false;
    }

    private int[,] GetAdjacentHeights(int[,] localHeightMap, int centerX, int centerZ)
    {
        int length = localHeightMap.GetLength(0);
        int[,] adjacentHeights = new int[3, 3];
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                int mapX = centerX + x;
                int mapZ = centerZ + z;
                if (mapX >= 0 && mapX < length && mapZ >= 0 && mapZ < length)
                {
                    adjacentHeights[x + 1, z + 1] = localHeightMap[mapX, mapZ];
                }
                else
                {
                    adjacentHeights[x + 1, z + 1] = -1; // Outside of map bounds
                }
            }
        }
        return adjacentHeights;
    }

    private WorldDirection GetHigherAdjacentDirection(int[,] adjacentHeights, bool direct)
    {
        int centerHeight = adjacentHeights[1, 1];

        if (direct)
        {
            // Direct Adjacent Cases
            if (adjacentHeights[0, 1] > centerHeight) return WorldDirection.East; // Top
            if (adjacentHeights[1, 0] > centerHeight) return WorldDirection.North;  // Left
            if (adjacentHeights[1, 2] > centerHeight) return WorldDirection.South; // Right
            if (adjacentHeights[2, 1] > centerHeight) return WorldDirection.West;   // Down
        }
        else
        {
            // Diagonal Cases - Assumes a clockwise rotation from the diagonal higher block to the nearest direct block
            if (adjacentHeights[0, 0] > centerHeight) return WorldDirection.South; // Upper Left
            if (adjacentHeights[0, 2] > centerHeight) return WorldDirection.East; // Upper Right
            if (adjacentHeights[2, 0] > centerHeight) return WorldDirection.West;  // Lower Left
            if (adjacentHeights[2, 2] > centerHeight) return WorldDirection.North; // Lower Right
        }

        return WorldDirection.North; // Default, no higher adjacent
    }

    private WorldDirection GetRotationForTwoConsecutiveHigher(int[,] adjacentHeights)
    {
        int centerHeight = adjacentHeights[1, 1];
        int leftHeight = adjacentHeights[1, 0];
        int topHeight = adjacentHeights[0, 1];
        int rightHeight = adjacentHeights[1, 2];
        int bottomHeight = adjacentHeights[2, 1];

        // Simplified logic for two consecutive higher blocks
        if (leftHeight > centerHeight && topHeight > centerHeight)
        {
            return WorldDirection.East; // Rotate clockwise from the first block
        }

        if (topHeight > centerHeight && rightHeight > centerHeight)
        {
            return WorldDirection.South;
        }

        if (rightHeight > centerHeight && bottomHeight > centerHeight)
        {
            return WorldDirection.West;
        }

        if (bottomHeight > centerHeight && leftHeight > centerHeight)
        {
            return WorldDirection.North;
        }

        return WorldDirection.North;
    }

    private WorldDirection GetRotationForSingleDiagonalHigher(int[,] adjacentHeights)
    {
        // Determine rotation based on single higher diagonal and nearest direct block
        // This requires detailed logic based on your specific prefab orientation needs

        int centerHeight = adjacentHeights[1, 1];
        int topLeftHeight = adjacentHeights[0, 0];
        int topRightHeight = adjacentHeights[0, 2];
        int bottomRightHeight = adjacentHeights[2, 2];
        int bottomLeftHeight = adjacentHeights[2, 0];

        if (topLeftHeight > centerHeight)
        {
            return WorldDirection.East; // Upper left diagonal higher
        }

        if (topRightHeight > centerHeight)
        {
            return WorldDirection.South; // Upper right diagonal higher
        }

        if (bottomRightHeight > centerHeight)
        {
            return WorldDirection.West; // Lower right diagonal higher
        }

        if (bottomLeftHeight > centerHeight)
        {
            return WorldDirection.North; // Lower left diagonal higher
        }


        return WorldDirection.North; // Placeholder
    }

    private WorldDirection GetRotationForTwoDiagonalHigher(int[,] adjacentHeights)
    {
        int centerHeight = adjacentHeights[1, 1];
        int topLeftHeight = adjacentHeights[0, 0];
        int topRightHeight = adjacentHeights[0, 2];
        int bottomRightHeight = adjacentHeights[2, 2];
        int bottomLeftHeight = adjacentHeights[2, 0];

        // Rotate based on the line connecting two higher diagonal blocks
        if (topLeftHeight > centerHeight && bottomRightHeight > centerHeight)
        {
            return WorldDirection.East; // Upper left to lower right diagonal higher
        }

        if (topRightHeight > centerHeight && bottomLeftHeight > centerHeight)
        {
            return WorldDirection.South; // Upper right to lower left diagonal higher
        }

        Debug.LogError("Unexpected diagonal higher configuration!");

        return WorldDirection.North;
    }

    private void ApplyHeightMapToCubes()
    {
        int length = StageSizeToMapSize(size);
        int offset = MAX_STAGE_SIZE - size;

        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                int mapX = x + offset;
                int mapZ = z + offset;

                LEDNode node = nodeArray[mapX, mapZ];

                node.TurnOff();
                node.transform.localPosition = new Vector3(x - size + 1, heightMap[mapX, mapZ], z - size + 1);
                node.gameObject.SetActive(true);
            }
        }

        textOnFloor.Clear();
    }

    private void DestroyPrevSlopsAndEnableNewSlops()
    {
        //InsideOutCoordinate,
        //replace the block in the previous inclined block with a cube block in the same location
        //and destroy the inclined block
        if (prevSlops.Count != 0)
        {
            for (int i = 0; i < prevSlops.Count; i++)
            {
                LEDNode node = prevSlops[i].Item1;
                LEDNode nodeInSameLoc = prevSlops[i].Item2;

                Debug.Assert(ReferenceEquals(node, nodeInSameLoc.replacement), "Replacement should be same as current corner cube");

                nodeInSameLoc.replacement = null;
                if (node != null)
                {
                    node.Release();
                }
            }

            prevSlops.Clear();
        }

        for (int i = 0; i < slops.Count; i++)
        {
            LEDNode node = slops[i].Item1;
            LEDNode nodeInSameLoc = slops[i].Item2;

            // �ڳ� ��尡 ���� ��带 ��ü
            nodeInSameLoc.replacement = node;
            node.gameObject.SetActive(true);
            prevSlops.Add((node, nodeInSameLoc));
        }

        slops.Clear();
    }

    private void Copy<T>(T[,] from, T[,] to)
    {
        int sizeFrom = from.GetLength(0);
        int sizeTo = to.GetLength(0);

        int offset = Mathf.RoundToInt((sizeTo - sizeFrom) / 2);

        for (int x = 0; x < sizeFrom; x++)
        {
            for (int z = 0; z < sizeFrom; z++)
            {
                // Calculate the target indices in the 'to' array
                int targetX = x + offset;
                int targetZ = z + offset;

                // Check if the calculated indices are within the bounds of the 'to' array
                if (targetX >= 0 && targetX < sizeTo && targetZ >= 0 && targetZ < sizeTo)
                {
                    to[targetX, targetZ] = from[x, z];
                }
                else
                {
                    // Optionally handle the case where 'to' array cannot accommodate 'from' array at the calculated position
                    // For example, log a warning or throw a custom exception if necessary
                    Debug.LogWarning("Attempting to copy to an out-of-range index in the 'to' array. Copy operation skipped for element at from[" + x + ", " + z + "].");
                }
            }
        }
    }

    private int[,] InterpolateLocalHeightMap(int[,] localHeightMap, int targetSize)
    {
        int sourceSize = localHeightMap.GetLength(0);
        int[,] interpolatedMap = new int[targetSize, targetSize];

        float scale = (float)(sourceSize - 1) / (targetSize - 1);

        for (int x = 0; x < targetSize; x++)
        {
            for (int z = 0; z < targetSize; z++)
            {
                // Calculate the corresponding position in the source map
                float sourceX = x * scale;
                float sourceZ = z * scale;

                int floorX = Mathf.FloorToInt(sourceX);
                int floorZ = Mathf.FloorToInt(sourceZ);
                int ceilX = Mathf.Min(Mathf.CeilToInt(sourceX), sourceSize - 1);
                int ceilZ = Mathf.Min(Mathf.CeilToInt(sourceZ), sourceSize - 1);

                // Calculate interpolation weights
                float weightX = sourceX - floorX;
                float weightZ = sourceZ - floorZ;

                // Perform bilinear interpolation
                float interpolatedValue = (1 - weightX) * (1 - weightZ) * localHeightMap[floorX, floorZ]
                                        + weightX * (1 - weightZ) * localHeightMap[ceilX, floorZ]
                                        + (1 - weightX) * weightZ * localHeightMap[floorX, ceilZ]
                                        + weightX * weightZ * localHeightMap[ceilX, ceilZ];

                // Assign the interpolated and rounded value to the new map
                interpolatedMap[x, z] = Mathf.RoundToInt(interpolatedValue);
            }
        }

        return interpolatedMap;
    }

    private int[,] GenerateLocalHeightMap(int targetMapSize, float threshold, bool doubleDeck = false)
    {
        int[,] localHeightMap = new int[targetMapSize, targetMapSize];

        // Define the scale of the noise for more natural terrain variation
        float scale = 0.1f;

        // Generate random offsets for x and z coordinates for unique maps on each generation
        float offsetX = RandomExtenstion.GetFloatInRange(0f, 99999f);
        float offsetZ = RandomExtenstion.GetFloatInRange(0f, 99999f);

        // Set thresholds for height mapping based on the doubleDeck parameter
        float thresholdFor0 = doubleDeck ? 0.6f : threshold; // Higher threshold for 0 if doubleDeck is true
                                                        // Threshold for -1 is implicitly the rest if doubleDeck is false, otherwise controlled by the next threshold
        float thresholdForMinus2 = 0.3f; // Used only if doubleDeck is true

        for (int x = 0; x < targetMapSize; x++)
        {
            for (int z = 0; z < targetMapSize; z++)
            { 
                // Calculate normalized position with offset for Perlin noise
                float xCoord = offsetX + x * scale;
                float zCoord = offsetZ + z * scale;

                // Generate Perlin noise value
                float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);

                // Map the noise value to height values
                if (noiseValue > thresholdFor0)
                {
                    localHeightMap[x, z] = 0; // Noise above the threshold maps to 0
                }
                else if (doubleDeck && noiseValue > thresholdForMinus2)
                {
                    localHeightMap[x, z] = -1; // For double deck, intermediate values map to -1
                }
                else
                {
                    // Remaining values map to -1 if doubleDeck is false, or -2 if true
                    localHeightMap[x, z] = doubleDeck ? -2 : -1;
                }
            }
        }

        return localHeightMap;
    }

    private int[,] GenerateLocalHeightMap(int targetMapSize, int padding, bool doubleDeck = false)
    {
        int[,] localHeightMap = new int[targetMapSize, targetMapSize];

        // Define the scale of the noise for more natural terrain variation
        float scale = 0.1f;

        // Generate random offsets for x and z coordinates for unique maps on each generation
        float offsetX = RandomExtenstion.GetFloatInRange(0f, 99999f);
        float offsetZ = RandomExtenstion.GetFloatInRange(0f, 99999f);

        // Set thresholds for height mapping based on the doubleDeck parameter
        float thresholdFor0 = doubleDeck ? 0.6f : thres1; // Higher threshold for 0 if doubleDeck is true
                                                        // Threshold for -1 is implicitly the rest if doubleDeck is false, otherwise controlled by the next threshold
        float thresholdForMinus2 = 0.3f; // Used only if doubleDeck is true

        for (int x = 0; x < targetMapSize; x++)
        {
            for (int z = 0; z < targetMapSize; z++)
            {
                if (x < padding || x >= targetMapSize - padding || z < padding || z >= targetMapSize - padding)
                {
                    localHeightMap[x, z] = -1;
                    continue;
                }

                // Calculate normalized position with offset for Perlin noise
                float xCoord = offsetX + x * scale;
                float zCoord = offsetZ + z * scale;

                // Generate Perlin noise value
                float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);

                // Map the noise value to height values
                if (noiseValue > thresholdFor0)
                {
                    localHeightMap[x, z] = 0; // Noise above the threshold maps to 0
                }
                else if (doubleDeck && noiseValue > thresholdForMinus2)
                {
                    localHeightMap[x, z] = -1; // For double deck, intermediate values map to -1
                }
                else
                {
                    // Remaining values map to -1 if doubleDeck is false, or -2 if true
                    localHeightMap[x, z] = doubleDeck ? -2 : -1;
                }
            }
        }

        return localHeightMap;
    }

    private int[,] GenerateFlatLocalHeightMap(int targetMapSize, int padding=0)
    {
        int[,] localHeightMap = new int[targetMapSize, targetMapSize];

        for (int x = 0; x < targetMapSize; x++)
        {
            for (int z = 0; z < targetMapSize; z++)
            {
                if (x < padding || x >= targetMapSize - padding || z < padding || z >= targetMapSize - padding)
                {
                    localHeightMap[x, z] = -1;
                    continue;
                }

                localHeightMap[x, z] = 0;
            }
        }

        return localHeightMap;
    }


    private int StageSizeToMapSize(int stageSize)
    {
        return 2 * stageSize - 1;
    }

    private int MapSizeToStageSize(int mapSize)
    {
        return (mapSize + 1) / 2;
    }

    public void TurnOnLayer(int layer)
    {
        for (int i = 0; i < insideOutLayer[layer].Length; i++)
        {
            LEDNode node = insideOutLayer[layer][i];

            if (node.HasReplacement())
            {
                node.replacement.TurnOn();
            }
            else
            {
                node.TurnOn();
            }
        }
    }

    public void TurnOffLayer(int layer)
    {
        for (int i = 0; i < insideOutLayer[layer].Length; i++)
        {
            LEDNode node = insideOutLayer[layer][i];

            if (node.HasReplacement())
            {
                node.replacement.TurnOff();
            }
            else
            {
                node.TurnOff();
            }
        }
    }

    public int GetOutterLayer()
    {
        return size - 1;
    }
    public void DrawText(GridTextSO digitGrid, Vector2Int startPosition)
    {
        for (int y = 0; y < digitGrid.Height; y++)
        {
            for (int x = 0; x < digitGrid.Width; x++)
            {
                if (digitGrid.GetCell(x, y).isOccupied)
                {
                    // Get the node from the array and turn it on
                    LEDNode node = GetNodeFromArrayLocal(startPosition.x + x, startPosition.y - y);

                    if (!ReferenceEquals(node, null))
                    {
                        node.TurnOn();
                    }
                }
            }
        }

        textOnFloor.Add((startPosition.x, startPosition.y, digitGrid.Height, digitGrid.Width));
    }

    private LEDNode GetNodeFromArray(Vector3 worldPos)
    {
        Vector2Int mapIndex = WorldPosToMapIndex(worldPos);

        int xIndex = mapIndex.x;
        int yIndex = mapIndex.y;

        //check if the index is out of range

        if (xIndex < 0 || xIndex >= mapSize || yIndex < 0 || yIndex >= mapSize)
        {
            return null;
        }

        LEDNode node = nodeArray[xIndex, yIndex];

        if (node.HasReplacement())
        {
            return node.replacement;
        }
        
        return node;
    }

    private LEDNode GetNodeFromArrayLocal(int x, int y)
    {
        Vector2Int arrayIndex = new Vector2Int(x, y);

        int xIndex = arrayIndex.x + MAX_STAGE_SIZE - 1;
        int yIndex = arrayIndex.y + MAX_STAGE_SIZE - 1;

        int offset = MAX_STAGE_SIZE - size;
        //check if the index is out of range

        if (xIndex < offset || xIndex >= MAX_MAP_SIZE - offset || yIndex < offset || yIndex >= MAX_MAP_SIZE - offset)
        {
            return null;
        }

        LEDNode node = nodeArray[xIndex, yIndex];

        if (node.HasReplacement())
        {
            return node.replacement;
        }

        return node;
    }

    public int GetHeightOf(Vector3 worldPos)
    {
        if (IsOnFloor(worldPos))
        {
            Vector2Int mapIndex = WorldPosToMapIndex(worldPos);

            int height = heightMap[mapIndex.x, mapIndex.y];

            return height;
        }

        // half block size ��ŭ �������� �ʾƵ� �Ǵ� ���� : LEDParent�� y��ǥ�� -0.5�̱� ����

        return MIN_HEIGHT;
    }

    public bool IsOnFloor(Vector3 worldPos, int truncate= 1)
    {
        int worldX = Mathf.RoundToInt(worldPos.x);
        int worldZ = Mathf.RoundToInt(worldPos.z);
        int radius = size - truncate;

        return worldX >= -radius && worldX <= radius && worldZ >= -radius && worldZ <= radius;
    }

    public void ClearTextOnFloor()
    {
        for (int i = 0; i < textOnFloor.Count; i++)
        {
            int startX = textOnFloor[i].Item1;
            int startY = textOnFloor[i].Item2;
            int height = textOnFloor[i].Item3;
            int width = textOnFloor[i].Item4;

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y <= width; y++)
                {
                    LEDNode node = GetNodeFromArrayLocal(startX + x, startY - y);

                    node.TurnOff();
                }
            }
        }
    }

    public bool TryGetOnFloorObjectPosition(out Vector3 position)
    {
        if (onFloorObjectLocatableNode.Count == 0)
        {
            position = Vector3.zero;
            return false;
        }

        LEDNode node = onFloorObjectLocatableNode.PickRandom();

        if (node.left != null)
        {
            onFloorObjectLocatableNode.Remove(node.left);
        }

        if (node.right != null)
        {
            onFloorObjectLocatableNode.Remove(node.right);
        }

        if (node.top != null)
        {
            onFloorObjectLocatableNode.Remove(node.top);
        }

        if (node.bottom != null)
        {
            onFloorObjectLocatableNode.Remove(node.bottom);
        }

        onFloorObjectLocatableNode.Remove(node);

        Vector3 worldPos = node.transform.position;
        worldPos.y = transform.position.y + HalfCubeHeight;
        position = worldPos;
        return true;
    }

    public bool TryGetOnFloorTexturePosition(out Vector3 position, out bool onEdge)
    {
        onEdge = false;

        if (onFloorTextureLocatableNode.Count == 0)
        {
            position = Vector3.zero;
            return false;
        }

        LEDNode node = onFloorTextureLocatableNode.PickRandom();
        onEdge = node.OnEdge;

        if (node.left != null)
        {
            onFloorTextureLocatableNode.Remove(node.left);
        }

        if (node.right != null)
        {
            onFloorTextureLocatableNode.Remove(node.right);
        }

        if (node.top != null)
        {
            onFloorTextureLocatableNode.Remove(node.top);
        }

        if (node.bottom != null)
        {
            onFloorTextureLocatableNode.Remove(node.bottom);
        }

        onFloorTextureLocatableNode.Remove(node);

        Vector3 worldPos = node.transform.position;
        worldPos.y = transform.position.y + HalfCubeHeight;
        position = worldPos;
        return true;
    }

    public bool TryGetESDPositionAndRotation(out Vector3 position, out Quaternion rotation)
    {
        if (ESDLocatableNodes.Count == 0)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        LEDNode node = ESDLocatableNodes.PickRandom();

        if (node.left != null)
        {
            ESDLocatableNodes.Remove(node.left);
            ESDLocatableNodes.Remove(node.left.left);
        }

        if (node.right != null)
        {
            ESDLocatableNodes.Remove(node.right);
            ESDLocatableNodes.Remove(node.right.right);
        }

        if (node.top != null)
        {
            ESDLocatableNodes.Remove(node.top);
            ESDLocatableNodes.Remove(node.top.top);
        }

        if (node.bottom != null)
        {
            ESDLocatableNodes.Remove(node.bottom);
            ESDLocatableNodes.Remove(node.bottom.bottom);
        }

        ESDLocatableNodes.Remove(node);

        Vector3 worldPos = node.transform.position;
        worldPos.y = transform.position.y + HalfCubeHeight;
        position = worldPos;
        rotation = GetLookCenterRotation(worldPos);

        print(worldDirectionChannel.RotationToDirection(rotation));

        return true;
    }

    public bool TryGetOnLiquidObjectPosition(out Vector3 position)
    {
        if (onLiquidObjectLocatableNode.Count == 0)
        {
            position = Vector3.zero;
            return false;
        }

        LEDNode node = onLiquidObjectLocatableNode.PickRandom();

        if (node.left != null)
        {
            onLiquidObjectLocatableNode.Remove(node.left);
            onLiquidObjectLocatableNode.Remove(node.left.left);
            onLiquidObjectLocatableNode.Remove(node.left.top);
            onLiquidObjectLocatableNode.Remove(node.left.bottom);
            onLiquidObjectLocatableNode.Remove(node.left.left.left);
        }

        if (node.right != null)
        {
            onLiquidObjectLocatableNode.Remove(node.right);
            onLiquidObjectLocatableNode.Remove(node.right.right);
            onLiquidObjectLocatableNode.Remove(node.right.top);
            onLiquidObjectLocatableNode.Remove(node.right.bottom);
            onLiquidObjectLocatableNode.Remove(node.right.right.right);
        }

        if (node.top != null)
        {
            onLiquidObjectLocatableNode.Remove(node.top);
            onLiquidObjectLocatableNode.Remove(node.top.top);
            onLiquidObjectLocatableNode.Remove(node.top.left);
            onLiquidObjectLocatableNode.Remove(node.top.right);
            onLiquidObjectLocatableNode.Remove(node.top.top.top);
        }

        if (node.bottom != null)
        {
            onLiquidObjectLocatableNode.Remove(node.bottom);
            onLiquidObjectLocatableNode.Remove(node.bottom.bottom);
            onLiquidObjectLocatableNode.Remove(node.bottom.left);
            onLiquidObjectLocatableNode.Remove(node.bottom.right);
            onLiquidObjectLocatableNode.Remove(node.bottom.bottom.bottom);
        }

        onLiquidObjectLocatableNode.Remove(node);

        Vector3 worldPos = node.transform.position;
        // y ��ǥ�� �ϴ� �������� �ʴ� ������ ����
        worldPos.y = transform.position.y;
        position = worldPos;
        return true;
    }

    /// <summary>
    /// If the position is local, it returns local rotation. If the position is world, it returns world rotation.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private Quaternion GetLookCenterRotation(Vector3 position)
    {
        Vector3 axis = Vector3.zero;
        float x = Mathf.Abs(position.x);
        float z = Mathf.Abs(position.z);
        int sign = 0;

        if (x > z)
        {
            axis = Vector3.forward;

            if (position.x > 0)
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }
        }
        else if (x < z)
        {
            axis = Vector3.right;

            if (position.z > 0)
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }
        }
        else
        {

        }
        
        axis *= sign;
        return lookCenterRotation[axis];
    }

    private void DisableUnusedNodes()
    {
        if (size >= prevSize)
        {
            return;
        }

        int prevMapSize = StageSizeToMapSize(prevSize);
        int offset = MAX_STAGE_SIZE - prevSize;
        int diff = prevSize - size;

        for (int x = 0; x < prevMapSize; x++)
        {
            for (int z = 0; z < prevMapSize; z++)
            {
                if ( x < diff || x >= prevMapSize - diff || z < diff || z >= prevMapSize - diff)
                {
                    nodeArray[x + offset, z + offset].gameObject.SetActive(false);
                }
            }
        }
    }

    public void DeactivateLEDFloor()
    {
        gameObject.SetActive(false);
    }   

    public void OnCubeSetChanged()
    {
        insideOut = InsideOutCoordinates(MAX_STAGE_SIZE);
        InstantiateLEDCubes(insideOut);
    }

    public void RemoveFloor()
    {
        for (int i = 0; i < MAX_MAP_SIZE; i++)
        {
            for (int j = 0; j < MAX_MAP_SIZE; j++)
            {
                LEDNode node = nodeArray[i, j];
                if (node != null)
                {
                    if (node.HasReplacement())
                    {
                        Destroy(node.replacement.gameObject);
                    }

                    Destroy(node.gameObject);

                    nodeArray[i, j] = null;
                }
            }
        }
    }

    public int GetESDLocations(int numLocs, int numNeighborsToRemove, List<WorldDirection> directions, Vector3[] locations, Quaternion[] rotations)
    {
        if (ESDLocations.Count == 0)
        {
            return 0;
        }

        SortLocations(directions);

        int count = 0;

        for (int i = 0; i < ESDLocations.Count; i++)
        {
            Vector2Int pos = ESDLocations[i];
            LEDNode node = nodeArray[pos.x, pos.y];

            if (!ESDLocatableNodes.Contains(node))
            {
                continue;
            }

            Vector3 worldPos = node.transform.position;
            worldPos.y = transform.position.y + HalfCubeHeight;
        
            locations[count] = worldPos;
            rotations[count] = GetLookCenterRotation(worldPos);
            count += 1;

            RemoveNodeFromESDLocatables(node, numNeighborsToRemove);

            if (count == numLocs)
            {
                break;
            }
        }

        return count;
    }

    private void RemoveNodeFromESDLocatables(LEDNode node, int numNeighborsToRemove)
    {
        LEDNode left = node.left;
        LEDNode right = node.right;
        LEDNode top = node.top;
        LEDNode bottom = node.bottom;

        for (int i = 0; i < numNeighborsToRemove; i++)
        {
            if (left != null)
            {
                ESDLocatableNodes.Remove(left);
            }
            else
            {
                break;
            }

            left = left.left;
        }

        for (int i = 0; i < numNeighborsToRemove; i++)
        {
            if (right != null)
            {
                ESDLocatableNodes.Remove(right);
            }
            else
            {
                break;
            }

            right = right.right;
        }

        for (int i = 0; i < numNeighborsToRemove; i++)
        {
            if (top != null)
            {
                ESDLocatableNodes.Remove(top);
            }
            else
            {
                break;
            }

            top = top.top;
        }

        for (int i = 0; i < numNeighborsToRemove; i++)
        {
            if (bottom != null)
            {
                ESDLocatableNodes.Remove(bottom);
            }
            else
            {
                break;
            }

            bottom = bottom.bottom;
        }

        ESDLocatableNodes.Remove(node);
    }

    /// <summary>
    /// ���õ� ���⿡ ���� locations ����Ʈ�� �����Ѵ�.
    /// ���� �켱������ ������ �������� ��ġ�ȴ�.
    /// </summary>
    /// <param name="directions">���õ� ���� ����Ʈ</param>
    private void SortLocations(List<WorldDirection> directions)
    {
        // ����Ʈ�� �������� ���� ���� �켱������ ������ �������� ���ĵǵ��� ��
        ESDLocations.FisherShuffle();
        // �Ÿ� �հ踦 �������� ����
        ESDLocations.Sort((a, b) =>
        {
            int sumA = GetSumDistance(a, directions);
            int sumB = GetSumDistance(b, directions);
            return sumA.CompareTo(sumB);
        });
    }

    /// <summary>
    /// ���õ� �������κ����� �Ÿ� �հ踦 ����Ѵ�.
    /// </summary>
    /// <param name="pos">��� ��ġ</param>
    /// <param name="directions">���õ� ���� ����Ʈ</param>
    /// <returns>�Ÿ��� �հ�</returns>
    private int GetSumDistance(Vector2Int pos, List<WorldDirection> directions)
    {
        int sumDistance = 0;

        for (int i = 0; i < directions.Count; i++)
        {
            var dir = directions[i];
            switch (dir)
            {
                case WorldDirection.North:
                    sumDistance += pos.y;
                    sumDistance += Mathf.Abs(MIDDLE_MAP_INDEX - pos.x);
                    break;
                case WorldDirection.South:
                    sumDistance += (mapSize - 1 - pos.y);
                    sumDistance += Mathf.Abs(MIDDLE_MAP_INDEX - pos.x);
                    break;
                case WorldDirection.West:
                    sumDistance += pos.x;
                    sumDistance += Mathf.Abs(MIDDLE_MAP_INDEX - pos.y);
                    break;
                case WorldDirection.East:
                    sumDistance += (mapSize - 1 - pos.x);
                    sumDistance += Mathf.Abs(MIDDLE_MAP_INDEX - pos.y);
                    break;
            }
        }

        return sumDistance;
    }
}



