# .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Risk Management](#risk-management)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

This plan outlines the upgrade of a Blazor WebAssembly solution from **.NET 8.0** to **.NET 10.0 (Long Term Support)**. The solution consists of three projects following the standard Blazor WASM hosted model with a shared library, client-side WebAssembly application, and ASP.NET Core server.

### Scope

**Solution:** Report_App_WASM.sln

**Projects to Upgrade:**
1. `Report_App_WASM.Shared` - Shared class library
2. `Report_App_WASM.Client` - Blazor WebAssembly client application  
3. `Report_App_WASM.Server` - ASP.NET Core server with Identity and API

**Current State:**
- All projects targeting net8.0
- 39 NuGet packages (25 compatible, 12 need updates, 2 incompatible/deprecated)
- 23,414 lines of code across 212 files
- 104 API compatibility issues identified
- No security vulnerabilities detected ✅

**Target State:**
- All projects targeting net10.0
- 12 core Microsoft packages upgraded to 10.0.3
- 2 package issues resolved (deprecated/incompatible)
- All API compatibility issues addressed

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in single atomic operation.

**Rationale:**
- **Small solution**: 3 projects only
- **Simple dependency structure**: Linear dependency chain (Shared → Client → Server)
- **All projects on modern .NET**: Currently on .NET 8.0
- **Low complexity**: All projects rated Low difficulty
- **No security vulnerabilities**: No critical blockers
- **Package compatibility**: 64.1% already compatible, clear upgrade paths for remaining
- **Homogeneous codebase**: Consistent Blazor WebAssembly patterns throughout

### Discovered Metrics

| Metric | Value | Classification Factor |
| :--- | :---: | :--- |
| Total Projects | 3 | ✅ Small (< 5) |
| Dependency Depth | 2 | ✅ Simple (≤ 2) |
| High-Risk Projects | 0 | ✅ No high-risk |
| Security Vulnerabilities | 0 | ✅ None |
| Target Framework Cycles | 0 | ✅ No cycles |
| Package Updates Required | 14 | ⚠️ Moderate |
| Total LOC | 23,414 | ✅ Small-Medium |
| API Issues | 104 | ⚠️ Moderate (mostly behavioral) |

**Complexity Classification:** **Simple Solution**

The solution exhibits all characteristics of a simple upgrade scenario: few projects, shallow dependencies, no critical risks, and manageable API compatibility surface. This enables a fast, coordinated all-at-once upgrade approach.

### Critical Issues

**API Compatibility:**
- 🔴 **3 Binary Incompatible APIs** - Require immediate code changes (Server project only)
  - `OptionsConfigurationServiceCollectionExtensions.Configure<T>` method signature changes
  - `ConfigurationBinder.Get<T>` method signature changes
- 🟡 **52 Source Incompatible APIs** - May require compilation fixes
  - Directory Services APIs (13 occurrences) - Namespace/package changes
  - OleDb APIs (14 occurrences) - Platform compatibility considerations
  - ASP.NET Core Identity/Diagnostics APIs - Package reorganization
- 🔵 **28 Behavioral Changes** - Require runtime validation
  - `HttpContent` usage patterns (14 occurrences)
  - `Uri` constructor validation (12 occurrences)
  - `TimeSpan.FromMinutes/FromSeconds` precision changes

**Package Issues:**
- ⚠️ **Azure.Identity 1.16.0** - Deprecated, replacement needed
- ⚠️ **Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.22.1** - Incompatible
- 📦 **Microsoft.NETCore.Platforms 7.0.4** - Functionality now included in framework

### Recommended Approach

**All-At-Once Migration** with single atomic upgrade task followed by comprehensive testing.

**Expected Iterations:** 8 iterations
- Phase 1: Discovery & Classification (3 iterations) ✅
- Phase 2: Foundation (3 iterations)
- Phase 3: Detail Generation (2 iterations) - All projects batched together

---

## Migration Strategy

### Approach Selection: All-At-Once Strategy

**Selected Approach:** All projects upgraded simultaneously in a single atomic operation.

### Justification

**Why All-At-Once is Optimal:**

1. **Small Solution Size** (3 projects)
   - Far below the 5-project threshold for incremental approach
   - Coordination overhead exceeds risk reduction benefits

2. **Simple Dependency Structure** (depth = 2)
   - Linear, acyclic dependency chain
   - No complex inter-project relationships
   - No circular dependencies requiring special handling

3. **Homogeneous Technology Stack**
   - All projects currently on .NET 8.0 (modern .NET)
   - All projects are SDK-style
   - Consistent Blazor WebAssembly patterns throughout

4. **Low Individual Complexity**
   - All projects rated "Low" difficulty
   - Shared: 1,866 LOC, 0 API issues
   - Client: 967 LOC, 32 API issues (mostly behavioral)
   - Server: 20,581 LOC, 51 API issues (manageable)

5. **Clear Package Upgrade Paths**
   - 64.1% packages already compatible
   - 30.8% have clear upgrade paths to 10.0.3
   - Only 5.1% require special handling (deprecated/incompatible)

6. **No Security Vulnerabilities**
   - No urgent security-driven sequencing required
   - Can optimize for speed rather than risk isolation

7. **No Blocking Dependencies**
   - All package updates can proceed in parallel
   - No legacy .NET Framework dependencies
   - No netstandard multi-targeting complications

**Why NOT Incremental:**
- Incremental approach adds unnecessary complexity for this solution size
- Multi-targeting would introduce more risk than it mitigates
- Testing surface is manageable for simultaneous upgrade
- Team can validate entire solution in single pass

### All-At-Once Strategy Rationale

The All-At-Once strategy provides:
- **Fastest completion time** - Single coordinated operation vs multiple phases
- **No multi-targeting complexity** - No intermediate states with mixed framework versions
- **Unified dependency resolution** - All projects resolve packages against same TFM
- **Simplified testing** - One comprehensive test pass vs incremental validation
- **Clean state transitions** - net8.0 → net10.0 with no intermediate builds

**Trade-offs Accepted:**
- Higher initial testing surface (entire solution at once)
- All developers must adapt simultaneously
- Single larger PR vs multiple smaller PRs

Given the solution characteristics, these trade-offs are favorable.

### Dependency-Based Ordering

While the upgrade is atomic (all projects updated together), the **natural dependency order** for understanding and validation purposes is:

**Bottom-Up Sequence:**
1. **Tier 1 (Leaf):** `Report_App_WASM.Shared`
   - No project dependencies
   - Provides foundation for other projects
   - Simplest upgrade (no API issues)

2. **Tier 2 (Mid):** `Report_App_WASM.Client`
   - Depends on: Shared
   - Blazor WebAssembly client application
   - Moderate API issues (32, mostly behavioral)

3. **Tier 3 (Root):** `Report_App_WASM.Server`
   - Depends on: Client, Shared
   - ASP.NET Core host with most complexity
   - Most API issues (51) and package updates (15)

**All-At-Once Ordering Principle:** While all projects are updated simultaneously, understanding the dependency order helps in:
- Prioritizing validation checks (validate Shared first, then Client, then Server)
- Troubleshooting build failures (issues in Shared affect all projects)
- Understanding impact propagation (changes in Shared ripple upward)

### Execution Approach

**Single Atomic Operation:**
1. Update all 3 project files to net10.0 simultaneously
2. Update all 14 package references across both projects
3. Resolve 2 incompatible/deprecated package issues
4. Restore dependencies for entire solution
5. Build entire solution (reveals all compatibility issues)
6. Fix all compilation errors in single pass (referencing breaking changes catalog)
7. Rebuild to verify all fixes
8. Comprehensive solution-wide testing

**No Intermediate States:** The solution transitions directly from "all net8.0" to "all net10.0" without mixed-version states.

### Parallel vs Sequential Execution

**Project File Updates:** Sequential recommended for clarity
- Update Shared first (safest, no dependencies)
- Update Client second (depends on Shared)
- Update Server last (depends on both)
- *Note: While performed sequentially for human clarity, this is still considered a single atomic operation before any build/test*

**Package Updates:** Can occur in parallel or sequential
- Updates are independent across projects
- No inter-package update dependencies
- Batching improves efficiency

**Build & Fix:** Must be sequential
- Build entire solution to identify all issues
- Fix issues systematically (dependency order preferred when issues relate)
- Rebuild to verify fixes

### Phase Definitions

**Phase 0: Prerequisites** *(Optional - validation only)*
- Verify .NET 10.0 SDK installed

**Phase 1: Atomic Upgrade** *(Core execution phase)*
- Update all project TFMs to net10.0
- Update all package references to 10.0.3+ versions
- Remove/replace incompatible packages
- Restore dependencies
- Build solution and address all compilation errors
- Rebuild and verify 0 errors

**Phase 2: Validation** *(Verification phase)*
- Run solution-wide tests
- Validate application functionality
- Verify no package conflicts
- Confirm no warnings or vulnerabilities

**Deliverables:**
- Phase 1: Solution builds successfully with 0 errors
- Phase 2: All functionality validated, tests passing

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution follows a clean, linear dependency structure typical of Blazor WebAssembly hosted applications:

```
Report_App_WASM.Server (Root - ASP.NET Core Host)
├── Report_App_WASM.Client (Blazor WASM Client)
│   └── Report_App_WASM.Shared (Shared Models/DTOs)
└── Report_App_WASM.Shared (Shared Models/DTOs)
```

**Characteristics:**
- **Depth:** 2 levels (very shallow)
- **Circular Dependencies:** None
- **Leaf Nodes:** 1 (Report_App_WASM.Shared)
- **Root Nodes:** 1 (Report_App_WASM.Server)
- **Complexity:** Low - Standard Blazor WASM architecture

### Project Groupings for Migration

**All-At-Once Strategy dictates a single atomic operation for all projects:**

**Single Atomic Upgrade Phase:**
- `Report_App_WASM.Shared` (Leaf - no dependencies)
- `Report_App_WASM.Client` (Mid-tier - depends on Shared)
- `Report_App_WASM.Server` (Root - depends on Client and Shared)

All three projects will have their target frameworks and packages updated simultaneously, ensuring no multi-targeting complexity and maintaining a consistent dependency resolution state.

