@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
	# Thin wall meshes must block the player from either adjacent room.
	for node in scene.find_children("Wall*", "MeshInstance3D", true, false):
		for collision in node.find_children("*", "CollisionShape3D", true, false):
			if collision.shape is ConcavePolygonShape3D:
				collision.shape.backface_collision = true
	return scene
