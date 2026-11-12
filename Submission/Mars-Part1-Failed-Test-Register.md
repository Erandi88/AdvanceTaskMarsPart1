# MARS Part 1 — failed-test review register

Review date: 12 November 2026, Auckland.

## Run and evidence

| Item | Result |
|---|---|
| Automated cases executed | 121 |
| Passed | 99 |
| Failed | 22 |
| Skipped | 0 |
| Execution pass rate | 81.8% |
| Run start | 12 November 2026, 12:42:39 pm |
| Run end | 12 November 2026, 2:00:36 pm |
| Duration | 1 hour 17 minutes 57 seconds |
| Failed fixtures | Languages: 11; Skills: 11 |

Source: the uploaded `ExtentReport.html` for this run. Manual-case references follow the reconciled test-case workbook. Keep this document and the HTML report in the same folder to open the relative evidence links. The report can also be navigated by its displayed `#test-id` values. The application is the locally configured MARS test environment; its application build/version was not supplied.

All 22 recorded failures are assertion mismatches. No browser, setup, timeout, or cleanup exception is recorded as a failure cause in these entries. This does not independently prove every cleanup completed or rule out all automation limitations. No new test execution was performed for this review.

## How to use this register

`REV-01` to `REV-07` are local review references, not assigned Jira or GitHub issue IDs. The supplied workbook does not provide an assigned tracker ID for these 22 failures. These seven groups describe similar behavior; they do not establish seven root causes or seven confirmed defects. Separate Language/Skill tickets or Add/Edit tickets may be appropriate after triage.

Eight failed cases show validation mismatches against the workbook expectations: whitespace-only names, existing names with different levels, and unchanged Update feedback. Fourteen failed cases depend on confirmation of trimming, case-insensitive uniqueness, numeric-name or allowed-character rules. All 22 remain failed in the recorded run. No expected result, historical manual result, or official defect ID has been changed by this register.

| Review reference | Behavior | Failed cases | Disposition |
|---|---|---:|---|
| REV-01 | Spaces-only names accepted on Add and Edit | 4 | Validation mismatch against workbook |
| REV-02 | Edit accepts an existing name with a different level | 2 | Validation mismatch against workbook |
| REV-03 | Unchanged Update reports the record as a duplicate | 2 | Validation mismatch against workbook |
| REV-04 | Leading and trailing spaces retained on Add and Edit | 4 | Requirement confirmation needed |
| REV-05 | Case-only duplicate names accepted on Add and Edit | 4 | Requirement confirmation needed |
| REV-06 | Numeric-only names accepted on Add | 2 | Requirement confirmation needed |
| REV-07 | Symbol-containing names accepted on Add and Edit | 4 | Requirement confirmation needed |

## REV-01 — Spaces-only names accepted on Add and Edit

**Status:** Validation mismatch against workbook. Tracker ID not assigned.

**Expected behavior:** A name containing only spaces is rejected. Add creates no record; Edit preserves the original name and level.

**Impact:** Blank-looking records can be created, or valid names can be replaced by whitespace.

**Reproduction flow:**

