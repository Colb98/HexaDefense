# Hexa Defense Dev Log

## Day 1

- **Ý tưởng**: Map tower defense với nhiều điểm bắt đầu và nhiều điểm kết thúc. Có thể đặt trụ trên đường đi/tường tuỳ loại trụ. Ô trên map sẽ là ô lục giác. Lưu data vẫn dùng mảng 2 chiều như bình thường nhưng cần linked đặc biệt hơn (1 ô -> 6 ô).
- Làm prefab cho tile.
- Script cho tile
- Script cho board
- Load map theo data tự dựng

Đã hiện được map theo file text có sẵn
![Map Gen tạm](./day1_map_generated.png)

## Day 2 
- Tìm đường đi ngắn nhất có random 
    - Mapping giữa toạ độ ô hex và toạ độ lập phương: 
        - (0, 0) => (0, 0, 0)
        - (1, 0) => (1, 0, 1)
        - (2, 0) => (2, 0, 2)
        - (0, 1) => (1, 1, 0)
        - (1, 1) => (2, 1, 1)
        => (x, y) => (x + y, y, x);
- Debug: Click trái để thêm trụ, click phải để xoá ô (thành GROUND)
## Day 3
- Đặt trụ (real)
- Quái di chuyển
## Day 4
- Trụ bắn quái
- Quái tới đích thì biến mất
## Day 5
- 2 Loại trụ và 2 loại đạn basic
    - Trụ đa mục tiêu (phép)
    - Trụ đơn mục tiêu (vật lý)
- Đạn: 
    - AOE
    - Đuổi
- Pool projectile
## Day 6
- Tower & Enemy Config
- Initialize with stats
- Fix lỗi đạn Fireball bị ẩn do không reset sau khi bỏ vào pool
## Day 7
- Tower selection UI
- Fixed Bug: Touch xuyên UI

## Day 8
- Entity có thể thu hút target
- Cần làm hàm lấy target cho kẻ thù -> tính khoảng cách
https://www.redblobgames.com/grids/hexagons/

Duyệt trên toạ độ cube 
Ta có q + r + s = 0
không xét tới s vì s = -q -r;
1 ô cách ô hiện tại range -> nghĩa là ta di chuyển trên 1 trục (q hoặc r hoặc s) range ô, tổng 2 trục còn lại phải là -range và không có trục nào được cùng chiều (âm hoặc dương) với trục đã chọn.
VD: cách 5 ô, thì khi đi trên trục -q 5 ô, trục r và s (chiều dương) phải đi dy và dz ô sao cho dy + dz = 5. 
Nói cách khác, |q + r| <= range (đúng với tất cả các cặp khác).

Khi đó, muốn duyệt tất cả ô trong tầm range ta sẽ duyệt từ -range -> +range của trục q
Trục r thì sao? 
Từ công thức |q + r| <= range ta sẽ => q + r <= range && q + r >= -range. (Do range >= 0)
=> r <= -q + range && r >= -q - range;
Do đó ta cần duyệt r từ -q - range đến -q + range
## Day X (TODO):
Công cụ soạn map: vẽ và "tô" để tạo ra map theo ý muốn
- Tower upgrade
- Enemy type
- Delete Tower
- Chuyển sang flow field (có vẻ hợp lý hơn vì địa hình ít thay đổi?) - Consider vì đang định làm trụ chặn đường, trụ có thể thay đổi khá nhiều?