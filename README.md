# Monster Attack

## Giới thiệu

Monster Attack là game hành động đi cảnh 2D (2D action-platformer) phát triển bằng Unity. Người chơi điều khiển nhân vật vượt màn, chiến đấu với quái vật bằng các đòn đánh và combo, đồng thời mở khóa nhân vật, nâng cấp chỉ số và chinh phục các màn chơi ở những mức độ khó khác nhau.

## Công nghệ

- Unity `6000.0.58f2` (Unity 6)
- C#
- Universal Render Pipeline (URP), cấu hình 2D

## Chạy dự án

1. Mở dự án trong Unity Hub bằng phiên bản Unity nêu trên.
2. Mở scene `Assets/Scenes/StartScene.unity`.
3. Nhấn **Play** trong Unity Editor.

Các scene đã được đăng ký trong Build Settings theo thứ tự: `StartScene`, `SelectCharacter`, `SelectMap`, `LV1`.

## Điều khiển

| Phím | Hành động |
| --- | --- |
| `A` / `D` | Di chuyển sang trái / phải |
| `Space` | Nhảy |
| `J` | Tấn công và nối combo |

Một số nhân vật có hành động riêng, ví dụ Huntress ném lao thông qua sự kiện animation.

## Cấu trúc và tệp quan trọng

### Scene

- `Assets/Scenes/StartScene.unity` — màn hình bắt đầu.
- `Assets/Scenes/SelectCharacter.unity` — chọn nhân vật.
- `Assets/Scenes/SelectMap.unity` — chọn màn chơi và độ khó.
- `Assets/Scenes/LV1.unity` — scene chiến đấu hiện được cấu hình trong Build Settings.

### Script gameplay (`Assets/Script/`)

- `GameSessionManager.cs` — quản lý nhân vật được chọn, danh sách nhân vật, mở khóa và lưu tiến trình nhân vật.
- `PlayerController.cs`, `PlayerMovement.cs`, `PlayerCombat.cs`, `PlayerHealth.cs` — điều khiển, di chuyển, chiến đấu và máu của người chơi.
- `EnemyProperties.cs`, `FlyingEnemyProperties.cs`, `Health.cs`, `WeaponHitbox.cs` — trạng thái và tương tác chiến đấu của quái vật/vũ khí.
- `LevelMapManager/LevelSessionManager.cs` — trạng thái màn chơi, độ khó, sao và mở khóa màn tiếp theo.
- `Character/CharacterSaveManager.cs`, `LevelMapManager/LevelSaveManager.cs` — lưu và tải tiến trình nhân vật/màn chơi.
- `SO/CombatCalculator.cs`, `SO/PlayerStatsSO.cs`, `SO/EnemyStatsSO.cs` — dữ liệu và tính toán chỉ số chiến đấu.

### Dữ liệu và tài nguyên

- `Assets/DataCharacterSO/` — dữ liệu nhân vật dạng ScriptableObject.
- `Assets/LevelDataSO/` — dữ liệu màn chơi dạng ScriptableObject.
- `Assets/PlayerSO/`, `Assets/EnemySO/` — dữ liệu chỉ số người chơi và quái vật.
- `Assets/Animation/`, `Assets/Prefab/`, `Assets/Tilemap/`, `Assets/Resource/` — animation, prefab, tilemap và tài nguyên game.

### Cấu hình dự án

- `Packages/manifest.json` — danh sách package Unity.
- `ProjectSettings/ProjectVersion.txt` — phiên bản Unity của dự án.
- `ProjectSettings/EditorBuildSettings.asset` — danh sách scene dùng khi chạy/build.

File lưu tiến trình được tạo trong `Application.persistentDataPath` của Unity, không nằm trong thư mục mã nguồn.