using Godot;

public partial class Camera2d : Camera2D
{
	[Export]
	public float minZoom = 2;

	[Export]
	public float maxZoom = 25;
	bool isDragging = false;
	Vector2 lastMousePos;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
			{
				isDragging = true;
				lastMousePos = inputEventMouseButton.Position;
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.Left && !inputEventMouseButton.Pressed)
			{
				isDragging = false;
			}
			else if ((inputEventMouseButton.ButtonIndex == MouseButton.WheelUp || inputEventMouseButton.ButtonIndex == MouseButton.WheelDown) && inputEventMouseButton.Pressed)
			{
				Vector2 mousePos = GetGlobalMousePosition();
				Vector2 newZoom = Zoom;
				if (inputEventMouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					newZoom += Vector2.One * 0.25f;
				}
				if (inputEventMouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					newZoom -= Vector2.One * 0.25f;
				}

				newZoom = newZoom.Clamp(new Vector2(minZoom, minZoom), new Vector2(maxZoom, maxZoom));
				Zoom = newZoom;

				Position += mousePos - GetGlobalMousePosition();
			}
		}

		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			if (isDragging)
			{
				Vector2 del = inputEventMouseMotion.Position - lastMousePos;
				this.Position -= del / Zoom.X;
				lastMousePos = inputEventMouseMotion.Position;
			}
		}
    }
}
