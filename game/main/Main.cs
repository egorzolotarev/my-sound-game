using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using Godot;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Linq;
public partial class Main : Node2D
{
	[Export]
	public float speed = 0.03f;
	private bool is_block_drugging = false;
	private bool isLineConnecting = false;
	//Блок, из которого исходит линия
	private GridBlock lineSenderBlock;
	private Line2D connectionLine;
	// Структура данных для хранения связанных блоков и прикрепленных к ним линий
	private Dictionary<GridBlock, List<(GridBlock, Line2D, string)>> blockConnections;

	// Начальные координаты перетаскиваемого блока, если место на которое его переносят уже занято
	private Vector2 blockStartPosition;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		blockConnections = new Dictionary<GridBlock, List<(GridBlock, Line2D, string)>>();

		StartBlock start = (StartBlock)(ResourceLoader.Load<PackedScene>("res://block//start_block//start_block.tscn").Instantiate());
		start.Position = new Vector2(517, 350);
			
		AddGridBlock(start);

		GridBlock block = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block.Position = new Vector2(537, 350);
			
		AddGridBlock(block);

		GridBlock block1 = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block1.Position = new Vector2(557, 350);
			
		AddGridBlock(block1);

		GridBlock block2 = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block2.Position = new Vector2(587, 350);
			
		AddGridBlock(block2);

		GridBlock block3 = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
		block3.Position = new Vector2(607, 350);
			
