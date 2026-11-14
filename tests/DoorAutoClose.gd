extends SceneTree

var failures := 0
var player: CharacterBody3D
var door: Node3D
var hinge: Node3D

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

func run():
	door = load("res://scenes/Door.tscn").instantiate()
	door.set("AutoCloseAfterPassage", true)
	root.add_child(door)
	hinge = door.get_node("DoorHinge")
	player = load("res://scenes/player.tscn").instantiate()
	player.position.z = 3
	root.add_child(player)
	player.set_physics_process(false)
	player.set_process(false)
	door.call("Interact")
	await create_timer(0.6).timeout
	await place(0.8)
	await create_timer(0.6).timeout
	check(hinge.rotation.y > 1.5, "Door closes while player is in doorway")
	await place(3)
	await create_timer(1.1).timeout
	check(hinge.rotation.y > 1.5, "Approaching and retreating counts as passage")
	await place(0.8)
	await place(-0.8)
	await place(-3)
	await create_timer(0.15).timeout
	check(hinge.rotation.y > 1.5, "Door closes before delay")
	await place(-0.8)
	await create_timer(0.8).timeout
	check(hinge.rotation.y > 1.5, "Door closes after player returns to doorway")
	await place(-3)
	await create_timer(1.1).timeout
	check(abs(hinge.rotation.y) < 0.02, "Door does not close after completed passage")
	# The same trigger works on subsequent passages in the opposite direction.
	door.call("Interact")
	await create_timer(0.6).timeout
	await place(-0.8)
	await place(0.8)
	await place(3)
	await create_timer(1.1).timeout
	check(abs(hinge.rotation.y) < 0.02, "Reverse passage does not close door")
	await place(0.8)
	door.call("Interact")
	await create_timer(0.6).timeout
	await place(-0.8)
	await place(-3)
	await create_timer(1.1).timeout
	check(abs(hinge.rotation.y) < 0.02, "Opening while already inside trigger misses passage")
	await place(3)
	door.call("Interact")
	await create_timer(0.6).timeout
	await place(0.8)
	await place(-0.8)
	await place(-3)
	door.call("Die")
	await create_timer(1.1).timeout
	check(not is_instance_valid(hinge), "Destroyed door keeps its leaf")
	print("DOOR AUTO CLOSE: ", failures, " failures")
	quit(1 if failures else 0)
