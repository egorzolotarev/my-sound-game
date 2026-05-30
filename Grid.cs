using Godot;
using System;

public partial class Grid : Node2D
{
	[Export]
	public int NumberOfHorizontalLines = 10;

	[Export]
	public int NumberOfVerticalLines = 6;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PackedScene lineForGridScene = ResourceLoader.Load<PackedScene>("res://line_for_grid.tscn");

		for (float i = 1; i <= NumberOfHorizontalLines; i++)
		{
			float x = GetViewportRect().Size.X / (NumberOfHorizontalLines + 1) * i;
			Line2D line = lineForGridScene.Instantiate<Line2D>();
			line.AddPoint(new Vector2(x, GetViewportRect().Size.Y));
			line.AddPoint(new Vector2(x, 0));
			AddChild(line);
		}

		for (int i = 1; i <= NumberOfVerticalLines; i++)
		{
			float y = GetViewportRect().Size.Y / (NumberOfVerticalLines + 1) * i;
			Line2D line = lineForGridScene.Instantiate<Line2D>();
			line.AddPoint(new Vector2(0, y));
			line.AddPoint(new Vector2(GetViewportRect().Size.X, y));
			AddChild(line);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
