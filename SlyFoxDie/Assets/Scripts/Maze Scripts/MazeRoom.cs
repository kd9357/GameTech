using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRoom : ScriptableObject {

    public int settingsIndex;

    public MazeRoomSettings settings;

    private List<MazeCell> cells = new List<MazeCell>();

	public void Add (MazeCell cell)
    {
        cell.room = this;
        cells.Add(cell);
    }

    public void Assimilate(MazeRoom room)
    {
        for(int i = 0; i < room.cells.Count; ++i)
        {
            Add(room.cells[i]);
        }
    }

    //Return the number of cells this room contains
    public int Size
    {
        get
        {
            return cells.Count;
        }
    }

    public void Hide()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].Hide();
        }
    }

    public void Show()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].Show();
        }
    }
}
