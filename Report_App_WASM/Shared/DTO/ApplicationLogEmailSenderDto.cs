﻿namespace Report_App_WASM.Shared.DTO;

public class ApplicationLogEmailSenderDto : IDto
{
    public int Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public string? EmailTitle { get; set; }
    public string? Result { get; set; }
    public bool Error { get; set; }
    public int NbrOfRecipients { get; set; }
    public string? RecipientList { get; set; }
}