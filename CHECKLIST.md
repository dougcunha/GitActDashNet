
### **Implementation Checklist: GitActDash for .NET**

**Overall Status:** ⏳ **In Progress**

---

#### **Phase 1: Initial Setup and Project Structure**
*   [x] **FR-N/A:** Create a new Blazor Web App project with .NET 9 (`dotnet new blazor`).
*   [x] **NFR-04:** Create the initial directory structure as specified (`Components/Pages`, `Services`, `Data`, etc.).
*   [x] **FR-01:** Add the GitHub `ClientId` and `ClientSecret` settings to `appsettings.json`.
*   [x] **NFR-01:** Configure `.gitignore` to ignore `appsettings.Development.json` and other sensitive files.
*   [x] **NFR-04:** Install and configure the Octokit.NET package in the project.
*   [x] **NFR-06:** Install and configure Serilog for structured logging with file and console outputs.

---

#### **Phase 2: Authentication and Basic Page Flow**
*   [x] **FR-01:** Configure GitHub authentication (`AddAuthentication().AddGitHub()`) in `Program.cs`.
*   [x] **NFR-04:** Register Octokit.NET's `IGitHubClient` in the DI container in `Program.cs`.
*   [x] **FR-04:** Create the login page (`Components/Pages/Home.razor`) with the "Login with GitHub" button.
*   [x] **FR-03:** Create the dashboard page (`Components/Pages/Dashboard.razor`) and protect it with the `[Authorize]` attribute.
*   [x] **FR-03:** Implement automatic redirection to `/dashboard` if the user is already authenticated.
*   [x] **FR-02:** Implement the logout functionality.

---

#### **Phase 3: GitHub API Logic and Data Models**
*   [x] **Section 5:** Create the `Data/GitHubModels.cs` file and define auxiliary models (enums and records) for internal application use.
*   [x] **NFR-05:** Create the `Utils/OperationResult.cs` and `Utils/OperationResultExtensions.cs` files to implement the monad pattern for error handling.
*   [x] **NFR-06:** Create the `Utils/LoggingExtensions.cs` file with contextual logging extensions for services and components.
*   [x] **NFR-04:** Create the `Services/GitHubService.cs` service as a wrapper for Octokit.NET using the OperationResult pattern.
*   [x] **NFR-04:** Configure `GitHubService` to use the DI-injected `IGitHubClient`.
*   [x] **FR-05:** Implement the method in `GitHubService` to fetch user repositories using `Repository.GetAllForCurrent()` and `Repository.GetAllForOrg()` with OperationResult pattern.
*   [x] **FR-10:** Implement the method in `GitHubService` to fetch repository workflows using `Actions.Workflows.List()` with OperationResult pattern.
*   [x] **FR-10:** Implement the method in `GitHubService` to fetch the latest workflow run using `Actions.Workflows.Runs.ListByWorkflow()` with OperationResult pattern.
*   [x] **NFR-06:** Add comprehensive logging to `GitHubService` with operation timing and contextual information.

---

#### **Phase 4: Repository Selection and Filter Panel**
*   [x] **NFR-05:** Create the `Services/LocalStorageService.cs` service using the OperationResult pattern for error handling.
*   [x] **NFR-06:** Add comprehensive logging to `LocalStorageService` with operation context and error details.
*   [ ] **NFR-04:** Create the `wwwroot/js/interop.js` file for additional JavaScript interoperability if needed.
*   [ ] **FR-06:** Create the `Components/Shared/FilterPanel.razor` component.
*   [ ] **FR-05:** In `Dashboard.razor`, call `GitHubService` to get the list of repositories and handle OperationResult responses in the `FilterPanel`.
*   [ ] **FR-08:** Implement the name search field in the `FilterPanel`.
*   [ ] **FR-08:** Implement the filter buttons ("All", "Personal", "Organization").
*   [ ] **FR-09:** Implement the repository list sorting functionality.
*   [ ] **FR-07:** Implement persistence of repository selection in `localStorage` using `LocalStorageService` when checking/unchecking a checkbox.
*   [ ] **FR-07:** Implement restoration of repository selection from `localStorage` using `LocalStorageService` when loading the panel.

---

#### **Phase 5: Workflow Panel Display**
*   [ ] **FR-11:** Create the `Components/Shared/RepositoryColumn.razor` component.
*   [ ] **FR-12:** Create the `Components/Shared/WorkflowCard.razor` component.
*   [ ] **FR-10:** In `Dashboard.razor`, for each selected repository, fetch the workflows and their latest runs using `GitHubService` and handle OperationResult responses.
*   [ ] **FR-11:** Display the selected repositories in a column layout using `RepositoryColumn`.
*   [ ] **FR-12:** Inside each column, display the `WorkflowCard` with name, status, and time of the last run.
*   [ ] **FR-12:** Implement colored visual indicators for run status (`success`, `failure`, etc.).
*   [ ] **FR-13:** Implement the link in `WorkflowCard` to open the workflow page on GitHub.

---

#### **Phase 6: Additional UI Features**
*   [ ] **FR-14:** Create the `Components/Shared/RefreshControls.razor` component.
*   [ ] **FR-14:** Implement the auto-refresh logic with a timer and interval selector using OperationResult pattern for error handling.
*   [ ] **FR-14:** Implement the manual refresh button with proper error handling.
*   [ ] **FR-16:** Create the `Components/Shared/ThemeToggle.razor` component.
*   [ ] **FR-16:** Implement theme toggling (light/dark) and persistence of the preference in `localStorage` using `LocalStorageService`.
*   [ ] **FR-15:** Implement fullscreen mode functionality with a control button.

---

#### **Phase 7: Polishing and Finalization**
*   [ ] **NFR-02:** Add loading indicators (spinners/skeletons) during Octokit calls with proper OperationResult error state handling.
*   [ ] **NFR-03:** Review and adjust application responsiveness for different screen sizes.
*   [ ] **Style:** Refine CSS to replicate the look of the original application, including dark theme.
*   [ ] **NFR-N/A:** Perform comprehensive manual testing of all functional requirements including error scenarios.
*   [ ] **NFR-05:** Document the use of OperationResult pattern and its benefits for error handling throughout the application.
*   [ ] **NFR-04:** Document the use of Octokit.NET and its main features used in the project.