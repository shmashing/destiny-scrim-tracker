# Destiny Scrims Tracker #
Version 1.0.0

Does you and your clan run a lot of private match scrimmages? Do you have a hard time trying to match up teams to be as fair as possible and still be a good fight?! Well then this is the tool for you! Destiny Scrims Tracker is a web interface to manage all of your internal clan scrimmaging and ranking! With this, all you need to do is input the results of a game and it will calculate a ELO system specific to your clan!

### Models

- [Guardian Object](Destiny.ScrimsTracker.Logic/Models/Guardian.cs)
- [GuardianMatchResult Object](Destiny.ScrimsTracker.Logic/Models/GuardianMatchResult.cs)
- [Match Object](Destiny.ScrimsTracker.Logic/Models/Match.cs)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project to a live system.

### Prerequsities

The projects contained in this repository are dependent on the following .NET versions, which can be downloaded at [here](https://dotnet.microsoft.com/download/dotnet-core):

- .NET Core SDK 3.0
- .NET Standard 2.1

#### Quickstart Examples

The following snippets are provided for illustrative purposes only and the tests should be referred to as the source of "implementation truth".

##### Starting the Service Locally

To build the service locally, from the root of the repository, run `dotnet build`.

To run the service locally, from the root of the repository, run 'dotnet run --project Destiny.ScrimsTracker.Api/Destiny.ScrimsTracker.Api.csproj'

##### Starting the Service Locally in Docker 

To build the docker image locally, make sure you have docker installed and run `docker build . -t destiny-scrims-tracker:latest`

Then, to run the image, run `docker run --publish 5500:80 --name destiny-scrims-tracker --rm destiny-scrims-tracker:latest`

## Running the tests

To run the tests locally, from the root of the repository, run `dotnet test`.

## Database Migration

This repository **will** include a DbUp console app for executing database migrations. Simply add a script to `Destiny.ScrimsTracker.Logic/Migrations/` and set the file as an embedded resource via the .csproj file.

To run the migration, execute the console app with the following command:
`dotnet run -s <server endpoint> -d <database name> -p <port> -u <user id> -w <password>`


## Deployment

### Deploying Through Heroku

<Once it's figured out it will be described here>

## Contributing

Feature branches should have pull requests opened between themselves and the `master` branch when ready for submission, with relevant stakeholders added as reviewers. PRs may be merged to `master` upon a single approval. As a general rule of thumb, it is generally prudent to wait for successful builds from the PR before merging it.


