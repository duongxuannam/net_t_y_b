# Net T Y B

This project keeps the frontend in React while replacing the backend with an ASP.NET Core Web API that mirrors the original Rust feature set (auth, todos, docs, health, and metrics).

## Structure

- `backend/` - ASP.NET Core Web API (`net8.0`)
- `frontend/` - React app powered by Vite

## API surface

All routes are under `/api`.

**Auth**
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`
- `POST /auth/forgot`
- `POST /auth/reset`

**Todos** (JWT required)
- `GET /todos`
- `POST /todos`
- `GET /todos/{id}`
- `PUT /todos/{id}`
- `DELETE /todos/{id}`

**Ops**
- `GET /health`
- `GET /metrics`
- `GET /docs` (Swagger UI)

## Configuration

Required:
- `JWT_SECRET` (min 32 chars)

Optional (defaults in parentheses):
- `ACCESS_TOKEN_TTL_MIN` (15)
- `REFRESH_TOKEN_TTL_DAYS` (7)
- `REFRESH_COOKIE_NAME` ("todo_refresh")
- `REFRESH_COOKIE_SECURE` (true in production, else false)
- `ALLOWED_ORIGINS` ("http://localhost:3000,http://localhost:5173")
- `RATE_LIMIT_PER_SECOND` (5)
- `RATE_LIMIT_BURST` (10)
- `PASSWORD_RESET_URL_BASE` ("http://localhost:5173/reset")
- `PASSWORD_RESET_TTL_MIN` (30)
- `SMTP_HOST` (required when wiring email delivery)
- `SMTP_USERNAME`
- `SMTP_PASSWORD`
- `SMTP_FROM`
- `SMTP_PORT` (587)
- `SMTP_FROM_NAME` ("Todo App")

## Run locally

### Backend

```bash
cd backend
dotnet restore
dotnet run
```

The API is available at `http://localhost:5000/api/health` and Swagger at `http://localhost:5000/api/docs`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Then open `http://localhost:5173`.
