using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private const int MAX_OBJECTS = 10;
    private const int MAX_LEVELS = 5;

    private int level;
    private List<GameObject> objects;
    private Rect bounds;
    private QuadTree[] nodes;

    public QuadTree(int pLevel, Rect pBounds)
    {
        level = pLevel;
        objects = new List<GameObject>();
        bounds = pBounds;
        nodes = new QuadTree[4];
    }

    public void Clear()
    {
        objects.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    private void Split()
    {
        float subWidth = bounds.width / 2;
        float subHeight = bounds.height / 2;
        float x = bounds.x;
        float y = bounds.y;

        nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));
        nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    private int GetIndex(GameObject gameObject)
    {
        int index = -1;
        float verticalMidpoint = bounds.x + bounds.width / 2;
        float horizontalMidpoint = bounds.y + bounds.height / 2;

        // Object can completely fit within the top quadrants
        bool topQuadrant = (gameObject.transform.position.y < horizontalMidpoint);
        // Object can completely fit within the bottom quadrants
        bool bottomQuadrant = (gameObject.transform.position.y > horizontalMidpoint);

        // Object can completely fit within the left quadrants
        if (gameObject.transform.position.x < verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 1;
            }
            else if (bottomQuadrant)
            {
                index = 2;
            }
        }
        // Object can completely fit within the right quadrants
        else if (gameObject.transform.position.x > verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 0;
            }
            else if (bottomQuadrant)
            {
                index = 3;
            }
        }

        return index;
    }

    public void Insert(GameObject gameObject)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(gameObject);

            if (index != -1)
            {
                nodes[index].Insert(gameObject);

                return;
            }
        }

        objects.Add(gameObject);

        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(objects[i]);
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public void Remove(GameObject gameObject)
    {
        int index = GetIndex(gameObject);
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Remove(gameObject);
        }
        else
        {
            objects.Remove(gameObject);
        }
    }

    // Add additional methods to query the QuadTree, etc.
}

