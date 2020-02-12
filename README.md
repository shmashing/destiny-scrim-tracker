# Destiny Scrims Tracker #
Version 0.0.1

Does you and your clan run a lot of private match scrimmages? Do you have a hard time trying to match up teams to be as fair as possible and still be a good fight?! Well then this is the tool for you! Destiny Scrims Tracker is a web interface to manage all of your internal clan scrimmaging and ranking! With this, all you need to do is input the results of a game and it will calculate a ELO system specific to your clan!

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project to a live system.

### Prerequsities

The projects contained in this repository are dependent on the following .NET versions, which can be downloaded at [here](https://dotnet.microsoft.com/download/dotnet-core):

- .NET Core SDK 3.0
- .NET Standard 2.1

#### Quickstart Examples

The following snippets are provided for illustrative purposes only and the tests should be referred to as the source of "implementation truth".

#### Getting The Database Up and Running

In order to run the service locally you will need a database to support it. Thankfully, this is fairly simple to get going. You can use the public `PostGres` docker image to spin up a local database.

To get this started, just run `docker run -p 5432:5432 -d --name scrims-db postgres`. 

Next, you will need to update the database so it reflects the latest migrations. To do this, from the root of the repo, run `dotnet ef database update --project Destiny.ScrimTracker.Api/Destiny.ScrimTracker.Api.csproj`

Note: you may need to run `dotnet tool restore` first.

##### Starting the Service Locally

To build the service locally, from the root of the repository, run `dotnet build`.

To run the service locally, from the root of the repository, run `dotnet run --project Destiny.ScrimsTracker.Api/Destiny.ScrimsTracker.Api.csproj`

##### Starting the Service Locally in Docker 

To build the docker image locally, make sure you have docker installed and run `docker build . -t destiny-scrims-tracker:latest`

Then, to run the image, run `docker run --publish 5500:80 --name destiny-scrims-tracker --rm destiny-scrims-tracker:latest`

## Running the tests

To run the tests locally, from the root of the repository, run `dotnet test`.

## Database Migration

If you are making database changes, making and running migrations is super easy.

Once you have made the changes you need, run `dotnet ef migrations add <Summary Of Changes> --project Destiny.ScrimTracker.Api/Destiny.ScrimTracker.Api.csproj`

Once that's completed, run `dotnet ef database update --project Destiny.ScrimTracker.Api/Destiny.ScrimTracker.Api.csproj`

## Deployment

### Deploying Through Heroku

Deploying to Heroku is super easy. We have it hooked into `git` already, so whenever you merge into the master branch, it will kick off a deploy to Heroku.

## Contributing

Feature branches should have pull requests opened between themselves and the `master` branch when ready for submission, with relevant stakeholders added as reviewers. PRs may be merged to `master` upon a single approval. As a general rule of thumb, it is generally prudent to wait for successful builds from the PR before merging it.
