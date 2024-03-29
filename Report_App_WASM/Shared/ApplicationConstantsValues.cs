﻿namespace Report_App_WASM.Shared;

public class ApplicationConstantsValues
{
    public string? ApplicationName { get; init; }
    public string? ApplicationLogo { get; init; }
    public bool LdapLogin { get; init; }
    public bool ActivateTaskSchedulerModule { get; set; }
    public bool ActivateAdHocQueriesModule { get; set; }
    public bool WindowsEnv { get; set; }
}