### Critical Path Identification

**Primary Migration Path:** Shared → Client → Server

While all projects upgrade simultaneously in terms of target framework changes, the **validation order** should respect dependencies:

1. **Report_App_WASM.Shared** validates first (no dependencies)
2. **Report_App_WASM.Client** validates second (depends on Shared)
3. **Report_App_WASM.Server** validates last (depends on both)

However, since this is an All-At-Once strategy, the actual **compilation** will occur for the entire solution together, revealing all issues at once. This is acceptable given the solution's small size and low complexity.

### Dependency Considerations

**Package Dependencies:**
- **Shared packages** (affect multiple projects):
  - `Microsoft.Extensions.Localization` → Client + Server
  - `Microsoft.AspNetCore.Components.WebAssembly` → Client + Server
  - `Microsoft.AspNetCore.Components.WebAssembly.Authentication` → Client + Server
  - `Blazor.SimpleGrid` → Client + Server

- **Server-specific packages** (11 packages):
  - Entity Framework Core, ASP.NET Core Identity, Database tools
  - Cloud services (Azure.Identity - deprecated)
  - Data access (MySQL, PostgreSQL, Oracle, OleDb)

- **Client-specific packages** (11 packages):
  - MudBlazor UI components
  - Blazor extensions (AceEditor, PivotTable, ApexCharts, DownloadFile)

**No Blocking Dependencies:** All package updates can proceed in parallel since the solution uses compatible versions that have clear upgrade paths to .NET 10.

### Circular Dependencies

**Status:** None detected ✅

The solution exhibits a clean, acyclic dependency graph with no circular references. This enables straightforward migration without special handling for dependency cycles.

---

## Project-by-Project Migration Plans

### Project 1: Report_App_WASM.Shared

**Project Type:** ClassLibrary (SDK-style)  
**Project Path:** `Report_App_WASM\Shared\Report_App_WASM.Shared.csproj`

#### Current State

- **Target Framework:** net8.0
- **Dependencies:** 0 projects
- **Dependants:** 2 projects (Client, Server)
- **NuGet Packages:** 0
- **Files:** 98 files
- **Lines of Code:** 1,866
- **API Issues:** 0
- **Risk Level:** Low

#### Target State

- **Target Framework:** net10.0
- **Package Updates:** None required
- **Expected Code Changes:** None (only TFM update)

#### Migration Steps

##### 1. Prerequisites
- None required (leaf project with no dependencies)

##### 2. Target Framework Update

Update project file `Report_App_WASM\Shared\Report_App_WASM.Shared.csproj`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

**No package updates required** - This project has no direct NuGet package references.

##### 4. Expected Breaking Changes

**None identified** - No API compatibility issues detected in this project.

##### 5. Code Modifications

**None required** - This project contains only shared models and DTOs with no identified API compatibility issues.

##### 6. Testing Strategy

**Build Validation:**
- Project builds without errors
- Project builds without warnings
- No dependency conflicts

**Compatibility Validation:**
- Shared classes remain compatible with Client and Server projects
- No serialization issues introduced (JSON/data transfer)

**Manual Checks:**
- Review any custom attributes or validation logic
- Verify data annotations still function correctly

##### 7. Validation Checklist

- [ ] Project file updated to net10.0
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] No package version conflicts
- [ ] Client and Server projects can still reference Shared successfully

---

### Project 2: Report_App_WASM.Client

**Project Type:** AspNetCore (Blazor WebAssembly, SDK-style)  
**Project Path:** `Report_App_WASM\Client\Report_App_WASM.Client.csproj`

#### Current State

- **Target Framework:** net8.0
- **Dependencies:** 1 project (Shared)
- **Dependants:** 1 project (Server)
- **NuGet Packages:** 11
- **Files:** 146 files
- **Lines of Code:** 967
- **API Issues:** 32 (0 binary incompatible, 5 source incompatible, 27 behavioral)
- **Risk Level:** Low

#### Target State

- **Target Framework:** net10.0
- **Package Updates:** 3 packages
- **Expected Code Changes:** Minimal (mostly validation/testing of behavioral changes)

#### Migration Steps

##### 1. Prerequisites

- Report_App_WASM.Shared must be updated to net10.0 first (handled in atomic operation)

##### 2. Target Framework Update

Update project file `Report_App_WASM\Client\Report_App_WASM.Client.csproj`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

| Package Name | Current Version | Target Version | Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.AspNetCore.Components.WebAssembly | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.Extensions.Localization | 9.0.9 | 10.0.3 | Framework alignment - recommended upgrade |

**Compatible Packages (no update required):**
- Blazor.AceEditorJs 1.2.1
- Blazor.PivotTable 1.0.8
- Blazor.SimpleGrid 8.0.1
- Blazor-ApexCharts 6.0.2
- BlazorDownloadFile 2.4.0.2
- CodeBeam.MudBlazor.Extensions 8.2.4
- CronExpressionDescriptor 2.44.0
- FluentValidation 12.0.0
- MudBlazor 8.12.0

##### 4. Expected Breaking Changes

**Source Incompatible (5 occurrences) - May require fixes:**

1. **TimeSpan.FromMinutes/FromSeconds** (2 occurrences)
   - **Change:** Precision/rounding behavior changes in .NET 10
   - **Impact:** Potential slight differences in calculated durations
   - **Action:** Review usage for precision-sensitive scenarios
   - **Files to check:** Search for `TimeSpan.FromMinutes` and `TimeSpan.FromSeconds`

2. **ASP.NET Core Identity/Diagnostics APIs** (3 occurrences)
   - **Change:** APIs may have moved to different assemblies/namespaces
   - **Impact:** May require additional using statements or assembly references
   - **Action:** Verify compilation succeeds; add missing references if needed

**Behavioral Changes (27 occurrences) - Runtime validation required:**

1. **HttpContent** (14 occurrences)
   - **Change:** Enhanced validation and behavior refinements
   - **Impact:** Stricter content validation, potential exceptions in edge cases
   - **Action:** Test all HTTP API calls thoroughly
   - **Files to check:** Components making HTTP requests (API clients, services)

2. **Uri** (12 occurrences)
   - **Change:** Stricter URI validation and parsing
   - **Impact:** Invalid URIs may throw exceptions earlier in construction
   - **Action:** Validate all URI construction, especially dynamic URLs
   - **Files to check:** Navigation logic, API endpoint construction

3. **UseExceptionHandler** (1 occurrence)
   - **Change:** Error handling middleware behavior refinements
   - **Impact:** Exception handling flow may differ slightly
   - **Action:** Test error scenarios to ensure proper handling

##### 5. Code Modifications

**Estimated Changes:** Minimal to None

**Areas Requiring Review:**

1. **HTTP Client Usage** (Priority: High)
   - **Location:** Services and components making API calls
   - **Review:** All HttpClient usages, especially custom content types
   - **Validation:** Test all API interactions

2. **Navigation Logic** (Priority: Medium)
   - **Location:** Components with NavigationManager usage
   - **Review:** All dynamic URL construction using Uri class
   - **Validation:** Test navigation flows, deep linking

3. **TimeSpan Usage** (Priority: Low)
   - **Location:** Timer components, scheduling logic
   - **Review:** Precision-sensitive duration calculations
   - **Validation:** Test timing-dependent features

4. **Authentication Flows** (Priority: High)
   - **Location:** Login, authorization components
   - **Review:** WebAssembly authentication patterns
   - **Validation:** Complete authentication/authorization test pass

##### 6. Testing Strategy

**Unit Tests:**
- No test project identified for Client
- **Recommendation:** Manual component testing required

**Integration Tests:**
- Test authentication flows end-to-end
- Validate API communication with Server

**Manual Tests:**
- [ ] Login/logout flows work correctly
- [ ] All navigation routes function
- [ ] HTTP API calls succeed
- [ ] File download functionality works (BlazorDownloadFile)
- [ ] Charts render correctly (ApexCharts)
- [ ] Grid components display data (SimpleGrid, PivotTable)
- [ ] Code editor functions (AceEditorJs)
- [ ] MudBlazor components render properly

**Performance Tests:**
- Initial load time acceptable
- Client-side rendering performance maintained
- No memory leaks in long-running sessions

##### 7. Validation Checklist

- [ ] Project file updated to net10.0
- [ ] All 3 packages updated to target versions
- [ ] Project restores without errors
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] No package version conflicts
- [ ] Client application loads in browser
- [ ] Authentication works correctly
- [ ] All UI components render
- [ ] API calls to Server succeed
- [ ] Navigation flows work
- [ ] No console errors in browser developer tools

---

### Project 3: Report_App_WASM.Server

**Project Type:** AspNetCore (Hosted Blazor WASM Server, SDK-style)  
**Project Path:** `Report_App_WASM\Server\Report_App_WASM.Server.csproj`

#### Current State

- **Target Framework:** net8.0
- **Dependencies:** 2 projects (Client, Shared)
- **Dependants:** 0 projects (root application)
- **NuGet Packages:** 28
- **Files:** 97 files
- **Lines of Code:** 20,581
- **API Issues:** 51 (3 binary incompatible, 47 source incompatible, 1 behavioral)
- **Risk Level:** Medium

#### Target State

- **Target Framework:** net10.0
- **Package Updates:** 12 packages (+ 2 removals + 1 potential addition)
- **Expected Code Changes:** Moderate (binary incompatible APIs, directory services)

#### Migration Steps

##### 1. Prerequisites

- Report_App_WASM.Client must be updated to net10.0 first (handled in atomic operation)
- Report_App_WASM.Shared must be updated to net10.0 first (handled in atomic operation)
- Verify database connection strings and access (for EF Core updates)

##### 2. Target Framework Update

Update project file `Report_App_WASM\Server\Report_App_WASM.Server.csproj`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

**Required Updates (12 packages):**

