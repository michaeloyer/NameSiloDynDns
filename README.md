# NameSilo DynDNS
Background Service To update a NameSilo DNS entry with the running computer's public IP Address.

## Purpose
This is meant to be run continuously on a computer to update a hostname (A Record) on [NameSilo](https://namesilo.com). The computer will send a request to [NameSilo's API](https://www.namesilo.com/api_reference.php). The request will have all of the hostnames for the domain specified in the appsettings.json config file as well as the public IP address it recieved the request from. It uses these two pieces of information to then make an update on NameSilo using the API if necessary.

## Config
The [config file](NameSiloDynDns/appsettings.json) controls how this program will behave.
* Host: **host**.domain.com
* Domain: host.**domain.com**
* APIKey: [Generate here](https://www.namesilo.com/account/api-manager)

## Install
This is a DotNet Core app that can be run anywhere. See your OS guides for setting up as service. You can build and publish this by using `dotnet publish --self-contained --runtime win-x64` to create an executable in Windows. To run the Executable in Windows you can use something like [NSSM](https://nssm.cc/) or use the Windows Task Scheduler

## Logging
This app uses [Serilog](https://serilog.net/) for logging. Logging is setup in the [appsettings.json](NameSiloDynDns/appsettings.json) file in the *Serilog* section. The following sinks are currently supported:
* Console
* File
* Email
* Http
