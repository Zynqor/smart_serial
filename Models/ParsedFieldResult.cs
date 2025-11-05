using CommunityToolkit.Mvvm.ComponentModel;

namespace SerialProtocolAssistant.Models;

public partial class ParsedFieldResult : ObservableObject
{
    [ObservableProperty]
    private string _fieldDescription = string.Empty;

    [ObservableProperty]
    private string _fieldName = string.Empty;

    [ObservableProperty]
    private string _rawBytes = string.Empty;

    [ObservableProperty]
    private string _parsedValue = string.Empty;

    [ObservableProperty]
    private string? _unit;
}