| Package Name | Current Version | Target Version | Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.AspNetCore.Components.WebAssembly | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Components.WebAssembly.Server | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.AspNetCore.Identity.UI | 8.0.20 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.9 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.EntityFrameworkCore.Tools | 9.0.9 | 10.0.3 | Framework alignment - required for .NET 10 |
| Microsoft.Extensions.Localization | 9.0.9 | 10.0.3 | Framework alignment - recommended upgrade |
| System.Data.OleDb | 9.0.9 | 10.0.3 | Framework alignment - recommended upgrade |
| System.DirectoryServices.AccountManagement | 9.0.9 | 10.0.3 | Framework alignment - recommended upgrade |
| System.Text.Json | 9.0.9 | 10.0.3 | Framework alignment - recommended upgrade |

**Packages to Remove (2 packages):**

| Package Name | Current Version | Reason for Removal |
| :--- | :---: | :--- |
| Microsoft.NETCore.Platforms | 7.0.4 | Functionality now included in framework reference |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.22.1 | Incompatible with .NET 10 - Visual Studio tooling package, safe to remove |

**Deprecated Package to Address (1 package):**

| Package Name | Current Version | Status | Action |
| :--- | :---: | :--- | :--- |
| Azure.Identity | 1.16.0 | Deprecated | Verify if newer version exists; if not, document deprecation and plan for future replacement |

**Compatible Packages (no update required - 15 packages):**
- AutoMapper 15.0.1
- AutoMapper.Extensions.ExpressionMapping 9.0.1
- Blazor.SimpleGrid 8.0.1
- CsvHelper 33.1.0
- EPPlus 7.7.3
- FluentFTP 53.0.1
- Hangfire.AspNetCore 1.8.21
- Hangfire.SqlServer 1.8.21
- Microsoft.AspNetCore.OData 9.4.0
- MySql.Data 9.4.0
- Newtonsoft.Json 13.0.4
- Npgsql 9.0.3
- Oracle.ManagedDataAccess.Core 23.9.1
- SSH.NET 2025.0.0
- Swashbuckle.AspNetCore 9.0.4
- ZNetCS.AspNetCore.Logging.EntityFrameworkCore 9.0.0

##### 4. Expected Breaking Changes

**🔴 Binary Incompatible (3 occurrences) - CRITICAL:**

1. **OptionsConfigurationServiceCollectionExtensions.Configure<T>** (2 occurrences)
   - **Change:** Method signature changed requiring explicit type argument specification
   - **Impact:** Compilation failures in service configuration
   - **Location:** Startup.cs or Program.cs where configuration is bound to options
   - **Fix:** Update calls to provide explicit type arguments or use updated overload
   - **Example Fix:**
     ```csharp
     // Old pattern (may fail):
     services.Configure<MyOptions>(configuration);

     // New pattern:
     services.Configure<MyOptions>(configuration.GetSection("MyOptions"));
     // OR with explicit binding:
     services.AddOptions<MyOptions>().Bind(configuration.GetSection("MyOptions"));
     ```

2. **ConfigurationBinder.Get<T>** (1 occurrence)
   - **Change:** Method signature changed requiring non-nullable return consideration
   - **Location:** Configuration binding code
   - **Fix:** Handle potential null returns or use required overload
   - **Example Fix:**
     ```csharp
     // Old pattern:
     var options = configuration.Get<MyOptions>();

     // New pattern (handle null):
     var options = configuration.Get<MyOptions>() ?? new MyOptions();
     // OR use GetValue with default:
     var options = configuration.GetSection("MyOptions").Get<MyOptions>() ?? throw new InvalidOperationException("Missing configuration");
     ```

**🟡 Source Incompatible (47 occurrences) - May cause compilation errors:**

1. **Directory Services APIs** (13 occurrences) - 25.5% of issues
   - **APIs Affected:**
     - `System.DirectoryServices.AccountManagement.UserPrincipal`
     - `System.DirectoryServices.AccountManagement.PrincipalContext`
     - `System.DirectoryServices.AccountManagement.IdentityType`
     - `System.DirectoryServices.AccountManagement.ContextType`
   - **Change:** APIs may have namespace/assembly reorganization
   - **Impact:** Potential missing namespace or assembly reference errors
   - **Files to check:** Authentication/authorization services using Active Directory/LDAP
   - **Fix:**
     - Verify `System.DirectoryServices.AccountManagement` package is properly referenced
     - Package update from 9.0.9 → 10.0.3 should resolve
     - Add explicit using statements if needed
   - **Migration Path:** The core functionality has been moved to separate packages. Install System.DirectoryServices.AccountManagement (user/group management) at version 10.0.3

2. **OleDb APIs** (14 occurrences)
   - **APIs Affected:**
     - `System.Data.OleDb.OleDbConnection`
     - `System.Data.OleDb.OleDbCommand`
     - `System.Data.OleDb.OleDbDataAdapter`
   - **Change:** Platform-specific API compatibility considerations
   - **Impact:** May have platform-specific compilation warnings or limitations
   - **Files to check:** Data access layer using OleDb connections (likely for Excel/Access)
   - **Fix:**
     - Update `System.Data.OleDb` package from 9.0.9 → 10.0.3
     - Verify platform compatibility (Windows-only)
     - Consider alternatives if cross-platform needed (EPPlus already referenced for Excel)

3. **ASP.NET Core Identity APIs** (6 occurrences)
   - **APIs Affected:**
     - `Microsoft.AspNetCore.Builder.MigrationsEndPointExtensions.UseMigrationsEndPoint`
     - `Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions.AddDefaultIdentity`
     - `Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions.AddEntityFrameworkStores`
     - `Microsoft.Extensions.DependencyInjection.DatabaseDeveloperPageExceptionFilterServiceExtensions.AddDatabaseDeveloperPageExceptionFilter`
   - **Change:** APIs reorganized across packages/namespaces in .NET 10
   - **Impact:** Missing namespace/assembly reference errors
   - **Location:** Program.cs, Startup.cs, Identity configuration
   - **Fix:**
     - Update Identity packages (Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.20 → 10.0.3)
     - Update Diagnostics package (Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 8.0.20 → 10.0.3)
     - Update UI package (Microsoft.AspNetCore.Identity.UI 8.0.20 → 10.0.3)
     - Verify all using statements present

4. **TimeSpan Methods** (11 occurrences total across projects)
   - **APIs Affected:**
     - `TimeSpan.FromMinutes(double)`
     - `TimeSpan.FromSeconds(double)`
   - **Change:** Precision behavior refinements
   - **Impact:** Minimal - mostly behavioral
   - **Action:** Review if precision-critical timing logic exists

**🔵 Behavioral Change (1 occurrence):**

1. **UseExceptionHandler** (1 occurrence)
   - **Change:** Error handling middleware behavior refinements
   - **Location:** Program.cs or Startup.cs
   - **Impact:** Low - exception handling flow may differ slightly
   - **Action:** Test error scenarios to ensure proper behavior

##### 5. Code Modifications

**Estimated Changes:** Moderate (primarily in configuration/startup code)

**Priority 1: Binary Incompatible Fixes (Required for Compilation)**

1. **Service Configuration Updates**
   - **Files:** `Program.cs` or service configuration files
   - **Search for:** `services.Configure<`
   - **Action:** Update to explicit type parameter overloads or use `AddOptions<T>().Bind()`
   - **Validation:** Code compiles

2. **Configuration Binding Updates**
   - **Files:** `Program.cs` or configuration initialization
   - **Search for:** `configuration.Get<`
   - **Action:** Add null handling or use throw expressions for required configuration
   - **Validation:** No null reference warnings

**Priority 2: Source Incompatible Fixes (Required if Compilation Fails)**

1. **Directory Services Integration**
   - **Files:** Authentication/authorization services (search for `DirectoryServices` namespace)
   - **Action:** 
     - Ensure `System.DirectoryServices.AccountManagement` 10.0.3 is referenced
     - Verify all using statements present
     - Test AD/LDAP authentication flows
   - **Validation:** Authentication against directory services works

2. **OleDb Data Access**
   - **Files:** Data access layer (search for `OleDbConnection`)
   - **Action:**
     - Ensure `System.Data.OleDb` 10.0.3 is referenced
     - Verify platform compatibility warnings addressed
     - Consider platform-specific build conditions if needed
   - **Validation:** Database queries execute successfully

3. **Identity/Diagnostics Middleware**
   - **Files:** `Program.cs`
   - **Action:**
     - Verify `UseMigrationsEndPoint()` still compiles
     - Verify `AddDefaultIdentity()` still compiles
     - Verify `AddDatabaseDeveloperPageExceptionFilter()` still compiles
   - **Validation:** Application starts without errors

**Priority 3: Behavioral Validation (Runtime Testing)**

1. **HTTP Client Behavior**
   - **Files:** Services making API calls
   - **Action:** Test all API integrations thoroughly
   - **Validation:** All API calls return expected results

2. **Exception Handling**
   - **Files:** `Program.cs` error handling middleware
   - **Action:** Test error scenarios (404, 500, exceptions)
   - **Validation:** Errors handled gracefully

##### 6. Testing Strategy

**Build Validation:**
- Project builds without errors
- Project builds without warnings
- No dependency conflicts
- Entity Framework migrations compile

**Unit Tests:**
- No test project identified for Server
- **Recommendation:** Create comprehensive manual test plan

**Integration Tests:**
- Database connectivity (SQL Server via EF Core)
- External database connections (MySQL, PostgreSQL, Oracle)
- OleDb connections (if applicable)
- Directory Services authentication
- API endpoints (OData, Swagger)
- Hangfire background jobs
- File operations (FTP, SSH)

**Manual Tests:**
- [ ] Application starts successfully
- [ ] Database migrations apply correctly
- [ ] Identity authentication/authorization works
- [ ] API endpoints respond correctly
- [ ] OData queries function
- [ ] Background jobs execute (Hangfire)
- [ ] File upload/download operations work
- [ ] LDAP/AD authentication works (if enabled)
- [ ] Excel export works (EPPlus)
- [ ] CSV export works (CsvHelper)
- [ ] All database providers connect (SQL, MySQL, PostgreSQL, Oracle)

**Performance Tests:**
- API response times acceptable
- Database query performance maintained
- Background job execution performance
- Memory usage patterns normal

##### 7. Validation Checklist

