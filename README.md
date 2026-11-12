# MARS Advanced Part 1 — QA Automation

Manual test cases and C# browser automation for MARS Advanced Part 1. The project uses NUnit, Selenium WebDriver, page objects, scenario JSON files and ExtentReports.

## Recorded execution

Full-suite execution on **12 November 2026**, from **12:42:39 pm to 2:00:36 pm**, in the tester's Auckland environment:

| Result | Cases |
|---|---:|
| Passed | 99 |
| Failed | 22 |
| Skipped | 0 |
| Total | 121 |

Execution pass rate: **81.8%**. Duration: **1 hour 17 minutes 57 seconds**. This is the recorded run result, not a claim of complete requirements coverage or an expected result for every future run.

The 22 failures are Language and Skill validation assertions. Eight cases show validation mismatches against workbook expectations. Fourteen depend on confirmation of trimming, case-insensitive uniqueness and allowed-name-format rules. The failed-test register contains the observations, mapped manual cases and report references. Failed assertions remain unchanged pending a product fix or an approved requirement decision.

## Prerequisites

- .NET 8 SDK.
- Google Chrome and a compatible ChromeDriver available to Selenium. The project creates ChromeDriver through `CommonDriver`; it does not pin a driver binary in the repository.
- Docker Desktop running the supplied MARS environment in Linux-container mode.
- A valid MARS test account accessible through `MARS_EMAIL` and `MARS_PASSWORD` environment variables.
- Network access for initial NuGet restore and, if needed, Selenium driver provisioning.
- Visual Studio with .NET support is optional when running through the command line.

## Start the application

Use the separately supplied MARS Docker setup. From the directory containing its Docker Compose file, run:

```powershell
docker compose up
```

Leave the services running. Open `http://localhost:5003/Home` and confirm the application is available. The tests currently use this local address directly. Docker application files and test automation source are separate projects.

## Configure credentials

In PowerShell, set these variables for the terminal session that will run the tests. Replace the example values with the test account credentials:

```powershell
$env:MARS_EMAIL = "your-test-email"
$env:MARS_PASSWORD = "your-test-password"
```

Do not commit real credentials to source files, JSON, documentation or screenshots. For Visual Studio Test Explorer, configure the variables before starting Visual Studio and restart it after environment changes so its test process inherits the values.

## Build and run

From the repository root containing the `MarsAdvanced.Tests` folder:

```powershell
dotnet restore .\MarsAdvanced.Tests\MarsAdvanced.Tests.csproj
dotnet build .\MarsAdvanced.Tests\MarsAdvanced.Tests.csproj --no-restore
dotnet test .\MarsAdvanced.Tests\MarsAdvanced.Tests.csproj --no-build
```

To run Search tests only:

```powershell
dotnet test .\MarsAdvanced.Tests\MarsAdvanced.Tests.csproj --no-build --filter "FullyQualifiedName~MarsAdvanced.Tests.SearchSkillTests"
```

Alternatively, open the solution in Visual Studio, build it, and use **Test → Test Explorer → Run All Tests**. Rebuild after editing source or JSON; `--no-build` uses the existing build.

A run containing assertion failures returns an unsuccessful test result. Review the failure details rather than changing the assertions to match an observed defect.

## Test data and cleanup

The tests read scenario-specific JSON files from `TestData` into models at test level. The project copies those JSON files to the build output. Add a matching copy entry when adding a new scenario file.

Use a dedicated test account. Several tests require an empty Language/Skill table, unused scenario names, or free Language slots. Preserve existing user data and meet those preconditions before execution. Do not run another session that changes the same account during a suite run, and do not enable parallel execution against shared account data.

Tests create identifiable records, verify the intended behavior and attempt cleanup in `finally`. Listing titles include unique suffixes. Profile settings tests capture their original values and attempt restoration. After an interrupted run or cleanup exception, inspect the account and remove only records belonging to that run before retrying. Cleanup is not a substitute for the test's deletion or rejection assertions.