1. Log in to the test account and open Profile → Languages or Skills.
2. For Add, use the empty-table precondition, open Add New, enter exactly five spaces and select a valid level.
3. For Edit, create the valid starting record, open its editor and replace its name with exactly five spaces.
4. Submit and compare the saved row and count against the expected result before removing test data.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.AddLanguageRejectsSpacesOnlyName` | TC_LANG_ADD_009 | Five spaces created one blank-looking language record. | [Test 25](ExtentReport.html#test-id=25) |
| `LanguageTests.EditLanguageRejectsSpacesOnlyName` | TC_LANG_EDIT_010 | Original language and level were not preserved after the spaces-only edit. | [Test 29](ExtentReport.html#test-id=29) |
| `SkillTests.AddSkillRejectsSpacesOnlyName` | TC_SKILL_ADD_008 | Five spaces created one blank-looking Skill record. | [Test 113](ExtentReport.html#test-id=113) |
| `SkillTests.EditSkillRejectsSpacesOnlyName` | TC_SKILL_EDIT_010 | Original Skill and level were not preserved after the spaces-only edit. | [Test 118](ExtentReport.html#test-id=118) |

**Triage action:** Confirm the required-name rule in the acceptance criteria, then log Language and Skill defects with their Add/Edit evidence. Do not infer a shared root cause.

## REV-02 — Edit accepts an existing name with a different level

**Status:** Validation mismatch against workbook. Tracker ID not assigned.

**Expected behavior:** Editing one record to another existing name is rejected even when the levels differ. Both original records remain unchanged.

**Impact:** An edit can replace the first record and leave two records with the same name at different levels.

**Reproduction flow:**

1. Create two distinct names with the levels specified in the test JSON.
2. Edit the first record to the second name using a different level from the second record.
3. Submit and inspect both original records, the number of occurrences of the existing name, and the attempted name/level combination.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.EditLanguageRejectsExistingNameWithDifferentLevel` | TC_LANG_EDIT_007 | Original record disappeared; English appeared twice and the attempted name/level combination was saved. | [Test 28](ExtentReport.html#test-id=28) |
| `SkillTests.EditSkillRejectsExistingNameWithDifferentLevel` | TC_SKILL_EDIT_007 | Original Skill disappeared; Selenium appeared twice and the attempted name/level combination was saved. | [Test 116](ExtentReport.html#test-id=116) |

**Triage action:** Confirm that uniqueness is by name rather than name-plus-level, and link the approved rule when logging the defect.

## REV-03 — Unchanged Update reports the record as a duplicate

**Status:** Validation mismatch against workbook. Tracker ID not assigned.

**Expected behavior:** Submitting unchanged values preserves the record and does not describe it as a duplicate of itself. A silent no-op or suitable no-change response is acceptable under the workbook.

**Impact:** Users receive misleading duplicate feedback for a record they have not changed. The reported failing assertion concerns feedback, not a proven extra record.

**Reproduction flow:**

1. Create a valid record and open its editor.
2. Leave the name and level unchanged, then click Update.
3. Read the response and verify that the original record and row count are unchanged.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.UpdateLanguageWithoutChanges` | TC_LANG_EDIT_016 | Unchanged Update returned a duplicate-language response. | [Test 33](ExtentReport.html#test-id=33) |
| `SkillTests.UpdateSkillWithoutChanges` | TC_SKILL_EDIT_013 | Unchanged Update returned a duplicate-Skill response. | [Test 121](ExtentReport.html#test-id=121) |

**Triage action:** Log the misleading feedback against the workbook expectation. Obtain mentor disposition if the intended no-change response is disputed.

## REV-04 — Leading and trailing spaces retained on Add and Edit

**Status:** Requirement confirmation needed. Tracker ID not assigned.

**Expected behavior:** The workbook expects surrounding whitespace to be removed: "  English  " becomes "English", and "  JMeter  " becomes "JMeter".

**Impact:** Saved names retain padding and can appear equivalent visually while differing in their stored text.

**Reproduction flow:**

1. Add or edit a record using the padded input specified in its JSON.
2. Save, refresh, reopen the saved record in Edit and inspect the raw input value, including its length.
3. Compare the raw value with the expected trimmed name.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.AddLanguageTrimsSurroundingSpaces` | TC_LANG_ADD_008 | Saved raw value was "  English  " (11 characters), expected "English" (7). | [Test 26](ExtentReport.html#test-id=26) |
| `LanguageTests.EditLanguageTrimsSurroundingSpaces` | TC_LANG_EDIT_009 | Saved edited raw value was "  English  " (11 characters), expected "English" (7). | [Test 31](ExtentReport.html#test-id=31) |
| `SkillTests.AddSkillTrimsSurroundingSpaces` | TC_SKILL_ADD_009 | Saved raw value was "  JMeter  " (10 characters), expected "JMeter" (6). | [Test 114](ExtentReport.html#test-id=114) |
| `SkillTests.EditSkillTrimsSurroundingSpaces` | TC_SKILL_EDIT_009 | Saved edited raw value was "  JMeter  " (10 characters), expected "JMeter" (6). | [Test 119](ExtentReport.html#test-id=119) |

**Triage action:** Ask whether surrounding spaces must be trimmed, rejected, or preserved. Keep these results failed against the current workbook until an approved requirement change is recorded.

## REV-05 — Case-only duplicate names accepted on Add and Edit

**Status:** Requirement confirmation needed. Tracker ID not assigned.

**Expected behavior:** The workbook expects case-insensitive uniqueness: English/english and Selenium/selenium are treated as the same name.

**Impact:** Names differing only by letter case can coexist; an edit may replace the first original name.

**Reproduction flow:**

1. Create the baseline name with its specified level.
2. For Add, submit the same name in lowercase. For Edit, change a different record to that lowercase name.
3. Verify that the attempted duplicate is rejected, the existing record remains, and Edit preserves the first original record.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.AddLanguageRejectsCaseOnlyDuplicate` | TC_LANG_ADD_010 | Lowercase english was saved alongside English; count increased from 1 to 2. | [Test 24](ExtentReport.html#test-id=24) |
| `LanguageTests.EditLanguageRejectsCaseOnlyDuplicate` | TC_LANG_EDIT_008 | Original French record disappeared and lowercase english was saved. | [Test 27](ExtentReport.html#test-id=27) |
| `SkillTests.AddSkillRejectsCaseOnlyDuplicate` | TC_SKILL_ADD_010 | Lowercase selenium was saved alongside Selenium; count increased from 1 to 2. | [Test 112](ExtentReport.html#test-id=112) |
| `SkillTests.EditSkillRejectsCaseOnlyDuplicate` | TC_SKILL_EDIT_008 | Original Skill disappeared and lowercase selenium was saved. | [Test 115](ExtentReport.html#test-id=115) |

**Triage action:** Confirm whether uniqueness is case-insensitive. Record a defect only against the approved rule; do not change expected results merely to obtain a passing run.

## REV-06 — Numeric-only names accepted on Add

**Status:** Requirement confirmation needed. Tracker ID not assigned.

**Expected behavior:** The workbook assumes numeric-only Language and Skill names are invalid and must not create records.

**Impact:** A numeric-only input creates a record despite the workbook rejection expectation.

**Reproduction flow:**

1. Open Add New with the scenario preconditions satisfied.
2. Enter the numeric-only name from the JSON and select its valid level.
3. Submit and compare the record count with the baseline.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.AddLanguageRejectsNumericOnlyName` | TC_LANG_ADD_006 | Numeric-only language was saved; row count increased from 0 to 1. | [Test 5](ExtentReport.html#test-id=5) |
| `SkillTests.AddSkillRejectsNumericOnlyName` | TC_SKILL_ADD_006 | Numeric-only Skill was saved; row count increased from 0 to 1. | [Test 94](ExtentReport.html#test-id=94) |

**Triage action:** Confirm the permitted name format. The workbook already identifies the numeric-only restriction as an assumption.

## REV-07 — Symbol-containing names accepted on Add and Edit

**Status:** Requirement confirmation needed. Tracker ID not assigned.

**Expected behavior:** The workbook expects the tested symbols-only Add inputs and the tested invalid Edit inputs to be rejected. Valid existing Edit data must remain unchanged.

**Impact:** Invalid inputs under the workbook rule can create records or replace valid names.

**Reproduction flow:**

1. For Add, enter the symbols-only input from the relevant JSON and select a valid level.
2. For Edit, create the valid record and replace its name with the invalid input: symbols-only for Language, and numbers-plus-symbols for the Skill edit case.
3. Submit and inspect the attempted record, original record and row count.

**Recorded evidence:**

| Automated test | Manual case | Observed result | Report entry |
|---|---|---|---|
| `LanguageTests.AddLanguageRejectsSpecialCharactersOnlyName` | TC_LANG_ADD_007 | Special-characters-only language was saved; row count increased from 0 to 1. | [Test 6](ExtentReport.html#test-id=6) |
| `LanguageTests.EditLanguageRejectsSpecialCharactersOnlyName` | TC_LANG_EDIT_011 | Original language disappeared and the invalid name was saved. | [Test 30](ExtentReport.html#test-id=30) |
| `SkillTests.AddSkillRejectsSpecialCharactersOnlyName` | TC_SKILL_ADD_007 | Special-characters-only Skill was saved; row count increased from 0 to 1. | [Test 95](ExtentReport.html#test-id=95) |
| `SkillTests.EditSkillRejectsNumbersAndSpecialCharacters` | TC_SKILL_EDIT_011 | Original Skill and level were not preserved after the numbers/symbols edit. | [Test 117](ExtentReport.html#test-id=117) |

**Triage action:** Obtain the allowed-character rule separately for Language and Skills. A Skill rule must not accidentally reject legitimate technology names containing punctuation, such as C# or C++. These tests do not establish a blanket ban on all punctuation.

## Evidence and verification limits

- Inspect the pre-cleanup screenshot where attached to a failed report entry. The standard teardown screenshot may show the page after cleanup; it should not be used alone to reconstruct the failure state.
- Some assertions stop the test before later checks execute. Do not claim that a failed Add case verified refresh persistence unless that step actually ran. The observations above reflect the assertion evidence, not unexecuted later steps.
- The case-only Skill Edit message mentions a “different level” because it reuses a verification helper. Its test name and mapped manual case identify the case-only scenario. Improve the assertion wording during future maintenance; it does not change the recorded result.
- Passing positive Add/Edit/Delete cases and prior cleanup checks provide supporting context, but do not establish a successful product fix for any failed validation case.
- Do not automatically assign severity from the test priority. Confirm user impact, scope and business rules before assigning severity, priority, owner and tracker ID.

## Submission actions

1. Review REV-01 to REV-03 against the workbook and acceptance criteria, then log or link the accepted validation defects.
2. Obtain mentor decisions for REV-04 to REV-07. Record the approved expected behavior and requirement reference before reclassifying any candidate.
3. Add actual tracker IDs to the workbook and this register after issues are created. No tickets have been created or messages sent by this review.
4. Include the reconciled workbook, full HTML report, this register, README and final source commit in the submission. Keep partial coverage and blocked scenarios visible.
5. A new full-suite run is not required solely to document these results. Re-run affected cases when product code, test behavior or approved expectations change.
