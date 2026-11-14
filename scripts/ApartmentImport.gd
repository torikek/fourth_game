@tool
extends EditorScenePostImport

# обрабатываем импортированную сцену в редакторе перед её сохранением
func _post_import(scene: Node) -> Object:
	# тонкие стены должны останавливать игрока с обеих сторон
	for node in scene.find_children("Wall*", "MeshInstance3D", true, false):
		for collision in node.find_children("*", "CollisionShape3D", true, false):
			if collision.shape is ConcavePolygonShape3D:
				collision.shape.backface_collision = true
	# возвращаем изменённую сцену для завершения импорта
	return scene