## Project structure

| Location | Purpose |
|---|---|
| `MarsAdvanced.Tests/*Tests.cs` | Test fixtures, assertions, scenario JSON reading and cleanup |
| `MarsAdvanced.Tests/Pages` | Browser interaction and page-specific waits |
| `MarsAdvanced.Tests/Models` | Typed scenario data |
| `MarsAdvanced.Tests/TestData` | JSON input files grouped by feature |
| `MarsAdvanced.Tests/Helpers` | Driver creation, JSON reading, reporting and shared support |
| `MarsAdvanced.Tests/TestRunSetup.cs` | Run-level report initialization and finalization |

The reviewed package references include Selenium 4.29.0, NUnit 3.14.0, NUnit3TestAdapter 4.5.0 and ExtentReports 5.0.4. The `.csproj` is authoritative for dependency versions.

## Reports and submission evidence

Each run writes an `ExtentReport.html` to a unique `Reports` subfolder under NUnit's working directory. The test output prints the exact path. In the recorded Visual Studio run, this was under `MarsAdvanced.Tests/bin/Debug/net8.0/Reports`.

Failure entries include assertion details, stack traces and screenshots where capture succeeds. Use a pre-cleanup screenshot where provided: the teardown screenshot can show the page after cleanup.

Place the final evidence at these repository-relative paths when preparing the submission:

| File | Purpose |
|---|---|
| `Submission/Mars_Advanced_Part1_TestCases_Updated.xlsx` | Original manual execution history, automation mappings, coverage limits and all 121 run results |
| `Submission/ExtentReport.html` | Recorded full-suite execution evidence |
| `Submission/Mars-Part1-Failed-Test-Register.md` | Seven behavior groups covering all 22 failed cases, pending decisions and reproduction flows |

Keep the three files together so report references remain usable. Copy the HTML report out of `bin` before packaging the source. Include the manual screenshot evidence at its actual submitted location as well; the workbook retains historical Windows paths that will not resolve on another machine. Those paths must be reconciled with the supplied evidence files rather than treated as portable links.

Exclude `bin`, `obj`, `.vs`, compiled binaries and unrelated intermediate outputs from the source submission. Include the solution/project files, source, models, JSON, documentation and selected evidence. If generated files are already tracked, an ignore rule alone does not remove them from Git tracking; review the staged file list before committing.

## Coverage and known limitations

The workbook contains 134 manual cases. Its reconciliation identifies 100 cases with the main behavior represented, 15 with material partial coverage, 13 requiring clarification and 6 blocked. These categories do not certify every manual step, exact baseline or success message. One automated test can map to multiple manual cases, and some automation covers additional scenarios, so 121/134 is not a coverage percentage.

- Language and Skill validation: 11 failed cases in each fixture. These are retained as failed results, not skipped tests. No assigned tracker IDs were supplied for the 22 failures; `REV-01` to `REV-07` are local review references only.
- Share Skill calendar availability: the valid-create automation leaves the Calendar empty. It does not cover calendar event creation and persistence.
- Empty credit: the observed UI defaults to zero and could not be cleared through the attempted keyboard interaction. Empty credit and numeric zero are different inputs. The empty-credit case has no automated result.
- Notifications: the empty menu is automated. Five other manual scenarios are blocked by unavailable notification records.
- Location editing, circular-arrow reset scope, long-name limits and session-expiry behavior need confirmed workflows or expectations before automation sign-off.
- Several Search cases use controlled keyword-scoped data; this does not prove all no-keyword browsing, displayed counts or selected-control states in the corresponding manual cases.
- The earlier Search-only Test Explorer display showed warnings whose cause was not established from the supplied log. The supplied current-project Build + IntelliSense Error List showed zero errors and zero warnings. Do not describe the unexplained indicator as a diagnosed or resolved issue.

The application build/version was not supplied with the recorded run. Outstanding product behavior, requirement decisions and coverage limitations are included for mentor review rather than represented as completed or passing work.
