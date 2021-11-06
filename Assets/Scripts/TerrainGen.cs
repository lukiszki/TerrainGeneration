using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainChunk
{
    public GameObject thisObject;

    Vector3 lastPos;

    MeshRenderer renderer;
    MeshFilter meshFilter;

    Vector3[] verticies;
    Vector2[] UVs;

    int[] triangles;
    Mesh mesh;

    TerrainSettings settings;

    float[,] heightMap;

    Texture2D texture;


    public float xOffset { private get; set; }
    public float yOffset { private get; set; }



    bool isCreated = false;
    public TerrainChunk(TerrainSettings _terrainSettings)
    {
        this.settings = _terrainSettings;
    }

    public void CreateTerrain(string terrainName)
    {
        if (isCreated) return;




        texture = new Texture2D(settings.texRes, settings.texRes);

        texture.filterMode = FilterMode.Point;

        mesh = new Mesh();

        thisObject = new GameObject(terrainName);
        renderer = thisObject.AddComponent<MeshRenderer>();
        meshFilter = thisObject.AddComponent<MeshFilter>();

        renderer.material = settings.material;


        meshFilter.mesh = mesh;
        RefreshTerrain();

        isCreated = true;
    }
    
    public void DestroyIt()
    {
        isCreated = false;
        GameObject.Destroy(thisObject);
    }


    public float GetDist(Vector3 playerPos)
    {
        return Vector3.Distance(lastPos, playerPos);
    }
    // Update is called once per frame
    float time = 0;


    void FillTexture()
    {
        for (int y = 0; y < settings.texRes; y++)
        {
            for (int x = 0; x < settings.texRes; x++)
            {
                float height = heightMap[x, y];

                Color col = GetColor(height);
                texture.SetPixel(x, y, col);
            }
        }
        texture.Apply();
    }

    private Color GetColor(float height)
    {
        Color color = new Color(1,0.75f,0.75f);
        for (int i = settings.regions.Length-1; i >= 0; i--)
        {
            if (settings.regions[i].Height >= height)
                color = settings.regions[i].color;

        }
        return color;
    }



    public void RefreshTerrain()
    {
        settings.xSize = settings.terrainRes;
        settings.zSize = settings.terrainRes;

        texture = new Texture2D(settings.texRes, settings.texRes);


        heightMap = Noise.GetHeightMap(settings.texRes +1, settings.texRes +1, settings.frequency / settings.texRes
            , settings.octaves, settings.lacunarity, settings.persistance, xOffset, yOffset,settings.meshHeightCurve);
        CreateShape();

        FillTexture();
        texture.filterMode = FilterMode.Point;

        renderer.material.SetTexture("_MainTexture", texture);

    }
    void CreateShape()
    {
        verticies = new Vector3[(settings.xSize + 1) * (settings.zSize + 1 )];
        UVs = new Vector2[(settings.xSize + 1) * (settings.zSize + 1)];
        for (int i = 0, z = 0; z <= settings.zSize; z++)
        {
            for (int x = 0; x <= settings.xSize; x++)
            {
                float height =  heightMap[x*(settings.texRes / settings.terrainRes), z * (settings.texRes / settings.terrainRes)] * settings.terrainAmplitude*settings.scale;


                verticies[i] = new Vector3((float)x/settings.texRes * settings.scale, height , (float)z / settings.texRes * settings.scale);
                UVs[i] = new Vector2((float)x / (float)settings.xSize, (float)z / (float)settings.zSize);
                i++;
            }
        }
        triangles = new int[settings.xSize * settings.zSize *6];
        int vert = 0;
        int tris = 0;


        for (int z = 0; z < settings.zSize; z++)
        {
            for (int x = 0; x < settings.xSize; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + settings.xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + settings.xSize + 1;
                triangles[tris + 5] = vert + settings.xSize + 2;

                vert++;
                tris += 6;
                UpdateMesh();
            }
            vert++;
        }
    }
    public void SetPos(float x,float y)
    {
        thisObject.transform.position = new Vector3(x, 0, y);
        lastPos = new Vector3(x, 0, y);
    }
    public void ApplyLastPos()
    {
        thisObject.transform.position = lastPos;
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
     
    }
    void OnDrawGizmos()
    {
        if (verticies == null)
            return;
        foreach (Vector3 vertex in verticies)
        {
            Gizmos.DrawSphere(vertex,0.01f);
        }
    }
    public void SetSettings(TerrainSettings settings)
    {
        this.settings = settings;
    }
}
public static class Noise
{
   public static float [,] GetHeightMap(int xSize, int ySize, float frequency,int octaves, float lacunarity,float persistance,float xOffset,float yOffset,AnimationCurve curve)
    {
        if (octaves < 1) octaves = 1;
        if (octaves > 5) octaves = 5;
        float amplitude = 1f;
        float[,] heightMap = new float[xSize, ySize];
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int i = 0; i < octaves; i++)
                {
                    amplitude = Mathf.Pow(persistance, i);
                    float freq = Mathf.Pow(lacunarity, i);
                    heightMap[x, y] += curve.Evaluate(Mathf.PerlinNoise(x * frequency*freq+(xOffset*freq), y * frequency *freq+(yOffset*freq) ) ) * amplitude;

                }

            }
        }
        return heightMap;
    }
}
[System.Serializable]
public struct TerrainRegion
{
    public Color color;
    public float Height;
}
[System.Serializable]
public class TerrainSettings
{

    public int xSize = 30;
    public int zSize = 30;
    public int terrainRes = 50;


    public float scale = 1;


    [Header("Terrain noise settings")]
    public float terrainAmplitude = 1;

    public float frequency = 0.1f;

    [Range(1, 5)]
    public int octaves = 3;

    [Range(0.5f, 4f)]
    public float lacunarity = 2;
    [Range(0.1f, 0.8f)]

    public float persistance = 0.5f;



    public int texRes = 50;


    public AnimationCurve meshHeightCurve;



    public TerrainRegion[] regions;
    public Material material;
}

