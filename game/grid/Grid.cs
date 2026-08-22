using System.Collections.Generic;
using Godot;

public partial class Grid : Node2D
{
	[Export]
	public int NumberOfHorizontalLines = 10;

	[Export]
	public int NumberOfVerticalLines = 6;

    public List<Vector2> dots = new List<Vector2>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		PackedScene lineForGridScene = ResourceLoader.Load<PackedScene>("res://game//grid//line_for_grid.tscn");

		for (float i = 1; i <= NumberOfVerticalLines; i++)
		{
			float x = GetViewportRect().Size.X / (NumberOfVerticalLines + 1) * i;
			Line2D line = lineForGridScene.Instantiate<Line2D>();
			line.AddPoint(new Vector2(x, GetViewportRect().Size.Y));
			line.AddPoint(new Vector2(x, 0));
			AddChild(line);
		}

		for (int i = 1; i <= NumberOfHorizontalLines; i++)
		{
			float y = GetViewportRect().Size.Y / (NumberOfHorizontalLines + 1) * i;
			Line2D line = lineForGridScene.Instantiate<Line2D>();
			line.AddPoint(new Vector2(0, y));
			line.AddPoint(new Vector2(GetViewportRect().Size.X, y));
			AddChild(line);
		}

		for (float i = 1; i <= NumberOfVerticalLines; i++)
		{
			float x = GetViewportRect().Size.X / (NumberOfVerticalLines + 1) * i;
			for (int j = 1; j <= NumberOfHorizontalLines; j++)
			{
				float y = GetViewportRect().Size.Y / (NumberOfHorizontalLines + 1) * j;
				dots.Add(new Vector2(x, y));
			}
		}

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