- [ ] Project file updated to net10.0
- [ ] All 12 packages updated to target versions
- [ ] Microsoft.NETCore.Platforms removed
- [ ] Microsoft.VisualStudio.Azure.Containers.Tools.Targets removed
- [ ] Azure.Identity deprecation documented (or updated if new version available)
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] No package version conflicts
- [ ] Entity Framework migrations compile
- [ ] Application starts without errors
- [ ] Database connectivity verified
- [ ] Identity system functional
- [ ] All API endpoints accessible
- [ ] Background jobs operational
- [ ] No runtime exceptions in logs

---

## Package Update Reference

### Overview

**Total Packages:** 39  
**Updates Required:** 14 actions (12 upgrades + 2 removals)  
**Compatible (no action):** 25 packages

### Common Package Updates (Affecting Multiple Projects)

| Package | Current | Target | Projects Affected | Update Reason |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.AspNetCore.Components.WebAssembly | 8.0.20 | 10.0.3 | Client, Server | Framework alignment - required |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 8.0.20 | 10.0.3 | Client, Server | Framework alignment - required |
| Microsoft.Extensions.Localization | 9.0.9 | 10.0.3 | Client, Server | Framework alignment - recommended |
| Blazor.SimpleGrid | 8.0.1 | *(no update)* | Client, Server | Already compatible |

### Server-Only Package Updates

| Package | Current | Target | Update Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.AspNetCore.Components.WebAssembly.Server | 8.0.20 | 10.0.3 | Framework alignment - required |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.20 | 10.0.3 | Framework alignment - required |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.20 | 10.0.3 | Framework alignment - required |
| Microsoft.AspNetCore.Identity.UI | 8.0.20 | 10.0.3 | Framework alignment - required |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.9 | 10.0.3 | Framework alignment - required |
| Microsoft.EntityFrameworkCore.Tools | 9.0.9 | 10.0.3 | Framework alignment - required |
| System.Data.OleDb | 9.0.9 | 10.0.3 | Framework alignment - recommended |
| System.DirectoryServices.AccountManagement | 9.0.9 | 10.0.3 | Framework alignment - recommended |
| System.Text.Json | 9.0.9 | 10.0.3 | Framework alignment - recommended |

### Packages to Remove

| Package | Current | Project | Reason |
| :--- | :---: | :--- | :--- |
| Microsoft.NETCore.Platforms | 7.0.4 | Server | Functionality included with framework reference in .NET 10 |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.22.1 | Server | Incompatible with .NET 10; Visual Studio tooling package |

**Action:** Remove `<PackageReference>` elements from Server project file.

### Deprecated Packages

| Package | Current | Project | Status | Action Required |
| :--- | :---: | :--- | :--- | :--- |
| Azure.Identity | 1.16.0 | Server | Deprecated | Investigate if newer version available; if yes, update; if no, document for future replacement |

**Investigation Steps:**
1. Check NuGet.org for Azure.Identity versions compatible with .NET 10
2. If version 1.17+ available, update to latest
3. If no compatible version, document deprecation and plan migration to alternative Azure authentication library
4. Test Azure authentication flows after decision

### Client-Only Compatible Packages (No Updates)

All packages already compatible with .NET 10:
- Blazor.AceEditorJs 1.2.1
- Blazor.PivotTable 1.0.8
- Blazor-ApexCharts 6.0.2
- BlazorDownloadFile 2.4.0.2
- CodeBeam.MudBlazor.Extensions 8.2.4
- CronExpressionDescriptor 2.44.0
- FluentValidation 12.0.0
- MudBlazor 8.12.0

### Server-Only Compatible Packages (No Updates)

All packages already compatible with .NET 10:
- AutoMapper 15.0.1
- AutoMapper.Extensions.ExpressionMapping 9.0.1
- CsvHelper 33.1.0
- EPPlus 7.7.3
- FluentFTP 53.0.1
- Hangfire.AspNetCore 1.8.21
- Hangfire.SqlServer 1.8.21
- Microsoft.AspNetCore.OData 9.4.0
- MySql.Data 9.4.0
- Newtonsoft.Json 13.0.4
- Npgsql 9.0.3
- Oracle.ManagedDataAccess.Core 23.9.1
- SSH.NET 2025.0.0
- Swashbuckle.AspNetCore 9.0.4
- ZNetCS.AspNetCore.Logging.EntityFrameworkCore 9.0.0

### Update Execution Order

**All-At-Once Approach:** All package updates occur simultaneously within the atomic upgrade operation. However, for troubleshooting purposes, the logical order is:

1. **Remove incompatible packages first** (prevents conflicts during restore)
   - Microsoft.NETCore.Platforms
   - Microsoft.VisualStudio.Azure.Containers.Tools.Targets

2. **Update framework-aligned packages** (core dependencies)
   - ASP.NET Core packages (8.0.20 → 10.0.3)
   - Entity Framework Core packages (9.0.9 → 10.0.3)

3. **Update extension packages** (dependent on framework packages)
   - System.Data.OleDb (9.0.9 → 10.0.3)
   - System.DirectoryServices.AccountManagement (9.0.9 → 10.0.3)
   - System.Text.Json (9.0.9 → 10.0.3)

4. **Address deprecated packages** (after core updates work)
   - Azure.Identity investigation and resolution

### Package Update Commands

**Automated Approach (recommended):**

All updates can be performed by editing project files directly, then running:
```bash
dotnet restore
dotnet build
```

**Individual Package Update Commands (reference):**

```bash
# Server project updates
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Components.WebAssembly -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Components.WebAssembly.Authentication -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Components.WebAssembly.Server -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.AspNetCore.Identity.UI -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.EntityFrameworkCore.SqlServer -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.EntityFrameworkCore.Tools -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.Extensions.Localization -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package System.Data.OleDb -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package System.DirectoryServices.AccountManagement -v 10.0.3
dotnet add Report_App_WASM\Server\Report_App_WASM.Server.csproj package System.Text.Json -v 10.0.3

# Client project updates
dotnet add Report_App_WASM\Client\Report_App_WASM.Client.csproj package Microsoft.AspNetCore.Components.WebAssembly -v 10.0.3
dotnet add Report_App_WASM\Client\Report_App_WASM.Client.csproj package Microsoft.AspNetCore.Components.WebAssembly.Authentication -v 10.0.3
dotnet add Report_App_WASM\Client\Report_App_WASM.Client.csproj package Microsoft.Extensions.Localization -v 10.0.3

# Remove incompatible packages from Server
dotnet remove Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.NETCore.Platforms
dotnet remove Report_App_WASM\Server\Report_App_WASM.Server.csproj package Microsoft.VisualStudio.Azure.Containers.Tools.Targets
```

---

## Breaking Changes Catalog

### Overview

This section consolidates all breaking changes identified across the solution, organized by severity and category.

### 🔴 Critical: Binary Incompatible APIs (Must Fix)

**These changes WILL cause compilation failures and must be addressed.**

#### 1. OptionsConfigurationServiceCollectionExtensions.Configure<T> (2 occurrences)

**Location:** `Report_App_WASM.Server` - Configuration initialization (likely Program.cs)

**Change Description:**
The `Configure<T>(IServiceCollection, IConfiguration)` extension method signature has changed in .NET 10, requiring explicit handling of configuration binding.

**Current Pattern (may fail):**
```csharp
services.Configure<MyOptions>(configuration);
```

**Updated Patterns:**
```csharp
// Option 1: Use GetSection for explicit section binding
services.Configure<MyOptions>(configuration.GetSection("MyOptions"));

// Option 2: Use AddOptions pattern for more control
services.AddOptions<MyOptions>()
    .Bind(configuration.GetSection("MyOptions"))
    .ValidateDataAnnotations();
```

**Files to Search:** `Program.cs`, any files with `services.Configure<`

**Validation:** Code compiles without errors related to Configure method overload resolution.

---

#### 2. ConfigurationBinder.Get<T> (1 occurrence)

**Location:** `Report_App_WASM.Server` - Configuration retrieval

**Change Description:**
The `Get<T>(IConfiguration)` method now requires explicit null handling due to nullability annotation changes.

**Current Pattern (may fail):**
```csharp
var options = configuration.Get<MyOptions>();
```

**Updated Patterns:**
```csharp
// Option 1: Provide default
var options = configuration.Get<MyOptions>() ?? new MyOptions();

// Option 2: Throw if required
var options = configuration.Get<MyOptions>() 
    ?? throw new InvalidOperationException("MyOptions configuration is required");

// Option 3: Use GetValue with default for simple types
var value = configuration.GetValue<string>("Key", "DefaultValue");

// Option 4: Use GetSection for complex types
var options = configuration.GetSection("MyOptions").Get<MyOptions>();
```

**Files to Search:** `Program.cs`, any files with `configuration.Get<`

**Validation:** No null reference warnings or runtime null exceptions.

---

### 🟡 Important: Source Incompatible APIs (May Cause Compilation Errors)

**These changes MAY cause compilation failures depending on usage patterns.**

#### 1. Directory Services APIs (13 occurrences - 25.5% of issues)

**Location:** `Report_App_WASM.Server` - Authentication/authorization services

**APIs Affected:**
- `System.DirectoryServices.AccountManagement.UserPrincipal` (2 occurrences)
- `System.DirectoryServices.AccountManagement.PrincipalContext` (1 occurrence)
- `System.DirectoryServices.AccountManagement.IdentityType` (2 occurrences)
- `System.DirectoryServices.AccountManagement.ContextType` (2 occurrences)
- `UserPrincipal.EmailAddress` property (2 occurrences)
- `UserPrincipal.FindByIdentity` method (1 occurrence)
- `PrincipalContext` constructor (1 occurrence)
- `IdentityType.SamAccountName` field (1 occurrence)
- `ContextType.Domain` field (1 occurrence)

**Change Description:**
Directory Services APIs have been moved to separate NuGet packages in .NET Core/.NET. The functionality is available but requires explicit package reference.

**Required Action:**
1. Ensure `System.DirectoryServices.AccountManagement` package version 10.0.3 is referenced in Server project
2. Verify all using statements are present:
   ```csharp
   using System.DirectoryServices.AccountManagement;
   ```

**Files to Search:** 
- Files with `using System.DirectoryServices`
- Files with `PrincipalContext`, `UserPrincipal`, `IdentityType`, `ContextType`
- Authentication/authorization service files

