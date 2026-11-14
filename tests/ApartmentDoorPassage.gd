extends SceneTree

var failures := 0

func _initialize():
	run.call_deferred()

func check(value: bool, message: String):
	if not value:
		failures += 1
		push_error(message)

func run():
	var scene = load("res://scenes/scene.tscn").instantiate()
	root.add_child(scene)
	var player = scene.get_node("CharacterBody3D")
	var door = scene.get_node("Apartment/Door")
	door.call("ReceiveDamage", 50.0)
	await process_frame
	# Start on the lower side and use real gravity and MoveAndSlide.
	player.global_position = door.global_position + Vector3(0, 0.4, 0.8)
	player.rotation = Vector3.ZERO
	player.velocity = Vector3.ZERO
	await create_timer(0.4).timeout
	Input.action_press("forward")
	await create_timer(0.32).timeout
	Input.action_release("forward")
	check(player.global_position.z < door.global_position.z - 0.3,
		"Cannot walk back through destroyed Door: player catches on apartment floor")
	Input.action_press("backward")
	await create_timer(0.32).timeout
	Input.action_release("backward")
	check(player.global_position.z > door.global_position.z + 0.3,
		"Cannot walk through doorway in opposite direction")
	print("APARTMENT DOOR PASSAGE: ", failures, " failures")
	quit(1 if failures else 0)
