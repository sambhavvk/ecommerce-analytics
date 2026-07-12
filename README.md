# Real‑Time E‑Commerce Analytics Platform

A polyglot microservices platform that tracks user behaviour, processes orders, delivers real‑time dashboards, and demonstrates integration between **.NET** and **Ruby**.

## Architecture


| Component            | Technology                       | Purpose                                   |
|----------------------|----------------------------------|-------------------------------------------|
| API Gateway          | ASP.NET Core + Ocelot            | Routing, authentication, rate limiting    |
| Order Service        | .NET gRPC + EF Core + PostgreSQL | Core order placement and persistence      |
| Analytics Service    | .NET SignalR + Redis + RabbitMQ  | Real‑time dashboards via WebSockets       |
| Background Workers   | Ruby + Sidekiq + Bunny           | Order confirmation emails, async tasks    |
| ETL Pipeline         | Ruby + Kiba + ClickHouse         | Move order data into analytics store      |
| Admin Dashboard      | Ruby + Sinatra + Hotwire         | Internal CRUD interface with live updates |

## Tech Stack

### Languages & Frameworks
- **.NET 10** – ASP.NET Core, gRPC, SignalR, Entity Framework Core
- **Ruby 3.4** – Sinatra, Sidekiq, Kiba

### Infrastructure
- PostgreSQL – transactional data
- RabbitMQ – event bus
- Redis – caching & real‑time counters
- ClickHouse – analytics column store

### DevOps
- Docker & Docker Compose
- GitHub Actions (CI/CD)

## Getting Started

### Prerequisites
- .NET 10 SDK
- Ruby 3.4+ with Bundler
- Docker Desktop
- Git

### 1. Clone the repository
```bash
git clone https://github.com/sambhavvk/ecommerce-analytics
cd ecommerce-analytics
```

### 2. Start infrastructure
```bash
docker compose up -d
```
This will run PostgreSQL, RabbitMQ, Redis, and ClickHouse.

### 3. Restore & run .NET services
```bash
# Order Service
cd src/OrderService
dotnet restore
dotnet ef database update
dotnet run

# API Gateway
cd ../ApiGateway
dotnet restore
dotnet run

# Analytics Service
cd ../AnalyticsService
dotnet restore
dotnet run
```

### 4. Set up Ruby workers
```bash
cd src/ruby-workers
bundle install
redis-server &          # if not using Docker Redis
sidekiq -r ./workers/order_confirmation_worker.rb &
ruby start_consumer.rb
```

### 5. Run the Admin Dashboard (optional)
```bash
cd src/ruby-admin
bundle install
ruby app.rb
```
Visit `http://localhost:4567`

## Project Structure

```
ecommerce-analytics/
├── docker-compose.yml
├── src/
│   ├── ApiGateway/            # .NET
│   ├── OrderService/          # .NET
│   ├── AnalyticsService/      # .NET
│   ├── ruby-workers/          # Ruby
│   ├── ruby-pipeline/         # Ruby
│   ├── ruby-admin/            # Ruby
│   └── Shared/                # Proto files, contracts
└── docs/
```

## Usage

### Placing an Order (gRPC)
```bash
grpcurl -plaintext -d '{"customer_id":"123","items":[{"product_id":"P100","quantity":2}]}' \
  localhost:5001 OrderService.PlaceOrder
```

### Viewing Real‑Time Analytics
Open the SPA (or any WebSocket client) and connect to `ws://localhost:5002/analytics-hub`.  
You will receive live `PageView` and `PageViewUpdate` messages as events flow through the system.

### Admin Dashboard
Navigate to `http://localhost:4567` to see recent orders. Use the “Ship” button to update statuses via Hotwire Turbo Streams.

## Testing

### .NET
```bash
dotnet test src/OrderService.Tests
```

### Ruby
```bash
cd src/ruby-workers
bundle exec rspec
```

## Deployment

A full production‑ready deployment can be done with the included `docker-compose.yml`.  
For cloud deployment, each service can be containerised individually and orchestrated with Kubernetes or AWS ECS.

---

**Happy coding!**