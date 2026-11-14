extends SceneTree

var failures := 0
var player: CharacterBody3D

func _initialize():
	run.call_deferred()

func check(value: bool, message: String):
	if not value:
		failures += 1
		push_error(message)

func place(z: float):
	player.position = Vector3(0, 0, z)
	await physics_frame
	await physics_frame
	await physics_frame

func press(action: String):
	var event := InputEventAction.new()
	event.action = action
	event.pressed = true
	player.call("_Input", event)

func run():
	var door = load("res://scenes/Door.tscn").instantiate()
	door.set("AutoCloseAfterPassage", true)
	door.set("LockAfterAutoClose", true)
	root.add_child(door)
	player = load("res://scenes/player.tscn").instantiate()
	player.position.z = 3
	root.add_child(player)
	player.set_physics_process(false)
	player.set_process(false)
	door.call("Interact")
	await create_timer(0.6).timeout
	await place(0.8)
	await place(-0.8)
	await place(-3)
	await create_timer(1.1).timeout
	check(door.get("IsLocked"), "Automatic closing does not lock door")
	await place(-1)
	player.get_node("Camera3D").look_at(Vector3(0, 1.387379, 0))
	press("interact")
	await create_timer(0.6).timeout
	check(abs(door.get_node("DoorHinge").rotation.y) < 0.02, "E opens locked door")
	check(player.get_node("CanvasLayer/CameraLabel").text.contains("ЛКМ"), "Locked door hint missing")
	check(player.test_move(player.transform, Vector3(0, 0, 2)), "Closed door does not block exit")
	for i in range(4):
		press("attack")
	check(door.has_node("DoorHinge"), "Door destroyed before fifth hit")
	press("attack")
	await process_frame
	await physics_frame
	check(not door.has_node("DoorHinge"), "Fifth hit does not destroy locked door")
	check(not player.test_move(player.transform, Vector3(0, 0, 2)), "Destroyed door still blocks exit")
	print("DOOR ESCAPE: ", failures, " failures")
	quit(1 if failures else 0)
