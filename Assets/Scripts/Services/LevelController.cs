using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController {
	private GameObject parentObject;
	
	private Vector2 GetCameraRect() {
		Camera mainCamera = Camera.main;
		Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
		Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

		float viewportWidth = topRight.x - bottomLeft.x;
		float viewportHeight = topRight.y - bottomLeft.y;
		return new Vector2(viewportWidth, viewportHeight);
	}
	private Rect GetUserRect(Vector2 CameraSize, Rect GrigRect) {
		var dtX = GrigRect.width - CameraSize.x;
		var dtY = GrigRect.height - CameraSize.y;		
		return new Rect(-dtX * 0.5f, -dtY * 0.5f, dtX, dtY);
	}
	public Rect Create9TileGrid(Sprite sprite) {
		if (sprite == null) {
			return new Rect();
		}

		var tileSize = sprite.bounds.size;

		parentObject = new GameObject("TileGridParent");


		float gridWidth = tileSize.x * 3;
		float gridHeight = tileSize.y * 3;

		float startX = -tileSize.x;
		float startY = -tileSize.y;

		for (int row = 0; row < 3; row++) {
			for (int col = 0; col < 3; col++) {
				Vector3 position = new Vector3(startX + col * tileSize.x, startY + row * tileSize.y, 0);

				GameObject tile = new GameObject($"Tile_{row}_{col}");
				tile.transform.position = position;
				tile.transform.parent = parentObject.transform;


				SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
				renderer.sortingOrder = -10;
				renderer.sprite = sprite;
			}
		}

		var GridRect= new Rect(startX, startY, gridWidth, gridHeight);
		return GetUserRect(GetCameraRect(), GridRect);
	}


	public void ClearTileGrid() {
		if (parentObject != null) {
			GameObject.Destroy(parentObject);
		}
	}
}
