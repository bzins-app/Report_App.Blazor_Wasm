// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme",
        Justification = "<En attente>", Scope = "member",
        Target =
            "~M:Report_App_WASM.Server.Services.RemoteDb.RemoteDbConnection.RemoteDbToDatableAsync(Report_App_WASM.Shared.RemoteQueryParameters.RemoteDbCommandParameters,System.Threading.CancellationToken,System.Int32)~System.Threading.Tasks.Task{System.Data.DataTable}")]
[assembly:
    SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme",
        Justification = "<En attente>", Scope = "member",
        Target =
            "~M:Report_App_WASM.Server.Services.RemoteDb.RemoteDbConnection.TryConnectAsync(Report_App_WASM.Shared.TypeDb,System.String)~System.Threading.Tasks.Task")]
[assembly:
    SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme",
        Justification = "<En attente>", Scope = "member",
        Target =
            "~M:Report_App_WASM.Server.Controllers.AuthorizeController.LdapLogin(Report_App_WASM.Shared.LoginParameters)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]