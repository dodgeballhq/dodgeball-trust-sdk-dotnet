# Dodgeball Server Trust SDK for ASP.NET

## Table of Contents
- [Purpose](#purpose)
- [Prerequisites](#prerequisites)
- [Related](#related)
- [Installation](#installation)
- [Usage](#usage)

## Purpose
[Dodgeball](https://dodgeballhq.com) enables developers to decouple security logic from their application code. This has several benefits including:
- The ability to toggle and compare security services like fraud engines, MFA, KYC, and bot prevention.
- Faster responses to new attacks. When threats evolve and new vulnerabilities are identified, your application's security logic can be updated without changing a single line of code.
- The ability to put in placeholders for future security improvements while focussing on product development.
- A way to visualize all application security logic in one place.

The Dodgeball Server Trust SDK for .NET makes integration with the Dodgeball API easy and is maintained by the Dodgeball team.

## Prerequisites
You will need to obtain a free Dodgeball account in order to leverage the Dodgeball SDK for .NET.  If you don't already have an account, please sign up at:

​    https://app.dodgeballhq.com/signup

Once enrolled, you can obtain an API key for your application from the [Dodgeball developer center](https://app.dodgeballhq.com/developer).

## Related
Check out the [Dodgeball Trust Client SDK](https://npmjs.com/package/@dodgeball/trust-sdk-client) for how to integrate Dodgeball into your frontend applications.

## Installation
We will be registering the Dodgeball SDK for .NET with nuget.  But until then, please clone the project and include it as source in your projects.  

## Usage

```ts
using Dodgeball.TrustServer.Api;

/*
 * However you obtain it, please register your Private API
 * on SDK instantiation
 */
var privateKey = this.Vars["PRIVATE_API_KEY"];
var dodgeball = new Dodgeball(privateKey);

/*
 * Contact Dodgeball for a full specification of the data input
 * vocabulary
 */
var checkpointData = new
  {
    transaction = new
    {
      amount = 10000 / 100,
      currency = "USD",
    },
    paymentMethod = "paymentMethodId",
    customer = new
    {
      primaryEmail = "simpleTest@dodgeballhq.com",
      dateOfBirth = "1990-01-01",
      primaryPhone = "17609003548",
      firstName = "CannedFirst",
    }
  };

/*
 * Execute a Dodgeball Checkpoint to protect a resource
 */
var dbResponse = await dodgeball.Checkpoint(
  new DodgeballEvent(
    "PAYMENT",
    "128.103.69.86",
    checkpointData),
  null,
  dateString,
  "test@dodgeballhq.com"
);


/*
 * Then follow the cases of IsAllowed, IsDenied, IsRunning to
 * either give access to the resource, deny access, or pass    
 * control back to the front end to validate the user using MFA 
 * or Shared Secrets 
 */
if (dodgeball.IsAllowed(dbResponse))
{
  // This is the scenario under which we have completed 
  // But we should be in blocked state with MFA
  // Perform back end operations to give access to resources
} else if (dodgeball.IsDenied(dbResponse)) {
  // Inform the user that their access has been refused
} else if (dodgeball.IsRunning(dbResponse)) {
  // Pass control back to the JS Client to render MFA
}
```
