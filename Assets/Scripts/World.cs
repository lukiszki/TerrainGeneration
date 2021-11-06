using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    TerrainSettings settings;

    [SerializeField]static int worldSize=32;
    [SerializeField] float viewDistance;
    [SerializeField] float offset = 1;

    [SerializeField] GameObject player;
    Vector3 lastPlayerPosition = Vector2.negativeInfinity;
    Vector3 currentPlayerPosition;


    TerrainChunk[,] terrainChunks = new TerrainChunk[worldSize, worldSize];
    void Start()
    {
        UpdatePlayerPosition();
        StartCoroutine(GenerateTerrain());
    }

    bool hasEndedBuldingTerrain = false;
    IEnumerator GenerateTerrain()
    {
        for (int y = 0; y < worldSize; y++)
        {
            for (int x = 0; x < worldSize; x++)
            {

                terrainChunks[x, y] = new TerrainChunk(settings);

                terrainChunks[x, y].xOffset = x * settings.frequency;
                terrainChunks[x, y].yOffset = y * settings.frequency;
                terrainChunks[x, y].CreateTerrain("Terrain "+((x+1)*(y+1)).ToString()+"#");


                float xWorldPos = x * settings.xSize;
                float yWorldPos = y * settings.zSize;

                terrainChunks[x, y].SetPos(xWorldPos, yWorldPos);
                yield return null;
            }
        }
        hasEndedBuldingTerrain = true;
    }
    // Update is called once per frame
    void Update()
    {
        StartCoroutine(UpdateChunks());
    }
    IEnumerator UpdateChunks()
    {
        UpdatePlayerPosition();
        if (currentPlayerPosition != lastPlayerPosition && hasEndedBuldingTerrain)
        {
            hasEndedBuldingTerrain = false;
            lastPlayerPosition = currentPlayerPosition;
            foreach (var item in terrainChunks)
            {
                if (item.thisObject != null)
                {
                    if (item.GetDist(currentPlayerPosition) > viewDistance)
                    {
                        item.DestroyIt();
                    }
                }
                else
                {
                    if (item.GetDist(currentPlayerPosition) < viewDistance)
                    {
                        item.CreateTerrain("Terrain " + Random.Range(0, 10000) + "#");
                        item.ApplyLastPos();
                        yield return null;
                    }
                }
            }
            hasEndedBuldingTerrain = true;
        }
    }

    /*public void SetOffset()
    {
        for (int y = 0; y < worldSize; y++)
        {
            for (int x = 0; x < worldSize; x++)
            {
                settings.xOffset = x * offset;
                settings.yOffset = y * offset;
                terrainChunks[x, y].SetSettings(settings);
                terrainChunks[x, y].RefreshTerrain();
            }
        }
    }*/
    void UpdatePlayerPosition()
    {
        currentPlayerPosition = player.transform.position;
    }
}
