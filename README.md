## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Employer AAN Web

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-employer-aan-web?branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3244&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-employer-aan-web&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-employer-aan-web)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This web solution is part of Apprentice Ambassador Network (AAN) project. Here the employer users can onboard to become ambassadors, find and sign-up for network events, find and collaborate with other ambassadors.

## How It Works
Users are expected to register themselves in the Employer portal. Once registered they will have access to on-boarding journey for AAN. Users who are already apprentices will also have access to on-board. 
When running this locally, with stub sign-in enabled, the launch url should be `https://localhost:5003/accounts/v6gr7d/`, where `v6gr7d` is a random account id. Once logged in the correct account id will automatically replace this.

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Optionally an Azure Active Directory account with the appropriate roles.

### Dependencies
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/EmployerAan) should be available either running locally or accessible in an Azure tenancy.

### Config
You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-employer-aan-web/SFA.DAS.EmployerAan.Web.json)

In the web project, if not exist already, add `AppSettings.Development.json` file with following content:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "ConfigNames": "SFA.DAS.EmployerAan.Web,SFA.DAS.Employer.GovSignIn,SFA.DAS.Encoding",
  "EnvironmentName": "LOCAL",
  "Version": "1.0",
  "APPINSIGHTS_INSTRUMENTATIONKEY": "",
  "cdn": {
    "url": "https://das-test-frnt-end.azureedge.net"
  },
  "ResourceEnvironmentName": "LOCAL",
  "StubAuth": true
}
```

## Technologies
* .Net 10.0
* NUnit
* Moq
* FluentAssertions
* RestEase