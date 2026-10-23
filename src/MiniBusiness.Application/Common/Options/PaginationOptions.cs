using System.ComponentModel.DataAnnotations;

namespace MiniBusiness.Application.Common.Options;

public class PaginationOptions
{
    public const string SectionName = "Pagination";

    public int MinPage { get; set; } = 1;
    
    [Range(1, int.MaxValue, ErrorMessage = "DefaultPageSize must be greater than 0.")]
    public int DefaultPageSize { get; set; } = 10;
    
    public int MinPageSize { get; set; } = 1;
    
    [Range(1, int.MaxValue, ErrorMessage = "MaxPageSize must be greater than 0.")]
    public int MaxPageSize { get; set; } = 100;
}