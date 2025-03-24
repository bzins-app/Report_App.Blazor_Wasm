using Microsoft.AspNetCore.Components;

public abstract class BaseGridComponent<T> : ComponentBase, IAsyncDisposable where T : class
{
    protected GridItemsProvider<T>? _itemsProvider;
    protected PaginationState _pagination = new() { ItemsPerPage = 10 };
    protected SimpleGridFieldsContent? _translations;
    protected SimpleGrid<T> _grid = null!;
    protected string? _errorMessage;
    protected bool _rendering;

    [Inject] protected DataInteractionService DataService { get; set; } = null!;
    [Inject] protected ApplicationService AppService { get; set; } = null!;

    protected override void OnInitialized()
    {
        _itemsProvider = async req =>
        {
            var response = await GetRemoteData(req);
            _rendering = false;
            if (response is null)
            {
                return GridItemsProviderResult.From(Array.Empty<T>(), 0);
            }

            return GridItemsProviderResult.From(response.Value!, response.Count);
        };
        _translations = AppService.GetGridTranslations();
    }

    protected abstract Task<ApiResponse<T>?> GetRemoteData(GridItemsProviderRequest<T> req);

    protected async Task OnRefresh()
    {
        _rendering = true;
        await _grid.RefreshDataAsync();
        _rendering = false;
    }

    protected async Task RemoveFilters()
    {
        _rendering = true;
        await _grid.RemoveFilters();
        _rendering = false;
    }

    public async ValueTask DisposeAsync()
    {
        await _grid.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}