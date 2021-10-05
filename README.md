# Trade Listing ASP.NET C# API + scheduler

## Setup

### Database

This API uses EF Core. I used the MySQL relational database however any relational database should work given that the explicit database types are the same. For best results use MySQL.

Steps
1. Set your connection string for your local database in `appsettings.json`

    _NB: a database named `defaulttest` will be created during migration_

2. Run the migration from the NuGet Console or via `dotnet`.

    > NuGet: `update-database`

    > Command line: `dotnet ef database update`

    Given that the build succeeds, we should now have a `defaulttest` database/schema.

3. We should now be able to run the solution with one of the following:
    - Run solution Visual Studio (open solution)
    - on the command line in the project folder: `dotnet run`

We should see a `Symbol tables successfully populated` message in the output.

Setup complete

________________

## Architecture

### Database design

### GraphQL

The application API utilizes GraphQL (HotChocolate.net) which you can try out at https://localhost:5001/graphql/ (Banana Cake Pop), which is a pretty neat user interface and great for testing. It also shows you the graph schema.

### REST API

There is a REST API which I was using for testing which is in the main controller, however it is not used by the front-end application however I thought I'd include it anyway.

### Job Scheduler

Here I used Quartz.net to schedule asynchronous tasks to fetch prices from the external API (Binance).

It works by sending batches of asychronous requests to the API and tracks its last update time and current batch. The batches of requests are sent one after the other without waiting for the results of each to return and then waits for all the responses before considering that batch done.

The scheduler starts one minute after the application has started. You can observe the request flow in the output.

### Persistent static caching

When a request is made to this API for prices, it will first check if it already has the records in the database; if not it will fetch them from the external API, add the missing records to the database, and return the results. 
