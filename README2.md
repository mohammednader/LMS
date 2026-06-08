# Training Service

Standalone microservice extracted from RCHapi — Training area.

## Structure

```
TrainingService/
├── backend/          ASP.NET Core 8 Web API
│   ├── Controllers/  CourseController, TestController, AuthController
│   ├── Services/     ITrainingService, ITestService, ICertificateService
│   ├── Data/         TrainingDbContext (EF Core, SQL Server)
│   ├── Models/       TrainingCourse, CourseSection, Test, Question, Answer, UserTestResult
│   └── DTOs/         All request/response DTOs
└── frontend/         Vue 3 + TypeScript + Vuetify
    └── src/
        ├── stores/   auth.ts, training.ts (Pinia)
        └── pages/    training/ (CoursesPage, TestPage, ManageCoursePage...)
```

## Quick Start

### Backend
```bash
cd backend
# Set your JWT key in appsettings.json (same as RCHapi)
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
# → http://localhost:5010
# → http://localhost:5010/swagger
```

### Frontend
```bash
cd frontend
npm install
# Edit .env: set VITE_RCHAPI_URL to your RCHapi URL
npm run dev
# → http://localhost:5174
```

## Configuration

### appsettings.json
| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Key` | **Must match RCHapi's JWT key** — users log in via RCHapi and tokens are validated here |
| `Cors:AllowedOrigins` | Frontend URL (default: http://localhost:5174) |

### .env (frontend)
| Key | Description |
|-----|-------------|
| `VITE_API_URL` | Training Service backend URL |
| `VITE_RCHAPI_URL` | RCHapi URL for login (shared JWT) |

## Auth Flow
1. User logs in via RCHapi (`POST /api/Account/login`)
2. Token is stored in localStorage
3. All Training Service API calls use the same token (shared JWT secret)

## Database
- Uses its own `TrainingServiceDB` SQL Server database
- Tables: TrainingCourses, CourseSections, Tests, Questions, Answers, UserTestResults, UserAnswers
- Auto-migrated on startup

## Improvements over original
- Replaced `ApplicationUser`/`UserManager` with JWT claims (no Identity dependency)
- Replaced `IBackendLog` with `ILogger<T>` (standard .NET logging)
- EF Core relations explicitly configured in `OnModelCreating`
- Certificate generation uses iText7 with a clean PDF template
