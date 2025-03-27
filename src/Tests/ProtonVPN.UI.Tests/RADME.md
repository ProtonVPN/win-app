# ProtonVPN Windows Automated UI Tests

## 🔧 Tech Stack

This project utilizes the following technologies for automated UI testing:

- [FlaUI](https://github.com/FlaUI/FlaUI) – A .NET UI automation library.  
- [NUnit](https://nunit.org/) – A unit-testing framework for .NET.  
- [FlaUI Inspect](https://github.com/FlaUI/FlaUInspect) – A tool for inspecting UI elements.

## 🔠 Test Categories 
Every test class or at least each test method should contain some sort of category assignment. It can be assigned to the whole class or individual methods.

**Category [ARM]** – Used to define which tests support ARM architecture.

**Category [1,2,3]** – Used for test parallelization in CI.

**Category [SLI]** – Used to define which tests are SLI measurements.

## 📂 Project Structure

### **Annotations**  
Contains annotations for SLI tests, defining the types of measurements taken during test execution.

### **ApiClient**  
Handles API interactions and is structured as follows:

- **Contracts** – API contracts required for authenticated API requests.  
- **Prod** – API client for making requests to the production environment.  
- **TestEnv** – API client for making requests to the test environment.

### **Enums**  
Stores enumerations used for test data.

### **Robots**  
Defines all UI actions and selectors for UI components. It follows the following structure:

- A Robot class is an object that stores all elements, actions, and verifications related to a UI component or page.  
- Every UI element is saved as a protected variable as an `Element` object.  
- Every action is saved as a function that returns the Robot it’s stored in.  
- Every Robot must contain an inner *Verification* class. Inside this inner class, verification methods should be stored.

### **TestBase**  
Includes various initialization methods for UI tests:

- **BaseTest** – Contains all minimal setup and teardown functions to run UI tests. It prepares relevant artifacts for UI tests. It also contains all methods related to environment setup (e.g., launching and cleaning up the app). *Note: This setup class is useful when there is no need to do cleanup and app launch between individual tests.*  
- **FreshSessionSetUp** – Inherits from `BaseTest`, but additionally *launches*, *closes*, and *clears application data* between individual tests.  
- **SliSetup** – Inherits from `BaseTest`. It should be used only for SLI tests. It helps send metrics after each measurement.

### **Tests**  
Contains the test suites:

- **E2ETests** – End-to-end tests executed on the production environment.  
- **SliTests** – Tests that track client performance. See the README inside this folder for more details.

### **TestsHelper**  
Provides helper functions for UI tests.

### **UiTools**  
Contains UI actions and the `Element` class, facilitating interactions with the UI.

## 🔑 Environment Variables

Tests are executed on production, so all users must be accessible in production.

**Main Variables**

`PLUS_USER` – VPN Plus plan user. Used for the majority of tests where paid flows are involved.  

`FREE_USER` – VPN Free plan user. Used where all Free flows are tested.

**Additional Variables**

`SPECIAL_CHARS_USER` – Used for the special characters login test case.  

`SSO_USER` – Used for SSO sign-in tests.  

`TWO_FACTOR_AUTH_USER` – Used for 2FA login test cases.  

`TWO_FA_KEY` – Used to get the 2FA key for login test cases.  

`TWO_PASS_USER` – Used for the two-pass login test case. The second password is used for mail, so VPN access must not be blocked.  

`VISIONARY_USER` – Used to sign in as a Visionary user.  

`ZERO_CONNECTIONS_USER` – Used for the zero assigned connections login test case.
