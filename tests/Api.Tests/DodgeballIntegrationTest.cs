using dotenv.net;
using Dodgeball.TrustServer.Api;

namespace Dodgeball.TrustServer.Api.Tests;

public class DodgeballIntegrationTests{
  
  private IDictionary<string, string> Vars;
    
  [SetUp]
  public void Setup()
  {        
    var root = Directory.GetCurrentDirectory();
    var dotenv = Path.Combine(root, ".env");
    var dotEnvOptions = new DotEnvOptions();
    DotEnv.Load();
    this.Vars = DotEnv.Read();
  }

  [Test]
  public async ValueTask TestCheckpoint()
  {
    // Remove the baseURL setting to test against prod
    var baseUrl = this.Vars["BASE_URL"];
    var privateKey = this.Vars["PRIVATE_API_KEY"];
    var dodgeball = new Dodgeball(
      privateKey,
      new DodgeballConfig(){ApiUrl = baseUrl});

    var date = DateTime.Now;
    var dateString = date.Date.ToShortDateString();

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
      },
      session = new
      {
        userAgent = "unknown user header",
        externalId = "UNK  RAW Session"
      },

      // For now we set a hard-coded list of phone numbers, this can
      // be filled in from the client in order to dynamically set
      // MFA phone numbers
      mfa = new
      {
        phoneNumbers = this.Vars["MFA_PHONE_NUMBERS"]
      },
      email = "test@dodgeballhq.com",
      // Gr4vy Testing
      gr4vy = new
      {
        buyerId = "d48f2a52-2cdf-4708-99c5-5bb8717ab11d",
        paymentMethodId = "3ab6199a-c689-4eae-a43c-fa728857f1f1",
        transactionId = "d2ed8384-1f35-4fa2-a950-617e55f9f711",
      },

      merchantRisk = new
      {
        application = new
        {
          id = "ABC123456789XYZ",
          time = "2020-12-31 13:45",
        },
        ipAddress = "65.199.91.101",
        business = new
        {
          name = "Yellowstone Pioneer Lodge",
          address = new
          {
            line1 = "1515 W Park Street",
            city = "Livingston",
            stateCode = "MT",
            postalCode = "59047",
            countryCode = "US",
          },
          phone = new
          {
            number = "406-222-6110",
            countryCode = "US",
          },
          emailAddress = "jdoe@yahoo.com",
        },
        individual = new
        {
          name = "John Doe",
          address = new
          {
            line1 = "1302 W Geyser St",
            city = "Livingston",
            stateCode = "MT",
            postalCode = "59047",
            countryCode = "US",
          },
          phone = new
          {
            number = "2069735100",
            countryCode = "US",
          },
          emailAddress = "jdoe@yahoo.com",
        },
      },

      //Kount Testing
      kount = new
      {
        isAuthorized = "A",
        currency = "USD",
        email = "ashkan@dodgeballhq.com",
        ipAddress = "127.0.0.1",
        paymentToken = "4111111111111111",
        paymentType = "CARD",
        totalAmount = "90000",
        product = new
        {
          description = "FlightBooking",
          itemSKU = "Online Flight Booking",
          price = 633,
          quantity = 2,
          title = "Flight Trip Booking",
        },
        name = "Ashkan Test",
        billingStreet1 = "West St.",
        billingStreet2 = "Apt 222",
        billingCity = "Bellevue",
        billingState = "WA",
        bankIdentificationNumber = "4111",
        ptokCountry = "US",
        billingEmailAge = 6,
      },

      deduce = new
      {
        // email: "billy.glass08@gmail.com",
        isTest = true
      },
      seon = new
      {
        // email: "example@example.com",
        // ip: "1.1.1.1",
        phone = "17609003548",
        fullName = "Example Name",
        firstName = "Example",
        middleName = "string",
        lastName = "string",
        dateOfBirth = "1990-01-01",
        placeOfBirth = "Budapest",
        photoIdNumber = "56789",
        userId = "123456",
        bin = "555555",
      },
      peopleDataLabs = new
      {
        enrichCompany = new
        {
          name = "Google",
          profile = "https://www.linkedin.com/company/google/",
        },
        enrichPerson = new
        {
          firstName = "Elon",
          lastName = "Musk",
          birthDate = " ",
          company = "Tesla",
          primaryEmail = " ",
          phone = " ",
        }
      }
    };


    var dbResponse = await dodgeball.Checkpoint(
      new DodgeballEvent(
        "WITH_MFA",
        "128.103.69.86",
        checkpointData),
      null,
      dateString,
      "test@dodgeballhq.com"
    );

    Assert.IsTrue(dbResponse.success);

    if (dodgeball.IsAllowed(dbResponse))
    {
      // This is the scenario under which we have completed 
      // But we should be in blocked state with MFA
      Assert.Fail("We should be blocked");
    } else if (dodgeball.IsDenied(dbResponse)) {
      Console.WriteLine("Pass a forbidden response to client");
    } else if (dodgeball.IsRunning(dbResponse)) {
      Console.WriteLine("Pass control back to the JS Client to render MFA");
    }
    else
    {
      Assert.Fail("This is an error state!");
    } 
    
  }
}