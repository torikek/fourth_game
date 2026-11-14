extends SceneTree

var failures := 0

func _initialize():
	run.call_deferred()

func run():
	var apartment = load("res://resources/apartment_baked/apartment__baked.glb").instantiate()
	var walls := 0
	for node in apartment.find_children("*", "CollisionShape3D", true, false):
		if not str(node.get_parent().get_parent().name).begins_with("Wall"):
			continue
		walls += 1
		# Test the actual imported collision mesh in isolation from furniture.
		var body := StaticBody3D.new()
		var collision := CollisionShape3D.new()
		collision.shape = node.shape
		body.add_child(collision)
		root.add_child(body)
		var probe := CharacterBody3D.new()
		var sphere := CollisionShape3D.new()
		var shape := SphereShape3D.new()
		shape.radius = 0.05
		sphere.shape = shape
		probe.add_child(sphere)
		root.add_child(probe)
		await physics_frame
		var faces: PackedVector3Array = node.shape.get_faces()
		var largest := 0.0
		var center := Vector3.ZERO
		var normal := Vector3.ZERO
		for i in range(0, faces.size(), 3):
			var cross := (faces[i + 1] - faces[i]).cross(faces[i + 2] - faces[i])
			if cross.length_squared() > largest:
				largest = cross.length_squared()
				normal = cross.normalized()
				center = (faces[i] + faces[i + 1] + faces[i + 2]) / 3.0
		for side in [-1.0, 1.0]:
			probe.position = center + normal * side * 0.25
			if not probe.test_move(probe.transform, -normal * side * 0.5):
				failures += 1
				push_error("Wall allows passage from side %s: %s" % [side, node.get_parent().get_parent().name])
		body.free()
		probe.free()
	assert(walls == 3, "Expected three imported wall meshes")
	apartment.free()
	print("Wall collision checks: ", walls * 2, "; failures: ", failures)
	quit(1 if failures else 0)
