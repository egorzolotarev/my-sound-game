using System;
using System.Collections.Generic;
using Godot;
public partial class Main : Node2D
{
	private bool is_block_drugging = false;
	private bool isLineConnecting = false;
	//Блок, из которого исходит линия
	private GridBlock lineSenderBlock;
	private Line2D connectionLine;
	// Структура данных для хранения связанных блоков и прикрепленных к ним линий
	private Dictionary<GridBlock, List<Tuple<GridBlock, Line2D, string>>> blockConnections;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		blockConnections = new Dictionary<GridBlock, List<Tuple<GridBlock, Line2D, string>>>();

		GridBlock block = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block.Position = new Vector2(537, 350);
			
		AddGridBlock(block);

		GridBlock block1 = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block1.Position = new Vector2(557, 350);
			
		AddGridBlock(block1);
	}

	public void AddGridBlock(GridBlock block)
	{
		Grid grid = (Grid)GetNode<Node2D>("Grid");
		grid.AddChild(block);
		block.GuiInput += BlockGuiInput;
		block.StopDragging += GridBlockStopDragging;
		block.RightMousePressed += GridBlockRightMousePressed;
		block.Dragging += GridBlockDragging;
	}

	// Когда блок перетаскивается, линия должна следовать за ним
 	private void GridBlockDragging(GridBlock sender)
	{
		GD.Print(blockConnections.Count);
		foreach (Tuple<GridBlock, Line2D, string> node in blockConnections[sender])
		{
			var (_, line, str) = node;
			if (str == "to")
			{
				line.SetPointPosition(0, line.ToLocal(sender.GlobalPosition + sender.Size / 2));
			}
			else
			{
				line.SetPointPosition(1, line.ToLocal(sender.GlobalPosition + sender.Size / 2));
			}
		}
	}

	// Когда блок перестают перетаскивать, приклеиваем его к одной из ближайших клеток
	private void GridBlockStopDragging(GridBlock sender)
	{
		GD.Print("Остановился, координаты: ", sender.Position.X, " ", sender.Position.Y);
		Grid grid = (Grid)GetNode<Node2D>("Grid");
		// Расстояние от координат центра блока до края сетки
		Vector2 distances = new Vector2(sender.Position.X + sender.Size.X/2, sender.Position.Y + sender.Size.X/2);

		// Вычисляем длину одного деления сетки
		float XLenght = grid.GetViewportRect().Size.X / (grid.NumberOfVerticalLines + 1);
		float YLenght = grid.GetViewportRect().Size.Y / (grid.NumberOfHorizontalLines + 1);

		// Вычисляем координаты, из которых можно получить ближайшие 4 точки поля, в квадрате из которых находится блок
		float XLeft = (int)(distances.X / XLenght) * XLenght;
		float XRight = ((int)(distances.X / XLenght) + 1) * XLenght;
		float YLeft = (int)(distances.Y / YLenght) * YLenght;
		float YRight = ((int)(distances.Y / YLenght) + 1) * YLenght;

		// Вычисляем растояния до координат, из которых можно получить ближайшие 4 точки поля, в квадрате из которых находится блок
		float XLeftDistance = distances.X - XLeft;
		float XRightDistance = XRight - distances.X;
		float YLeftDistance = distances.Y - YLeft;
		float YRightDistance = YRight - distances.Y;

		Vector2 newPosition;
		if (XLeftDistance < XRightDistance)
		{
			newPosition.X = XLeft;
		}
		else
		{
			newPosition.X = XRight;
		}
		if (YLeftDistance < YRightDistance)
		{
			newPosition.Y = YLeft;
		}
		else
		{
			newPosition.Y = YRight;
		}
		
		// Подгоняем центр блока к точке
		newPosition.X -= sender.Size.X/2;
		newPosition.Y -= sender.Size.Y/2;

		sender.Position = newPosition;

		// Чтобы линия не отставала от блока, когда его координаты корректируются под нужную точку, переопределяем ее позицию
		GridBlockDragging(sender);
	}

	// Создаем линию на блоке
	private void GridBlockRightMousePressed(GridBlock sender)
	{
		isLineConnecting = true;
		lineSenderBlock = sender;
		// Создаем линию
		connectionLine = (Line2D)(ResourceLoader.Load<PackedScene>("res://game//line_for_block//line_for_block.tscn").Instantiate());
		connectionLine.AddPoint(connectionLine.ToLocal(sender.GlobalPosition + sender.Size / 2));
		connectionLine.AddPoint(connectionLine.ToLocal(GetGlobalMousePosition()));
		AddChild(connectionLine);
	}

	private void BlockGuiInput(InputEvent @event)
	{
		Camera2d camera = GetNode<Camera2d>("Camera2D");
		Grid grid = (Grid)GetNode<Node2D>("Grid");
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left
			&& mouseButton.Pressed)
			{
				camera.canMove = false;
			}
			if (mouseButton.ButtonIndex == MouseButton.Left
			&& !mouseButton.Pressed)
			{
				camera.canMove = true;
				camera.isDragging = false;
			}
		}
	}

	public override void _Process(double delta)
	{
	}

	// Возвращает блок по заданным координатам
	GridBlock GetBlockUnderPosition(Vector2 pos)
	{
		Grid grid = GetNode<Grid>("Grid");

		foreach (Node child in grid.GetChildren())
		{
			if (child is GridBlock block)
			{
				Rect2 rect = new Rect2(block.GlobalPosition, block.Size);
				if (rect.HasPoint(pos))
				{
					return block;
				}
			}
		}

		return null;
	}

	public override void _Input(InputEvent @event)
    {
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			// Во время создания линии между блоками перерисовываем ее чтобы она следовала за мышкой
			if (isLineConnecting)
			{
				connectionLine.SetPointPosition(1, connectionLine.ToLocal(GetGlobalMousePosition()));
			}
		}

		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Right && !inputEventMouseButton.Pressed)
			{
				// Смотрим, остановилось ли создание линии между блоками на блоке, и если да, то создаем ее
				if (isLineConnecting)
				{
					isLineConnecting = false;
					GridBlock toBlock = GetBlockUnderPosition(GetGlobalMousePosition());

					// Если линию отпустили на пустом месте, то удаляем ее
					if (toBlock == null || toBlock == lineSenderBlock)
					{
						lineSenderBlock = null;

						RemoveChild(connectionLine);
						connectionLine.QueueFree();
						connectionLine = null;
					}
					// Иначе создаем линию
					else
					{
						connectionLine.SetPointPosition(1, connectionLine.ToLocal(toBlock.GlobalPosition + toBlock.Size/2));

						if (!blockConnections.ContainsKey(lineSenderBlock))
						{
							blockConnections[lineSenderBlock] = new List<Tuple<GridBlock, Line2D, string>>();
						}
						blockConnections[lineSenderBlock].Add(new Tuple<GridBlock, Line2D, string>(toBlock, connectionLine, "to"));

						if (!blockConnections.ContainsKey(toBlock))
						{
							blockConnections[toBlock] = new List<Tuple<GridBlock, Line2D, string>>();
						}
						blockConnections[toBlock].Add(new Tuple<GridBlock, Line2D, string>(lineSenderBlock, connectionLine, "from"));

						lineSenderBlock = null;
						connectionLine = null;
					}
				}
			}
		}
	}
}
