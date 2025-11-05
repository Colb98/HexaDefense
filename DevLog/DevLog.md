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

## Day 9 
- Unit có thể target vào trụ (giới hạn tuỳ vào sức "khiêu khích" của trụ);
- BUG: Unit gần tới đích thì quay lại đánh trụ, unit gần thì không đánh

# Day 10
- Dùng BFS tạo 1 map "tạm" để biết được khoảng cách tới đích: nếu lính gần đích hơn trụ thì lính sẽ đi tiếp thay vì quay đầu tìm đường đánh trụ
- Các vấn đề gặp phải trong suốt quá trình:
 - Nhiều case do không muốn tìm đường mỗi ô lính đi (TFT làm như này thì phải) vì có nhiều đơn vị, map to. Các case gồm: đơn vị chết (tìm con khác bù vào để tấn công), trụ chết (các con đang đi tới phải tìm lại), đặt trụ (tìm đường cho tất cả lính - chọn con "phù hợp" nhất). Phù hợp là gần nhưng không nên quay đầu.
 - Các bug xảy ra khi pooling: lính chết mang theo target cũ, trụ chết mang theo các loại lính cũ => Cần reset đúng thời điểm. Vì lưu cả mục tiêu và cả các lính chọn mình làm mục tiêu (2 chiều).
 - Một số bug kì lạ: Đạn bắn không di chuyển (AreaProjectile)

# Day 11
- Refactoring: Pausable system (dừng objects) - Manager để quản lý ticks
- Fix bug Unit tự dưng xuất hiện giữa không khí? Do không reset cẩn thận: reset path, vị trí, progress...
[Bug unit xuất hiện giữa đường](./bug_reset_units.mp4)
- Registry - Centralize các managers và không cần ref tùm lum nữa (TODO) - Cân nhắc có thể không làm, cái này để thay đổi manager tuỳ ý -> dễ test hơn

# Day 12
- Đã pause được, button Pause
- Game Manager -> Gold, Wave & HP

# Day 13
- Fix lỗi Tick vẫn được gọi khi trụ chết dẫn tới vẫn bắn đạn khi đã bị phá huỷ
- Thanh máu cho trụ/enemy
- Register và Unregister Entity khi lấy ra khỏi pool và khi trả về pool
- Bug: Đang duyệt qua các tickables, rồi khi tick thì nó kill 1 đơn vị làm xoá mất đơn vị đó khi đang duyệt mảng -> duyệt ra ngoài mảng tickables. Fix bằng cách không xoá ngay mà chỉ cho vào mảng tạm. Khi hết tick mới xoá khỏi mảng tickables

# Day 14
- Fix Error: HealthBar có 1 coroutine để tự ẩn sau 5s, khi unit hết máu, nó ẩn đi, nên gọi hàm StartCoroutine trả về Error, khiến crash hết 1 đoạn phía sau lời gọi đó.
- Menu upgrade and remove towers, HUD chỉ hiện khi click lên trụ. HUD Bugs: Click và hiện UI trong frame, nên khi kết thúc frame, cái nút thấy có event nên nó tự bấm luôn -> solution: hiện UI delay sau 1 frame.
- Fix bug duyệt tickables 2 lần. Do khi pool của UnitManager hết phần tử, nó tạo ra 1 Unit mới và cũng cho vào pool, khiến unit đó bị lấy ra 2 lần, đăng ký với Manager 2 lần và update 2 lần trong 1 frame luôn. Good part: vì dùng TickManager nên nhìn tốc độ di chuyển sẽ biết được lỗi này, nếu không thì cũng khó mà nhận ra.
- Blocker: Nâng cấp hàm của blocker. Blocker giờ sẽ bắn ra cho IBlockerListener biết khi nào nó được bấm, dùng để detect khi nào bấm ra bên ngoài 1 popup, tooltip... để ẩn chúng đi nếu cần.

# Day 15
- Thử đổi kích thước map từ 100 x 75 -> 36 x 21 thì nổ khá nhiều. Lý do là do code (chủ yếu là các đoạn gen AI): không chú ý tới scale của map, map scale lại rồi vị trí khi set các unit/trụ/đạn bị lệch hết (do tính theo công thức rồi đặt position là world pos), hàm static tính vị trí của Tile bị hard code theo scale cũ (thật ra cũng chưa nghĩ ra cách để nó đọc được state của value gán trong Map), các sprite thể hiện tầm bắn, tầm ảnh hưởng cần phải chỉnh trong editor lại để scale đúng tỉ lệ.
- Hàm upgrade và remove trụ. Đọc config level sau và init lại chỉ số. Cộng tiền khi sell và check điều kiện nếu upgrade thì UI sẽ có nút upgrade.

# Day 16
- Điều kiện kết thúc màn.

# Day 17 
- Menu chỉ số (hiện chỉ số, modifier và tên entity). Đã done cho trụ, có thể mở rộng cho xem quái và boss

# Day 18
- Menu cho Quái (Touch vào quái đã được), icon cho từng line menu
- Dùng gold

# Day 19
- Fix bug Buff vẫn gây hiệu ứng lên Entity kể cả khi đã chết (đã remove buff). Do Buff khi chưa được add đã thay đổi Stat của entity

# Day 20
- Hệ thốngn đọc ghi và vẽ map bằng tay (design mode). 
- Map size 36x21 -> 15x10

# Day 21
- Resize lại kích thước tile để map vừa vặn hơn
- Fix lại các bug về kích thước trụ, tầm bắn, tầm nổ sau khi đổi size map
- Cơ chế target của lính vào trụ - tấn công trụ chặn đường, chọn lính gần nhất để target trụ (khi chưa bão hoà)
- Trụ giờ chỉ chiếm 1 tile (thay vì 7 tile như trước)
- Tool Design map có vẻ ổn

# Day 22
- Kích thước lính -> nhỏ hơn, cho "lúc nhúc" hơn bằng cách random + physics push nhau
## Day X (TODO):
- Size trụ về 1
- Chuyển sang flow field (có vẻ hợp lý hơn vì địa hình ít thay đổi?) - Consider vì đang định làm trụ chặn đường, trụ có thể thay đổi khá nhiều?
- Kiểm tra ô đặt trụ có hợp lệ không (nếu chặn hết đường đi từ Start tới Goal thì không) - Low Priority
- Cost của ô trụ nên là 10 hay gì đó, nếu đường đi có đi qua ô trụ thì sẽ đánh trụ bất chấp có bị thu hút hay không? - Low Priority