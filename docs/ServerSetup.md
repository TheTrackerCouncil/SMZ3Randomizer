# SMZ3 Cas’ Multiplayer Server Setup

The SMZ3 Cas' Multiplayer server is built as a cross platform SignalR .NET application designed to route messages between various players in a Cas' Multiplayer game. In this implementation, all seed and rom generation is handled on the client end and the server simply acts a sort of router for the traffic between the players in a game.

## Prerequisites

- ASP.NET Core 6
- .NET Core 6
- IIS, Nginx, or similar to use as a proxy (recommended)

## appsettings.json

To run the server, you will need to execute to have a configured appsettings.json file in the working directory from where you run the server executable. You can find an example of the appsettings [here](appsettings.example.json).

Below are explanations of all of the settings:

- SMZ3
    - **ServerUrl**: Required. Specifies the base URL that is used for generating game URLs to send to players. Include the protocol and subdomain (e.g. https<nolink>://smz3.example.com)
    - **GameMemoryExpirationInMinutes**: Optional. Defaults to 60 minutes. Specifies how long in minutes before a game is removed from memory if no updates have been sent by any players. If a server does not have a database setup or if a game is not setup to be async or multi-session, then the game will be inaccessible after that point in time.
    - **GameCheckFrequencyInMinutes**: Optional. Defaults to 15 minutes. Specifies how frequently the server will check for expired games that need to be removed from memory or the database. Based on start time of the application.
    - **GameDatabaseExpirationInDays**: Optional. Defaults to 30 days. Specifies how long in days before a game is removed from the database if no updates have been sent by any players. Games saved to the database will be inaccessible after that point in time.
    - **SQLiteFilePath**: Optional. Used to specify the path to the sqlite database file that is used for storing games. Can be either a relative or absolute path. If no database is specified, then the server will not support saving games to the database.

## Ports

By default .NET apps are only accessible via http<nolink>://localhost:5000 and https<nolink>://localhost:5001 and cannot be accessed by other devices. It is recommended for you to use IIS, Nginx, Apache, or something similar to act as a proxy to those paths as you can setup security certicates and things like that.

### Changing port numbers

To change to another port, simply add the following section to your appsettings.json file, changing the port as desired.

```
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:4567"
      },
      "Https": {
        "Url": "https://localhost:4568"
      }
    }
  },
```

### HTTPS Only

To set the application to only run on HTTPS, add the following section to your appsettings.json file.

```
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  },
```

### Direct traffic

If you want people to be able to directly access the server without any sort of proxy in between, you can set the  domain to 0.0.0.0 instead of localhost. Note that if you are running this on a home computer, you may not to setup port forwarding.

```
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001"
      }
    }
  },
```

### Running

To run the server you will need to run one of the following commands, depending on your environment:

```
Randomizer.Multiplayer.Server.exe
```

or

```
dotnet Randomizer.Multiplayer.Server.dll
```
