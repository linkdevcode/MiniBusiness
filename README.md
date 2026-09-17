# MiniBusiness

Hệ thống bán hàng mini — project ôn luyện .NET + Clean Architecture.

## Tài liệu

Toàn bộ vision, roadmap, TODO và mentor guide nằm trong **[`docs/`](./docs/README.md)**.

| Bắt đầu | Link |
|---|---|
| Ý tưởng project | [`docs/vision/Idea.md`](./docs/vision/Idea.md) |
| Lộ trình dài hạn | [`docs/roadmap/ROADMAP.md`](./docs/roadmap/ROADMAP.md) |
| Task hôm nay | [`docs/TODO.md`](./docs/TODO.md) |
| Mentor Level 0 | [`docs/mentor/level-0/`](./docs/mentor/level-0/README.md) |

## Chạy nhanh

```bash
docker compose up -d

dotnet ef database update \
  --project src/MiniBusiness.Infrastructure \
  --startup-project src/MiniBusiness.Api

dotnet run --project src/MiniBusiness.Api
```

Level hiện tại: **0** — hoàn thiện module Customer.
