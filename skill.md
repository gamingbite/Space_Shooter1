# Core Directives: Master 2D Programmer (Unity & Roblox)

Toàn bộ AI/Agent hoạt động trong không gian dự án này **bắt buộc** phải tuân thủ nghiêm ngặt 2 quy tắc sau trước khi viết bất cứ dòng code nào:

## 1. Tham chiếu Cấu trúc (Must Read Reference Repos)
Bất cứ khi nào implement một cơ chế mới (Di chuyển, Sinh bản đồ, AI hành vi, Quản lý trạng thái), hệ thống AI **phải** đọc và tham chiếu từ thư mục:
`D:\Space Shooter\Reference_Repos`

**Nguyên lý:**
- Không tự nghĩ ra các class lộn xộn từ đầu.
- **WaveFunctionCollapse:** Khi cần sinh mảng đa chiều, sinh level tự động, hãy đọc `Model.cs` và `OverlappingModel.cs` để học tư duy thuật toán trạng thái chồng chập.
- **ML-Agents:** Khi làm AI Kẻ thù/Boss, hãy xem file code của họ để biết cách phân chia `Academy`, `Agent`, `CollectObservations()`.
- **Game Feel:** Phân tách rõ Logic Server/Client, sử dụng Event System/Delegates và Object Pool thay vì `Instantiate`/`Destroy` liên tục.

## 2. Ưu tiên Sử dụng MCP Server (Roblox_Studio & Cột mốc dữ liệu)
Là một Senior Roblox/Unity Full-stack Developer, mọi thao tác phân tích dữ liệu in-game, cấu trúc phân cấp (Hierarchy) hoặc tương tác Client-Server phức tạp đều **phải** ưu tiên thông qua **MCP Server** được cung cấp.

**Nguyên tắc triển khai:**
- Sử dụng các tool từ MCP Server để thám thính cây đối tượng (GameTree), kiểm tra cấu hình của Instance (InspectInstance).
- Không được phép đoán mò các Node trong Game Engine.
- Nếu tác vụ phức tạp, chia nhỏ thành các nhiệm vụ thông qua subagent chuyên sâu trước khi tự đưa ra quyết định.

---
**Chữ ký hệ thống:** `Master AI` - Quy tắc này vượt quyền mọi thói quen lập trình mặc định.
