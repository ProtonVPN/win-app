# Performance (SLI) Tests

## 🎯 Purpose

This project is meant to track E2E performance of critical client paths. 

## ⚙️ How does it work?

It combines E2E tests with Loki and Grafana. While the test is running, specific flows are measured and reported to Loki with logs, allowing investigation of each individual event.

## 🏷️ Annotations

Annotations are used to define what type of measurement is being performed. 

**Duration** – Marks the test to save the duration of marked flows using `SliHelper.MeasureTime`.

**TestStatus** – Marks the test to report if it passed/failed. Passed/Failed status will only be reported if the failure happened during the measured flow. Useful when you need to check if the desired flow is stable.

**Sli** – Defines the name of the measurement.

**Workflow** – Defines to which group of measurements it belongs; this helps map logs to the measurement.

## 🛠️ How to Set Up a Performance (SLI) Test?

There are 3 main parts to convert an E2E test into a performance test:

- The test class must extend the `SliSetUp` class, so that results are reported at the end of the test.

- The test class must be annotated with the `Workflow` annotation to ensure the measurement is grouped and can be mapped to logs. The `Sli` annotation must be added to each individual measurement to give it a unique name.

- The flow that is going to be measured must be wrapped with `SliHelper.MeasureTime`. Check the `SliHelper` class for more measurement types.

```csharp
SliHelper.MeasureTime(() =>
{
    HomeRobot.Verify.IsWelcomeModalDisplayed();
});    
```

The full test should look like this:

```csharp
[TestFixture]
[Category("SLI")] // <----- Set category to mark this as an SLI test, so that it's run only in SLI pipelines.
[Workflow("main_measurements")] // <----- Define SLI group
public class LoginSLIs : SliSetUp // <----- Extend test class with SliSetUp so that events are sent after each test
{

    [Test]
    [Duration, TestStatus]
    [Sli("login")] // <----- Define the name of the measurement
    public void LoginPerformance()
    {
        LoginRobot
            .Login(TestUserData.PlusUser);

        SliHelper.MeasureTime(() => // <----- Mark the flow to be measured during the test
        {
            HomeRobot.Verify.IsWelcomeModalDisplayed();
        });

        HomeRobot.DismissWelcomeModal();
    }
}
```

## 🔑 Environment Variables

Use the main environment variables from the main test suite and add the following variables listed below:

`LOKI_ENDPOINT` – Endpoint to the internal Loki instance. Used by the API client to push data.  
`LOKI_PRIVATE_KEY_WINDOWS` – Private key of the internal Loki instance. Used to authenticate API calls.  
`LOKI_CERTIFICATE_WINDOWS` – Certificate of the internal Loki instance. Used to authenticate API calls.  
`BTI_CONTROLLER_URL` – Used to control networking scenarios on internal testing infrastructure. Required for Anti-Censorship SLIs.
