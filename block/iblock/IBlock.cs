using Godot;
using System;

public partial class IBlock : TextureRect
{
	[Export]
	public AudioStream sound { get; set; }
	public AudioStreamPlayer player {get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = new AudioStreamPlayer();
		player.Stream = sound;
		AddChild(player);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PlaySound()
	{
		player.Play();
	}
}