**Example Usage Pattern:**
```csharp
using System.DirectoryServices.AccountManagement;

// Typical AD authentication pattern
using (var context = new PrincipalContext(ContextType.Domain, "DOMAIN", "DC=domain,DC=com", "username", "password"))
{
    var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "username");
    if (user != null)
    {
        var email = user.EmailAddress;
        // ... authentication logic
    }
}
```

**Validation:** 
- Code compiles without missing namespace errors
- AD/LDAP authentication flows work correctly in runtime testing

---

#### 2. OleDb APIs (14 occurrences)

**Location:** `Report_App_WASM.Server` - Data access layer

**APIs Affected:**
- `System.Data.OleDb.OleDbConnection` (4 occurrences)
- `System.Data.OleDb.OleDbCommand` (3 occurrences)
- `System.Data.OleDb.OleDbDataAdapter` (1 occurrence)
- Various properties/methods (CommandText, CommandType, CommandTimeout, Cancel, etc.)

**Change Description:**
OleDb APIs are platform-specific (Windows-only) and may have compatibility annotations or platform-specific warnings in .NET 10.

**Required Action:**
1. Ensure `System.Data.OleDb` package version 10.0.3 is referenced
2. Verify platform compatibility requirements (Windows-only)
3. Consider adding platform-specific build conditions if cross-platform support needed

**Files to Search:**
- Files with `using System.Data.OleDb`
- Files with `OleDbConnection`, `OleDbCommand`, `OleDbDataAdapter`
- Data access services for Excel/Access databases

**Example Usage Pattern:**
```csharp
using System.Data.OleDb;

// Typical OleDb pattern (Windows-only)
using (var connection = new OleDbConnection(connectionString))
{
    connection.Open();
    using (var command = new OleDbCommand("SELECT * FROM Table", connection))
    {
        command.CommandTimeout = 30;
        command.CommandType = CommandType.Text;
        // ... execute query
    }
}
```

**Validation:**
- Code compiles (may have platform-specific warnings)
- Database queries execute successfully on Windows
- Consider EPPlus (already referenced at 7.7.3) as cross-platform Excel alternative

---

#### 3. ASP.NET Core Identity APIs (6 occurrences)

**Location:** `Report_App_WASM.Server` - Identity configuration (Program.cs)

**APIs Affected:**
- `MigrationsEndPointExtensions.UseMigrationsEndPoint` (1 occurrence)
- `IdentityServiceCollectionUIExtensions.AddDefaultIdentity` (1 occurrence)
- `IdentityEntityFrameworkBuilderExtensions.AddEntityFrameworkStores` (1 occurrence)
- `DatabaseDeveloperPageExceptionFilterServiceExtensions.AddDatabaseDeveloperPageExceptionFilter` (1 occurrence)

**Change Description:**
Identity and diagnostics extension methods have been reorganized across assemblies in .NET 10. Package updates should resolve, but explicit using statements may be required.

**Required Action:**
1. Update all Identity packages to 10.0.3 (listed in Package Update Reference)
2. Verify using statements:
   ```csharp
   using Microsoft.AspNetCore.Builder;
   using Microsoft.AspNetCore.Identity;
   using Microsoft.Extensions.DependencyInjection;
   ```

**Files to Search:**
- `Program.cs`
- `Startup.cs` (if exists)
- Identity configuration files

**Example Usage Pattern:**
```csharp
// Service registration (typically in Program.cs)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Middleware registration (typically in Program.cs)
app.UseMigrationsEndPoint();
```

**Validation:**
- Code compiles without missing type/method errors
- Application starts successfully
- Identity registration completes without errors

---

#### 4. TimeSpan Factory Methods (11 occurrences)

**Location:** `Report_App_WASM.Server` and potentially Client

**APIs Affected:**
- `TimeSpan.FromMinutes(double)` (9 occurrences)
- `TimeSpan.FromSeconds(double)` (2 occurrences)

**Change Description:**
Precision and rounding behavior refinements in TimeSpan calculation methods.

**Impact:** Low - Mostly behavioral, but flagged as source incompatible due to potential precision differences.

**Required Action:**
1. Code will likely compile without changes
2. Review precision-sensitive scenarios (timers, scheduling, rate limiting)
3. Test timing-dependent functionality

**Files to Search:**
- Files with `TimeSpan.From`
- Scheduling/timer logic
- Rate limiting implementations
- Background job scheduling (Hangfire configuration)

**Validation:**
- Code compiles
- Timing-dependent features work correctly
- No off-by-millisecond errors in scheduling

---

### 🔵 Monitor: Behavioral Changes (Runtime Validation Required)

**These changes will NOT cause compilation failures but may affect runtime behavior.**

#### 1. HttpContent (14 occurrences - 16.9% of issues)

**Location:** `Report_App_WASM.Client` - HTTP client services

**Change Description:**
Enhanced validation and behavior refinements in HttpContent handling in .NET 10.

**Impact:** 
- Stricter content validation
- Potential exceptions in edge cases (malformed content, encoding issues)
- More consistent behavior across platforms

**Areas to Test:**
- All API calls from Client to Server
- Content serialization/deserialization
- File upload/download operations (BlazorDownloadFile)
- Custom content types

**Files to Search:**
- Services making HTTP calls
- API client classes
- File upload/download components

**Validation Testing:**
- All HTTP API calls succeed
- POST/PUT operations with request bodies work
- File operations complete successfully
- No unexpected exceptions in network layer

---

#### 2. Uri (12 occurrences - 14.5% of issues)

**Location:** `Report_App_WASM.Client` - Navigation and API endpoint construction

**Change Description:**
Stricter URI validation and parsing in .NET 10. Invalid URIs may throw exceptions earlier in construction phase.

**Impact:**
- More rigorous format validation
- Earlier exception throwing for malformed URIs
- More consistent parsing behavior

**Areas to Test:**
- Navigation logic (NavigationManager)
- Dynamic URL construction
- API endpoint building
- Query string manipulation

**Files to Search:**
- Components with NavigationManager
- URI construction code
- Redirect logic
- External link handling

**Validation Testing:**
- All navigation routes work
- Deep linking functions correctly
- Query parameters parsed properly
- External links open correctly
- No URI format exceptions

---

### Breaking Changes by Project

#### Report_App_WASM.Shared
- **Binary Incompatible:** 0
- **Source Incompatible:** 0
- **Behavioral:** 0
- **Total Issues:** 0
- **Assessment:** No breaking changes identified ✅

#### Report_App_WASM.Client
- **Binary Incompatible:** 0
- **Source Incompatible:** 5 (TimeSpan: 2, Identity APIs: 3)
- **Behavioral:** 27 (HttpContent: 14, Uri: 12, UseExceptionHandler: 1)
- **Total Issues:** 32
- **Assessment:** Primarily behavioral changes requiring runtime validation

#### Report_App_WASM.Server
- **Binary Incompatible:** 3 (Configure<T>: 2, Get<T>: 1) ⚠️ CRITICAL
- **Source Incompatible:** 47 (Directory Services: 13, OleDb: 14, Identity: 6, TimeSpan: 9, others: 5)
- **Behavioral:** 1 (UseExceptionHandler: 1)
- **Total Issues:** 51
- **Assessment:** Most complex project with critical compilation blockers

### Breaking Changes Resolution Priority

**Priority 1 (Compilation Blockers):**
1. Fix binary incompatible APIs in Server (3 occurrences)
2. Verify Directory Services package reference and compilation
3. Verify OleDb package reference and compilation
4. Verify Identity APIs compilation

**Priority 2 (Behavioral Validation):**
1. Test HttpContent usage in Client (14 occurrences)
2. Test Uri usage in Client (12 occurrences)
3. Test exception handling middleware

**Priority 3 (Low Impact):**
1. Review TimeSpan precision in scheduling logic
2. Validate timing-dependent features

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

Since this is an All-At-Once migration, testing occurs after the atomic upgrade completes. The testing strategy follows a bottom-up validation approach aligned with project dependencies.

---

### Level 1: Build Validation (Immediate - Post-Upgrade)

**Objective:** Confirm entire solution compiles successfully.

**Validation Steps:**
1. **Restore Dependencies**
   ```bash
   dotnet restore Report_App_WASM.sln
   ```
   - ✅ Success criteria: No restore errors, all packages resolved

2. **Build Entire Solution**
   ```bash
   dotnet build Report_App_WASM.sln --no-restore
   ```
   - ✅ Success criteria: 0 errors, 0 warnings
   - ⚠️ If warnings present: Document and assess impact

3. **Individual Project Validation** (for troubleshooting if solution build fails)
   ```bash
   dotnet build Report_App_WASM\Shared\Report_App_WASM.Shared.csproj
   dotnet build Report_App_WASM\Client\Report_App_WASM.Client.csproj
   dotnet build Report_App_WASM\Server\Report_App_WASM.Server.csproj
   ```

**Exit Criteria:** Solution builds successfully with 0 errors.

---

### Level 2: Smoke Tests (Quick Validation)

**Objective:** Verify application launches and core functionality works.

**Server Smoke Tests:**
- [ ] Application starts without exceptions
- [ ] Swagger UI loads (`/swagger`)
- [ ] Health check endpoint responds (if configured)
- [ ] Database connection established (EF Core context initialization)
- [ ] Hangfire dashboard loads (if configured)
- [ ] No startup errors in console/logs

**Client Smoke Tests:**
- [ ] Client application loads in browser
- [ ] No console errors in browser developer tools
- [ ] Initial page renders correctly
- [ ] Static assets load (CSS, JS, images)

**Integration Smoke Tests:**
- [ ] Client successfully calls Server API endpoint
- [ ] Authentication challenge works (redirect to login)
- [ ] Basic navigation between pages functions

**Exit Criteria:** Application is functionally alive with no immediate failures.

---

### Level 3: Functional Testing (Comprehensive)

**Objective:** Validate all features work correctly with .NET 10.

#### Authentication & Authorization Testing

**Critical Path (High Priority):**
- [ ] User registration works
- [ ] User login succeeds
- [ ] JWT token issuance and validation
- [ ] Logout clears authentication state
- [ ] Authorized-only pages protect correctly
- [ ] Role-based authorization enforces correctly
- [ ] LDAP/AD authentication works (if configured - 13 Directory Services API usages)
- [ ] Identity database tables accessible

