# GameWinterWol

## 1. **Phân tích và xác định lại luồng gameplay**

- Đọc và hiểu yêu cầu gameplay mới:  
  + Di chuyển item từ bảng xuống thanh bar (bottom bar).
  + Khi bar đầy hoặc clear hết bảng thì thắng/thua.
  + Clear nhóm 3 item giống nhau trên bar.
  + Không thể trả item về bảng (trừ mode đặc biệt).

---

## 2. **Thiết kế lại dữ liệu và kiến trúc code**

- Xây dựng lại hệ thống quản lý `Board`, `Cell`, `Item` để hỗ trợ thao tác mới.
- Thiết kế thêm hoặc sửa class `BottomBarManager` để chứa các slot item dưới bar.
- Thêm list lưu trữ type, sprite, và vị trí gốc của từng item để đồng bộ khi trả về bảng.

---

## 3. **Xây dựng chức năng chuyển item từ bảng xuống bar**

- Thêm sự kiện click vào từng ô trên bảng để lấy item.
- Khi click, kiểm tra bar còn slot trống, gọi hàm chuyển item xuống bar.
- Thực hiện hiệu ứng bay item từ bảng xuống bar bằng DOTween.
- Sau khi chuyển, xóa item khỏi bảng, cập nhật lại UI.

---

## 4. **Xử lý logic bar**

- Khi add item vào bar, lưu lại type, sprite, và cell gốc.
- Kiểm tra điều kiện clear triplet: nếu có 3 item giống nhau, thực hiện hiệu ứng scale về 0 rồi xóa khỏi bar.
- Sau khi clear, cập nhật lại UI, kiểm tra điều kiện thắng.

---

## 5. **Xử lý điều kiện thắng/thua**

- Kiểm tra sau mỗi lần chuyển/xóa item:
  + Thắng: nếu bảng không còn item.
  + Thua: nếu bar đầy mà chưa clear được triplet.
- Hiển thị màn hình thắng/thua đơn giản bằng panel UI.

---

## Task 1: Re-skin

**Yêu cầu:** Đổi skin tất cả các item thành cá (Fish).

**Các bước đã thực hiện:**
1. Tìm và tổng hợp toàn bộ asset hình cá trong dự án.
2. Thay thế các sprite item cũ (thực phẩm, đồ vật...) bằng các sprite hình cá trong code khởi tạo bảng và trong resource.
3. Kiểm tra, test lại toàn bộ UI để đảm bảo các item đều hiển thị đúng hình cá trên board và bar.
4. Sửa code sinh item mới để luôn lấy đúng sprite hình cá tương ứng với type.

---

## Task 2: Change the Gameplay

**Yêu cầu:** Đổi gameplay sang dạng mới (từ board sang bottom bar).

**Các bước đã thực hiện:**
1. Sửa logic click: Khi người chơi bấm vào item trên bảng, item đó sẽ di chuyển xuống thanh bar phía dưới (bottom bar).
2. Đảm bảo khi item đã xuống bar, không thể di chuyển ngược về lại bảng (khóa logic trả về).
3. Khi có đúng 3 item giống nhau trên bar, sẽ tự động clear cả 3 item đó và cập nhật UI.
4. Thắng khi clear hết item trên bảng.
5. Thua khi bar đầy (không còn slot trống).

---

## Task 2: Requirements

**Yêu cầu chi tiết:**
1. Số lượng item giống nhau trên bảng phải chia hết cho 3.
2. Thanh bar phía dưới chứa đúng 5 cell.
3. Thêm màn hình thắng đơn giản khi người chơi thắng.
4. Thêm màn hình thua đơn giản khi người chơi thua.
5. Tạo màn hình Home với nút 'Autoplay', khi bấm tự động chơi đến khi thắng, mỗi hành động delay 0.5s.
6. Thêm nút 'Auto Lose', khi bấm sẽ tự động chơi với mục tiêu thua, mỗi hành động delay 0.5s.

**Các bước đã thực hiện:**
- Sửa hệ thống sinh fish để đảm bảo số lượng fish luôn chia hết cho 3.
- Cố định số lượng slot bar là 5 và chỉnh lại UI bar cho phù hợp.
- Tạo các panel/thông báo Win & Lose đơn giản, show khi game kết thúc.
- Thiết kế màn hình Home, thêm các nút chức năng Autoplay và Auto Lose, cài đặt logic tự động chơi, delay 0.5s mỗi bước.
- Kiểm thử lại các kịch bản thắng/thua, tự động chơi, tự động thua.

---

## Task 3: Improve the gameplay

**Yêu cầu:** Hoàn thiện và mở rộng gameplay

**Các bước đã thực hiện:**
1. Đảm bảo khi khởi tạo bảng luôn có đủ các loại cá khác nhau (đầy đủ type).
2. Thêm hiệu ứng animation bay từ bảng xuống bar khi chuyển item, hiệu ứng scale to 0 khi clear 3 item giống nhau.
3. Thêm chế độ Time Attack:
    - Thêm nút Time Attack lên màn hình Home.
    - Khi vào mode này, người chơi không bị thua khi bar đầy, chỉ thua khi hết giờ 1 phút.
    - Cho phép trả lại item từ bar về đúng vị trí cũ trên bảng bằng cách bấm vào slot bar (sửa lại logic BottomBarManager).
    - Xử lý thua khi hết giờ mà chưa clear hết bảng.
4. Kiểm thử lại toàn bộ các chức năng và hiệu ứng mới của game.

- **Thêm chế độ Time Attack:**  
  - Thêm nút "Time Attack" trên giao diện chính.
  - Bổ sung đồng hồ đếm ngược thời gian.
  - Xử lý win/lose khi hết thời gian hoặc clear hết bảng.
  - Không bị thua khi bar đầy, chỉ thua khi hết giờ.

- **Cho phép trả lại item từ bar về đúng ô trên bảng:**  
  - Mở rộng BottomBarManager:  
    - Thêm list lưu trữ Cell gốc của từng item.
    - Khi bấm slot bar, trả lại item về đúng ô nếu ô trống.
    - Đồng bộ cập nhật UI, xóa item khỏi bar khi trả về thành công.

