using Godot;

public partial class GridBlock: IBlock
{
	[Signal]
	public delegate void StopDraggingEventHandler(GridBlock sender);

    public bool isDragging = false;
	public Vector2 dragOffset;
    public override void _Ready()
	{
		base._Ready();
	}

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
			{
				isDragging = true;
				dragOffset = GetGlobalMousePosition() - GlobalPosition;
				AcceptEvent();
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left && !inputEventMouseButton.Pressed)
			{
				GD.Print("Остановился, отправляю сигнал");
				isDragging = false;
				EmitSignal(SignalName.StopDragging, this);
				AcceptEvent();
			}
		}
    }

    public override void _Input(InputEvent @event)
    {
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			if (isDragging)
			{
				GlobalPosition = GetGlobalMousePosition() - dragOffset;
			}
		}
	}
}