**Files to Test:** 
- `LoginDisplay.razor` (referenced in context)
- Authentication services
- Authorization policies

#### API & Data Access Testing

**Database Operations (Critical):**
- [ ] SQL Server connectivity (EF Core)
- [ ] MySQL connectivity (MySql.Data)
- [ ] PostgreSQL connectivity (Npgsql)
- [ ] Oracle connectivity (Oracle.ManagedDataAccess.Core)
- [ ] OleDb connections work (if applicable - 14 API occurrences)
- [ ] Entity Framework queries execute
- [ ] Database writes succeed
- [ ] Migrations apply correctly

**API Endpoints (High Priority):**
- [ ] OData queries return results (Microsoft.AspNetCore.OData)
- [ ] REST API endpoints respond
- [ ] API authentication/authorization works
- [ ] Swagger documentation accurate

#### UI Component Testing (Client)

**MudBlazor Components:**
- [ ] MudMenu displays correctly (LoginDisplay.razor uses MudMenu)
- [ ] MudAvatar renders
- [ ] MudButton interactions work
- [ ] MudCard layouts correct
- [ ] All MudBlazor components render properly

**Blazor Extensions:**
- [ ] AceEditor loads and functions (Blazor.AceEditorJs)
- [ ] PivotTable renders data (Blazor.PivotTable)
- [ ] ApexCharts display correctly (Blazor-ApexCharts)
- [ ] File download works (BlazorDownloadFile)
- [ ] SimpleGrid displays data (Blazor.SimpleGrid)
- [ ] MudBlazor extensions work (CodeBeam.MudBlazor.Extensions)

**Navigation & Routing:**
- [ ] All routes navigate correctly
- [ ] Deep linking works
- [ ] Back/forward browser buttons function
- [ ] Query parameters preserved

#### Background Services Testing

**Hangfire Jobs:**
- [ ] Hangfire dashboard accessible
- [ ] Scheduled jobs execute
- [ ] Background tasks complete
- [ ] Job persistence works (SQL Server storage)

#### File Operations Testing

**FTP/SSH Operations:**
- [ ] FTP uploads work (FluentFTP)
- [ ] FTP downloads work
- [ ] SSH connections establish (SSH.NET)
- [ ] File transfer operations complete

**Export Functionality:**
- [ ] Excel export works (EPPlus)
- [ ] CSV export works (CsvHelper)
- [ ] File downloads from Client work

---

### Level 4: Behavioral Change Validation (Specific Focus Areas)

**Objective:** Validate the 28 identified behavioral changes don't break functionality.

#### HttpContent Validation (14 occurrences - Priority: High)

**Test Scenarios:**
- [ ] POST requests with JSON body
- [ ] PUT requests with JSON body
- [ ] File upload with multipart/form-data
- [ ] Custom content types
- [ ] Large request payloads
- [ ] Content encoding variations (UTF-8, etc.)
- [ ] Empty/null content handling

**Validation Method:**
- Monitor browser developer tools network tab
- Check for unexpected 400/415 status codes
- Verify request bodies serialize correctly
- Test edge cases (empty bodies, special characters)

#### Uri Validation (12 occurrences - Priority: High)

**Test Scenarios:**
- [ ] Navigation to all application routes
- [ ] Query string parameters in URLs
- [ ] Special characters in URLs (encoding)
- [ ] Dynamic URL construction
- [ ] External URL links (GitHub link in LoginDisplay.razor)
- [ ] Relative vs absolute URL handling
- [ ] URL encoding/decoding

**Validation Method:**
- Test all navigation flows
- Verify no URI format exceptions in console
- Check query parameter parsing
- Test deep linking scenarios

#### Exception Handling Validation (1 occurrence - Priority: Medium)

**Test Scenarios:**
- [ ] 404 Not Found pages display correctly
- [ ] 500 Internal Server Error handling
- [ ] Unhandled exception middleware catches errors
- [ ] Error page rendering (/Error route)
- [ ] Development vs production error details

**Validation Method:**
- Trigger various error conditions intentionally
- Verify error pages render
- Check logging captures exceptions
- Ensure no sensitive data leaks in production errors

---

### Level 5: Performance & Stability Testing (Optional but Recommended)

**Objective:** Ensure .NET 10 doesn't introduce performance regressions.

**Performance Benchmarks:**
- [ ] Application startup time (cold start)
- [ ] Initial page load time (Client WASM download/initialization)
- [ ] API response times (baseline vs .NET 10)
- [ ] Database query performance
- [ ] Memory usage patterns (no leaks)
- [ ] CPU utilization (no spikes)

**Stability Tests:**
- [ ] Application runs for extended period without crashes
- [ ] No memory leaks in long-running sessions
- [ ] Background jobs continue executing
- [ ] Hangfire stability over time

**Tools:**
- Browser developer tools (Network, Performance tabs)
- dotnet-counters (memory, CPU monitoring)
- Application Insights (if configured)
- SQL Server profiler (database performance)

---

### Test Execution Order

**Sequential Testing (Dependency Order):**

1. **Shared Project Validation** (5 minutes)
   - Build validation only (no runnable tests)
   - Verify compatibility with dependent projects

2. **Client Project Validation** (15-20 minutes)
   - Build validation
   - UI component smoke tests
   - Basic navigation validation

3. **Server Project Validation** (30-40 minutes)
   - Build validation
   - Server startup and smoke tests
   - Database connectivity
   - API endpoints

4. **Full Integration Testing** (45-60 minutes)
   - End-to-end user scenarios
   - Authentication flows
   - Data CRUD operations
   - Background jobs
   - File operations

5. **Behavioral Change Validation** (20-30 minutes)
   - HttpContent scenarios
   - Uri scenarios
   - Exception handling scenarios

6. **Performance Validation** (Optional - 30 minutes)
   - Baseline comparisons
   - Stability testing

**Total Estimated Testing Duration:** 2-3 hours (excluding optional performance testing)

**Note:** Duration is relative estimate for planning purposes. Actual time depends on tester familiarity and environment.

---

### Test Data Requirements

**Database:**
- Test database with sample data
- User accounts for authentication testing
- Representative data volumes for performance testing

**External Services:**
- LDAP/AD test environment (if Directory Services used)
- FTP/SSH test servers (if file operations used)
- Azure services (if Azure.Identity used)

**Browsers:**
- Modern browser for Client testing (Chrome, Edge, Firefox)
- Browser developer tools enabled

---

### Defect Management

**If Tests Fail:**

1. **Categorize Failure:**
   - Build failure → Review breaking changes catalog, fix compilation errors
   - Runtime exception → Review behavioral changes, add error handling
   - Functional failure → Review API changes, adjust implementation
   - Performance regression → Profile and optimize

2. **Document Issue:**
   - Exact error message
   - Steps to reproduce
   - Expected vs actual behavior
   - Affected component/API

3. **Resolution Path:**
   - Search for known .NET 10 migration issues
   - Consult breaking changes documentation
   - Implement fix and retest
   - If blocked, escalate or consider rollback

4. **Regression Testing:**
   - After fix applied, rerun affected test area
   - Verify fix doesn't introduce new issues
   - Run smoke tests to ensure no collateral damage

---

### Success Criteria for Testing Phase

**Mandatory (Must Pass):**
- ✅ Solution builds with 0 errors
- ✅ Solution builds with 0 warnings (or all warnings documented and accepted)
- ✅ Application starts without exceptions
- ✅ Core user flows complete successfully (login, navigation, data access)
- ✅ No security vulnerabilities introduced
- ✅ No package dependency conflicts

**Recommended (Should Pass):**
- ✅ All functional tests pass
- ✅ Performance within acceptable range (< 10% regression)
- ✅ All UI components render correctly
- ✅ Background jobs execute successfully

**Optional (Nice to Have):**
- ✅ Performance improvements observed
- ✅ Code quality metrics improved
- ✅ Technical debt reduced

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation Strategy |
| :--- | :---: | :--- | :--- |
| Report_App_WASM.Server | Medium | 3 binary incompatible APIs in dependency injection configuration | Review all `services.Configure<T>()` and `configuration.Get<T>()` calls; update to use required overloads with generic type inference fixes |
| Report_App_WASM.Server | Medium | 13 Directory Services API usages may have changed namespaces/packages | Verify `System.DirectoryServices.AccountManagement` package compatibility; may need explicit package reference |
| Report_App_WASM.Server | Low | Azure.Identity package deprecated (1.16.0) | Replace with current Azure.Identity package or update to latest version if still supported |
| Report_App_WASM.Server | Low | Microsoft.VisualStudio.Azure.Containers.Tools.Targets incompatible | Remove package reference; functionality may be integrated or no longer needed |
| Report_App_WASM.Client | Low | 27 behavioral changes in HttpContent/Uri usage | Comprehensive testing required to validate behavior remains correct |
| All Projects | Low | All-At-Once strategy increases initial testing surface | Comprehensive solution-wide test pass required before completion |

### Security Vulnerabilities

**Status:** ✅ No security vulnerabilities detected

All packages are free from known CVEs. No immediate security-driven updates required.

### Contingency Plans

**If Binary Incompatible APIs Block Compilation:**
- **Alternative 1:** Research updated API patterns in .NET 10 documentation
- **Alternative 2:** Use backward-compatible overloads if available
- **Alternative 3:** Refactor configuration initialization to use new patterns
- **Rollback Trigger:** If more than 3 binary incompatible issues beyond those identified

**If Directory Services APIs Fail:**
- **Alternative 1:** Explicitly add `System.DirectoryServices.AccountManagement` NuGet package
- **Alternative 2:** Update to platform-specific APIs if cross-platform compatibility not needed
- **Alternative 3:** Abstract directory service calls behind interface for easier testing/replacement
- **Rollback Trigger:** Fundamental incompatibility with .NET 10 platform

**If Package Conflicts Arise:**
- **Alternative 1:** Use `dotnet list package --include-transitive` to identify conflict sources
- **Alternative 2:** Explicitly pin conflicting package versions
- **Alternative 3:** Remove indirect package references if functionality now in framework
- **Rollback Trigger:** Unresolvable dependency graph conflicts

