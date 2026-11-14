extends SceneTree

var failures := 0

func check(value: bool, message: String):
	if not value:
		failures += 1
		push_error(message)

func _initialize():
	run.call_deferred()

func run():
	var scene = load("res://scenes/scene.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	await physics_frame
	await physics_frame
	var player = scene.get_node("CharacterBody3D")
	var panel = player.get_node("ReadingPanel")
	var button = panel.get_node("Panel/Margin/VBox/Close")
	var label = panel.get_node("Panel/Margin/VBox/Text")
	var count = player.get_node("CanvasLayer/LabelItemsNumber")
	var apartment = scene.get_node("Apartment")
	var door = apartment.get_node("Door")
	# Проверяем обнаружение через настоящий луч и обработчик ввода игрока.
	var ray_book = apartment.get_node("Книга_о_предметах")
	player.global_position = ray_book.global_position + Vector3(0, 0.1, 1)
	player.get_node("Camera3D").look_at(ray_book.global_position)
	await physics_frame
	await create_timer(0.1).timeout
	var prompt = player.get_node("CanvasLayer/CameraLabel")
	check(prompt.text.contains(str(ray_book.name)), "Ray does not detect book")
	var interact_event = InputEventAction.new()
	interact_event.action = "interact"
	interact_event.pressed = true
	player.call("_Input", interact_event)
	check(panel.visible, "E does not open targeted book")
	button.emit_signal("pressed")
	var hinge = door.get_node("DoorHinge")
	door.call("Interact")
	await create_timer(0.65).timeout
	check(abs(hinge.rotation.y - PI / 2) < 0.05, "Door did not open")
	door.call("Interact")
	await create_timer(0.65).timeout
	check(abs(hinge.rotation.y) < 0.05, "Door did not close")
	door.call("ReceiveDamage", 40.0)
	check(not hinge.is_queued_for_deletion(), "Door destroyed before 50 damage")
	door.call("ReceiveDamage", 10.0)
	await process_frame
	check(not is_instance_valid(hinge), "Door not destroyed")
	check(door.has_node("Frame"), "Door frame removed")
	var chair = apartment.get_node("Стул")
	chair.call("ReceiveDamage", 9.0)
	check(not chair.is_queued_for_deletion(), "Chair destroyed before 10 damage")
	chair.call("ReceiveDamage", 1.0)
	await process_frame
	check(not is_instance_valid(chair), "Chair not destroyed")
	var book = apartment.get_node("Книга_о_доме")
	book.call("Interact")
	check(panel.visible and label.text == book.get("Text"), "Book text not shown")
	button.emit_signal("pressed")
	check(not panel.visible and is_instance_valid(book), "Book was collected")
	var book2 = apartment.get_node("Книга_о_предметах")
	book2.call("Interact")
	check(label.text == book2.get("Text") and label.text != book.get("Text"), "Books have incorrect texts")
	button.emit_signal("pressed")
	var note = apartment.get_node("Записка_первая")
	note.call("Interact")
	check(panel.visible and count.text == "0", "Note collected before closing")
	var old_position = player.position
	Input.action_press("forward")
	await create_timer(0.1).timeout
	Input.action_release("forward")
	check(Vector2(player.position.x, player.position.z).distance_to(Vector2(old_position.x, old_position.z)) < 0.01, "Movement not blocked during reading")
	button.emit_signal("pressed")
	check(count.text == "1", "Note counter incorrect")
	note.call("CloseText")
	check(count.text == "1", "Note counted twice")
	await process_frame
	check(not is_instance_valid(note), "Collected note still exists")
	var note2 = apartment.get_node("Записка_вторая")
	note2.call("Interact")
	button.emit_signal("pressed")
	check(count.text == "2", "Second note not counted")
	print("INTERACTION CHECKS: ", failures, " failures")
	quit(0 if failures == 0 else 1)
