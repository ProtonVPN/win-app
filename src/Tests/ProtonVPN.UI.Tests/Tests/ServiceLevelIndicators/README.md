# Windows VPN Client Service Indicator (SLI) Tracking

### Description

This project was implemented to track our client's performance and stability, inspired by service reliability tracking adapted for client use. It is being executed on testing devices.

### Detailed documentation
In  confluence search for ```Clients SLIs tracking technical implementation```.

### Required environment varriables
```LOKI_CERTIFICATE_WINDOWS``` - How to get this value is described in confluence.

```LOKI_PRIVATE_KEY_WINDOWS``` - How to get this value is described in confluence.

```All environment varriables required for E2E tests```

### Components

- **Nexus**: Used to upload builds for SLI measurements.
- **E2E Tests**: Reused from our E2E test suite to measure specific actions.
- **Loki**: Stores metrics and logs.
- **Grafana**: Queries metrics and displays them in dashboards.

## Usage Examples

Below is an example showing how to measure login speed SLI.

1. **Write the Test:**

   ```csharp
   [Test]
   public void LoginPerformance()
   {
       _loginRobot
           .Wait(TestConstants.StartupDelay)
           .DoLogin(TestUserData.PlusUser);
       _homeRobot.DoCloseWelcomeOverlay();
   }
   ```
2. **Add SLI name and Workflow name using attributes.** 

    SLI name selects the desired metric in a query, and the workflow name helps combine logs with the metric result.

    ```csharp
    [Test]
    [Sli("login"), Workflow("login_performance")] <-- Define
    public void LoginPerformance()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);
         _homeRobot.VerifyIfLoggedIn();
    }
   ```
   If the workflow name is the same for all tests in the class, add it above the class:
    ```csharp
    [Workflow("main_measurements")] //<-- Whole test class will use this name.
    public class MainMeasurementsSLI : TestSession
    {
        // SLI measurements goes here
    }
    ```
    
3. **Select what you want to measure using attribute.** 

    Currently supported metrics are:
    - **Duration** - Used to track how long action takes.
    - **TestStatus** - Used to track if measurement passed.

    ```csharp
    [Test]
    [Duration, TestStatus] // <-- Tell test what it should measure
    [Sli("login"), Workflow("login_performance")]
    public void LoginPerformance()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);
        _homeRobot.VerifyIfLoggedIn();
    }
   ```
4. **Select which part you want to measure and where do you want to push metrics.**

    ```csharp
    [Test]
    [Duration, TestStatus]
    [Sli("login"), Workflow("login_performance")]
    public void LoginPerformance()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        // Move verification function inside measure block
        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyIfLoggedIn();
        });
    }
    
    [TearDown]
    public void TestCleanup()
    {
        new LokiPusher().PushMetrics(); // <-- Push metrics after each test
        SliHelper.Reset();
    }
   ```