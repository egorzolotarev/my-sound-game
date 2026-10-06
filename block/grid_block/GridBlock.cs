using Godot;

public partial class GridBlock: IBlock
{
	[Signal]
	public delegate void StartDraggingEventHandler(GridBlock sender);

	[Signal]
	public delegate void StopDraggingEventHandler(GridBlock sender);

	[Signal]
	public delegate void RightMousePressedEventHandler(GridBlock sender);

	[Signal]
	public delegate void DraggingEventHandler(GridBlock sender);

    public bool isDragging = false;
	public Vector2 dragOffset;

	// Для определения клик или зажатие
	private bool isClick = false;
	private Vector2 blockStartPosition;
	private ulong startBlockTime;
	private const float MaxClickDistance = 5.0f;
	private const ulong MaxClickTime = 300;

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
				isClick = true;
				startBlockTime = Time.GetTicksMsec();
				EmitSignal(SignalName.StartDragging, this);
				dragOffset = GetGlobalMousePosition() - GlobalPosition;
				blockStartPosition = GlobalPosition;
				AcceptEvent();
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left && !inputEventMouseButton.Pressed)
			{
				isDragging = false;
				EmitSignal(SignalName.StopDragging, this);

				float distance = blockStartPosition.DistanceTo(GlobalPosition);
				if (distance > MaxClickDistance)
				{
					isClick = false;
				}

				ulong holdDuration = Time.GetTicksMsec() - startBlockTime;
				if (holdDuration > MaxClickTime)
				{
					isClick = false;
				}

				if (isClick)
				{
					isClick = false;
					EmitSignal(SignalName.LeftClick, this);
				}

				AcceptEvent();
			}
			// Обработака нажатия правой кнопки мыши
			if (inputEventMouseButton.ButtonIndex == MouseButton.Right && inputEventMouseButton.Pressed)
			{
				EmitSignal(SignalName.RightMousePressed, this);
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
				EmitSignal(SignalName.Dragging, this);
			}
		}
	}
}