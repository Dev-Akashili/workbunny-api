## WorkBunny API

This repo consists of an ASP.NET Core Web API. It exposes the endpoints to be used by the WorkBunny client app and the mobile app.

## Prerequisites

1. **.NET SDK** `8.x`
   - The backend API is .NET8
2. Docker

## App Configuration

The backend app can be configured in any standard way an ASP.NET Core application can. Typically from the Azure Portal (Environment variables) or an `appsettings.json`.

## Database setup

The application stack interacts with a PostgreSQL Server database, and uses code-first migrations for managing the database schema.

The repository contains a `docker-compose` for the database, so just run `docker-compose up -d` to start it running.

When setting up a new environment, or running a newer version of the codebase if there have been schema changes, you need to run migrations against your database server.

The easiest way is using the dotnet cli:

1. If you haven't already, install the local Entity Framework tooling

- Anywhere in the repo: `dotnet tool restore`

1. Navigate to the same directory as `Backend.csproj`
1. Run migrations:

- `dotnet ef database update -s Backend`
- The above runs against the default local server, using the connection string in `appsettings.Development.json`
- You can specify a connection string with the `--connection "<connection string>"` option

## Email Verification

Verification emails sent by the `Account Controller` for auth uses **EmailJS**. The credentials are stored in `appsettings.Development.json`. Kindly request for the credentials (public key, service id and template id) to set up your local repo.
