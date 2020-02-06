Code coverage: 98%
of SimpleAuth.Core|Services|Shared the 3 projects brings core value of this solution

#### I know 50% is too little, but the core part, the service one, is tested

# SimpleAuth (Simple authentication & authrozization)
## A lightweight Authorization server based on RBAC

## Motivation and reason behind
Designing an AA (Authentication & Authorization) from scratch is a costly, repetitive and time-consuming task. OAuth2 and OpenID connect seem like a very reasonable solution for centralize AA in our industry. However it's very complex and working with it is really stressful when you are just starting out with new ideas. 
Simple Auth is a simple solution, near zero-configuration, easy idea (RBAC).

### Features
- Authorization APIs. (Core interfaces of SimpleAuth).
- SDKs for zero integration (Currently support .NET Core)
- Built-in external authentication providers (Google, WIP: Facebook, Linkedin, etc)
- Persistence of your choice (Postgres, sql lite, in memory, etc).

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them

```
Give examples
```

### Installing

A step by step series of examples that tell you how to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Currently we use minicover to generate code coverage report.
```
./mini.sh
```

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Dotnet Core 3.0](https://dotnet.microsoft.com/download)
* [.Net Standard Library 2.1](https://dotnet.microsoft.com/download)
* [Minicover](https://github.com/lucaslorentz/minicover) - Used to generate code coverage report

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **HungPV & TuyenNT** - *Initial work* - [SimpleAuth TODO link](https://github.com/SimpleAuth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