		AddGridBlock(block3);
	}

	public void AddGridBlock(GridBlock block)
	{
		Grid grid = (Grid)GetNode<Node2D>("Grid");
		grid.AddChild(block);
		block.GuiInput += BlockGuiInput;
		block.StopDragging += GridBlockStopDragging;
		block.RightMousePressed += GridBlockRightMousePressed;
		block.Dragging += GridBlockDragging;
		block.StartDragging += GridBlockStartDragging;
		block.LeftClick += GridBlockLeftClick;
	}

	private void GridBlockLeftClick(IBlock sender)
	{
		if (sender is StartBlock startBlock)
		{
			_ = SendSound(startBlock);
		}
		else if (sender is GridBlock gridBlock)
		{
			gridBlock.PlaySound();
		}
	}

	// Отправка звука связанным блокам
	private async Task SendSound(GridBlock sender)
	{
		foreach (var (block, line, text) in blockConnections[sender])
		{
			if (text == "to")
			{
				_ = SendSoundToLines(block, line.Points[0].DistanceTo(line.Points[1]));
			}
		}
	}

	// Асинхронный метод, доставляющи звук к блоку и запускающий цепочку уже от него
	private async Task SendSoundToLines(GridBlock recipient, float distance)
	{
		float time = distance * speed;
		await ToSignal(GetTree().CreateTimer(time), SceneTreeTimer.SignalName.Timeout);
		recipient.PlaySound();
		_ = SendSound(recipient);
		//recipient.EmitSignal(GridBlock.SignalName.LeftClick, recipient);
	}



	private void GridBlockStartDragging(GridBlock sender)
	{
		blockStartPosition = sender.Position;
	}

	// Когда блок перетаскивается, линия должна следовать за ним
 	private void GridBlockDragging(GridBlock sender)
	{
		GD.Print("Вызвалась GridBlockDragging");
		foreach (var (_, line, text) in blockConnections[sender])
		{
			if (text == "to")
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
		Grid grid = (Grid)GetNode<Node2D>("Grid");
		// Расстояние от координат центра блока до края сетки
		Vector2 distances = new Vector2(sender.Position.X + sender.Size.X/2, sender.Position.Y + sender.Size.X/2);
		//GD.Print("Координаты центра: ", distances.X, " ", distances.Y);

		// Вычисляем длину одного деления сетки
		float XLenght = grid.GetViewportRect().Size.X / (grid.NumberOfVerticalLines + 1);
		float YLenght = grid.GetViewportRect().Size.Y / (grid.NumberOfHorizontalLines + 1);

		// Вычисляем координаты, из которых можно получить ближайшие 4 точки поля, в квадрате из которых находится блок
		float XLeft = (int)(distances.X / XLenght) * XLenght;
		float XRight = ((int)(distances.X / XLenght) + 1) * XLenght;
		float YLeft = (int)(distances.Y / YLenght) * YLenght;
		float YRight = ((int)(distances.Y / YLenght) + 1) * YLenght;
		// GD.Print("Координаты ближайших точек: ");
		// GD.Print("XLeft: ", XLeft);
		// GD.Print("XRight: ", XRight);
		// GD.Print("YLeft: ", YLeft);
		// GD.Print("YRight: ", YRight);

		// Вычисляем расстояния до координат, из которых можно получить ближайшие 4 точки поля, в квадрате из которых находится блок
		float XLeftDistance = distances.X - XLeft;
		float XRightDistance = XRight - distances.X;
		float YLeftDistance = distances.Y - YLeft;
		float YRightDistance = YRight - distances.Y;

		// GD.Print("Расстояния до координат ближайших точек: ");
		// GD.Print("XLeft: ", XLeftDistance);
		// GD.Print("XRight: ", XRightDistance);
		// GD.Print("YLeft: ", YLeftDistance);
		// GD.Print("YRight: ", YRightDistance);

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

		// Если место уже занято, отправляем блок обратно
		GridBlock isBlockOnPosition = GetBlockUnderPosition(newPosition);
		if (isBlockOnPosition != null)
		{
			sender.Position = blockStartPosition;
			GridBlockDragging(sender);
			return;
		}

		//GD.Print("Новая позиция: ", newPosition.X, " ", newPosition.Y);

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
				Rect2 rect = new Rect2(block.Position, block.Size);
				if (rect.HasPoint(pos))
				{
					return block;
				}
			}
		}

		return null;
	}

	private bool DFS(GridBlock current, Dictionary<GridBlock, string> colours)
	{
		colours[current] = "gray";
		foreach (var (toBlock, _, text) in blockConnections[current])
		{
			if (text == "to")
			{
				if (colours[toBlock] == "gray")
				{
					return true;
				}
				if (colours[toBlock] == "white")
				{
					if (DFS(toBlock, colours))
					{
						return true;
					}
				}
			}
		}
		colours[current] = "black";
		return false;
	}

	private bool IsCycleInConnections()
	{
		Dictionary<GridBlock, string> colours = new Dictionary<GridBlock, string>();
		foreach (GridBlock block in blockConnections.Keys)
		{
			colours[block] = "white";
		}

		foreach (GridBlock block in colours.Keys)
		{
			if (colours[block] == "white")
			{
				if (DFS(block, colours))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void DeleteLine(Line2D deletingLine)
	{
		List<GridBlock> blocksToRemove = new List<GridBlock>();

		foreach (GridBlock block in blockConnections.Keys)
		{
			blockConnections[block].RemoveAll(node => node.Item2 == deletingLine);
			if (blockConnections[block].Count == 0)
			{
				blocksToRemove.Add(block);
			}
		}

		foreach (GridBlock block in blocksToRemove)
		{
			blockConnections.Remove(block);
		}

		if (IsInstanceValid(deletingLine))
		{
			deletingLine.QueueFree();
		}
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
					GD.Print("isLineConnecting");
					isLineConnecting = false;

					Grid grid = (Grid)GetNode<Node2D>("Grid");
					GridBlock toBlock = GetBlockUnderPosition(grid.GetLocalMousePosition());

					// Если линию отпустили на пустом месте, то удаляем ее
					if (toBlock == null || toBlock == lineSenderBlock || toBlock is StartBlock)
					{
						GD.Print("isLineConnecting линия удаляется");
						lineSenderBlock = null;

						RemoveChild(connectionLine);
						connectionLine.QueueFree();
						connectionLine = null;
					}
					// Иначе создаем линию
					else
					{
						GD.Print("isLineConnecting линия создается");

						if (!blockConnections.ContainsKey(lineSenderBlock))
						{
							blockConnections[lineSenderBlock] = new List<(GridBlock, Line2D, string)>();
						}
						blockConnections[lineSenderBlock].Add((toBlock, connectionLine, "to"));

						if (!blockConnections.ContainsKey(toBlock))
						{
							blockConnections[toBlock] = new List<(GridBlock, Line2D, string)>();
						}
						blockConnections[toBlock].Add((lineSenderBlock, connectionLine, "from"));

						if (IsCycleInConnections())
						{
							GD.Print("Есть цикл");
							DeleteLine(connectionLine);
						} else
						{
							connectionLine.SetPointPosition(1, connectionLine.ToLocal(toBlock.GlobalPosition + toBlock.Size/2));
						}

						lineSenderBlock = null;
						connectionLine = null;
					}
				}
			}
		}
	}
}
