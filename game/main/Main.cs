	using Godot;
	using System;
using System.ComponentModel.DataAnnotations;

public partial class Main : Node2D
	{
		private bool is_block_drugging = false;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			GridBlock block = (GridBlock)(ResourceLoader.Load<PackedScene>("res://block//grid_block//grid_block.tscn").Instantiate());
			block.Position = new Vector2(537, 350);
			
			AddBlock(block);
		}

		public void AddBlock(GridBlock block)
		{
			Grid grid = (Grid)GetNode<Node2D>("Grid");
			grid.AddChild(block);
			block.GuiInput += BlockGuiInput;
			block.StopDragging += GridBlockStopDragging;
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


		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}


	}
