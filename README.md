# Net T Y B

This project keeps the frontend in React while replacing the backend with an ASP.NET Core Web API.

## Structure

- `backend/` - ASP.NET Core Web API (`net8.0`)
- `frontend/` - React app powered by Vite

## Run locally

### Backend

```bash
cd backend
dotnet restore
dotnet run
```

The API is available at `http://localhost:5000/api/status`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Then open `http://localhost:5173`.