**If Performance Degrades:**
- **Alternative 1:** Profile application to identify regression sources
- **Alternative 2:** Review behavioral changes in frequently-called APIs
- **Alternative 3:** Adjust code to align with .NET 10 performance best practices
- **Rollback Trigger:** >20% performance degradation in critical paths

**If Behavioral Changes Break Functionality:**
- **Alternative 1:** Review `HttpContent` and `Uri` usage patterns, add explicit validation
- **Alternative 2:** Add unit tests for changed behaviors before upgrading
- **Alternative 3:** Implement compatibility shims for critical behaviors
- **Rollback Trigger:** Critical business logic failures without clear fix path

### Rollback Strategy

**Pre-Upgrade Safety:**
- All work occurs on dedicated branch: `upgrade-to-NET10`
- Source branch (`net10`) remains untouched
- Can abandon branch and restart if needed

**Rollback Steps:**
1. Document specific issues encountered
2. Switch back to source branch: `git checkout net10`
3. Delete upgrade branch: `git branch -D upgrade-to-NET10`
4. Analyze blockers and determine alternative approach
5. Create new upgrade attempt branch when ready

**Partial Rollback Not Applicable:**
- All-At-Once strategy means no partial state to preserve
- Either complete upgrade succeeds or entire operation rolls back

---

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Dependencies | Risk | Key Factors |
| :--- | :---: | :---: | :---: | :--- |
| Report_App_WASM.Shared | **Low** | 0 projects | Low | Leaf node, no API issues, no package updates, minimal code changes |
| Report_App_WASM.Client | **Low** | 1 project | Low | 32 API issues (mostly behavioral), 3 package updates, 967 LOC, UI-focused |
| Report_App_WASM.Server | **Medium** | 2 projects | Medium | 51 API issues (3 binary incompatible), 15 package updates, 20,581 LOC, complex backend |

### Phase Complexity Assessment

**Phase 0: Prerequisites**
- **Complexity:** Low
- **Effort:** Minimal
- **Activities:** Verify .NET 10 SDK installation

**Phase 1: Atomic Upgrade**
- **Complexity:** Medium (entire solution at once)
- **Effort:** Moderate
- **Activities:** Update 3 project files, 14 package references, fix compilation errors
- **Critical Factors:**
  - 3 binary incompatible APIs require immediate attention
  - 52 source incompatible APIs may cause compilation failures
  - Directory Services namespace/package changes
  - ASP.NET Core Identity/Diagnostics API reorganization

**Phase 2: Validation**
- **Complexity:** Medium
- **Effort:** Moderate
- **Activities:** Solution-wide testing, behavioral validation, runtime verification
- **Critical Factors:**
  - 28 behavioral changes require runtime validation
  - HttpContent/Uri behavior changes in frequently-used APIs
  - Authentication/authorization flow validation (Identity changes)
  - Database access validation (OleDb platform compatibility)

### Relative Complexity Ratings

**Low Complexity (1 project):**
- Report_App_WASM.Shared
  - Fewest LOC (1,866)
  - No API compatibility issues
  - No package updates
  - Straightforward TFM change only

**Low-Medium Complexity (1 project):**
- Report_App_WASM.Client
  - Small LOC (967)
  - API issues are mostly behavioral (27/32)
  - Limited package updates (3)
  - UI-focused (easier to test visually)

**Medium Complexity (1 project):**
- Report_App_WASM.Server
  - Largest LOC (20,581 - 87.9% of codebase)
  - 3 binary incompatible APIs (require code changes)
  - Most package updates (15)
  - Complex backend services (Identity, EF Core, multiple databases)
  - Directory Services integration (13 API occurrences)
  - OleDb usage (14 API occurrences) - platform-specific considerations

### Resource Requirements

**Skill Levels Required:**
- **.NET Framework Expertise:** Understanding .NET 8 → .NET 10 breaking changes
- **ASP.NET Core Knowledge:** Identity, middleware, configuration patterns
- **Blazor WebAssembly Familiarity:** Client-side compilation, authentication flows
- **Entity Framework Core:** Package version compatibility, migration considerations
- **Dependency Injection:** Service registration pattern changes (binary incompatible APIs)

**Parallel Work Capacity:**
- **Not Applicable** - All-At-Once strategy means work is sequential by nature
- Single developer can execute full upgrade
- Multiple developers can assist in different areas during Phase 2 testing

**Estimated Relative Effort Distribution:**
- Phase 0 (Prerequisites): 5% - Quick validation
- Phase 1 (Atomic Upgrade): 60% - Bulk of technical work
  - Project/package updates: 10%
  - Compilation error fixes: 35%
  - Rebuild/verification: 15%
- Phase 2 (Validation): 35% - Comprehensive testing
  - Build validation: 5%
  - Functional testing: 20%
  - Behavioral change validation: 10%

**Note:** Percentages represent relative effort distribution, not time estimates. Actual duration depends on team familiarity, environment setup, and unforeseen issues.

---

## Source Control Strategy

### Branching Strategy

**Source Branch:** `net10`  
**Upgrade Branch:** `upgrade-to-NET10` (currently active)  
**Target Merge Branch:** `net10`

**Branch Purpose:**
- All upgrade work occurs on isolated `upgrade-to-NET10` branch
- Source branch `net10` remains stable and untouched
- Allows safe experimentation and easy rollback if needed
- Enables PR-based review before merging

**Branch Protection:**
- Do not merge to source branch until ALL success criteria met
- Keep upgrade branch until production validation complete
- Tag source branch before starting upgrade for easy rollback reference

---

### Commit Strategy

**All-At-Once Approach: Single Commit Preferred**

Given the small solution size and atomic nature of the upgrade, a **single comprehensive commit** is recommended:

**Single Commit Structure:**
```
Upgrade solution to .NET 10.0

- Update all project files from net8.0 to net10.0
- Update 12 NuGet packages to version 10.0.3
- Remove 2 incompatible packages (Microsoft.NETCore.Platforms, VS Container Tools)
- Fix 3 binary incompatible API usages (Configure<T>, Get<T>)
- Fix 52 source incompatible API usages (Directory Services, OleDb, Identity)
- Address Azure.Identity deprecation
- Verify all builds and tests pass

Projects updated:
- Report_App_WASM.Shared
- Report_App_WASM.Client  
- Report_App_WASM.Server

Breaking changes addressed:
- Dependency injection configuration patterns
- Directory Services namespace updates
- Identity/diagnostics API reorganization

Resolves: [Issue/ticket number if applicable]
```

**Rationale for Single Commit:**
- Small solution (3 projects, 23k LOC)
- Changes are tightly coupled (framework + packages + fixes are atomic)
- No value in intermediate non-compiling states
- Easier to review as single cohesive changeset
- Simpler to revert if needed (one commit vs multiple)
- Clear atomic boundary for PR review

**Alternative: Two-Commit Approach (if issues encountered):**

If compilation errors are extensive or fixes are complex, split into:

1. **Commit 1: Framework and Package Updates**
   ```
   Update framework to .NET 10.0 and upgrade packages

   - Update all project TFMs to net10.0
   - Update 12 packages to version 10.0.3
   - Remove 2 incompatible packages
   ```

2. **Commit 2: Breaking Change Fixes**
   ```
   Fix compilation errors for .NET 10.0 compatibility

   - Fix binary incompatible APIs (Configure<T>, Get<T>)
   - Fix source incompatible APIs (Directory Services, Identity, OleDb)
   - Address behavioral change considerations
   - All builds pass with 0 errors
   ```

**Choose single commit unless complexity requires split.**

---

### Commit Guidelines

**Message Format:**
- **Subject line:** Concise summary (50-72 characters)
- **Body:** Detailed description of changes
- **Footer:** Reference issues, breaking changes, validation status

**When to Commit:**
- After atomic upgrade completes and solution builds successfully
- After all tests pass (if two-commit approach, commit fixes after tests pass)

**What to Include:**
- All project file changes (.csproj files)
- Any code changes for breaking changes
- Any configuration changes (if needed)
- Updated documentation (if applicable)

**What NOT to Commit:**
- Build artifacts (bin/, obj/)
- User-specific files (.vs/, *.user)
- Temporary files
- NuGet package cache

---

### Review and Merge Process

#### Pre-Merge Checklist

**Before creating PR:**
- [ ] All projects build with 0 errors
- [ ] All projects build with 0 warnings (or warnings documented)
- [ ] All tests pass (comprehensive validation complete)
- [ ] No package conflicts
- [ ] No security vulnerabilities
- [ ] Application functionally validated
- [ ] Breaking changes documented
- [ ] Commit messages clear and descriptive

#### Pull Request Requirements

**PR Title:**
```
Upgrade Report_App_WASM solution to .NET 10.0 (LTS)
```

**PR Description Template:**
```markdown
## Summary
Upgrades the Report_App_WASM Blazor WebAssembly solution from .NET 8.0 to .NET 10.0 (Long Term Support).

## Strategy
All-At-Once: All 3 projects upgraded simultaneously in atomic operation.

## Projects Updated
- ✅ Report_App_WASM.Shared (net8.0 → net10.0)
- ✅ Report_App_WASM.Client (net8.0 → net10.0)
- ✅ Report_App_WASM.Server (net8.0 → net10.0)

## Package Updates
- 12 packages upgraded to version 10.0.3
- 2 packages removed (incompatible/redundant)
- 1 package deprecation addressed (Azure.Identity)

## Breaking Changes Addressed
- ✅ 3 binary incompatible APIs fixed (dependency injection configuration)
- ✅ 52 source incompatible APIs addressed (Directory Services, OleDb, Identity)
- ✅ 28 behavioral changes validated (HttpContent, Uri)

## Testing
- ✅ All projects build successfully (0 errors, 0 warnings)
- ✅ Smoke tests passed
- ✅ Functional tests passed
- ✅ Authentication flows validated
- ✅ Database connectivity verified
- ✅ UI components functional
- [ ] Performance validated (optional)

## Risk Assessment
- Risk Level: Medium (managed through comprehensive testing)
- Rollback Plan: Revert commit and return to net8.0 on source branch

## Validation
- ✅ Code review completed
- ✅ All success criteria met (see plan.md)
- ✅ No regressions identified

## References
- Assessment: `.github/upgrades/scenarios/new-dotnet-version_e0a9fc/assessment.md`
- Plan: `.github/upgrades/scenarios/new-dotnet-version_e0a9fc/plan.md`
- .NET 10 Migration Guide: [link if available]
```

