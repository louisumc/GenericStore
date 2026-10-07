# GenericStore

API REST multi-tenant de lojas virtuais construída com ASP.NET Core 8, C#, Entity Framework Core e SQL Server.

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server (LocalDB)
- xUnit + FluentAssertions + NSubstitute
- Swagger

## Arquitetura

Clean Architecture com 4 camadas:
Api → Application → Domain
↑
Infrastructure

- **Domain**: entidades, value objects, exceções de domínio
- **Application**: casos de uso (Services), DTOs, interfaces
- **Infrastructure**: DbContext, configurações EF, repositórios
- **Api**: Controllers, Middleware, Program

## Domínio

- **User** — proprietário de lojas
- **Store** — loja de um usuário, identificada por slug
- **Product** — produto de uma loja

Relacionamentos: `User 1:N Store 1:N Product`

## Endpoints

### Users
- `POST /api/users`
- `GET /api/users/{id}`
- `GET /api/users`
- `GET /api/users/{userId}/stores`

### Stores
- `POST /api/stores`
- `GET /api/stores/{id}`
- `GET /api/stores`
- `GET /api/stores/{storeId}/products`

### Products
- `POST /api/products`
- `GET /api/products/{id}`
- `GET /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}` (soft delete)
- `PATCH /api/products/{id}/activate`

## Como executar

### Pré-requisitos
- .NET 8 SDK
- SQL Server LocalDB (vem com Visual Studio) ou SQL Server

### Passos

1. Clone o repositório
2. Ajuste a connection string em `GenericStore.Api/appsettings.json` se necessário
3. No Package Manager Console:
Update-Database -Project GenericStore.Infrastructure -StartupProject GenericStore.Api
4. Rode a API (F5)
5. Swagger em `https://localhost:XXXX/swagger`

## Testes

```powershell
dotnet test
22 testes cobrindo User, Store e Product.

Decisões de arquitetura
Clean Architecture: dependências apontam para dentro. Domain não conhece EF, Application não conhece DbContext.

DDD pragmático: entidades com comportamento (Update, Activate, Deactivate), Value Objects para Email e Slug, validações dentro do domínio.

DTOs com record: imutáveis, semântica de valor.

UseCases: orquestração separada das regras de domínio.

Soft delete em Product: DELETE marca Active = false para preservar histórico.

Middleware de exceção: traduz DomainException, NotFoundException, ConflictException para status HTTP apropriados.
