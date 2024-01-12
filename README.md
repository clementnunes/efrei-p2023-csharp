# efrei-p2023-csharp SHARD

## Overview

**efrei-p2023-csharp SHARD** is a project for _.NET and C# Environment Module - M1 @ EFREI Paris_.
Shard is a simulation of a real-time space combat and survival game.
It is represented in the form of an API.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)

## Installation

Clone the Repository:
Open a terminal or command prompt and use the git clone command to clone the repository.

```
git clone https://github.com/clementnunes/efrei-p2023-csharp
```

Navigate to the Project Directory:
Change your current directory to the one where the project has been cloned.

```
cd repository
```

## Usage
After cloning the project :
- Install .NET SDK 5.0
- Install Nuget Packages
- Reload all the project
- Build the project
- Run the application
- Run tests

## Features

This application is written in C# in .NET environment, it is a Web API.

All the routes and features:

- Route: **/users** - _UsersController_
    - **PUT /users/id** - Create a new user
    - **GET /users/id** - Returns details of an existing user
    
- Route: **/users/{userId}** - _UnitsController_
    - **GET /users** - Returns all units of a user.
    - **GET /users/{unitId}** - Return information about one single unit of a user.
    - **GET /users/{unitId}/location** - Returns more detailed information about the location a unit of user currently is about
    - **PUT /users/{unitId}** - Change the status of a unit of a user. Right now, only its position (system and planet) can be changed - which is akin to moving it.
      
- Route: **/systems** - _SystemsController_
    - **GET /systems** - Fetches all systems and their planets
    - **GET /systems/{systemName}** - Fetches a single system and all its planets
    - **GET /systems/{systemName}/planets** - Fetches all planets of a single system
    - **GET /systems/{systemName}/planets/{planetName}** - Fetches a single planet of a system

- Route: **/users/{userId}** - _BuildingsController_
    - **GET /users/{userId}/buildings** - Returns all buildings of a user.
    - **GET /users/{userId}/buildings/{buildingId}** - Return information about one single building of a user.
    - **POST /users/{userId}/buildings** - Creates a building at a location
    - **POST /users/{userId}/buildings/{starportId}/queue** - Add a unit to the build queue of the starport. Currently immediately returns the unit
      
## Contact

**Clement Nunes**\
**clement.nunes@efrei.net**