#### PR Review Checklist

**Code Review Focus Areas:**
- [ ] Project file changes correct (TFM = net10.0)
- [ ] Package versions match plan (10.0.3 for Microsoft packages)
- [ ] Binary incompatible API fixes implemented correctly
- [ ] Directory Services package properly referenced
- [ ] Identity configuration updated correctly
- [ ] No hardcoded versions or temporary workarounds
- [ ] Code follows project conventions
- [ ] No unintended changes included

**Functional Review:**
- [ ] Reviewer can build solution locally
- [ ] Reviewer can run application locally
- [ ] Smoke tests pass for reviewer
- [ ] No obvious regressions observed

#### Merge Criteria

**Required for Merge Approval:**
1. ✅ All PR checklist items completed
2. ✅ At least one approval from code owner
3. ✅ All CI/CD pipeline checks pass (if configured)
4. ✅ No merge conflicts with target branch
5. ✅ All conversations resolved

**Merge Method:**
- **Recommended:** Squash and merge (creates clean single commit in main branch)
- **Alternative:** Standard merge (preserves commit history if using two-commit approach)

**Post-Merge Actions:**
1. Delete upgrade branch (`upgrade-to-NET10`) after successful merge
2. Tag merged commit: `v[version]-net10.0-upgrade`
3. Update project documentation with .NET 10 requirements
4. Communicate upgrade completion to team

---

### Rollback Procedure

**If Critical Issues Discovered During Review:**

1. **Do NOT merge PR**
2. **Document blockers** in PR comments
3. **Keep upgrade branch active**
4. **Options:**
   - **Option A:** Fix issues on upgrade branch and re-request review
   - **Option B:** Close PR, delete branch, restart upgrade with adjusted plan
   - **Option C:** Downgrade back to net8.0 on upgrade branch to isolate specific issues

**If Issues Discovered After Merge:**

1. **Assess severity:**
   - **Critical (production down):** Immediate revert commit
   - **High (major features broken):** Hotfix on main branch or revert
   - **Medium (minor issues):** Forward fix in new PR
   - **Low (cosmetic):** Schedule fix for next sprint

2. **Revert Process:**
   ```bash
   git checkout net10
   git revert <merge-commit-sha>
   git push origin net10
   ```

3. **Post-Revert:**
   - Document root cause
   - Update plan with lessons learned
   - Prepare improved upgrade attempt

---

### Branch Hygiene

**During Upgrade:**
- Commit regularly at stable checkpoints (if using multi-commit)
- Keep branch focused (no unrelated changes)
- Sync with source branch regularly if upgrade extends multiple days

**After Completion:**
- Delete local upgrade branch after merge
- Delete remote upgrade branch after validation period
- Clean up any temporary branches

---

## Success Criteria

### Technical Criteria (Mandatory)

**All projects successfully migrated:**
- ✅ `Report_App_WASM.Shared.csproj` targets net10.0
- ✅ `Report_App_WASM.Client.csproj` targets net10.0
- ✅ `Report_App_WASM.Server.csproj` targets net10.0

**All package updates applied:**
- ✅ 12 packages updated to version 10.0.3:
  - Microsoft.AspNetCore.Components.WebAssembly
  - Microsoft.AspNetCore.Components.WebAssembly.Authentication
  - Microsoft.AspNetCore.Components.WebAssembly.Server
  - Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.AspNetCore.Identity.UI
  - Microsoft.EntityFrameworkCore.SqlServer
  - Microsoft.EntityFrameworkCore.Tools
  - Microsoft.Extensions.Localization
  - System.Data.OleDb
  - System.DirectoryServices.AccountManagement
  - System.Text.Json
- ✅ 2 packages removed (Microsoft.NETCore.Platforms, Microsoft.VisualStudio.Azure.Containers.Tools.Targets)
- ✅ Azure.Identity deprecation addressed (updated or documented)

**Builds pass:**
- ✅ `dotnet restore Report_App_WASM.sln` succeeds with no errors
- ✅ `dotnet build Report_App_WASM.sln` succeeds with 0 errors
- ✅ `dotnet build Report_App_WASM.sln` produces 0 warnings (or all warnings documented and accepted)
- ✅ Each individual project builds successfully

**Tests pass:**
- ✅ All functional tests complete successfully
- ✅ Smoke tests pass (application starts, basic functionality works)
- ✅ Authentication/authorization tests pass
- ✅ Database connectivity tests pass
- ✅ API endpoint tests pass
- ✅ UI component tests pass

**No vulnerabilities:**
- ✅ No security vulnerabilities in packages (`dotnet list package --vulnerable`)
- ✅ No deprecated packages (or deprecations documented with migration plan)
- ✅ No known CVEs in dependency tree

**No dependency conflicts:**
- ✅ All package versions resolve without conflicts
- ✅ No package downgrade warnings
- ✅ Transitive dependencies compatible

---

### Quality Criteria (Recommended)

**Code quality maintained:**
- ✅ No new compiler warnings introduced
- ✅ Code analysis rules passing (if configured)
- ✅ Code follows project conventions
- ✅ No dead code introduced
- ✅ No technical debt added (workarounds properly documented if needed)

**Test coverage maintained:**
- ✅ Existing tests still pass
- ✅ Test coverage percentage unchanged or improved
- ✅ New tests added for new framework-specific behavior (if applicable)

**Documentation updated:**
- ✅ README.md updated with .NET 10 requirements
- ✅ Setup instructions reflect new SDK version
- ✅ Known issues documented
- ✅ Breaking changes documented for team
- ✅ `assessment.md` and `plan.md` committed to `.github/upgrades/scenarios/new-dotnet-version_e0a9fc/`

**Performance acceptable:**
- ✅ No significant performance regressions (< 10% slowdown in critical paths)
- ✅ Memory usage patterns acceptable
- ✅ Application startup time reasonable
- ✅ Client WASM download/initialization time acceptable

---

### Process Criteria (Compliance)

**All-At-Once Strategy followed:**
- ✅ All projects updated simultaneously in single atomic operation
- ✅ No multi-targeting introduced
- ✅ No intermediate mixed-version states
- ✅ Dependency order respected during validation
- ✅ Single comprehensive test pass completed

**Source Control Strategy followed:**
- ✅ All work performed on dedicated upgrade branch (`upgrade-to-NET10`)
- ✅ Source branch (`net10`) remains clean
- ✅ Single commit approach used (or two-commit if complexity required)
- ✅ Commit messages follow guidelines
- ✅ PR created with complete description
- ✅ PR review checklist completed
- ✅ Merge criteria satisfied

**Plan adherence:**
- ✅ All projects from plan upgraded
- ✅ All packages from plan updated
- ✅ All breaking changes from plan addressed
- ✅ All testing requirements from plan completed
- ✅ All validation checklists from plan checked

---

### Acceptance Gates

**Gate 1: Compilation Success** (Phase 1 Exit Criteria)
- Entire solution builds without errors
- No unresolved compilation issues
- All binary incompatible APIs fixed
- All source incompatible APIs resolved

**Gate 2: Smoke Test Success** (Phase 2 Entry Criteria)
- Application starts without exceptions
- Basic functionality works (login, navigation, API call)
- No immediate runtime failures

**Gate 3: Functional Test Success** (Phase 2 Exit Criteria)
- All core user scenarios complete successfully
- Authentication/authorization functional
- Database operations work
- API endpoints respond correctly
- UI components render properly
- Background services operational

**Gate 4: PR Approval** (Merge Gate)
- Code review approval received
- All PR checklist items completed
- No unresolved conversations
- CI/CD pipeline green (if configured)

**Gate 5: Production Validation** (Final Gate)
- Application deployed successfully
- Production smoke tests pass
- No critical issues in production logs
- User acceptance validation (if required)

---

### Definition of Done

The .NET 10.0 upgrade is **COMPLETE** when:

1. ✅ All Technical Criteria met (builds, tests, packages, vulnerabilities)
2. ✅ All Quality Criteria met (code quality, test coverage, documentation, performance)
3. ✅ All Process Criteria met (strategy followed, source control followed, plan adhered)
4. ✅ All Acceptance Gates passed (compilation, smoke tests, functional tests, PR approval)
5. ✅ PR merged to source branch (`net10`)
6. ✅ Upgrade branch deleted
7. ✅ Production deployment successful (if applicable)
8. ✅ Team notified of completion

---

### Metrics for Success Tracking

**Quantitative Metrics:**
- Build errors: 0
- Build warnings: 0 (or all documented)
- Test pass rate: 100%
- Package vulnerabilities: 0
- Performance regression: < 10%
- Code coverage change: ≥ 0% (no decrease)

**Qualitative Metrics:**
- Code reviewer satisfaction
- Team confidence in stability
- User acceptance (no complaints)
- Production stability (no incidents related to upgrade)

---

### Sign-Off Requirements

**Technical Sign-Off:**
- ✅ Developer: All code changes implemented and tested
- ✅ Code Reviewer: PR approved
- ✅ QA/Tester: All tests passed (if QA team exists)

**Process Sign-Off:**
- ✅ Plan followed completely (or deviations documented)
- ✅ All success criteria verified
- ✅ Documentation updated

**Deployment Sign-Off:**
- ✅ Staging environment validated (if applicable)
- ✅ Production deployment approved
- ✅ Rollback plan confirmed

---

### Post-Upgrade Validation Window

**Monitoring Period:** 7 days post-merge

**Monitor for:**
- Runtime exceptions in logs
- Performance degradation
- User-reported issues
- Background job failures
- Database connection issues
- Memory leaks

**Action if Issues Found:**
- Assess severity (critical/high/medium/low)
- Determine if related to .NET 10 upgrade
- Apply forward fix or rollback based on severity
- Update plan with lessons learned for future upgrades
