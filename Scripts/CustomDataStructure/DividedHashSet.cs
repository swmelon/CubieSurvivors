using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class DividedHashSet<T> where T : class 
{
    public int Count { get { return pieceIndexMap.Count; } }
    private HashSet<T>[] pieces; 
    private Dictionary<T, int> pieceIndexMap = new Dictionary<T, int>();
    private int currentPieceIndex = 0;
    private int regularAddIndex = 0;
    private Queue<int> needFillUpQueue = new Queue<int>();

    public DividedHashSet(int numPieces)
    {
        pieces = new HashSet<T>[numPieces];
        needFillUpQueue = new Queue<int>();

        for (int i = 0; i < numPieces; i++)
        {
            pieces[i] = new HashSet<T>();
        }
    }

    public void Add(T item)
    {
        if (Contains(item))
        {
            Debug.LogError("DevidedHashSet already contains item: " + item);
            return;
        }

        if (regularAddIndex >= pieces.Length)
        {
            regularAddIndex = 0;
        }

        int index;

        if (needFillUpQueue.Count > 0)
        {
            index = needFillUpQueue.Dequeue();
        }
        else
        {
            index = regularAddIndex++;
        }

        pieces[index].Add(item);
        pieceIndexMap.Add(item, index);
    }

    public void Remove(T item)
    {
        if (!Contains(item))
        {
            Debug.LogError("DevidedHashSet does not contain item: " + item);
            return;
        }

        int index = pieceIndexMap[item];
        pieces[index].Remove(item);
        pieceIndexMap.Remove(item);
        needFillUpQueue.Enqueue(index);
    }

    public void Clear()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            pieces[i].Clear();
        }

        pieceIndexMap.Clear();
        needFillUpQueue.Clear();
        currentPieceIndex = 0;
        regularAddIndex = 0;
    }

    public bool Contains(T item)
    {
        return pieceIndexMap.ContainsKey(item);
    }

    public HashSet<T> GetNextPiece()
    {

        if (currentPieceIndex >= pieces.Length)
        {
            currentPieceIndex = 0;
        }

        return pieces[currentPieceIndex++];
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (HashSet<T> piece in pieces)
        {
            foreach (T item in piece)
            {
                yield return item;
            }
        }
    }
}
